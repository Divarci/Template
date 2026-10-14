using Adapter.Restful.Controllers.VersionOne.System.AuditLogs;
using Adapter.Restful.Controllers.VersionOne.System.AuditLogs.Models.Responses;
using AwesomeAssertions;
using Core.Domain.Entities.System.AuditLogs;
using Xunit;

namespace Adapter.Restful.Tests.Controllers.VersionOne.System.AuditLogs;

public sealed class AuditLogMapperToArrayResponseTests
{
    [Fact]
    public void ToArrayResponse_WithAuditLogs_MapsEachAuditLogInOrder()
    {
        // Arrange
        AuditLog first = AuditLogTestData.CreateAuditLog(description: "Note created successfully");
        AuditLog second = AuditLogTestData.CreateAuditLog(description: "Note updated successfully");

        // Act
        AuditLogResponse[] responses = AuditLogMapper.ToArrayResponse([first, second]);

        // Assert
        responses.Select(r => r.Id).Should().Equal(first.Id, second.Id);
        responses.Select(r => r.Description).Should().Equal("Note created successfully", "Note updated successfully");
    }

    [Fact]
    public void ToArrayResponse_WithNoAuditLogs_ReturnsEmptyArray()
    {
        // Arrange
        AuditLog[] auditLogs = [];

        // Act
        AuditLogResponse[] responses = AuditLogMapper.ToArrayResponse(auditLogs);

        // Assert
        responses.Should().BeEmpty();
    }
}
