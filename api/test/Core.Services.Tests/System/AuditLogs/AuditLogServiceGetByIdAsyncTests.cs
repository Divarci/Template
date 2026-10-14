using AwesomeAssertions;
using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.System.AuditLogs;

public sealed class AuditLogServiceGetByIdAsyncTests
{
    [Fact]
    public async Task GetByIdAsync_WhenAuditLogExists_ReturnsMappedAuditLog()
    {
        // Arrange
        AuditLogServiceTestSetup setup = new();
        AuditLog auditLog = AuditLogTestData.CreateAuditLog(description: "Note created successfully");
        setup.AuditLogRepository
            .Setup(r => r.GetByIdAsync(auditLog.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuditLog>.Success(
                message: "Audit log retrieved",
                data: auditLog,
                statusCode: HttpStatusCode.OK));

        // Act
        Result<string> result = await setup.Service.GetByIdAsync(
            auditLog.Id,
            l => l.Description,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("Audit log retrieved");
        result.Data.Should().Be("Note created successfully");
    }

    [Fact]
    public async Task GetByIdAsync_WhenAuditLogIsNotFound_PropagatesNotFound()
    {
        // Arrange
        AuditLogServiceTestSetup setup = new();
        Guid missingId = Guid.NewGuid();
        setup.AuditLogRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuditLog>.Failure(
                message: "Audit log not found.",
                statusCode: HttpStatusCode.NotFound));

        // Act
        Result<string> result = await setup.Service.GetByIdAsync(
            missingId,
            l => l.Description,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Audit log not found.");
        result.Data.Should().BeNull();
    }
}
