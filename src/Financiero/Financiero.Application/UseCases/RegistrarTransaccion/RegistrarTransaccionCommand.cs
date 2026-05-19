using Financiero.Application.Abstractions;
using Financiero.Domain.Aggregates.TransaccionAgg;
using Financiero.Domain.Repositories;
using Financiero.Domain.Services;
using Financiero.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;
using Shared.Kernel.ValueObjects;
using DomainEvents = Financiero.Domain.Events;

namespace Financiero.Application.UseCases.RegistrarTransaccion;

public sealed record ConceptoInput(string Descripcion, decimal Valor);
public sealed record RegistrarTransaccionCommand(
    Guid IdOrden,
    IReadOnlyCollection<ConceptoInput> Conceptos,
    IReadOnlyCollection<decimal> Descuentos,
    decimal ValorTotal,
    string Moneda,
    MetodoPago MetodoPago
) : IRequest<Guid>;

public sealed class RegistrarTransaccionHandler(
    ITransaccionRepository repo,
    IPasarelaClient pasarela,
    SvcProcesoPago proceso,
    IEventPublisher publisher,
    IUnitOfWork uow) : IRequestHandler<RegistrarTransaccionCommand, Guid>
{
    public async Task<Guid> Handle(RegistrarTransaccionCommand cmd, CancellationToken ct)
    {
        // Idempotencia: no permitir dos transacciones para el mismo OrdenId
        if (await repo.ExistsByOrdenIdAsync(cmd.IdOrden, ct))
            throw new ConflictException("Ya existe transacción para la orden (idempotencia)");

        var conceptos = cmd.Conceptos.Select(c => ConceptoFacturable.Of(c.Descripcion, Money.Of(c.Valor, cmd.Moneda))).ToList();
        var orden = OrdenDepurada.Of(cmd.IdOrden, conceptos, cmd.Descuentos, Money.Of(cmd.ValorTotal, cmd.Moneda));

        var transaccion = Transaccion.Registrar(orden, cmd.MetodoPago);
        await repo.AddAsync(transaccion, ct);

        // Publicar TransaccionRegistrada (incluso antes del pago)
        foreach (var e in transaccion.DomainEvents.OfType<DomainEvents.TransaccionRegistrada>())
            await publisher.PublishAsync(new Messaging.Contracts.Financiero.TransaccionRegistrada(
                e.IdTransaccion.Value, e.IdOrden, e.ValorTotal.Amount, e.ValorTotal.Currency, e.OccurredOn), ct);
        transaccion.ClearEvents();

        // Procesar pago en pasarela externa (entity stays in Added state; EF captures final values on INSERT)
        await proceso.ProcesarPago(transaccion, (o, m, c) => pasarela.ProcesarPagoAsync(o, m, c), ct);

        foreach (var e in transaccion.DomainEvents)
        {
            switch (e)
            {
                case DomainEvents.PagoAprobado pa:
                    await publisher.PublishAsync(new Messaging.Contracts.Financiero.PagoAprobado(
                        pa.IdTransaccion.Value, pa.IdOrden, pa.ReferenciaExterna, pa.OccurredOn), ct);
                    break;
                case DomainEvents.PagoRechazado pr:
                    await publisher.PublishAsync(new Messaging.Contracts.Financiero.PagoRechazado(
                        pr.IdTransaccion.Value, pr.IdOrden, pr.Motivo, pr.OccurredOn), ct);
                    break;
            }
        }

        await uow.SaveChangesAsync(ct);
        transaccion.ClearEvents();
        return transaccion.Id.Value;
    }
}
