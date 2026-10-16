using AwesomeAssertions;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Domain.Tests.Entities.System.OutboxMessages;

public sealed partial class OutboxMessageTests
{
    private const int TypeMaxLength = 2000;
    private const int ContentMaxLength = 100000;
    private const string ValidType = "Core.Domain.Entities.Samples.Notes.DomainEvents.NoteCreatedDomainEvent, Core.Domain";
    private const string ValidContent = "{\"noteId\":\"3f2504e0-4f89-11d3-9a0c-0305e82c3301\"}";

    private static OutboxMessage CreateValidOutboxMessage(
        string type = ValidType,
        string content = ValidContent)
    {
        Result<OutboxMessage> result = OutboxMessage.Create(Guid.NewGuid(), type, content, DateTime.UtcNow);

        return result.Data!;
    }

    private static OutboxMessage CreateProcessedOutboxMessage(string? error)
    {
        OutboxMessage message = CreateValidOutboxMessage();

        message.MarkProcessed(error);

        return message;
    }

    private static string CreateStringOfLength(int length)
    {
        return new string('A', length);
    }

    private static void AssertSuccessResult(Result result, HttpStatusCode expectedStatusCode)
    {
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }

    private static void AssertSuccessResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Data.Should().NotBeNull();
    }

    private static void AssertFailureResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailureAndNoData.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }
}
