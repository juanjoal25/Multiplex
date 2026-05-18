using MassTransit;
using Microsoft.EntityFrameworkCore;
using Programacion.Domain.Aggregates.AlquilerAgg;
using Programacion.Domain.Aggregates.CarteleraAgg;
using Programacion.Domain.Aggregates.FuncionAgg;
using Programacion.Domain.Aggregates.PeliculaAgg;

namespace Programacion.Infrastructure.Persistence;

public sealed class ProgramacionDbContext(DbContextOptions<ProgramacionDbContext> options) : DbContext(options)
{
    public DbSet<Pelicula> Peliculas => Set<Pelicula>();
    public DbSet<Funcion> Funciones => Set<Funcion>();
    public DbSet<Cartelera> Carteleras => Set<Cartelera>();
    public DbSet<Alquiler> Alquileres => Set<Alquiler>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.HasDefaultSchema("programacion");
        mb.ApplyConfigurationsFromAssembly(typeof(ProgramacionDbContext).Assembly);
        mb.AddInboxStateEntity();
        mb.AddOutboxMessageEntity();
        mb.AddOutboxStateEntity();
    }
}
