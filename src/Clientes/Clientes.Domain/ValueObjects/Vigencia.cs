using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Clientes.Domain.ValueObjects;

public sealed class Vigencia : ValueObject
{
    public DateTime FechaInicio { get; }
    public DateTime FechaFin { get; }

    private Vigencia(DateTime inicio, DateTime fin) { FechaInicio = inicio; FechaFin = fin; }

    public static Vigencia Of(DateTime inicio, DateTime fin)
    {
        if (fin <= inicio)
            throw new InvariantViolationException("Vigencia.FechaFin debe ser mayor a FechaInicio");
        return new Vigencia(inicio, fin);
    }

    public bool IncluyeMomento(DateTime momento) => momento >= FechaInicio && momento <= FechaFin;
    public bool HaExpirado(DateTime momento) => momento > FechaFin;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FechaInicio;
        yield return FechaFin;
    }
}
