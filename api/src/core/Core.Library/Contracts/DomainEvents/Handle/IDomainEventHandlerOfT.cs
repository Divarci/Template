using Core.Library.Contracts.DomainEvents.Process;

namespace Core.Library.Contracts.DomainEvents.Handle;

/// <summary>
/// Defines the contract for handling a specific domain event of type <typeparamref name="TDomainEvent"/>.
/// Extends the non-generic <see cref="IDomainEventHandler"/> to provide strongly-typed event handling.
/// </summary>
/// <typeparam name="TDomainEvent">The specific domain event type this handler processes. Contravariant.</typeparam>
public interface IDomainEventHandler<in TDomainEvent> : IDomainEventHandler
    where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// Handles the specified domain event of type <typeparamref name="TDomainEvent"/>.
    /// </summary>
    /// <param name="domainEvent">The domain event to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous handling operation.</returns>
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
