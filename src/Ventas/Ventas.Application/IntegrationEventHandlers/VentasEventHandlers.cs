using MediatR;
using Shared.Kernel.Exceptions;
using Ventas.Application.UseCases.CancelarOrden;
using Ventas.Domain.Repositories;
using Ventas.Domain.ValueObjects;

namespace Ventas.Application.IntegrationEventHandlers;

public sealed record PagoAprobadoNotificacion(Guid IdOrden, string ReferenciaExterna) : INotification;
public sealed record PagoRechazadoNotificacion(Guid IdOrden, string Motivo) : INotification;
public sealed record FuncionCanceladaParaOrdenes(Guid IdFuncion) : INotification;

public sealed class PagoRechazadoHandler(IMediator mediator) : INotificationHandler<PagoRechazadoNotificacion>
{
    public async Task Handle(PagoRechazadoNotificacion n, CancellationToken ct)
    {
        try { await mediator.Send(new CancelarOrdenCommand(n.IdOrden, $"Pago rechazado: {n.Motivo}"), ct); }
        catch (PreconditionFailedException) { /* orden ya no PENDIENTE */ }
    }
}

public sealed class FuncionCanceladaParaOrdenesHandler(IOrdenRepository repo, IMediator mediator)
    : INotificationHandler<FuncionCanceladaParaOrdenes>
{
    public async Task Handle(FuncionCanceladaParaOrdenes n, CancellationToken ct)
    {
        var ordenes = await repo.GetPendientesConFuncionAsync(n.IdFuncion, ct);
        foreach (var o in ordenes)
        {
            try { await mediator.Send(new CancelarOrdenCommand(o.Id.Value, "Funcion cancelada"), ct); }
            catch (PreconditionFailedException) { }
        }
    }
}
