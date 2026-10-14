using AwesomeAssertions;
using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.System.AuditLogs;

public sealed class AuditLogCreateTests
{
    [Fact]
    public void Create_WhenValuesAreValid_ReturnsCreatedAuditLog()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();

        // Act
        Result<AuditLog> result = AuditLog.Create(
            entityId,
            "Note created successfully",
            AuditLogTestData.CreatedOnUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Message.Should().Be("Audit log created successfully");
    }

    [Fact]
    public void Create_WhenValuesAreValid_CopiesValuesOntoAuditLog()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();

        // Act
        Result<AuditLog> result = AuditLog.Create(
            entityId,
            "Note created successfully",
            AuditLogTestData.CreatedOnUtc);

        // Assert
        AuditLog auditLog = result.Data!;
        auditLog.Id.Should().NotBe(Guid.Empty);
        auditLog.EntityId.Should().Be(entityId);
        auditLog.Description.Should().Be("Note created successfully");
        auditLog.CreatedOnUtc.Should().Be(AuditLogTestData.CreatedOnUtc);
    }

    [Fact]
    public void Create_WhenValuesAreValid_RaisesNoDomainEvent()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();

        // Act
        Result<AuditLog> result = AuditLog.Create(
            entityId,
            "Note created successfully",
            AuditLogTestData.CreatedOnUtc);

        // Assert
        result.Data!.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Create_WhenEntityIdIsEmpty_ReturnsUnprocessableEntity()
    {
        // Arrange
        Guid entityId = Guid.Empty;

        // Act
        Result<AuditLog> result = AuditLog.Create(
            entityId,
            "Note created successfully",
            AuditLogTestData.CreatedOnUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("EntityId cannot be empty guid");
        result.Data.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenDescriptionIsEmptyOrWhitespace_ReturnsUnprocessableEntity(string description)
    {
        // Arrange
        Guid entityId = Guid.NewGuid();

        // Act
        Result<AuditLog> result = AuditLog.Create(
            entityId,
            description,
            AuditLogTestData.CreatedOnUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Description cannot be null or empty.");
    }

    [Fact]
    public void Create_WhenDescriptionIsAtMaximumLength_ReturnsCreatedAuditLog()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();

        // Act
        Result<AuditLog> result = AuditLog.Create(
            entityId,
            new string('a', 500),
            AuditLogTestData.CreatedOnUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WhenDescriptionExceedsMaximumLength_ReturnsUnprocessableEntity()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();

        // Act
        Result<AuditLog> result = AuditLog.Create(
            entityId,
            new string('a', 501),
            AuditLogTestData.CreatedOnUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Description must be 500 characters or less.");
    }

    [Fact]
    public void Create_WhenCreatedOnUtcIsInTheFuture_ReturnsUnprocessableEntity()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        DateTime futureUtc = DateTime.UtcNow.AddYears(1);

        // Act
        Result<AuditLog> result = AuditLog.Create(
            entityId,
            "Note created successfully",
            futureUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("CreatedOnUtc cannot be a future date");
    }

    [Fact]
    public void Create_WhenSeveralValuesAreInvalid_ReturnsEveryError()
    {
        // Arrange
        Guid entityId = Guid.Empty;

        // Act
        Result<AuditLog> result = AuditLog.Create(
            entityId,
            string.Empty,
            AuditLogTestData.CreatedOnUtc);

        // Assert
        result.ErrorDetails!.Errors.Should().BeEquivalentTo(
            "EntityId cannot be empty guid",
            "Description cannot be null or empty.");
    }
}
