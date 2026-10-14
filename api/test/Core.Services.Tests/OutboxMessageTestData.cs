using Core.Domain.Entities.System.OutboxMessages;

namespace Core.Services.Tests;

internal static class OutboxMessageTestData
{
    public static OutboxMessage CreateMessage() =>
        OutboxMessage.Create(
            Guid.NewGuid(),
            "Some.Event, Some.Assembly",
            "{}",
            new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)).Data!;
}
