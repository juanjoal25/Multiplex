using Shared.Kernel;

namespace Programacion.Application.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default) where TEvent : notnull;
}

public interface IInfraestructuraClient
{
    Task<bool> SalaExisteYDisponibleAsync(Guid idSala, CancellationToken ct = default);
    Task<string?> TipoSalaAsync(Guid idSala, CancellationToken ct = default);
}

public interface IUnitOfWork { Task<int> SaveChangesAsync(CancellationToken ct = default); }
