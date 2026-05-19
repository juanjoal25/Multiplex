using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Ventas.Domain.ValueObjects;

public enum NivelOrigen { Normal, Oro, Platino, Corporativo, SinSuscripcion }

public sealed class Descuento : ValueObject
{
    public decimal Porcentaje { get; }
    public NivelOrigen NivelOrigen { get; }

    private Descuento() { }
    private Descuento(decimal porc, NivelOrigen nivel) { Porcentaje = porc; NivelOrigen = nivel; }

    public static Descuento Of(decimal porcentaje, NivelOrigen nivel)
    {
        if (porcentaje < 0m || porcentaje > 1m)
            throw new InvariantViolationException("Descuento entre 0.0 y 1.0");
        return new(decimal.Round(porcentaje, 4), nivel);
    }

    public static Descuento Cero(NivelOrigen origen = NivelOrigen.SinSuscripcion) => new(0m, origen);

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Porcentaje; yield return NivelOrigen; }
}

public sealed class Expiracion : ValueObject
{
    public DateTime Valor { get; }
    private Expiracion() { }
    private Expiracion(DateTime v) => Valor = v;
    public static Expiracion Of(DateTime v, DateTime ahora)
    {
        if (v <= ahora) throw new InvariantViolationException("Expiracion debe ser futura");
        return new(v);
    }
    public bool HaExpirado(DateTime ahora) => ahora >= Valor;
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Valor; }
}

public enum EstadoOrden { Pendiente, Confirmada, Expirada, Cancelada }
