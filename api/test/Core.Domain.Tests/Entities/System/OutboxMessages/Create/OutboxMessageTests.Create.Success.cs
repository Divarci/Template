using AwesomeAssertions;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.System.OutboxMessages;

public sealed partial class OutboxMessageTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        DateTime occurredOnUtc = DateTime.UtcNow.AddMinutes(-5);

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(id, ValidType, ValidContent, occurredOnUtc);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.Created);
        result.Message.Should().Be("Outbox message created successfully");
        result.Data!.Id.Should().Be(id);
        result.Data.Type.Should().Be(ValidType);
        result.Data.Content.Should().Be(ValidContent);
        result.Data.OccurredOnUtc.Should().Be(occurredOnUtc);
    }

    [Fact]
    public void Create_WithValidData_StartsUnprocessed()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(id, ValidType, ValidContent, DateTime.UtcNow);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.Created);
        result.Data!.ProcessedOnUtc.Should().BeNull();
        result.Data.Error.Should().BeNull();
    }
}
