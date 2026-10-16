using AwesomeAssertions;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.System.OutboxMessages;

public sealed partial class OutboxMessageTests
{
    [Fact]
    public void MarkProcessed_WithoutError_ReturnsSuccessAndSetsProcessedOnUtc()
    {
        // Arrange
        OutboxMessage message = CreateValidOutboxMessage();

        // Act
        Result result = message.MarkProcessed(null);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Message.Should().Be("Outbox message marked as processed");
        message.ProcessedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        message.Error.Should().BeNull();
    }

    [Fact]
    public void MarkProcessed_WhenErrorIsSupplied_RecordsError()
    {
        // Arrange
        OutboxMessage message = CreateValidOutboxMessage();
        const string error = "Handler failed to send the notification.";

        // Act
        Result result = message.MarkProcessed(error);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        message.ProcessedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        message.Error.Should().Be(error);
    }

    [Fact]
    public void MarkProcessed_WithValidMessage_DoesNotChangeOtherProperties()
    {
        // Arrange
        OutboxMessage message = CreateValidOutboxMessage(type: ValidType, content: ValidContent);
        Guid originalId = message.Id;
        DateTime originalOccurredOnUtc = message.OccurredOnUtc;

        // Act
        Result result = message.MarkProcessed(null);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        message.Id.Should().Be(originalId);
        message.Type.Should().Be(ValidType);
        message.Content.Should().Be(ValidContent);
        message.OccurredOnUtc.Should().Be(originalOccurredOnUtc);
    }

    [Fact]
    public void MarkProcessed_WhenAlreadyProcessedWithError_ReplacesError()
    {
        // Arrange
        OutboxMessage message = CreateProcessedOutboxMessage(error: "First attempt failed.");

        // Act
        Result result = message.MarkProcessed(null);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        message.ProcessedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        message.Error.Should().BeNull();
    }
}
