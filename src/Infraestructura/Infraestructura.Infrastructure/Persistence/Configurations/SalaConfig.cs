using Infraestructura.Domain.Aggregates.SalaAgg;
using Infraestructura.Domain.States;
using Infraestructura.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Infrastructure.Persistence.Configurations;

public sealed class SalaConfig : IEntityTypeConfiguration<Sala>
{
    public void Configure(EntityTypeBuilder<Sala> b)
    {
        b.ToTable("salas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(id => id.Value, v => SalaId.Of(v)).ValueGeneratedNever();
        b.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        b.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
        b.OwnsOne(x => x.Aforo, a => a.Property(p => p.Valor).HasColumnName("aforo"));
        b.Ignore(x => x.Estado);
        b.Property<EstadoSalaTipo>("_estadoTipo").HasField("_estadoTipo").HasColumnName("estado").HasConversion<string>().HasMaxLength(20).IsRequired();

        b.OwnsMany(x => x.Sillas, s =>
        {
            s.ToTable("sillas");
            s.WithOwner().HasForeignKey("SalaId");
            s.HasKey(x => x.Id);
            s.Property(x => x.Id).HasConversion(id => id.Value, v => SillaId.Of(v)).ValueGeneratedNever();
            s.OwnsOne(x => x.Posicion, p =>
            {
                p.Property(z => z.Fila).HasColumnName("fila").HasMaxLength(8);
                p.Property(z => z.Columna).HasColumnName("columna");
            });
            s.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
            s.Ignore(x => x.Estado);
            s.Property<EstadoSillaTipo>("_estadoTipo").HasField("_estadoTipo").HasColumnName("estado").HasConversion<string>().HasMaxLength(20).IsRequired();
            s.OwnsOne(x => x.ReservaExpiracion, r => r.Property(z => z.Valor).HasColumnName("reserva_expiracion"));
            s.Property(x => x.IdFuncionReservada).HasColumnName("id_funcion_reservada");
            s.Property(x => x.IdOrdenReservada).HasColumnName("id_orden_reservada");
        });

        b.Ignore(x => x.DomainEvents);
    }
}
