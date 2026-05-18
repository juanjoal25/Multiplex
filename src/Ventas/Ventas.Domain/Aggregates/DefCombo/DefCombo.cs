using Shared.Kernel;
using Shared.Kernel.Exceptions;
using Shared.Kernel.ValueObjects;
using Ventas.Domain.Events;
using Ventas.Domain.ValueObjects;

namespace Ventas.Domain.Aggregates.DefComboAgg;

public sealed class ComboItem
{
    public ProductoId ProductoId { get; }
    public int Cantidad { get; }
    public Money PrecioUnitario { get; }

    public ComboItem(ProductoId p, int q, Money precio)
    {
        if (q <= 0) throw new InvariantViolationException("Cantidad > 0");
        ProductoId = p; Cantidad = q; PrecioUnitario = precio;
    }

    public Money Subtotal() => PrecioUnitario.Multiply(Cantidad);
}

public sealed class DefCombo : AggregateRoot<DefComboId>
{
    private readonly List<ComboItem> _items = new();
    public string Nombre { get; private set; }
    public Money PrecioEspecial { get; private set; }
    public bool Activo { get; private set; }
    public IReadOnlyCollection<ComboItem> Items => _items.AsReadOnly();

    private DefCombo(DefComboId id, string nombre, Money precio, bool activo) : base(id)
    { Nombre = nombre; PrecioEspecial = precio; Activo = activo; }

    public static DefCombo Crear(string nombre, Money precioEspecial, IEnumerable<ComboItem> items)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new InvariantViolationException("Nombre requerido");
        var lista = items.ToList();
        if (lista.Count == 0) throw new InvariantViolationException("Combo requiere al menos un item");
        var suma = lista.Aggregate(Money.Zero(), (acc, x) => acc.Add(x.Subtotal()));
        if (precioEspecial.Amount >= suma.Amount)
            throw new InvariantViolationException("PrecioEspecial debe ser menor a la suma individual");
        var c = new DefCombo(DefComboId.New(), nombre.Trim(), precioEspecial, false);
        c._items.AddRange(lista);
        return c;
    }

    public static DefCombo Restore(DefComboId id, string n, Money p, bool activo, IEnumerable<ComboItem> items)
    {
        var d = new DefCombo(id, n, p, activo);
        d._items.AddRange(items);
        return d;
    }

    public void Activar()
    {
        if (Activo) throw new PreconditionFailedException("Combo ya activo");
        Activo = true;
    }

    public void Desactivar()
    {
        if (!Activo) throw new PreconditionFailedException("Combo ya inactivo");
        Activo = false;
        Raise(new ComboDesactivado(Id, Nombre));
    }
}
