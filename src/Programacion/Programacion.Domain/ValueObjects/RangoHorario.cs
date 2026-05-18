using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Programacion.Domain.ValueObjects;

public sealed class RangoHorario : ValueObject
{
    public DateTime Inicio { get; }
    public DateTime Fin { get; }

    private RangoHorario(DateTime inicio, DateTime fin) { Inicio = inicio; Fin = fin; }

    public static RangoHorario Of(DateTime inicio, DateTime fin)
    {
        if (fin <= inicio)
            throw new InvariantViolationException("RangoHorario: fin debe ser mayor a inicio");
        return new RangoHorario(inicio, fin);
    }

    public static RangoHorario Futuro(DateTime inicio, DateTime fin, DateTime ahora)
    {
        if (inicio <= ahora)
            throw new InvariantViolationException("RangoHorario.Inicio debe ser futuro");
        return Of(inicio, fin);
    }

    public bool SolapaCon(RangoHorario otro) => Inicio < otro.Fin && otro.Inicio < Fin;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Inicio;
        yield return Fin;
    }
}

public sealed class PeriodoCartelera : ValueObject
{
    public DateTime Inicio { get; }
    public DateTime Fin { get; }

    private PeriodoCartelera(DateTime inicio, DateTime fin) { Inicio = inicio; Fin = fin; }

    public static PeriodoCartelera Of(DateTime inicio, DateTime fin)
    {
        if (fin <= inicio)
            throw new InvariantViolationException("PeriodoCartelera: fin debe ser mayor a inicio");
        return new(inicio, fin);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Inicio; yield return Fin; }
}
