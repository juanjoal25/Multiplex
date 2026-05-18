using Shared.Kernel;

namespace Clientes.Application.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default) where TEvent : notnull;
    Task PublishDomainEventsAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}

public interface IFinancieroClient
{
    Task<bool> PagoFueAprobadoAsync(Guid idOrdenPago, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
