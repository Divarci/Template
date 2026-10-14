using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using Moq;
using Xunit;

namespace App.UseCases.Tests.Samples.Notes;

public sealed class NoteUseCaseGetPaginatedAsyncTests
{
    [Theory]
    [InlineData(null, 1)]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(1, 1)]
    [InlineData(4, 4)]
    public async Task GetPaginatedAsync_WhenPageIsSupplied_NormalisesPage(int? pageQuery, int expectedPage)
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();

        // Act
        await setup.UseCase.GetPaginatedAsync(
            pageQuery,
            10,
            "title",
            true,
            null,
            null,
            NoteTestData.TitlesMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.VerifyServiceCalledWith(
            expectedPage,
            10,
            "title",
            true);
    }

    [Theory]
    [InlineData(null, 20)]
    [InlineData(0, 20)]
    [InlineData(-5, 20)]
    [InlineData(50, 50)]
    public async Task GetPaginatedAsync_WhenPageSizeIsSupplied_NormalisesPageSize(int? pageSizeQuery, int expectedPageSize)
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();

        // Act
        await setup.UseCase.GetPaginatedAsync(
            2,
            pageSizeQuery,
            "title",
            true,
            null,
            null,
            NoteTestData.TitlesMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.VerifyServiceCalledWith(
            2,
            expectedPageSize,
            "title",
            true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPaginatedAsync_WhenSortByIsMissing_DefaultsToTitle(string? sortByQuery)
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();

        // Act
        await setup.UseCase.GetPaginatedAsync(
            2,
            10,
            sortByQuery,
            true,
            null,
            null,
            NoteTestData.TitlesMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.VerifyServiceCalledWith(
            2,
            10,
            "title",
            true);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenSortByIsSupplied_KeepsSortBy()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();

        // Act
        await setup.UseCase.GetPaginatedAsync(
            2,
            10,
            "isPinned",
            true,
            null,
            null,
            NoteTestData.TitlesMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.VerifyServiceCalledWith(
            2,
            10,
            "isPinned",
            true);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenSortDirectionIsMissing_DefaultsToDescending()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();

        // Act
        await setup.UseCase.GetPaginatedAsync(
            2,
            10,
            "title",
            null,
            null,
            null,
            NoteTestData.TitlesMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.VerifyServiceCalledWith(
            2,
            10,
            "title",
            false);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenFiltersAreSupplied_BuildsCollectionKeyFromNormalisedInputs()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();

        // Act
        await setup.UseCase.GetPaginatedAsync(
            2,
            10,
            "isPinned",
            true,
            "Shop",
            true,
            NoteTestData.TitlesMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.CacheProvider.Verify(
            c => c.GetPaginatedCollectionAsync(
                "collection_note:getpaginatedasync_2_10_ispinned_true_shop_true",
                true,
                It.IsAny<Func<Task<PaginatedResult<string[]>>>>(),
                TimeSpan.FromMinutes(30)),
            Times.Once);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenOptionalFiltersAreNull_BuildsCollectionKeyWithEmptySegments()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();

        // Act
        await setup.UseCase.GetPaginatedAsync(
            null,
            null,
            "title",
            null,
            null,
            null,
            NoteTestData.TitlesMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.CacheProvider.Verify(
            c => c.GetPaginatedCollectionAsync(
                "collection_note:getpaginatedasync_1_20_title_false__",
                true,
                It.IsAny<Func<Task<PaginatedResult<string[]>>>>(),
                TimeSpan.FromMinutes(30)),
            Times.Once);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenCacheMisses_PassesFiltersAndMapperToService()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenCollectionCacheMiss();
        PaginatedResult<string[]> serviceResult = PaginatedResult<string[]>.Success(
            ["Shopping list"],
            "List retrieved",
            2,
            10,
            11,
            2);
        setup.NoteService
            .Setup(s => s.GetPaginatedAsync(
                2,
                10,
                "title",
                true,
                "Shop",
                false,
                NoteTestData.TitlesMapper,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        PaginatedResult<string[]> result = await setup.UseCase.GetPaginatedAsync(
            2,
            10,
            "title",
            true,
            "Shop",
            false,
            NoteTestData.TitlesMapper,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(serviceResult);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenCacheHits_ReturnsCachedResultWithoutCallingService()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
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
            1,
            20,
            "title",
            false,
            null,
            null,
            NoteTestData.TitlesMapper,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(cachedResult);
        setup.NoteService.Verify(
            s => s.GetPaginatedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<Func<Note[], string[]>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
