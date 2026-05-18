namespace Shared.Kernel;

public interface IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task AddAsync(TAggregate aggregate, CancellationToken ct = default);
    Task UpdateAsync(TAggregate aggregate, CancellationToken ct = default);
    Task DeleteAsync(TAggregate aggregate, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
