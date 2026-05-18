using MassTransit;
using MediatR;
using Messaging.Contracts.Infraestructura;
using Programacion.Application.IntegrationEventHandlers;

namespace Programacion.Infrastructure.Messaging;

public sealed class SalaEnMantenimientoConsumer(IMediator mediator) : IConsumer<SalaEnMantenimiento>
{
    public Task Consume(ConsumeContext<SalaEnMantenimiento> ctx)
        => mediator.Publish(new SalaEnMantenimientoNotificacion(ctx.Message.IdSala), ctx.CancellationToken);
}

public sealed class SalaReactivadaConsumer(IMediator mediator) : IConsumer<SalaReactivada>
{
    public Task Consume(ConsumeContext<SalaReactivada> ctx)
        => mediator.Publish(new SalaReactivadaNotificacion(ctx.Message.IdSala), ctx.CancellationToken);
}
