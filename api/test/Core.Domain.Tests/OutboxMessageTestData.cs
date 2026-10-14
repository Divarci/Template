using Core.Domain.Entities.System.OutboxMessages;

namespace Core.Domain.Tests;

internal static class OutboxMessageTestData
{
    public const string EventType = "Core.Domain.Entities.Samples.Notes.DomainEvents.NoteCreatedDomainEvent, Core.Domain";
    public const string EventContent = "{\"noteId\":\"6f9619ff-8b86-d011-b42d-00c04fc964ff\"}";

    public static readonly DateTime OccurredOnUtc = new(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    public static OutboxMessage CreateMessage() =>
        OutboxMessage.Create(
            Guid.NewGuid(),
            EventType,
            EventContent,
            OccurredOnUtc).Data!;
}
