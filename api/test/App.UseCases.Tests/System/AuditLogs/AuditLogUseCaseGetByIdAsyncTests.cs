using AwesomeAssertions;
using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace App.UseCases.Tests.System.AuditLogs;

public sealed class AuditLogUseCaseGetByIdAsyncTests
{
    [Fact]
    public async Task GetByIdAsync_WhenCalled_ReadsThroughCacheWithSingleKeyAndThirtyMinuteExpiry()
    {
        // Arrange
        AuditLogUseCaseTestSetup setup = new();
        setup.GivenSingleCacheMiss();
        Guid id = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF");

        // Act
        await setup.UseCase.GetByIdAsync(
            id,
            AuditLogTestData.DescriptionMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.CacheProvider.Verify(
            c => c.GetSingleAsync(
                It.IsAny<Func<Task<Result<string>>>>(),
                true,
                "auditlog:id(6f9619ff-8b86-d011-b42d-00c04fc964ff)",
                TimeSpan.FromMinutes(30)),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheMisses_CallsServiceWithIdAndMapper()
    {
        // Arrange
        AuditLogUseCaseTestSetup setup = new();
        setup.GivenSingleCacheMiss();
        Guid id = Guid.NewGuid();
        Result<string> serviceResult = Result<string>.Success(
            "Audit log retrieved",
            "Note created successfully",
            HttpStatusCode.OK);
        setup.AuditLogService
            .Setup(s => s.GetByIdAsync(
                id,
                AuditLogTestData.DescriptionMapper,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        Result<string> result = await setup.UseCase.GetByIdAsync(
            id,
            AuditLogTestData.DescriptionMapper,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(serviceResult);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheHits_ReturnsCachedResultWithoutCallingService()
    {
        // Arrange
        AuditLogUseCaseTestSetup setup = new();
        Guid id = Guid.NewGuid();
        Result<string> cachedResult = Result<string>.Success(
            "Audit log retrieved",
            "Cached",
            HttpStatusCode.OK);
        setup.CacheProvider
            .Setup(c => c.GetSingleAsync(
                It.IsAny<Func<Task<Result<string>>>>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>()))
            .ReturnsAsync(cachedResult);

        // Act
        Result<string> result = await setup.UseCase.GetByIdAsync(
            id,
            AuditLogTestData.DescriptionMapper,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(cachedResult);
        setup.AuditLogService.Verify(
            s => s.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Func<AuditLog, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
