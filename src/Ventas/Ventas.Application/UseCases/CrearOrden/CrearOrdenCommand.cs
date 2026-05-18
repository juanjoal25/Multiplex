using MediatR;
using Shared.Kernel.Exceptions;
using Shared.Kernel.ValueObjects;
using Ventas.Application.Abstractions;
using Ventas.Domain.Aggregates.OrdenAgg;
using Ventas.Domain.Repositories;
using Ventas.Domain.Services;
using Ventas.Domain.ValueObjects;
using DomainEvents = Ventas.Domain.Events;

namespace Ventas.Application.UseCases.CrearOrden;

public sealed record BoletaInput(Guid IdFuncion, Guid IdSilla);
public sealed record ConfiteriaInput(Guid IdProducto, int Cantidad);

public sealed record CrearOrdenCommand(
    Guid IdEspectador,
    IReadOnlyCollection<BoletaInput> Boletas,
    IReadOnlyCollection<ConfiteriaInput> Confiterias,
    int MinutosExpiracion,
    bool EsEventoCorporativo,
    string? TerceroCorporativo
) : IRequest<CrearOrdenResult>;

public sealed record CrearOrdenResult(Guid IdOrden, decimal Total, DateTime Expiracion);

public sealed class CrearOrdenHandler(
    IOrdenRepository ordenRepo,
    IProductoRepository productoRepo,
    IClientesClient clientes,
    IProgramacionClient programacion,
    IInfraestructuraClient infra,
    ICadenaClient cadena,
    SvcCalculoPrecio precio,
    IEventPublisher publisher,
    IUnitOfWork uow,
    ParametrosPrecio parametros) : IRequestHandler<CrearOrdenCommand, CrearOrdenResult>
{
    public async Task<CrearOrdenResult> Handle(CrearOrdenCommand cmd, CancellationToken ct)
    {
        // Validación corporativa
        if (cmd.EsEventoCorporativo)
        {
            if (string.IsNullOrWhiteSpace(cmd.TerceroCorporativo))
                throw new PreconditionFailedException("TerceroCorporativo requerido para evento corporativo");
            if (!await cadena.ContratoVigenteAsync(cmd.TerceroCorporativo, ct))
                throw new PreconditionFailedException("No existe contrato vigente con el tercero");
        }

        // Descuento del espectador
        Descuento descuento;
        if (cmd.EsEventoCorporativo)
            descuento = Descuento.Cero(NivelOrigen.Corporativo);
        else
        {
            var info = await clientes.ConsultarDescuentoAsync(cmd.IdEspectador, ct)
                ?? throw new PreconditionFailedException("Espectador no existe en Clientes");
            var nivel = info.Estado.Equals("Activa", StringComparison.OrdinalIgnoreCase)
                ? Enum.TryParse<NivelOrigen>(info.Nivel, true, out var n) ? n : NivelOrigen.SinSuscripcion
                : NivelOrigen.SinSuscripcion;
            descuento = info.Estado.Equals("Activa", StringComparison.OrdinalIgnoreCase)
                ? Descuento.Of(info.Porcentaje, nivel)
                : Descuento.Cero(NivelOrigen.SinSuscripcion);
        }

        // Construir boletas (precio resuelto desde Programacion+Infra)
        var boletas = new List<ItemBoleta>();
        foreach (var b in cmd.Boletas)
        {
            var func = await programacion.ConsultarFuncionAsync(b.IdFuncion, ct)
                ?? throw new PreconditionFailedException($"Funcion {b.IdFuncion} no existe");
            var silla = await infra.ConsultarSillaAsync(b.IdSilla, ct)
                ?? throw new PreconditionFailedException($"Silla {b.IdSilla} no existe");
            if (!silla.Disponible) throw new ConflictException($"Silla {b.IdSilla} no disponible");

            var tipoSilla = Enum.TryParse<TipoBoleta>(silla.TipoSilla, true, out var ts) ? ts : TipoBoleta.General;
            var precioBase = precio.CalcularPrecioBoleta(tipoSilla, Money.Of(func.PrecioExtraFormato), parametros);
            boletas.Add(ItemBoleta.Crear(FuncionRef.Of(b.IdFuncion), SillaRef.Of(b.IdSilla), precioBase, tipoSilla));
        }

        // Construir items confitería (validar stock)
        var confiterias = new List<ItemConfiteria>();
        foreach (var c in cmd.Confiterias)
        {
            var p = await productoRepo.GetByIdAsync(ProductoId.Of(c.IdProducto), ct)
                ?? throw new PreconditionFailedException($"Producto {c.IdProducto} no existe");
            if (p.Stock < c.Cantidad)
                throw new PreconditionFailedException($"Stock insuficiente para {p.Nombre}");
            confiterias.Add(ItemConfiteria.Crear(p.Id, c.Cantidad, p.Precio));
        }

        var ahora = DateTime.UtcNow;
        var expiracion = Expiracion.Of(ahora.AddMinutes(cmd.MinutosExpiracion), ahora);
        var orden = Orden.Crear(EspectadorRef.Of(cmd.IdEspectador), boletas, confiterias, descuento, expiracion);

        await ordenRepo.AddAsync(orden, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var e in orden.DomainEvents.OfType<DomainEvents.OrdenCreada>())
            await publisher.PublishAsync(new Messaging.Contracts.Ventas.OrdenCreada(
                e.IdOrden.Value, e.Espectador.Value, e.Sillas,
                cmd.Boletas.Select(b => b.IdFuncion).ToList(), e.Total.Amount, e.Expiracion), ct);

        orden.ClearEvents();
        return new CrearOrdenResult(orden.Id.Value, orden.Total.Amount, orden.Expiracion.Valor);
    }
}
