using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Programacion.Domain.Aggregates.AlquilerAgg;
using Programacion.Domain.Aggregates.CarteleraAgg;
using Programacion.Domain.Aggregates.FuncionAgg;
using Programacion.Domain.Aggregates.PeliculaAgg;
using Programacion.Domain.States;
using Programacion.Domain.Strategies;
using Programacion.Domain.ValueObjects;

namespace Programacion.Infrastructure.Persistence.Configurations;

public sealed class PeliculaConfig : IEntityTypeConfiguration<Pelicula>
{
    public void Configure(EntityTypeBuilder<Pelicula> b)
    {
        b.ToTable("peliculas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(id => id.Value, v => PeliculaId.Of(v)).ValueGeneratedNever();
        b.OwnsOne(x => x.Titulo, t => t.Property(p => p.Valor).HasColumnName("titulo").HasMaxLength(200).IsRequired());
        b.OwnsOne(x => x.Genero, g => g.Property(p => p.Valor).HasColumnName("genero").HasMaxLength(60).IsRequired());
        b.OwnsOne(x => x.Duracion, d => d.Property(p => p.Minutos).HasColumnName("duracion_minutos"));
        b.Property(x => x.Clasificacion).HasConversion<string>().HasMaxLength(8);
        b.Property(x => x.FormatoDisponible).HasConversion<string>().HasMaxLength(20);
        b.Ignore(x => x.DomainEvents);
    }
}

public sealed class FuncionConfig : IEntityTypeConfiguration<Funcion>
{
    public void Configure(EntityTypeBuilder<Funcion> b)
    {
        b.ToTable("funciones");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(id => id.Value, v => FuncionId.Of(v)).ValueGeneratedNever();
        b.OwnsOne(x => x.PeliculaRef, p => p.Property(r => r.IdPelicula).HasColumnName("id_pelicula"));
        b.OwnsOne(x => x.SalaRef, s =>
        {
            s.Property(r => r.IdSala).HasColumnName("id_sala");
            s.Property(r => r.Tipo).HasColumnName("tipo_sala").HasConversion<string?>().HasMaxLength(20);
        });
        b.OwnsOne(x => x.Horario, h =>
        {
            h.Property(p => p.Inicio).HasColumnName("inicio");
            h.Property(p => p.Fin).HasColumnName("fin");
        });
        b.Ignore(x => x.Formato);
        b.Property<TipoFormato>("FormatoTipo").HasColumnName("formato").HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Ignore(x => x.Estado);
        b.Property<EstadoFuncionTipo>("EstadoTipo").HasColumnName("estado").HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Ignore(x => x.DomainEvents);
    }
}

public sealed class CarteleraConfig : IEntityTypeConfiguration<Cartelera>
{
    public void Configure(EntityTypeBuilder<Cartelera> b)
    {
        b.ToTable("carteleras");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(id => id.Value, v => CarteleraId.Of(v)).ValueGeneratedNever();
        b.OwnsOne(x => x.Periodo, p =>
        {
            p.Property(z => z.Inicio).HasColumnName("periodo_inicio");
            p.Property(z => z.Fin).HasColumnName("periodo_fin");
        });
        b.Property<List<Guid>>("FuncionesIds").HasColumnName("funciones").HasColumnType("uuid[]");
        b.Ignore(x => x.Funciones);
        b.Ignore(x => x.DomainEvents);
    }
}

public sealed class AlquilerConfig : IEntityTypeConfiguration<Alquiler>
{
    public void Configure(EntityTypeBuilder<Alquiler> b)
    {
        b.ToTable("alquileres");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(id => id.Value, v => AlquilerId.Of(v)).ValueGeneratedNever();
        b.OwnsOne(x => x.SalaRef, s =>
        {
            s.Property(r => r.IdSala).HasColumnName("id_sala");
            s.Property(r => r.Tipo).HasColumnName("tipo_sala").HasConversion<string?>().HasMaxLength(20);
        });
        b.OwnsOne(x => x.Rango, h =>
        {
            h.Property(p => p.Inicio).HasColumnName("inicio");
            h.Property(p => p.Fin).HasColumnName("fin");
        });
        b.Property(x => x.Solicitante).HasMaxLength(200);
        b.Property(x => x.Proposito).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Ignore(x => x.DomainEvents);
    }
}
