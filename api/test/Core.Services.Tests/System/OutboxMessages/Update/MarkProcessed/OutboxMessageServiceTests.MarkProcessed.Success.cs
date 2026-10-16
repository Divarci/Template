using AwesomeAssertions;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.System.OutboxMessages;

public sealed partial class OutboxMessageServiceTests
{
    [Fact]
    public void MarkProcessed_WithoutError_ReturnsNoContent()
    {
        // Arrange
        OutboxMessage message = CreateValidOutboxMessage();
        GivenOutboxMessageUpdateSucceeds();

        // Act
        Result result = _sut.MarkProcessed(message, null);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        result.Message.Should().Be("Outbox message marked as processed");
    }

    [Fact]
    public void MarkProcessed_WithoutError_SetsProcessedOnUtc()
    {
        // Arrange
        OutboxMessage message = CreateValidOutboxMessage();
        GivenOutboxMessageUpdateSucceeds();

        // Act
        Result result = _sut.MarkProcessed(message, null);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        message.ProcessedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        message.Error.Should().BeNull();
    }

    [Fact]
    public void MarkProcessed_WhenErrorIsSupplied_RecordsError()
    {
        // Arrange
        OutboxMessage message = CreateValidOutboxMessage();
        const string error = "Handler failed to send the notification.";
        GivenOutboxMessageUpdateSucceeds();

        // Act
        Result result = _sut.MarkProcessed(message, error);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        message.ProcessedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        message.Error.Should().Be(error);
    }

    [Fact]
    public void MarkProcessed_WithValidMessage_StagesEntityOnRepository()
    {
        // Arrange
        OutboxMessage message = CreateValidOutboxMessage();
        GivenOutboxMessageUpdateSucceeds();

        // Act
        Result result = _sut.MarkProcessed(message, null);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        _outboxMessageRepositoryMock.Verify(r => r.Update(message), Times.Once);
    }
}
