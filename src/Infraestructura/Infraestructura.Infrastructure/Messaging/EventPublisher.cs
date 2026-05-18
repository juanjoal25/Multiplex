using Infraestructura.Application.Abstractions;
using MassTransit;

namespace Infraestructura.Infrastructure.Messaging;

public sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default) where TEvent : notnull
        => publishEndpoint.Publish(integrationEvent, ct);
}
