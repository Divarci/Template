using AwesomeAssertions;
using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.System.AuditLogs;

public sealed partial class AuditLogTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        DateTime createdOnUtc = DateTime.UtcNow.AddMinutes(-5);

        // Act
        Result<AuditLog> result = AuditLog.Create(entityId, ValidDescription, createdOnUtc);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.Created);
        result.Message.Should().Be("Audit log created successfully");
        result.Data!.Id.Should().NotBeEmpty();
        result.Data.EntityId.Should().Be(entityId);
        result.Data.Description.Should().Be(ValidDescription);
        result.Data.CreatedOnUtc.Should().Be(createdOnUtc);
    }

    [Fact]
    public void Create_WithValidData_RaisesNoDomainEvent()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();

        // Act
        Result<AuditLog> result = AuditLog.Create(entityId, ValidDescription, DateTime.UtcNow);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.Created);
        result.Data!.DomainEvents.Should().BeEmpty();
    }
}
