using Infraestructura.Application.Abstractions;
using Infraestructura.Domain.Aggregates.SalaAgg;
using Infraestructura.Domain.Repositories;
using Infraestructura.Domain.States;
using Infraestructura.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Infrastructure.Persistence;

public sealed class SalaRepository(InfraestructuraDbContext db) : ISalaRepository
{
    public Task<Sala?> GetByIdAsync(SalaId id, CancellationToken ct = default)
        => db.Salas.Include(s => s.Sillas).FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(Sala a, CancellationToken ct = default) => await db.Salas.AddAsync(a, ct);
    public Task UpdateAsync(Sala a, CancellationToken ct = default) { db.Salas.Update(a); return Task.CompletedTask; }
    public Task DeleteAsync(Sala a, CancellationToken ct = default) { db.Salas.Remove(a); return Task.CompletedTask; }

    public async Task<IReadOnlyCollection<Sala>> GetAllAsync(CancellationToken ct = default)
        => await db.Salas.Include(s => s.Sillas).ToListAsync(ct);

    public Task<Sala?> GetBySillaIdAsync(SillaId sillaId, CancellationToken ct = default)
        => db.Salas.Include(s => s.Sillas).FirstOrDefaultAsync(s => s.Sillas.Any(x => x.Id == sillaId), ct);

    public async Task<IReadOnlyCollection<Sala>> GetConReservasExpiradasAsync(DateTime ahora, CancellationToken ct = default)
    {
        var data = await db.Salas.Include(s => s.Sillas).ToListAsync(ct);
        return data.Where(s => s.Sillas.Any(si => si.Estado.Tipo == EstadoSillaTipo.Reservada
            && si.ReservaExpiracion != null && si.ReservaExpiracion.HaExpirado(ahora))).ToList();
    }
}

public sealed class UnitOfWork(InfraestructuraDbContext db) : IUnitOfWork
{ public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct); }
