using Financiero.Application.Abstractions;
using Financiero.Domain.Aggregates.TransaccionAgg;
using Financiero.Domain.Repositories;
using Financiero.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Financiero.Infrastructure.Persistence;

public sealed class TransaccionRepository(FinancieroDbContext db) : ITransaccionRepository
{
    public Task<Transaccion?> GetByIdAsync(TransaccionId id, CancellationToken ct = default)
        => db.Transacciones.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(Transaccion a, CancellationToken ct = default) => await db.Transacciones.AddAsync(a, ct);
    public Task UpdateAsync(Transaccion a, CancellationToken ct = default) { db.Transacciones.Update(a); return Task.CompletedTask; }
    public Task DeleteAsync(Transaccion a, CancellationToken ct = default) { db.Transacciones.Remove(a); return Task.CompletedTask; }

    public Task<Transaccion?> GetByOrdenIdAsync(Guid idOrden, CancellationToken ct = default)
        => db.Transacciones.FirstOrDefaultAsync(t => t.Orden.IdOrden == idOrden, ct);

    public Task<bool> ExistsByOrdenIdAsync(Guid idOrden, CancellationToken ct = default)
        => db.Transacciones.AnyAsync(t => t.Orden.IdOrden == idOrden, ct);

    public async Task<IReadOnlyCollection<Transaccion>> GetHistorialAsync(DateTime inicio, DateTime fin, CancellationToken ct = default)
        => await db.Transacciones.Where(t => t.Timestamp >= inicio && t.Timestamp <= fin).ToListAsync(ct);
}

public sealed class UnitOfWork(FinancieroDbContext db) : IUnitOfWork
{ public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct); }
