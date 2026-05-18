using Clientes.Application.Abstractions;

namespace Clientes.Infrastructure.Persistence;

public sealed class UnitOfWork(ClientesDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
