namespace Shared.Kernel;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
