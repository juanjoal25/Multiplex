using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Programacion.Domain.ValueObjects;

public sealed class FuncionId : ValueObject
{
    public Guid Value { get; }
    private FuncionId(Guid v) => Value = v;
    public static FuncionId New() => new(Guid.NewGuid());
    public static FuncionId Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("FuncionId vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value.ToString();
}

public sealed class PeliculaId : ValueObject
{
    public Guid Value { get; }
    private PeliculaId(Guid v) => Value = v;
    public static PeliculaId New() => new(Guid.NewGuid());
    public static PeliculaId Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("PeliculaId vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value.ToString();
}

public sealed class CarteleraId : ValueObject
{
    public Guid Value { get; }
    private CarteleraId(Guid v) => Value = v;
    public static CarteleraId New() => new(Guid.NewGuid());
    public static CarteleraId Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("CarteleraId vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}

public sealed class AlquilerId : ValueObject
{
    public Guid Value { get; }
    private AlquilerId(Guid v) => Value = v;
    public static AlquilerId New() => new(Guid.NewGuid());
    public static AlquilerId Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("AlquilerId vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}

public sealed class PeliculaRef : ValueObject
{
    public Guid IdPelicula { get; }
    private PeliculaRef() { }
    private PeliculaRef(Guid idPelicula) => IdPelicula = idPelicula;
    public static PeliculaRef Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("PeliculaRef vacío"); return new(v); }
    public static PeliculaRef Of(PeliculaId id) => new(id.Value);
    protected override IEnumerable<object?> GetEqualityComponents() { yield return IdPelicula; }
}

public sealed class SalaRef : ValueObject
{
    public Guid IdSala { get; }
    public TipoSala? Tipo { get; }
    private SalaRef() { }
    private SalaRef(Guid idSala, TipoSala? tipo) { IdSala = idSala; Tipo = tipo; }
    public static SalaRef Of(Guid v, TipoSala? tipo = null) { if (v == Guid.Empty) throw new InvariantViolationException("SalaRef vacío"); return new(v, tipo); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return IdSala; yield return Tipo; }
}

public enum TipoSala { General, Vip, Imax, Especial }
