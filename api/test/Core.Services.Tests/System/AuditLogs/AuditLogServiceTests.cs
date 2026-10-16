using AwesomeAssertions;
using Core.Domain.Entities.System.AuditLogs;
using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Core.Library.ResultPattern;
using Core.Services.System.AuditLogs;
using Moq;
using System.Net;

namespace Core.Services.Tests.System.AuditLogs;

public sealed partial class AuditLogServiceTests
{
    private const int Page = 1;
    private const int PageSize = 10;
    private const string SortBy = "createdOnUtc";

    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();
    private readonly AuditLogService _sut;

    public AuditLogServiceTests()
    {
        _sut = new AuditLogService(_auditLogRepositoryMock.Object);
    }

    private static AuditLog CreateValidAuditLog(
        string description = "Note created successfully")
    {
        Result<AuditLog> result = AuditLog.Create(Guid.NewGuid(), description, DateTime.UtcNow);

        return result.Data!;
    }

    private static string[] MapToDescriptions(AuditLog[] auditLogs)
    {
        return [.. auditLogs.Select(auditLog => auditLog.Description)];
    }

    private void GivenAuditLogFound(AuditLog auditLog)
    {
        _auditLogRepositoryMock
            .Setup(r => r.GetByIdAsync(auditLog.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuditLog>.Success(
                message: "Audit log retrieved successfully",
                data: auditLog,
                statusCode: HttpStatusCode.OK));
    }

    private void GivenAuditLogNotFound(Guid id)
    {
        _auditLogRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuditLog>.Failure(
                message: "Audit log not found.",
                statusCode: HttpStatusCode.NotFound));
    }

    private void GivenAuditLogPageFound(AuditLog[] auditLogs)
    {
        _auditLogRepositoryMock
            .Setup(r => r.GetPaginatedAsync(
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuditLog[]>.Success(
                message: "Audit logs retrieved successfully",
                data: auditLogs,
                statusCode: HttpStatusCode.OK));
    }

    private void GivenAuditLogPageFails(string message, HttpStatusCode statusCode)
    {
        _auditLogRepositoryMock
            .Setup(r => r.GetPaginatedAsync(
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuditLog[]>.Failure(
                message: message,
                statusCode: statusCode));
    }

    private void GivenAuditLogCountFound(int totalCount)
    {
        _auditLogRepositoryMock
            .Setup(r => r.CountFilteredAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(
                message: "Audit logs counted successfully",
                data: totalCount,
                statusCode: HttpStatusCode.OK));
    }

    private void GivenAuditLogCountFails(string message, HttpStatusCode statusCode)
    {
        _auditLogRepositoryMock
            .Setup(r => r.CountFilteredAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Failure(
                message: message,
                statusCode: statusCode));
    }

    private void VerifyNoWriteSideEffects()
    {
        _auditLogRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Never);
        _auditLogRepositoryMock.Verify(r => r.Update(It.IsAny<AuditLog>()), Times.Never);
        _auditLogRepositoryMock.Verify(r => r.Delete(It.IsAny<AuditLog>()), Times.Never);
    }

    private static void AssertSuccessResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Data.Should().NotBeNull();
    }

    private static void AssertFailureResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailureAndNoData.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }
}
