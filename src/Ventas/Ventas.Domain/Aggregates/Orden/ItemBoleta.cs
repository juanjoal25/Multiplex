using Shared.Kernel;
using Shared.Kernel.Exceptions;
using Shared.Kernel.ValueObjects;
using Ventas.Domain.ValueObjects;

namespace Ventas.Domain.Aggregates.OrdenAgg;

public enum TipoBoleta { General, Vip, Especial }

public sealed class ItemBoleta : Entity<Guid>
{
    public FuncionRef FuncionRef { get; private set; }
    public SillaRef SillaRef { get; private set; }
    public Money PrecioBase { get; private set; }
    public TipoBoleta Tipo { get; private set; }

    private ItemBoleta(Guid id, FuncionRef fref, SillaRef sref, Money precio, TipoBoleta tipo) : base(id)
    {
        FuncionRef = fref; SillaRef = sref; PrecioBase = precio; Tipo = tipo;
    }

    public static ItemBoleta Crear(FuncionRef f, SillaRef s, Money precio, TipoBoleta tipo)
    {
        if (precio.Amount <= 0m) throw new InvariantViolationException("PrecioBase debe ser > 0");
        return new(Guid.NewGuid(), f, s, precio, tipo);
    }

    public static ItemBoleta Restore(Guid id, FuncionRef f, SillaRef s, Money precio, TipoBoleta tipo)
        => new(id, f, s, precio, tipo);
}
