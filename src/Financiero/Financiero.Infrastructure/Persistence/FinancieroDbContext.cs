using Financiero.Domain.Aggregates.TransaccionAgg;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Financiero.Infrastructure.Persistence;

public sealed class FinancieroDbContext(DbContextOptions<FinancieroDbContext> options) : DbContext(options)
{
    public DbSet<Transaccion> Transacciones => Set<Transaccion>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.HasDefaultSchema("financiero");
        mb.ApplyConfigurationsFromAssembly(typeof(FinancieroDbContext).Assembly);
        mb.AddInboxStateEntity();
        mb.AddOutboxMessageEntity();
        mb.AddOutboxStateEntity();
    }
}
