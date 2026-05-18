using Infraestructura.Domain.Aggregates.SalaAgg;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Infrastructure.Persistence;

public sealed class InfraestructuraDbContext(DbContextOptions<InfraestructuraDbContext> options) : DbContext(options)
{
    public DbSet<Sala> Salas => Set<Sala>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.HasDefaultSchema("infra");
        mb.ApplyConfigurationsFromAssembly(typeof(InfraestructuraDbContext).Assembly);
        mb.AddInboxStateEntity();
        mb.AddOutboxMessageEntity();
        mb.AddOutboxStateEntity();
    }
}
