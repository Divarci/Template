namespace Core.Library.Contracts.DomainEvents.Process;

/// <summary>
/// Defines the contract for a domain event, requiring a unique identifier
/// and a UTC timestamp indicating when the event occurred.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of this domain event instance.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the UTC timestamp at which the domain event occurred.
    /// </summary>
    DateTime OccurredOnUtc { get; }
}