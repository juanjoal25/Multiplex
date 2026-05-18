using Clientes.Application.Abstractions;
using MassTransit;
using Shared.Kernel;

namespace Clientes.Infrastructure.Messaging;

public sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default) where TEvent : notnull
        => publishEndpoint.Publish(integrationEvent, ct);

    public async Task PublishDomainEventsAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var e in events) await publishEndpoint.Publish(e, ct);
    }
}
