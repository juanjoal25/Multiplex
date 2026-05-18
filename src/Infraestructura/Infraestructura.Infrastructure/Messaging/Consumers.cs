using Infraestructura.Application.IntegrationEventHandlers;
using MassTransit;
using MediatR;
using Messaging.Contracts.Programacion;
using Messaging.Contracts.Ventas;

namespace Infraestructura.Infrastructure.Messaging;

public sealed class OrdenCreadaConsumer(IMediator m) : IConsumer<OrdenCreada>
{
    public Task Consume(ConsumeContext<OrdenCreada> ctx) =>
        m.Publish(new OrdenCreadaNotificacion(ctx.Message.IdOrden, ctx.Message.SillasReservadas, ctx.Message.Funciones, ctx.Message.Expiracion), ctx.CancellationToken);
}

public sealed class OrdenExpiradaConsumer(IMediator m) : IConsumer<OrdenExpirada>
{
    public Task Consume(ConsumeContext<OrdenExpirada> ctx) =>
        m.Publish(new OrdenExpiradaNotificacion(ctx.Message.IdOrden, ctx.Message.SillaRefs), ctx.CancellationToken);
}

public sealed class OrdenCanceladaConsumer(IMediator m) : IConsumer<OrdenCancelada>
{
    public Task Consume(ConsumeContext<OrdenCancelada> ctx) =>
        m.Publish(new OrdenCanceladaNotificacion(ctx.Message.IdOrden, ctx.Message.SillaRefs), ctx.CancellationToken);
}

public sealed class FuncionCanceladaConsumer(IMediator m) : IConsumer<FuncionCancelada>
{
    public Task Consume(ConsumeContext<FuncionCancelada> ctx) =>
        m.Publish(new FuncionCanceladaNotificacion(ctx.Message.IdFuncion, ctx.Message.IdSala), ctx.CancellationToken);
}
