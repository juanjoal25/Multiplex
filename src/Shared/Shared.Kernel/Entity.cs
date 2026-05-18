namespace Shared.Kernel;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; }

    protected Entity(TId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }

    public bool Equals(Entity<TId>? other) =>
        other is not null && EqualityComparer<TId>.Default.Equals(Id, other.Id);

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && Equals(other);

    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);

    public static bool operator ==(Entity<TId>? a, Entity<TId>? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(Entity<TId>? a, Entity<TId>? b) => !(a == b);
}
