using AwesomeAssertions;
using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.System.AuditLogs;

public sealed class AuditLogServiceGetPaginatedAsyncTests
{
    [Fact]
    public async Task GetPaginatedAsync_WhenRepositorySucceeds_ReturnsMappedPageWithMetadata()
    {
        // Arrange
        AuditLogServiceTestSetup setup = new();
        Guid entityId = Guid.NewGuid();
        setup.AuditLogRepository
            .Setup(r => r.GetPaginatedAsync(
                entityId,
                1,
                20,
                "createdDate",
                false,
                "created",
                AuditLogTestData.FromDate,
                AuditLogTestData.ToDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuditLog[]>.Success(
                message: "Audit logs retrieved",
                data: [AuditLogTestData.CreateAuditLog(description: "Note created successfully")],
                statusCode: HttpStatusCode.OK));
        setup.AuditLogRepository
            .Setup(r => r.CountByEntityAsync(
                entityId,
                "created",
                AuditLogTestData.FromDate,
                AuditLogTestData.ToDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(
                message: "Counted",
                data: 41,
                statusCode: HttpStatusCode.OK));

        // Act
        PaginatedResult<string[]> result = await setup.Service.GetPaginatedAsync<string>(
            entityId,
            1,
            20,
            "createdDate",
            false,
            "created",
            AuditLogTestData.FromDate,
            AuditLogTestData.ToDate,
            logs => [.. logs.Select(l => l.Description)],
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().Equal("Note created successfully");
        result.Metadata!.PageNumber.Should().Be(1);
        result.Metadata.PageSize.Should().Be(20);
        result.Metadata.TotalCount.Should().Be(41);
        result.Metadata.PageCount.Should().Be(3);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenPageRetrievalFails_PropagatesFailureWithoutCounting()
    {
        // Arrange
        AuditLogServiceTestSetup setup = new();
        setup.AuditLogRepository
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
                message: "Query failed",
                statusCode: HttpStatusCode.InternalServerError));

        // Act
        PaginatedResult<string[]> result = await setup.Service.GetPaginatedAsync<string>(
            null,
            1,
            20,
            "createdDate",
            false,
            null,
            null,
            null,
            logs => [.. logs.Select(l => l.Description)],
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Be("Query failed");
        setup.AuditLogRepository.Verify(
            r => r.CountByEntityAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenCountFails_PropagatesFailure()
    {
        // Arrange
        AuditLogServiceTestSetup setup = new();
        setup.AuditLogRepository
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
                message: "Audit logs retrieved",
                data: [],
                statusCode: HttpStatusCode.OK));
        setup.AuditLogRepository
            .Setup(r => r.CountByEntityAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Failure(
                message: "Count failed",
                statusCode: HttpStatusCode.InternalServerError));

        // Act
        PaginatedResult<string[]> result = await setup.Service.GetPaginatedAsync<string>(
            null,
            1,
            20,
            "createdDate",
            false,
            null,
            null,
            null,
            logs => [.. logs.Select(l => l.Description)],
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Be("Count failed");
    }
}
