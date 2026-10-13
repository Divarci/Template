namespace Core.Library.Contracts.DomainEvents.Process;

public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
        : this(Guid.NewGuid(), DateTime.UtcNow)
    {
    }

    protected DomainEvent(Guid id, DateTime occurredOnUtc)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; init; }
    public DateTime OccurredOnUtc { get; init; }
}
