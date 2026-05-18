using Cadena.Domain.Aggregates.SucursalAgg;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Cadena.Infrastructure.Persistence;

public sealed class CadenaDbContext(DbContextOptions<CadenaDbContext> options) : DbContext(options)
{
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.HasDefaultSchema("cadena");
        mb.ApplyConfigurationsFromAssembly(typeof(CadenaDbContext).Assembly);
        mb.AddInboxStateEntity();
        mb.AddOutboxMessageEntity();
        mb.AddOutboxStateEntity();
    }
}
