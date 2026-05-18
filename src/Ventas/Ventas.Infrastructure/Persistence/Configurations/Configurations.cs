using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Kernel.ValueObjects;
using Ventas.Domain.Aggregates.DefComboAgg;
using Ventas.Domain.Aggregates.OrdenAgg;
using Ventas.Domain.Aggregates.ProductoAgg;
using Ventas.Domain.ValueObjects;

namespace Ventas.Infrastructure.Persistence.Configurations;

public sealed class OrdenConfig : IEntityTypeConfiguration<Orden>
{
    public void Configure(EntityTypeBuilder<Orden> b)
    {
        b.ToTable("ordenes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(id => id.Value, v => OrdenId.Of(v)).ValueGeneratedNever();
        b.OwnsOne(x => x.EspectadorRef, e => e.Property(p => p.Value).HasColumnName("id_espectador"));
        b.OwnsOne(x => x.Descuento, d =>
        {
            d.Property(p => p.Porcentaje).HasColumnName("descuento_porcentaje");
            d.Property(p => p.NivelOrigen).HasColumnName("descuento_nivel").HasConversion<string>().HasMaxLength(20);
        });
        b.OwnsOne(x => x.Expiracion, e => e.Property(p => p.Valor).HasColumnName("expiracion"));
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.OwnsOne(x => x.Total, m =>
        {
            m.Property(p => p.Amount).HasColumnName("total");
            m.Property(p => p.Currency).HasColumnName("moneda").HasMaxLength(8);
        });

        b.OwnsMany(x => x.Boletas, ib =>
        {
            ib.ToTable("orden_boletas");
            ib.WithOwner().HasForeignKey("OrdenId");
            ib.HasKey(x => x.Id);
            ib.OwnsOne(x => x.FuncionRef, f => f.Property(p => p.Value).HasColumnName("id_funcion"));
            ib.OwnsOne(x => x.SillaRef, s => s.Property(p => p.Value).HasColumnName("id_silla"));
            ib.OwnsOne(x => x.PrecioBase, m =>
            {
                m.Property(p => p.Amount).HasColumnName("precio_base");
                m.Property(p => p.Currency).HasColumnName("moneda").HasMaxLength(8);
            });
            ib.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
        });

        b.OwnsMany(x => x.Confiterias, ic =>
        {
            ic.ToTable("orden_confiterias");
            ic.WithOwner().HasForeignKey("OrdenId");
            ic.HasKey(x => x.Id);
            ic.Property(x => x.ProductoId).HasConversion(id => id.Value, v => ProductoId.Of(v)).HasColumnName("id_producto");
            ic.Property(x => x.Cantidad).HasColumnName("cantidad");
            ic.OwnsOne(x => x.PrecioUnitario, m =>
            {
                m.Property(p => p.Amount).HasColumnName("precio_unitario");
                m.Property(p => p.Currency).HasColumnName("moneda").HasMaxLength(8);
            });
            ic.Ignore(x => x.Combo);
        });

        b.Ignore(x => x.DomainEvents);
    }
}

public sealed class ProductoConfig : IEntityTypeConfiguration<ProductoConfiteria>
{
    public void Configure(EntityTypeBuilder<ProductoConfiteria> b)
    {
        b.ToTable("productos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(id => id.Value, v => ProductoId.Of(v)).ValueGeneratedNever();
        b.Property(x => x.Nombre).HasMaxLength(120);
        b.OwnsOne(x => x.Precio, m =>
        {
            m.Property(p => p.Amount).HasColumnName("precio");
            m.Property(p => p.Currency).HasColumnName("moneda").HasMaxLength(8);
        });
        b.Property(x => x.Stock);
        b.Ignore(x => x.DomainEvents);
    }
}

public sealed class DefComboConfig : IEntityTypeConfiguration<DefCombo>
{
    public void Configure(EntityTypeBuilder<DefCombo> b)
    {
        b.ToTable("def_combos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(id => id.Value, v => DefComboId.Of(v)).ValueGeneratedNever();
        b.Property(x => x.Nombre).HasMaxLength(120);
        b.OwnsOne(x => x.PrecioEspecial, m =>
        {
            m.Property(p => p.Amount).HasColumnName("precio_especial");
            m.Property(p => p.Currency).HasColumnName("moneda").HasMaxLength(8);
        });
        b.Property(x => x.Activo);
        b.Ignore(x => x.Items);
        b.Ignore(x => x.DomainEvents);
    }
}
