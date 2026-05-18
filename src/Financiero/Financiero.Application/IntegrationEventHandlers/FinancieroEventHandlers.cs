using Financiero.Application.UseCases.RegistrarTransaccion;
using Financiero.Domain.ValueObjects;
using MediatR;

namespace Financiero.Application.IntegrationEventHandlers;

public sealed record OrdenConfirmadaNotificacion(
    Guid IdOrden,
    IReadOnlyCollection<ConceptoInput> Conceptos,
    IReadOnlyCollection<decimal> Descuentos,
    decimal ValorTotal,
    string Moneda) : INotification;

public sealed class OrdenConfirmadaHandler(IMediator mediator) : INotificationHandler<OrdenConfirmadaNotificacion>
{
    public async Task Handle(OrdenConfirmadaNotificacion n, CancellationToken ct)
    {
        await mediator.Send(new RegistrarTransaccionCommand(
            n.IdOrden, n.Conceptos, n.Descuentos, n.ValorTotal, n.Moneda, MetodoPago.TarjetaCredito), ct);
    }
}
