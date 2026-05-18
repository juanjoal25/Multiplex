using Cadena.Application.Abstractions;
using Cadena.Domain.Aggregates.SucursalAgg;
using Cadena.Domain.Repositories;
using Cadena.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Cadena.Infrastructure.Persistence;

public sealed class SucursalRepository(CadenaDbContext db) : ISucursalRepository
{
    public Task<Sucursal?> GetByIdAsync(SucursalId id, CancellationToken ct = default)
        => db.Sucursales.Include(s => s.Contratos).FirstOrDefaultAsync(s => s.Id == id, ct);
    public async Task AddAsync(Sucursal a, CancellationToken ct = default) => await db.Sucursales.AddAsync(a, ct);
    public Task UpdateAsync(Sucursal a, CancellationToken ct = default) { db.Sucursales.Update(a); return Task.CompletedTask; }
    public Task DeleteAsync(Sucursal a, CancellationToken ct = default) { db.Sucursales.Remove(a); return Task.CompletedTask; }
    public async Task<IReadOnlyCollection<Sucursal>> GetTodasAsync(CancellationToken ct = default)
        => await db.Sucursales.Include(s => s.Contratos).ToListAsync(ct);
    public Task<Sucursal?> GetByContratoIdAsync(ContratoId id, CancellationToken ct = default)
        => db.Sucursales.Include(s => s.Contratos).FirstOrDefaultAsync(s => s.Contratos.Any(c => c.Id == id), ct);
}

public sealed class UnitOfWork(CadenaDbContext db) : IUnitOfWork
{ public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct); }
