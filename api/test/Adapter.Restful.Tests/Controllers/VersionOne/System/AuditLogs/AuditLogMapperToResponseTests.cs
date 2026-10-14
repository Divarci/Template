using Adapter.Restful.Controllers.VersionOne.System.AuditLogs;
using Adapter.Restful.Controllers.VersionOne.System.AuditLogs.Models.Responses;
using AwesomeAssertions;
using Core.Domain.Entities.System.AuditLogs;
using Xunit;

namespace Adapter.Restful.Tests.Controllers.VersionOne.System.AuditLogs;

public sealed class AuditLogMapperToResponseTests
{
    [Fact]
    public void ToResponse_WithAuditLog_MapsEveryField()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        AuditLog auditLog = AuditLogTestData.CreateAuditLog(
            entityId: entityId,
            description: "Note created successfully",
            createdOnUtc: AuditLogTestData.CreatedOnUtc);

        // Act
        AuditLogResponse response = AuditLogMapper.ToResponse(auditLog);

        // Assert
        response.Id.Should().Be(auditLog.Id);
        response.EntityId.Should().Be(entityId);
        response.Description.Should().Be("Note created successfully");
        response.CreatedOnUtc.Should().Be(AuditLogTestData.CreatedOnUtc);
    }
}
