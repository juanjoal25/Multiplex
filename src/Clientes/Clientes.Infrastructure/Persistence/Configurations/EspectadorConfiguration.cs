using Clientes.Domain.Aggregates.EspectadorAgg;
using Clientes.Domain.States;
using Clientes.Domain.Strategies;
using Clientes.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clientes.Infrastructure.Persistence.Configurations;

public sealed class EspectadorConfiguration : IEntityTypeConfiguration<Espectador>
{
    public void Configure(EntityTypeBuilder<Espectador> b)
    {
        b.ToTable("espectadores");

        b.HasKey(e => e.Id);
        b.Property(e => e.Id)
            .HasConversion(id => id.Value, v => EspectadorId.Of(v))
            .ValueGeneratedNever();

        b.OwnsOne(e => e.Nombre, n =>
        {
            n.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(120).IsRequired();
            n.Property(p => p.Apellido).HasColumnName("apellido").HasMaxLength(120).IsRequired();
        });

        b.OwnsOne(e => e.Correo, c =>
        {
            c.Property(p => p.Value).HasColumnName("correo").HasMaxLength(254).IsRequired();
            c.HasIndex(p => p.Value).IsUnique();
        });

        b.OwnsOne(e => e.Documento, d =>
        {
            d.Property(p => p.Tipo).HasColumnName("documento_tipo").HasConversion<string>().HasMaxLength(8).IsRequired();
            d.Property(p => p.Numero).HasColumnName("documento_numero").HasMaxLength(40).IsRequired();
            d.HasIndex(p => new { p.Tipo, p.Numero }).IsUnique();
        });

        b.OwnsOne(e => e.Suscripcion, s =>
        {
            s.ToTable("suscripciones");
            s.Property<Guid>("Id").ValueGeneratedNever();
            s.HasKey("Id");
            s.WithOwner().HasForeignKey("EspectadorId");

            s.Property<EstadoSuscripcionTipo>("EstadoTipo")
                .HasColumnName("estado").HasConversion<string>().HasMaxLength(20).IsRequired();
            s.Property<TipoNivel>("NivelTipo")
                .HasColumnName("nivel").HasConversion<string>().HasMaxLength(20).IsRequired();

            s.Ignore(x => x.Estado);
            s.Ignore(x => x.Nivel);

            s.OwnsOne(x => x.Vigencia, v =>
            {
                v.Property(p => p.FechaInicio).HasColumnName("vigencia_inicio");
                v.Property(p => p.FechaFin).HasColumnName("vigencia_fin");
            });
        });

        b.Ignore(e => e.DomainEvents);
    }
}
