using MassTransit;
using MediatR;
using Messaging.Contracts.Financiero;
using Messaging.Contracts.Programacion;
using Ventas.Application.Abstractions;
using Ventas.Application.IntegrationEventHandlers;

namespace Ventas.Infrastructure.Messaging;

public sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default) where TEvent : notnull
        => publishEndpoint.Publish(integrationEvent, ct);
}

public sealed class PagoAprobadoConsumer(IMediator m) : IConsumer<PagoAprobado>
{
    public Task Consume(ConsumeContext<PagoAprobado> ctx) =>
        m.Publish(new PagoAprobadoNotificacion(ctx.Message.IdOrden, ctx.Message.ReferenciaExterna), ctx.CancellationToken);
}

public sealed class PagoRechazadoConsumer(IMediator m) : IConsumer<PagoRechazado>
{
    public Task Consume(ConsumeContext<PagoRechazado> ctx) =>
        m.Publish(new PagoRechazadoNotificacion(ctx.Message.IdOrden, ctx.Message.Motivo), ctx.CancellationToken);
}

public sealed class FuncionCanceladaConsumer(IMediator m) : IConsumer<FuncionCancelada>
{
    public Task Consume(ConsumeContext<FuncionCancelada> ctx) =>
        m.Publish(new FuncionCanceladaParaOrdenes(ctx.Message.IdFuncion), ctx.CancellationToken);
}
