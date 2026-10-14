using Core.Library.Contracts.DomainEvents.Process;

namespace Core.Library.Contracts.DomainEvents.Handle;

public abstract class DomainEventHandler<TDomainEvent> : IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public abstract Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default);

    public Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        HandleAsync((TDomainEvent)domainEvent, cancellationToken);
}
