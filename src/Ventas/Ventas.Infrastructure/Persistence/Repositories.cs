using Microsoft.EntityFrameworkCore;
using Ventas.Application.Abstractions;
using Ventas.Domain.Aggregates.DefComboAgg;
using Ventas.Domain.Aggregates.OrdenAgg;
using Ventas.Domain.Aggregates.ProductoAgg;
using Ventas.Domain.Repositories;
using Ventas.Domain.ValueObjects;

namespace Ventas.Infrastructure.Persistence;

public sealed class OrdenRepository(VentasDbContext db) : IOrdenRepository
{
    public Task<Orden?> GetByIdAsync(OrdenId id, CancellationToken ct = default)
        => db.Ordenes.Include(o => o.Boletas).Include(o => o.Confiterias).FirstOrDefaultAsync(o => o.Id == id, ct);
    public async Task AddAsync(Orden a, CancellationToken ct = default) => await db.Ordenes.AddAsync(a, ct);
    public Task UpdateAsync(Orden a, CancellationToken ct = default) { db.Ordenes.Update(a); return Task.CompletedTask; }
    public Task DeleteAsync(Orden a, CancellationToken ct = default) { db.Ordenes.Remove(a); return Task.CompletedTask; }
    public async Task<IReadOnlyCollection<Orden>> GetExpiradasAsync(DateTime ahora, CancellationToken ct = default)
        => await db.Ordenes.Where(o => o.Estado == EstadoOrden.Pendiente && o.Expiracion.Valor < ahora).ToListAsync(ct);
    public async Task<IReadOnlyCollection<Orden>> GetPendientesConFuncionAsync(Guid idFuncion, CancellationToken ct = default)
        => await db.Ordenes.Include(o => o.Boletas)
            .Where(o => o.Estado == EstadoOrden.Pendiente && o.Boletas.Any(b => b.FuncionRef.Value == idFuncion))
            .ToListAsync(ct);
}

public sealed class ProductoRepository(VentasDbContext db) : IProductoRepository
{
    public Task<ProductoConfiteria?> GetByIdAsync(ProductoId id, CancellationToken ct = default) => db.Productos.FirstOrDefaultAsync(p => p.Id == id, ct);
    public async Task AddAsync(ProductoConfiteria a, CancellationToken ct = default) => await db.Productos.AddAsync(a, ct);
    public Task UpdateAsync(ProductoConfiteria a, CancellationToken ct = default) { db.Productos.Update(a); return Task.CompletedTask; }
    public Task DeleteAsync(ProductoConfiteria a, CancellationToken ct = default) { db.Productos.Remove(a); return Task.CompletedTask; }
}

public sealed class DefComboRepository(VentasDbContext db) : IDefComboRepository
{
    public Task<DefCombo?> GetByIdAsync(DefComboId id, CancellationToken ct = default) => db.DefCombos.FirstOrDefaultAsync(c => c.Id == id, ct);
    public async Task AddAsync(DefCombo a, CancellationToken ct = default) => await db.DefCombos.AddAsync(a, ct);
    public Task UpdateAsync(DefCombo a, CancellationToken ct = default) { db.DefCombos.Update(a); return Task.CompletedTask; }
    public Task DeleteAsync(DefCombo a, CancellationToken ct = default) { db.DefCombos.Remove(a); return Task.CompletedTask; }
}

public sealed class UnitOfWork(VentasDbContext db) : IUnitOfWork
{ public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct); }
