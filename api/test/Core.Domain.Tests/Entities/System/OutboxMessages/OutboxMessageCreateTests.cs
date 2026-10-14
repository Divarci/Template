using AwesomeAssertions;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.System.OutboxMessages;

public sealed class OutboxMessageCreateTests
{
    [Fact]
    public void Create_WhenValuesAreValid_ReturnsCreatedMessage()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(
            id,
            OutboxMessageTestData.EventType,
            OutboxMessageTestData.EventContent,
            OutboxMessageTestData.OccurredOnUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Message.Should().Be("Outbox message created successfully");
    }

    [Fact]
    public void Create_WhenValuesAreValid_CreatesUnprocessedMessage()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(
            id,
            OutboxMessageTestData.EventType,
            OutboxMessageTestData.EventContent,
            OutboxMessageTestData.OccurredOnUtc);

        // Assert
        OutboxMessage message = result.Data!;
        message.Id.Should().Be(id);
        message.Type.Should().Be(OutboxMessageTestData.EventType);
        message.Content.Should().Be(OutboxMessageTestData.EventContent);
        message.OccurredOnUtc.Should().Be(OutboxMessageTestData.OccurredOnUtc);
        message.ProcessedOnUtc.Should().BeNull();
        message.Error.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenTypeIsEmptyOrWhitespace_ReturnsUnprocessableEntity(string type)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(
            id,
            type,
            OutboxMessageTestData.EventContent,
            OutboxMessageTestData.OccurredOnUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Type cannot be null or empty.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Create_WhenTypeIsAtMaximumLength_ReturnsCreatedMessage()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(
            id,
            new string('a', 2000),
            OutboxMessageTestData.EventContent,
            OutboxMessageTestData.OccurredOnUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WhenTypeExceedsMaximumLength_ReturnsUnprocessableEntity()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(
            id,
            new string('a', 2001),
            OutboxMessageTestData.EventContent,
            OutboxMessageTestData.OccurredOnUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Type must be 2000 characters or less.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenContentIsEmptyOrWhitespace_ReturnsUnprocessableEntity(string content)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(
            id,
            OutboxMessageTestData.EventType,
            content,
            OutboxMessageTestData.OccurredOnUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Content cannot be null or empty.");
    }

    [Fact]
    public void Create_WhenContentIsAtMaximumLength_ReturnsCreatedMessage()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(
            id,
            OutboxMessageTestData.EventType,
            new string('a', 100000),
            OutboxMessageTestData.OccurredOnUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WhenContentExceedsMaximumLength_ReturnsUnprocessableEntity()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(
            id,
            OutboxMessageTestData.EventType,
            new string('a', 100001),
            OutboxMessageTestData.OccurredOnUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Content must be 100000 characters or less.");
    }
}
