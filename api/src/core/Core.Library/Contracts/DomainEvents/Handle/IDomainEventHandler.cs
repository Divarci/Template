using Core.Library.Contracts.DomainEvents.Process;

namespace Core.Library.Contracts.DomainEvents.Handle;

/// <summary>
/// Defines the non-generic contract for handling any <see cref="IDomainEvent"/>.
/// Used as a common base for resolving handlers without knowing the specific event type at compile time.
/// </summary>
public interface IDomainEventHandler
{
    /// <summary>
    /// Handles the specified domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous handling operation.</returns>
    Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
