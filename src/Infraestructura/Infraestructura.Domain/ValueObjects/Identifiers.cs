using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Infraestructura.Domain.ValueObjects;

public sealed class SalaId : ValueObject
{
    public Guid Value { get; }
    private SalaId(Guid v) => Value = v;
    public static SalaId New() => new(Guid.NewGuid());
    public static SalaId Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("SalaId vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value.ToString();
}

public sealed class SillaId : ValueObject
{
    public Guid Value { get; }
    private SillaId(Guid v) => Value = v;
    public static SillaId New() => new(Guid.NewGuid());
    public static SillaId Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("SillaId vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}

public sealed class Posicion : ValueObject
{
    public string Fila { get; }
    public int Columna { get; }
    private Posicion() { Fila = null!; }
    private Posicion(string fila, int columna) { Fila = fila; Columna = columna; }
    public static Posicion Of(string fila, int columna)
    {
        if (string.IsNullOrWhiteSpace(fila)) throw new InvariantViolationException("Fila requerida");
        if (columna < 1) throw new InvariantViolationException("Columna debe ser >= 1");
        return new(fila.Trim().ToUpperInvariant(), columna);
    }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Fila; yield return Columna; }
    public override string ToString() => $"{Fila}{Columna}";
}

public sealed class Aforo : ValueObject
{
    public int Valor { get; }
    private Aforo() { }
    private Aforo(int v) => Valor = v;
    public static Aforo Of(int v)
    {
        if (v < 1) throw new InvariantViolationException("Aforo debe ser >= 1");
        return new(v);
    }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Valor; }
}

public enum TipoSala { General, Vip, Imax, Especial }
public enum TipoSilla { General, Vip, Especial, Acompanante }

public sealed class ReservaExpiracion : ValueObject
{
    public DateTime Valor { get; }
    private ReservaExpiracion() { }
    private ReservaExpiracion(DateTime v) => Valor = v;
    public static ReservaExpiracion Of(DateTime v, DateTime ahora)
    {
        if (v <= ahora) throw new InvariantViolationException("ReservaExpiracion debe ser futura");
        return new(v);
    }
    public bool HaExpirado(DateTime ahora) => ahora >= Valor;
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Valor; }
}
