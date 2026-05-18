using Infraestructura.Application.UseCases.LiberarSilla;
using Infraestructura.Application.UseCases.ReservarSilla;
using MediatR;

namespace Infraestructura.Application.IntegrationEventHandlers;

// Notifications recibidas vía MassTransit consumers (a definir en Infrastructure)
public sealed record OrdenCreadaNotificacion(Guid IdOrden, IReadOnlyCollection<Guid> Sillas, IReadOnlyCollection<Guid> Funciones, DateTime Expiracion) : INotification;
public sealed record OrdenExpiradaNotificacion(Guid IdOrden, IReadOnlyCollection<Guid> Sillas) : INotification;
public sealed record OrdenCanceladaNotificacion(Guid IdOrden, IReadOnlyCollection<Guid> Sillas) : INotification;
public sealed record FuncionCanceladaNotificacion(Guid IdFuncion, Guid IdSala) : INotification;

public sealed class OrdenCreadaHandler(IMediator mediator) : INotificationHandler<OrdenCreadaNotificacion>
{
    public async Task Handle(OrdenCreadaNotificacion n, CancellationToken ct)
    {
        var i = 0;
        foreach (var silla in n.Sillas)
        {
            var idFuncion = n.Funciones.ElementAtOrDefault(i++);
            await mediator.Send(new ReservarSillaCommand(silla, idFuncion, n.IdOrden, n.Expiracion), ct);
        }
    }
}

public sealed class OrdenExpiradaHandler(IMediator mediator) : INotificationHandler<OrdenExpiradaNotificacion>
{
    public async Task Handle(OrdenExpiradaNotificacion n, CancellationToken ct)
    {
        foreach (var silla in n.Sillas)
            await mediator.Send(new LiberarSillaCommand(silla, "EXPIRACION"), ct);
    }
}

public sealed class OrdenCanceladaHandler(IMediator mediator) : INotificationHandler<OrdenCanceladaNotificacion>
{
    public async Task Handle(OrdenCanceladaNotificacion n, CancellationToken ct)
    {
        foreach (var silla in n.Sillas)
            await mediator.Send(new LiberarSillaCommand(silla, "CANCELACION"), ct);
    }
}
