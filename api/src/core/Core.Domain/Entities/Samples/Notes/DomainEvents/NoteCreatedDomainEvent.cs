using Core.Library.Contracts.DomainEvents.Process;

namespace Core.Domain.Entities.Samples.Notes.DomainEvents;

public sealed record NoteCreatedDomainEvent(Guid NoteId) : DomainEvent;
