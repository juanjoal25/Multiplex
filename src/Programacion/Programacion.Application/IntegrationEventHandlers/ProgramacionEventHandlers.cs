using MassTransitStub = System.Object;
using MediatR;
using Programacion.Domain.Repositories;

namespace Programacion.Application.IntegrationEventHandlers;

// Reacciona a SalaEnMantenimiento / SalaReactivada de Infraestructura
public sealed record SalaEnMantenimientoNotificacion(Guid IdSala) : INotification;
public sealed record SalaReactivadaNotificacion(Guid IdSala) : INotification;

public sealed class SalaEnMantenimientoHandler : INotificationHandler<SalaEnMantenimientoNotificacion>
{
    public Task Handle(SalaEnMantenimientoNotificacion notification, CancellationToken ct) => Task.CompletedTask;
}

public sealed class SalaReactivadaHandler : INotificationHandler<SalaReactivadaNotificacion>
{
    public Task Handle(SalaReactivadaNotificacion notification, CancellationToken ct) => Task.CompletedTask;
}
