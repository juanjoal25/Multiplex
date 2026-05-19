using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Cadena.Domain.ValueObjects;

public sealed class SucursalId : ValueObject
{
    public Guid Value { get; }
    private SucursalId(Guid v) => Value = v;
    public static SucursalId New() => new(Guid.NewGuid());
    public static SucursalId Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("SucursalId vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}

public sealed class ContratoId : ValueObject
{
    public Guid Value { get; }
    private ContratoId(Guid v) => Value = v;
    public static ContratoId New() => new(Guid.NewGuid());
    public static ContratoId Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("ContratoId vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}

public sealed class NombreSucursal : ValueObject
{
    public string Valor { get; }
    private NombreSucursal() { Valor = null!; }
    private NombreSucursal(string v) => Valor = v;
    public static NombreSucursal Of(string v)
    {
        var trimmed = v?.Trim() ?? "";
        if (trimmed.Length < 3 || trimmed.Length > 100)
            throw new InvariantViolationException("NombreSucursal: 3-100 caracteres");
        return new(trimmed);
    }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Valor; }
}

public sealed class Vigencia : ValueObject
{
    public DateTime FechaInicio { get; }
    public DateTime FechaFin { get; }
    private Vigencia() { }
    private Vigencia(DateTime i, DateTime f) { FechaInicio = i; FechaFin = f; }
    public static Vigencia Of(DateTime i, DateTime f)
    {
        if (f <= i) throw new InvariantViolationException("Vigencia.FechaFin debe ser mayor a FechaInicio");
        return new(i, f);
    }
    public bool EstaVigenteEn(DateTime momento) => momento >= FechaInicio && momento <= FechaFin;
    public bool HaVencido(DateTime momento) => momento > FechaFin;
    protected override IEnumerable<object?> GetEqualityComponents() { yield return FechaInicio; yield return FechaFin; }
}

public enum EstadoContrato { Vigente, Vencido, Cancelado }
public enum TipoParametro { String, Entero, Booleano, Decimal }

public sealed class ParametroGlobal : ValueObject
{
    public string Clave { get; }
    public string Valor { get; }
    public TipoParametro Tipo { get; }

    private ParametroGlobal() { Clave = null!; Valor = null!; }
    private ParametroGlobal(string c, string v, TipoParametro t) { Clave = c; Valor = v; Tipo = t; }

    public static ParametroGlobal Of(string clave, string valor, TipoParametro tipo)
    {
        if (string.IsNullOrWhiteSpace(clave)) throw new InvariantViolationException("Clave requerida");
        return new(clave.Trim(), valor ?? "", tipo);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Clave; yield return Valor; yield return Tipo; }
}
