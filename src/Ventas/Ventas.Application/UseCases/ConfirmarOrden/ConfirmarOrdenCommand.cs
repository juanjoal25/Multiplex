using MediatR;
using Shared.Kernel.Exceptions;
using Ventas.Application.Abstractions;
using Ventas.Domain.Aggregates.OrdenAgg;
using Ventas.Domain.Repositories;
using Ventas.Domain.ValueObjects;
using DomainEvents = Ventas.Domain.Events;

namespace Ventas.Application.UseCases.ConfirmarOrden;

public sealed record ConfirmarOrdenCommand(Guid IdOrden) : IRequest<Unit>;

public sealed class ConfirmarOrdenHandler(
    IOrdenRepository ordenRepo,
    IProductoRepository productoRepo,
    IEventPublisher publisher,
    IUnitOfWork uow) : IRequestHandler<ConfirmarOrdenCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmarOrdenCommand cmd, CancellationToken ct)
    {
        var orden = await ordenRepo.GetByIdAsync(OrdenId.Of(cmd.IdOrden), ct)
            ?? throw new PreconditionFailedException("Orden no existe");

        orden.Confirmar(DateTime.UtcNow);

        // Reducir stock de cada confitería
        foreach (var item in orden.Confiterias)
        {
            var p = await productoRepo.GetByIdAsync(item.ProductoId, ct)
                ?? throw new PreconditionFailedException("Producto no existe");
            p.ReducirStock(item.Cantidad);
            await productoRepo.UpdateAsync(p, ct);
        }

        await ordenRepo.UpdateAsync(orden, ct);
        await uow.SaveChangesAsync(ct);

        // Construir OrdenDepurada e integration event
        foreach (var e in orden.DomainEvents.OfType<DomainEvents.OrdenConfirmada>())
        {
            var conceptos = new List<Messaging.Contracts.Ventas.ConceptoFacturableDto>();
            foreach (var b in orden.Boletas)
                conceptos.Add(new("Boleta " + b.Tipo, b.PrecioBase.Amount, b.PrecioBase.Currency));
            foreach (var c in orden.Confiterias)
                conceptos.Add(new("Confiteria producto " + c.ProductoId.Value, c.Subtotal().Amount, c.Subtotal().Currency));

            var descuentos = orden.Descuento.Porcentaje > 0
                ? new[] { new Messaging.Contracts.Ventas.DescuentoAplicadoDto(orden.Descuento.Porcentaje, orden.Descuento.NivelOrigen.ToString()) }
                : Array.Empty<Messaging.Contracts.Ventas.DescuentoAplicadoDto>();

            await publisher.PublishAsync(new Messaging.Contracts.Ventas.OrdenConfirmada(
                e.IdOrden.Value, conceptos, descuentos, e.ValorTotal.Amount, e.ValorTotal.Currency), ct);
        }

        orden.ClearEvents();
        return Unit.Value;
    }
}
