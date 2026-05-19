using Shared.Kernel;
using Shared.Kernel.Exceptions;
using Shared.Kernel.ValueObjects;
using Ventas.Domain.ValueObjects;

namespace Ventas.Domain.Aggregates.OrdenAgg;

public sealed class Combo : ValueObject
{
    public DefComboId IdDefCombo { get; }
    public Money PrecioEspecial { get; }
    public IReadOnlyCollection<Guid> Consumibles { get; }

    private Combo(DefComboId id, Money precio, IReadOnlyCollection<Guid> consumibles)
    {
        IdDefCombo = id; PrecioEspecial = precio; Consumibles = consumibles;
    }

    public static Combo Of(DefComboId id, Money precio, IEnumerable<Guid> consumibles)
    {
        var list = consumibles.ToList();
        if (list.Count == 0) throw new InvariantViolationException("Combo requiere al menos un consumible");
        if (precio.Amount <= 0) throw new InvariantViolationException("Combo.PrecioEspecial > 0");
        return new(id, precio, list);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return IdDefCombo;
        yield return PrecioEspecial;
        foreach (var c in Consumibles) yield return c;
    }
}

public sealed class ItemConfiteria : Entity<Guid>
{
    public ProductoId ProductoId { get; private set; }
    public int Cantidad { get; private set; }
    public Money PrecioUnitario { get; private set; }
    public Combo? Combo { get; private set; }

    private ItemConfiteria() { ProductoId = null!; PrecioUnitario = null!; }
    private ItemConfiteria(Guid id, ProductoId producto, int cantidad, Money precio, Combo? combo) : base(id)
    {
        ProductoId = producto; Cantidad = cantidad; PrecioUnitario = precio; Combo = combo;
    }

    public static ItemConfiteria Crear(ProductoId producto, int cantidad, Money precio, Combo? combo = null)
    {
        if (cantidad <= 0) throw new InvariantViolationException("Cantidad debe ser > 0");
        if (precio.Amount < 0) throw new InvariantViolationException("PrecioUnitario no negativo");
        return new(Guid.NewGuid(), producto, cantidad, precio, combo);
    }

    public static ItemConfiteria Restore(Guid id, ProductoId p, int q, Money precio, Combo? combo) => new(id, p, q, precio, combo);

    public Money Subtotal() => Combo is not null ? Combo.PrecioEspecial.Multiply(Cantidad) : PrecioUnitario.Multiply(Cantidad);
}
