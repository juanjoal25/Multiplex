using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Programacion.Domain.ValueObjects;

public enum Clasificacion { G, PG, PG13, R }

public sealed class Duracion : ValueObject
{
    public int Minutos { get; }
    private Duracion(int m) => Minutos = m;
    public static Duracion Of(int minutos)
    {
        if (minutos <= 0 || minutos > 300)
            throw new InvariantViolationException("Duracion debe estar entre 1 y 300 minutos");
        return new(minutos);
    }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Minutos; }
    public override string ToString() => $"{Minutos} min";
}

public sealed class Titulo : ValueObject
{
    public string Valor { get; }
    private Titulo(string v) => Valor = v;
    public static Titulo Of(string v)
    {
        if (string.IsNullOrWhiteSpace(v) || v.Trim().Length < 2)
            throw new InvariantViolationException("Titulo requiere mínimo 2 caracteres");
        return new(v.Trim());
    }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Valor; }
    public override string ToString() => Valor;
}

public sealed class Genero : ValueObject
{
    public string Valor { get; }
    private Genero(string v) => Valor = v;
    public static Genero Of(string v)
    {
        if (string.IsNullOrWhiteSpace(v))
            throw new InvariantViolationException("Genero requerido");
        return new(v.Trim());
    }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Valor; }
}
