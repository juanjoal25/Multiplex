using Financiero.Application.Abstractions;
using Financiero.Application.IntegrationEventHandlers;
using Financiero.Application.UseCases.RegistrarTransaccion;
using Financiero.Domain.ValueObjects;
using MassTransit;
using MediatR;
using Messaging.Contracts.Ventas;

namespace Financiero.Infrastructure.Messaging;

public sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default) where TEvent : notnull
        => publishEndpoint.Publish(integrationEvent, ct);
}

public sealed class OrdenConfirmadaConsumer(IMediator m) : IConsumer<OrdenConfirmada>
{
    public Task Consume(ConsumeContext<OrdenConfirmada> ctx)
    {
        var conceptos = ctx.Message.ConceptosFacturables
            .Select(c => new ConceptoInput(c.Descripcion, c.Valor)).ToList();
        var descuentos = ctx.Message.DescuentosAplicados.Select(d => d.Porcentaje).ToList();
        return m.Publish(new OrdenConfirmadaNotificacion(
            ctx.Message.IdOrden, conceptos, descuentos, ctx.Message.ValorTotal, ctx.Message.Moneda),
            ctx.CancellationToken);
    }
}

// Stub PasarelaClient: aprueba todos los pagos en desarrollo
public sealed class StubPasarelaClient : IPasarelaClient
{
    public Task<(EstadoPago estado, ReferenciaExterna? referencia, string? motivo)> ProcesarPagoAsync(
        OrdenDepurada orden, MetodoPago metodo, CancellationToken ct)
    {
        var refExt = ReferenciaExterna.Of($"STUB-{Guid.NewGuid():N}");
        return Task.FromResult<(EstadoPago, ReferenciaExterna?, string?)>((EstadoPago.Aprobado, refExt, null));
    }
}
