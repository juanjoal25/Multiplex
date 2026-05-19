using Shared.Kernel;
using Shared.Kernel.Exceptions;
using Shared.Kernel.ValueObjects;

namespace Financiero.Domain.ValueObjects;

public sealed class TransaccionId : ValueObject
{
    public Guid Value { get; }
    private TransaccionId(Guid v) => Value = v;
    public static TransaccionId New() => new(Guid.NewGuid());
    public static TransaccionId Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("TransaccionId vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}

public enum EstadoPago { Pendiente, Aprobado, Rechazado }
public enum MetodoPago { TarjetaCredito, TarjetaDebito, EfectivoTaquilla, BilleteraDigital }

public sealed class ReferenciaExterna : ValueObject
{
    public string Valor { get; }
    private ReferenciaExterna() { Valor = null!; }
    private ReferenciaExterna(string v) => Valor = v;
    public static ReferenciaExterna Of(string v)
    {
        if (string.IsNullOrWhiteSpace(v)) throw new InvariantViolationException("ReferenciaExterna requerida");
        return new(v.Trim());
    }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Valor; }
}

public sealed class ConceptoFacturable : ValueObject
{
    public string Descripcion { get; }
    public Money Valor { get; }

    private ConceptoFacturable(string d, Money v) { Descripcion = d; Valor = v; }

    public static ConceptoFacturable Of(string descripcion, Money valor)
    {
        if (string.IsNullOrWhiteSpace(descripcion)) throw new InvariantViolationException("Descripcion requerida");
        if (valor.Amount < 0) throw new InvariantViolationException("Valor de concepto no negativo");
        return new(descripcion.Trim(), valor);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Descripcion; yield return Valor; }
}

public sealed class OrdenDepurada : ValueObject
{
    public Guid IdOrden { get; }
    public IReadOnlyCollection<ConceptoFacturable> Conceptos { get; }
    public IReadOnlyCollection<decimal> DescuentosAplicados { get; }
    public Money ValorTotal { get; }

    private OrdenDepurada() { Conceptos = Array.Empty<ConceptoFacturable>(); DescuentosAplicados = Array.Empty<decimal>(); ValorTotal = null!; }
    private OrdenDepurada(Guid id, IReadOnlyCollection<ConceptoFacturable> c, IReadOnlyCollection<decimal> d, Money total)
    { IdOrden = id; Conceptos = c; DescuentosAplicados = d; ValorTotal = total; }

    public static OrdenDepurada Of(Guid idOrden, IEnumerable<ConceptoFacturable> conceptos, IEnumerable<decimal> descuentos, Money total)
    {
        if (idOrden == Guid.Empty) throw new InvariantViolationException("IdOrden vacío");
        var lc = conceptos.ToList();
        if (lc.Count == 0) throw new InvariantViolationException("ConceptosFacturables no puede estar vacío");
        if (total.Amount <= 0) throw new InvariantViolationException("ValorTotal debe ser > 0");
        return new(idOrden, lc, descuentos.ToList(), total);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return IdOrden;
        yield return ValorTotal;
        foreach (var c in Conceptos) yield return c;
    }
}

public sealed class RegistroContable : ValueObject
{
    public Guid IdTransaccion { get; }
    public DateTime Fecha { get; }
    public Money ValorTotal { get; }
    public EstadoPago EstadoPago { get; }
    public ReferenciaExterna? Referencia { get; }

    private RegistroContable(Guid idT, DateTime fecha, Money valor, EstadoPago estado, ReferenciaExterna? r)
    { IdTransaccion = idT; Fecha = fecha; ValorTotal = valor; EstadoPago = estado; Referencia = r; }

    public static RegistroContable Of(TransaccionId id, Money total, EstadoPago estado, ReferenciaExterna? r = null)
        => new(id.Value, DateTime.UtcNow, total, estado, r);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return IdTransaccion; yield return Fecha; yield return ValorTotal; yield return EstadoPago;
    }
}
