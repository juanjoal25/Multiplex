using Shared.Kernel;
using Shared.Kernel.Exceptions;
using Shared.Kernel.ValueObjects;
using Ventas.Domain.Events;
using Ventas.Domain.ValueObjects;

namespace Ventas.Domain.Aggregates.OrdenAgg;

public sealed class Orden : AggregateRoot<OrdenId>
{
    private readonly List<ItemBoleta> _boletas = new();
    private readonly List<ItemConfiteria> _confiterias = new();

    public EspectadorRef EspectadorRef { get; private set; }
    public Descuento Descuento { get; private set; }
    public EstadoOrden Estado { get; private set; }
    public Expiracion Expiracion { get; private set; }
    public Money Total { get; private set; }
    public IReadOnlyCollection<ItemBoleta> Boletas => _boletas.AsReadOnly();
    public IReadOnlyCollection<ItemConfiteria> Confiterias => _confiterias.AsReadOnly();

    private Orden(OrdenId id, EspectadorRef esp, Descuento d, Expiracion exp, EstadoOrden est, Money total) : base(id)
    {
        EspectadorRef = esp; Descuento = d; Expiracion = exp; Estado = est; Total = total;
    }

    public static Orden Crear(
        EspectadorRef esp,
        IEnumerable<ItemBoleta> boletas,
        IEnumerable<ItemConfiteria> confiterias,
        Descuento descuento,
        Expiracion expiracion)
    {
        var lb = boletas.ToList();
        var lc = confiterias.ToList();
        if (lb.Count == 0 && lc.Count == 0)
            throw new InvariantViolationException("Orden requiere al menos un item");

        // Invariante: no dos ItemBoleta con misma FuncionRef + SillaRef
        var dup = lb.GroupBy(b => (b.FuncionRef, b.SillaRef)).Any(g => g.Count() > 1);
        if (dup) throw new InvariantViolationException("ItemBoleta duplicado (misma función + silla)");

        var subtotalBoletas = lb.Aggregate(Money.Zero(), (acc, b) => acc.Add(b.PrecioBase));
        var subtotalConf = lc.Aggregate(Money.Zero(), (acc, c) => acc.Add(c.Subtotal()));
        var subtotal = subtotalBoletas.Add(subtotalConf);
        var total = subtotal.Multiply(1m - descuento.Porcentaje);

        var orden = new Orden(OrdenId.New(), esp, descuento, expiracion, EstadoOrden.Pendiente, total);
        orden._boletas.AddRange(lb);
        orden._confiterias.AddRange(lc);

        var sillaIds = lb.Select(b => b.SillaRef.Value).ToList();
        orden.Raise(new OrdenCreada(orden.Id, esp, sillaIds, total, expiracion.Valor));
        return orden;
    }

    public static Orden Restore(OrdenId id, EspectadorRef esp, Descuento d, Expiracion exp, EstadoOrden est, Money total,
        IEnumerable<ItemBoleta> boletas, IEnumerable<ItemConfiteria> confiterias)
    {
        var o = new Orden(id, esp, d, exp, est, total);
        o._boletas.AddRange(boletas);
        o._confiterias.AddRange(confiterias);
        return o;
    }

    public void Confirmar(DateTime ahora)
    {
        if (Estado != EstadoOrden.Pendiente)
            throw new PreconditionFailedException($"Solo orden PENDIENTE puede confirmarse (actual: {Estado})");
        if (Expiracion.HaExpirado(ahora))
            throw new PreconditionFailedException("Orden ha expirado");
        Estado = EstadoOrden.Confirmada;
        Raise(new OrdenConfirmada(Id, Total));
    }

    public void Cancelar(string motivo)
    {
        if (Estado != EstadoOrden.Pendiente)
            throw new PreconditionFailedException("Solo orden PENDIENTE puede cancelarse");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new PreconditionFailedException("Motivo requerido");
        Estado = EstadoOrden.Cancelada;
        var sillas = _boletas.Select(b => b.SillaRef.Value).ToList();
        Raise(new OrdenCancelada(Id, EspectadorRef, sillas, motivo));
    }

    public void Expirar()
    {
        if (Estado != EstadoOrden.Pendiente)
            throw new PreconditionFailedException("Solo orden PENDIENTE puede expirar");
        Estado = EstadoOrden.Expirada;
        var sillas = _boletas.Select(b => b.SillaRef.Value).ToList();
        Raise(new OrdenExpirada(Id, EspectadorRef, sillas));
    }
}
