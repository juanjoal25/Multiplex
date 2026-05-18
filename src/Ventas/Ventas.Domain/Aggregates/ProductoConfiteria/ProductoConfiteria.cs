using Shared.Kernel;
using Shared.Kernel.Exceptions;
using Shared.Kernel.ValueObjects;
using Ventas.Domain.Events;
using Ventas.Domain.ValueObjects;

namespace Ventas.Domain.Aggregates.ProductoAgg;

public sealed class ProductoConfiteria : AggregateRoot<ProductoId>
{
    public string Nombre { get; private set; }
    public Money Precio { get; private set; }
    public int Stock { get; private set; }

    private ProductoConfiteria(ProductoId id, string nombre, Money precio, int stock) : base(id)
    {
        Nombre = nombre; Precio = precio; Stock = stock;
    }

    public static ProductoConfiteria Crear(string nombre, Money precio, int stockInicial)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new InvariantViolationException("Nombre requerido");
        if (stockInicial < 0) throw new InvariantViolationException("Stock inicial >= 0");
        return new(ProductoId.New(), nombre.Trim(), precio, stockInicial);
    }

    public static ProductoConfiteria Restore(ProductoId id, string n, Money p, int s) => new(id, n, p, s);

    public void ReducirStock(int cantidad)
    {
        if (cantidad <= 0) throw new PreconditionFailedException("Cantidad > 0");
        if (Stock < cantidad) throw new PreconditionFailedException("Stock insuficiente");
        Stock -= cantidad;
        Raise(new StockReducido(Id, cantidad, Stock));
        if (Stock == 0) Raise(new StockAgotado(Id, Nombre));
    }

    public void Reabastecer(int cantidad)
    {
        if (cantidad <= 0) throw new PreconditionFailedException("Cantidad > 0");
        Stock += cantidad;
    }
}
