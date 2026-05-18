using Microsoft.EntityFrameworkCore;
using Programacion.Application.Abstractions;
using Programacion.Domain.Aggregates.AlquilerAgg;
using Programacion.Domain.Aggregates.CarteleraAgg;
using Programacion.Domain.Aggregates.FuncionAgg;
using Programacion.Domain.Aggregates.PeliculaAgg;
using Programacion.Domain.Repositories;
using Programacion.Domain.ValueObjects;

namespace Programacion.Infrastructure.Persistence;

public sealed class PeliculaRepository(ProgramacionDbContext db) : IPeliculaRepository
{
    public Task<Pelicula?> GetByIdAsync(PeliculaId id, CancellationToken ct = default) => db.Peliculas.FirstOrDefaultAsync(p => p.Id == id, ct);
    public async Task AddAsync(Pelicula a, CancellationToken ct = default) => await db.Peliculas.AddAsync(a, ct);
    public Task UpdateAsync(Pelicula a, CancellationToken ct = default) { db.Peliculas.Update(a); return Task.CompletedTask; }
    public Task DeleteAsync(Pelicula a, CancellationToken ct = default) { db.Peliculas.Remove(a); return Task.CompletedTask; }
}

public sealed class FuncionRepository(ProgramacionDbContext db) : IFuncionRepository
{
    public Task<Funcion?> GetByIdAsync(FuncionId id, CancellationToken ct = default) => db.Funciones.FirstOrDefaultAsync(f => f.Id == id, ct);
    public async Task AddAsync(Funcion a, CancellationToken ct = default) => await db.Funciones.AddAsync(a, ct);
    public Task UpdateAsync(Funcion a, CancellationToken ct = default) { db.Funciones.Update(a); return Task.CompletedTask; }
    public Task DeleteAsync(Funcion a, CancellationToken ct = default) { db.Funciones.Remove(a); return Task.CompletedTask; }
    public async Task<IReadOnlyCollection<Funcion>> GetByVigentesEnSalaAsync(Guid idSala, CancellationToken ct = default)
        => await db.Funciones.Where(f => f.SalaRef.IdSala == idSala).ToListAsync(ct);
}

public sealed class CarteleraRepository(ProgramacionDbContext db) : ICarteleraRepository
{
    public Task<Cartelera?> GetByIdAsync(CarteleraId id, CancellationToken ct = default) => db.Carteleras.FirstOrDefaultAsync(c => c.Id == id, ct);
    public async Task AddAsync(Cartelera a, CancellationToken ct = default) => await db.Carteleras.AddAsync(a, ct);
    public Task UpdateAsync(Cartelera a, CancellationToken ct = default) { db.Carteleras.Update(a); return Task.CompletedTask; }
    public Task DeleteAsync(Cartelera a, CancellationToken ct = default) { db.Carteleras.Remove(a); return Task.CompletedTask; }
    public Task<Cartelera?> GetVigenteAsync(DateTime ahora, CancellationToken ct = default)
        => db.Carteleras.FirstOrDefaultAsync(c => c.Periodo.Inicio <= ahora && c.Periodo.Fin >= ahora, ct);
}

public sealed class AlquilerRepository(ProgramacionDbContext db) : IAlquilerRepository
{
    public Task<Alquiler?> GetByIdAsync(AlquilerId id, CancellationToken ct = default) => db.Alquileres.FirstOrDefaultAsync(a => a.Id == id, ct);
    public async Task AddAsync(Alquiler a, CancellationToken ct = default) => await db.Alquileres.AddAsync(a, ct);
    public Task UpdateAsync(Alquiler a, CancellationToken ct = default) { db.Alquileres.Update(a); return Task.CompletedTask; }
    public Task DeleteAsync(Alquiler a, CancellationToken ct = default) { db.Alquileres.Remove(a); return Task.CompletedTask; }
}

public sealed class UnitOfWork(ProgramacionDbContext db) : IUnitOfWork
{ public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct); }
