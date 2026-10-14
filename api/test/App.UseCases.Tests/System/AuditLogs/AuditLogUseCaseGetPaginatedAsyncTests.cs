using AwesomeAssertions;
using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using Moq;
using Xunit;

namespace App.UseCases.Tests.System.AuditLogs;

public sealed class AuditLogUseCaseGetPaginatedAsyncTests
{
    [Theory]
    [InlineData(null, 1)]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(4, 4)]
    public async Task GetPaginatedAsync_WhenPageIsSupplied_NormalisesPage(int? pageQuery, int expectedPage)
    {
        // Arrange
        AuditLogUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();

        // Act
        await setup.UseCase.GetPaginatedAsync(
            null,
            pageQuery,
            10,
            "description",
            true,
            null,
            null,
            null,
            AuditLogTestData.DescriptionsMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.VerifyServiceCalledWith(
            expectedPage,
            10,
            "description",
            true);
    }

    [Theory]
    [InlineData(null, 20)]
    [InlineData(0, 20)]
    [InlineData(50, 50)]
    public async Task GetPaginatedAsync_WhenPageSizeIsSupplied_NormalisesPageSize(int? pageSizeQuery, int expectedPageSize)
    {
        // Arrange
        AuditLogUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();

        // Act
        await setup.UseCase.GetPaginatedAsync(
            null,
            2,
            pageSizeQuery,
            "description",
            true,
            null,
            null,
            null,
            AuditLogTestData.DescriptionsMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.VerifyServiceCalledWith(
            2,
            expectedPageSize,
            "description",
            true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPaginatedAsync_WhenSortByIsMissing_DefaultsToCreatedDate(string? sortByQuery)
    {
        // Arrange
        AuditLogUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();

        // Act
        await setup.UseCase.GetPaginatedAsync(
            null,
            2,
            10,
            sortByQuery,
            true,
            null,
            null,
            null,
            AuditLogTestData.DescriptionsMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.VerifyServiceCalledWith(
            2,
            10,
            "createdDate",
            true);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenSortDirectionIsMissing_DefaultsToDescending()
    {
        // Arrange
        AuditLogUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();

        // Act
        await setup.UseCase.GetPaginatedAsync(
            null,
            2,
            10,
            "description",
            null,
            null,
            null,
            null,
            AuditLogTestData.DescriptionsMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.VerifyServiceCalledWith(
            2,
            10,
            "description",
            false);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenFiltersAreSupplied_BuildsCollectionKeyFromNormalisedInputs()
    {
        // Arrange
        AuditLogUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();
        Guid entityId = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF");

        // Act
        await setup.UseCase.GetPaginatedAsync(
            entityId,
            2,
            10,
            "description",
            true,
            "Created",
            AuditLogTestData.FromDate,
            AuditLogTestData.ToDate,
            AuditLogTestData.DescriptionsMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.CacheProvider.Verify(
            c => c.GetPaginatedCollectionAsync(
                "collection_auditlog:getpaginatedasync_6f9619ff-8b86-d011-b42d-00c04fc964ff_2_10_description_true_created_2026-01-01t00:00:00.0000000z_2026-02-01t00:00:00.0000000z",
                true,
                It.IsAny<Func<Task<PaginatedResult<string[]>>>>(),
                TimeSpan.FromMinutes(30)),
            Times.Once);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenOptionalInputsAreNull_BuildsCollectionKeyWithDefaultsAndEmptySegments()
    {
        // Arrange
        AuditLogUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();

        // Act
        await setup.UseCase.GetPaginatedAsync(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            AuditLogTestData.DescriptionsMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.CacheProvider.Verify(
            c => c.GetPaginatedCollectionAsync(
                "collection_auditlog:getpaginatedasync__1_20_createddate_false___",
                true,
                It.IsAny<Func<Task<PaginatedResult<string[]>>>>(),
                TimeSpan.FromMinutes(30)),
            Times.Once);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenCacheMisses_PassesFiltersAndMapperToService()
    {
        // Arrange
        AuditLogUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();
        Guid entityId = Guid.NewGuid();
        PaginatedResult<string[]> serviceResult = PaginatedResult<string[]>.Success(
            ["Note created successfully"],
            "List retrieved",
            1,
            20,
            1,
            1);
        setup.AuditLogService
            .Setup(s => s.GetPaginatedAsync(
                entityId,
                1,
                20,
                "createdDate",
                false,
                "Created",
                AuditLogTestData.FromDate,
                AuditLogTestData.ToDate,
                AuditLogTestData.DescriptionsMapper,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        PaginatedResult<string[]> result = await setup.UseCase.GetPaginatedAsync(
            entityId,
            1,
            20,
            "createdDate",
            false,
            "Created",
            AuditLogTestData.FromDate,
            AuditLogTestData.ToDate,
            AuditLogTestData.DescriptionsMapper,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(serviceResult);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenCacheHits_ReturnsCachedResultWithoutCallingService()
    {
        // Arrange
        AuditLogUseCaseTestSetup setup = new();
        PaginatedResult<string[]> cachedResult = PaginatedResult<string[]>.Success(
            ["Cached"],
            "List retrieved",
            1,
            20,
            1,
            1);
        setup.CacheProvider
            .Setup(c => c.GetPaginatedCollectionAsync(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<Func<Task<PaginatedResult<string[]>>>>(),
                It.IsAny<TimeSpan>()))
            .ReturnsAsync(cachedResult);

        // Act
        PaginatedResult<string[]> result = await setup.UseCase.GetPaginatedAsync(
            null,
            1,
            20,
            "createdDate",
            false,
            null,
            null,
            null,
            AuditLogTestData.DescriptionsMapper,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(cachedResult);
        setup.AuditLogService.Verify(
            s => s.GetPaginatedAsync(
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<Func<AuditLog[], string[]>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
