using AwesomeAssertions;
using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.System.AuditLogs;

public sealed partial class AuditLogTests
{
    [Fact]
    public void Create_WithEmptyEntityId_ReturnsValidationError()
    {
        // Act
        Result<AuditLog> result = AuditLog.Create(Guid.Empty, ValidDescription, DateTime.UtcNow);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceDescription_ReturnsValidationError(string? description)
    {
        // Arrange
        Guid entityId = Guid.NewGuid();

        // Act
        Result<AuditLog> result = AuditLog.Create(entityId, description!, DateTime.UtcNow);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Create_WithTooLongDescription_ReturnsValidationError()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        string description = CreateStringOfLength(DescriptionMaxLength + 1);

        // Act
        Result<AuditLog> result = AuditLog.Create(entityId, description, DateTime.UtcNow);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Create_WithFutureCreatedOnUtc_ReturnsValidationError()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        DateTime createdOnUtc = DateTime.UtcNow.AddDays(1);

        // Act
        Result<AuditLog> result = AuditLog.Create(entityId, ValidDescription, createdOnUtc);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Create_WithSeveralInvalidFields_ReturnsOneErrorPerRule()
    {
        // Arrange
        DateTime createdOnUtc = DateTime.UtcNow.AddDays(1);

        // Act
        Result<AuditLog> result = AuditLog.Create(Guid.Empty, string.Empty, createdOnUtc);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().HaveCount(3);
    }
}
