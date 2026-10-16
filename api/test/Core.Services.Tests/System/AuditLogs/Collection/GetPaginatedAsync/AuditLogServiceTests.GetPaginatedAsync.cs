using AwesomeAssertions;
using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.System.AuditLogs;

public sealed partial class AuditLogServiceTests
{
    [Fact]
    public async Task GetPaginatedAsync_WithValidParameters_ReturnsSuccessResult()
    {
        // Arrange
        AuditLog[] auditLogs = [CreateValidAuditLog(), CreateValidAuditLog()];
        GivenAuditLogPageFound(auditLogs);
        GivenAuditLogCountFound(auditLogs.Length);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            null,
            Page,
            PageSize,
            SortBy,
            false,
            null,
            null,
            null,
            MapToDescriptions,
            TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Message.Should().Be("List retrieved successfully");
    }

    [Fact]
    public async Task GetPaginatedAsync_WithValidParameters_ReturnsMappedPage()
    {
        // Arrange
        AuditLog[] auditLogs =
        [
            CreateValidAuditLog(description: "Note created successfully"),
            CreateValidAuditLog(description: "Note soft-deleted successfully")
        ];
        GivenAuditLogPageFound(auditLogs);
        GivenAuditLogCountFound(auditLogs.Length);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            null,
            Page,
            PageSize,
            SortBy,
            false,
            null,
            null,
            null,
            MapToDescriptions,
            TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Data.Should().Equal("Note created successfully", "Note soft-deleted successfully");
    }

    [Fact]
    public async Task GetPaginatedAsync_WithValidParameters_ReturnsPaginationMetadata()
    {
        // Arrange
        int page = 3;
        int pageSize = 5;
        int totalCount = 12;
        AuditLog[] auditLogs = [CreateValidAuditLog()];
        GivenAuditLogPageFound(auditLogs);
        GivenAuditLogCountFound(totalCount);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            null,
            page,
            pageSize,
            SortBy,
            false,
            null,
            null,
            null,
            MapToDescriptions,
            TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Metadata!.PageNumber.Should().Be(page);
        result.Metadata.PageSize.Should().Be(pageSize);
        result.Metadata.TotalCount.Should().Be(totalCount);
        result.Metadata.PageCount.Should().Be(3);
        result.Metadata.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithFilters_PassesParametersToRepository()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        int page = 2;
        int pageSize = 10;
        string sortBy = "entityId";
        bool isAscending = true;
        string keyword = "title";
        DateTime fromDate = DateTime.UtcNow.AddDays(-7);
        DateTime toDate = DateTime.UtcNow;
        AuditLog[] auditLogs = [CreateValidAuditLog()];
        GivenAuditLogPageFound(auditLogs);
        GivenAuditLogCountFound(auditLogs.Length);

        // Act
        await _sut.GetPaginatedAsync(
            entityId,
            page,
            pageSize,
            sortBy,
            isAscending,
            keyword,
            fromDate,
            toDate,
            MapToDescriptions,
            TestContext.Current.CancellationToken);

        // Assert
        _auditLogRepositoryMock.Verify(
            r => r.GetPaginatedAsync(entityId, page, pageSize, sortBy, isAscending, keyword, fromDate, toDate, It.IsAny<CancellationToken>()),
            Times.Once);
        _auditLogRepositoryMock.Verify(
            r => r.CountFilteredAsync(entityId, keyword, fromDate, toDate, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithEmptyResult_ReturnsSuccessWithEmptyArray()
    {
        // Arrange
        AuditLog[] auditLogs = [];
        GivenAuditLogPageFound(auditLogs);
        GivenAuditLogCountFound(0);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            null,
            Page,
            PageSize,
            SortBy,
            false,
            null,
            null,
            null,
            MapToDescriptions,
            TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Data.Should().BeEmpty();
        result.Metadata!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenRepositoryFails_ReturnsRepositoryFailure()
    {
        // Arrange
        const string failureMessage = "Failed to retrieve audit logs.";
        GivenAuditLogPageFails(failureMessage, HttpStatusCode.InternalServerError);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            null,
            Page,
            PageSize,
            SortBy,
            false,
            null,
            null,
            null,
            MapToDescriptions,
            TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be(failureMessage);
        _auditLogRepositoryMock.Verify(
            r => r.CountFilteredAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenCountFails_ReturnsCountFailure()
    {
        // Arrange
        const string failureMessage = "Failed to count audit logs.";
        AuditLog[] auditLogs = [CreateValidAuditLog()];
        GivenAuditLogPageFound(auditLogs);
        GivenAuditLogCountFails(failureMessage, HttpStatusCode.InternalServerError);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            null,
            Page,
            PageSize,
            SortBy,
            false,
            null,
            null,
            null,
            MapToDescriptions,
            TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be(failureMessage);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task GetPaginatedAsync_WithValidParameters_DoesNotStageChanges()
    {
        // Arrange
        AuditLog[] auditLogs = [CreateValidAuditLog()];
        GivenAuditLogPageFound(auditLogs);
        GivenAuditLogCountFound(auditLogs.Length);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            null,
            Page,
            PageSize,
            SortBy,
            false,
            null,
            null,
            null,
            MapToDescriptions,
            TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        VerifyNoWriteSideEffects();
    }
}
