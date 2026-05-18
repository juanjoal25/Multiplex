using Cadena.Domain.Aggregates.SucursalAgg;
using Cadena.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cadena.Infrastructure.Persistence.Configurations;

public sealed class SucursalConfig : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> b)
    {
        b.ToTable("sucursales");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(id => id.Value, v => SucursalId.Of(v)).ValueGeneratedNever();
        b.OwnsOne(x => x.Nombre, n => n.Property(p => p.Valor).HasColumnName("nombre").HasMaxLength(100));

        b.OwnsOne(x => x.Configuracion, c =>
        {
            c.ToTable("configuraciones");
            c.WithOwner().HasForeignKey("SucursalId");
            c.Property<Guid>("Id").ValueGeneratedNever();
            c.HasKey("Id");
            c.Property(p => p.ZonaHoraria).HasColumnName("zona_horaria").HasMaxLength(60);
            c.Property(p => p.Moneda).HasColumnName("moneda").HasMaxLength(8);
            c.OwnsMany(p => p.Parametros, pp =>
            {
                pp.ToTable("config_parametros");
                pp.WithOwner().HasForeignKey("ConfiguracionId");
                pp.Property<int>("Id").ValueGeneratedOnAdd();
                pp.HasKey("Id");
                pp.Property(z => z.Clave).HasMaxLength(80);
                pp.Property(z => z.Valor).HasMaxLength(400);
                pp.Property(z => z.Tipo).HasConversion<string>().HasMaxLength(20);
            });
        });

        b.OwnsMany(x => x.Contratos, c =>
        {
            c.ToTable("contratos");
            c.WithOwner().HasForeignKey("SucursalId");
            c.HasKey(z => z.Id);
            c.Property(z => z.Id).HasConversion(id => id.Value, v => ContratoId.Of(v)).ValueGeneratedNever();
            c.Property(z => z.Tercero).HasMaxLength(200);
            c.OwnsOne(z => z.Vigencia, v =>
            {
                v.Property(p => p.FechaInicio).HasColumnName("vigencia_inicio");
                v.Property(p => p.FechaFin).HasColumnName("vigencia_fin");
            });
            c.Property(z => z.Condiciones).HasMaxLength(2000);
            c.Property(z => z.Estado).HasConversion<string>().HasMaxLength(20);
        });

        b.Ignore(x => x.DomainEvents);
    }
}
