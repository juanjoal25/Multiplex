namespace Infraestructura.Application.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default) where TEvent : notnull;
}

public interface IUnitOfWork { Task<int> SaveChangesAsync(CancellationToken ct = default); }
