using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace App.UseCases.Tests.Samples.Notes;

public sealed class NoteUseCaseGetByIdAsyncTests
{
    [Fact]
    public async Task GetByIdAsync_WhenCalled_ReadsThroughCacheWithSingleKeyAndThirtyMinuteExpiry()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenSingleCacheMiss();
        Guid id = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF");

        // Act
        await setup.UseCase.GetByIdAsync(
            id,
            NoteTestData.TitleMapper,
            TestContext.Current.CancellationToken);

        // Assert
        setup.CacheProvider.Verify(
            c => c.GetSingleAsync(
                It.IsAny<Func<Task<Result<string>>>>(),
                true,
                "note:id(6f9619ff-8b86-d011-b42d-00c04fc964ff)",
                TimeSpan.FromMinutes(30)),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheMisses_CallsServiceWithIdAndMapper()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenSingleCacheMiss();
        Guid id = Guid.NewGuid();
        Result<string> serviceResult = Result<string>.Success(
            "Note retrieved",
            "Shopping list",
            HttpStatusCode.OK);
        setup.NoteService
            .Setup(s => s.GetByIdAsync(
                id,
                NoteTestData.TitleMapper,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        Result<string> result = await setup.UseCase.GetByIdAsync(
            id,
            NoteTestData.TitleMapper,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(serviceResult);
        setup.NoteService.Verify(
            s => s.GetByIdAsync(
                id,
                NoteTestData.TitleMapper,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheHits_ReturnsCachedResultWithoutCallingService()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        Guid id = Guid.NewGuid();
        Result<string> cachedResult = Result<string>.Success(
            "Note retrieved",
            "Cached title",
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
            NoteTestData.TitleMapper,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(cachedResult);
        setup.NoteService.Verify(
            s => s.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Func<Note, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
