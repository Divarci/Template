using Core.Domain.Entities.Samples.Notes.DomainEvents;
using Core.Domain.Entities.System.OutboxMessages;
using System.Text.Json;

namespace App.Processor.Tests;

internal static class OutboxMessageTestData
{
    public static readonly JsonSerializerOptions SerializerOptions = JsonSerializerOptions.Default;

    private static readonly DateTime OccurredOnUtc = new(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    public static OutboxMessage CreateMessage(
        string type,
        string content) =>
        OutboxMessage.Create(
            Guid.NewGuid(),
            type,
            content,
            OccurredOnUtc).Data!;

    public static OutboxMessage CreateNoteCreatedMessage(Guid noteId) =>
        CreateMessage(
            typeof(NoteCreatedDomainEvent).AssemblyQualifiedName!,
            JsonSerializer.Serialize(new NoteCreatedDomainEvent(noteId), SerializerOptions));
}
