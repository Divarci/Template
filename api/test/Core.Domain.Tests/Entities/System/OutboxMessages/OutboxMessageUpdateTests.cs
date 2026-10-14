using AwesomeAssertions;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.System.OutboxMessages;

public sealed class OutboxMessageUpdateTests
{
    [Fact]
    public void Update_WhenNoErrorIsSupplied_ReturnsOk()
    {
        // Arrange
        OutboxMessage message = OutboxMessageTestData.CreateMessage();

        // Act
        Result result = message.Update(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("Outbox message updated successfully");
    }

    [Fact]
    public void Update_WhenNoErrorIsSupplied_MarksMessageProcessedWithoutError()
    {
        // Arrange
        OutboxMessage message = OutboxMessageTestData.CreateMessage();

        // Act
        message.Update(null);

        // Assert
        message.ProcessedOnUtc.Should().NotBeNull();
        message.ProcessedOnUtc!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        message.Error.Should().BeNull();
    }

    [Fact]
    public void Update_WhenErrorIsSupplied_MarksMessageProcessedAndRecordsError()
    {
        // Arrange
        OutboxMessage message = OutboxMessageTestData.CreateMessage();

        // Act
        message.Update("Handler failed");

        // Assert
        message.ProcessedOnUtc.Should().NotBeNull();
        message.Error.Should().Be("Handler failed");
    }
}
