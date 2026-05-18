namespace Shared.Kernel;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType()) return false;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj) => obj is ValueObject vo && Equals(vo);

    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(0, (h, o) => HashCode.Combine(h, o));

    public static bool operator ==(ValueObject? a, ValueObject? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
}
