using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Ventas.Domain.ValueObjects;

public sealed class OrdenId : ValueObject
{
    public Guid Value { get; }
    private OrdenId(Guid v) => Value = v;
    public static OrdenId New() => new(Guid.NewGuid());
    public static OrdenId Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("OrdenId vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value.ToString();
}

public sealed class ProductoId : ValueObject
{
    public Guid Value { get; }
    private ProductoId(Guid v) => Value = v;
    public static ProductoId New() => new(Guid.NewGuid());
    public static ProductoId Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("ProductoId vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}

public sealed class DefComboId : ValueObject
{
    public Guid Value { get; }
    private DefComboId(Guid v) => Value = v;
    public static DefComboId New() => new(Guid.NewGuid());
    public static DefComboId Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("DefComboId vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}

public sealed class EspectadorRef : ValueObject
{
    public Guid Value { get; }
    private EspectadorRef(Guid v) => Value = v;
    public static EspectadorRef Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("EspectadorRef vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}

public sealed class FuncionRef : ValueObject
{
    public Guid Value { get; }
    private FuncionRef(Guid v) => Value = v;
    public static FuncionRef Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("FuncionRef vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}

public sealed class SillaRef : ValueObject
{
    public Guid Value { get; }
    private SillaRef(Guid v) => Value = v;
    public static SillaRef Of(Guid v) { if (v == Guid.Empty) throw new InvariantViolationException("SillaRef vacío"); return new(v); }
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}
