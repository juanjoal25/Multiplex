using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Aggregates.DefComboAgg;
using Ventas.Domain.Aggregates.OrdenAgg;
using Ventas.Domain.Aggregates.ProductoAgg;

namespace Ventas.Infrastructure.Persistence;

public sealed class VentasDbContext(DbContextOptions<VentasDbContext> options) : DbContext(options)
{
    public DbSet<Orden> Ordenes => Set<Orden>();
    public DbSet<ProductoConfiteria> Productos => Set<ProductoConfiteria>();
    public DbSet<DefCombo> DefCombos => Set<DefCombo>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.HasDefaultSchema("ventas");
        mb.ApplyConfigurationsFromAssembly(typeof(VentasDbContext).Assembly);
        mb.AddInboxStateEntity();
        mb.AddOutboxMessageEntity();
        mb.AddOutboxStateEntity();
    }
}
