using Clientes.Domain.Aggregates.EspectadorAgg;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Infrastructure.Persistence;

public sealed class ClientesDbContext(DbContextOptions<ClientesDbContext> options) : DbContext(options)
{
    public DbSet<Espectador> Espectadores => Set<Espectador>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.HasDefaultSchema("clientes");
        mb.ApplyConfigurationsFromAssembly(typeof(ClientesDbContext).Assembly);

        mb.AddInboxStateEntity();
        mb.AddOutboxMessageEntity();
        mb.AddOutboxStateEntity();
    }
}
