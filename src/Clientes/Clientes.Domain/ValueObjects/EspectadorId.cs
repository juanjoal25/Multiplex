using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Clientes.Domain.ValueObjects;

public sealed class EspectadorId : ValueObject
{
    public Guid Value { get; }

    private EspectadorId(Guid value) => Value = value;

    public static EspectadorId New() => new(Guid.NewGuid());

    public static EspectadorId Of(Guid value)
    {
        if (value == Guid.Empty) throw new InvariantViolationException("EspectadorId no puede ser Guid.Empty");
        return new EspectadorId(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    public override string ToString() => Value.ToString();
}
