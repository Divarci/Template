using AwesomeAssertions;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.System.OutboxMessages;

public sealed partial class OutboxMessageTests
{
    [Fact]
    public void Create_WithEmptyId_ReturnsValidationError()
    {
        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(Guid.Empty, ValidType, ValidContent, DateTime.UtcNow);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.Message.Should().Be("Validation failed");
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Id cannot be empty guid");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceType_ReturnsValidationError(string? type)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(id, type!, ValidContent, DateTime.UtcNow);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Create_WithTooLongType_ReturnsValidationError()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string type = CreateStringOfLength(TypeMaxLength + 1);

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(id, type, ValidContent, DateTime.UtcNow);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceContent_ReturnsValidationError(string? content)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(id, ValidType, content!, DateTime.UtcNow);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Create_WithTooLongContent_ReturnsValidationError()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string content = CreateStringOfLength(ContentMaxLength + 1);

        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(id, ValidType, content, DateTime.UtcNow);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Create_WithSeveralInvalidFields_ReturnsOneErrorPerRule()
    {
        // Act
        Result<OutboxMessage> result = OutboxMessage.Create(Guid.Empty, string.Empty, string.Empty, DateTime.UtcNow);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().HaveCount(3);
    }
}
