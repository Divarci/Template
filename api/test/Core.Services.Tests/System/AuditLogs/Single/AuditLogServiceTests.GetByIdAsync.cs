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
    public async Task GetByIdAsync_WithExistingId_ReturnsSuccessResult()
    {
        // Arrange
        AuditLog auditLog = CreateValidAuditLog();
        GivenAuditLogFound(auditLog);

        // Act
        Result<string> result = await _sut.GetByIdAsync(auditLog.Id, a => a.Description, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsMappedAuditLog()
    {
        // Arrange
        AuditLog auditLog = CreateValidAuditLog(description: "Note title updated successfully");
        GivenAuditLogFound(auditLog);

        // Act
        Result<string> result = await _sut.GetByIdAsync(auditLog.Id, a => a.Description, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Data.Should().Be("Note title updated successfully");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_CallsRepositoryWithId()
    {
        // Arrange
        AuditLog auditLog = CreateValidAuditLog();
        GivenAuditLogFound(auditLog);

        // Act
        await _sut.GetByIdAsync(auditLog.Id, a => a.Description, TestContext.Current.CancellationToken);

        // Assert
        _auditLogRepositoryMock.Verify(r => r.GetByIdAsync(auditLog.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAuditLogNotFound_ReturnsNotFoundError()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        bool isMapperInvoked = false;
        Func<AuditLog, string> mapper = a =>
        {
            isMapperInvoked = true;

            return a.Description;
        };
        GivenAuditLogNotFound(id);

        // Act
        Result<string> result = await _sut.GetByIdAsync(id, mapper, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.NotFound);
        isMapperInvoked.Should().BeFalse();
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_DoesNotStageChanges()
    {
        // Arrange
        AuditLog auditLog = CreateValidAuditLog();
        GivenAuditLogFound(auditLog);

        // Act
        Result<string> result = await _sut.GetByIdAsync(auditLog.Id, a => a.Description, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        VerifyNoWriteSideEffects();
    }
}
