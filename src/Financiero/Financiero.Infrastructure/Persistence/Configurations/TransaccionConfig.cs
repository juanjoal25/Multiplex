using Financiero.Domain.Aggregates.TransaccionAgg;
using Financiero.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financiero.Infrastructure.Persistence.Configurations;

public sealed class TransaccionConfig : IEntityTypeConfiguration<Transaccion>
{
    public void Configure(EntityTypeBuilder<Transaccion> b)
    {
        b.ToTable("transacciones");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(id => id.Value, v => TransaccionId.Of(v)).ValueGeneratedNever();

        b.OwnsOne(x => x.Orden, o =>
        {
            o.Property(p => p.IdOrden).HasColumnName("id_orden");
            o.HasIndex(p => p.IdOrden).IsUnique(); // idempotencia
            o.OwnsOne(p => p.ValorTotal, m =>
            {
                m.Property(z => z.Amount).HasColumnName("valor_total");
                m.Property(z => z.Currency).HasColumnName("moneda").HasMaxLength(8);
            });
            o.Ignore(p => p.Conceptos);
            o.Ignore(p => p.DescuentosAplicados);
        });

        b.Property(x => x.MetodoPago).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.EstadoPago).HasConversion<string>().HasMaxLength(20);
        b.OwnsOne(x => x.Referencia, r => r.Property(p => p.Valor).HasColumnName("referencia_externa"));
        b.Property(x => x.Timestamp);
        b.Ignore(x => x.Registro);
        b.Ignore(x => x.Reversion);
        b.Ignore(x => x.DomainEvents);
    }
}
