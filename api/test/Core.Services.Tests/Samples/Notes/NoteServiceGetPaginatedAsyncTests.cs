using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.Samples.Notes;

public sealed class NoteServiceGetPaginatedAsyncTests
{
    [Fact]
    public async Task GetPaginatedAsync_WhenRepositorySucceeds_ReturnsMappedPage()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        setup.NoteRepository
            .Setup(r => r.GetPaginatedAsync(
                2,
                10,
                "title",
                true,
                "shop",
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note[]>.Success(
                message: "Notes retrieved",
                data: [NoteTestData.CreateNote(title: "Shopping list"), NoteTestData.CreateNote(title: "Shop opening hours")],
                statusCode: HttpStatusCode.OK));
        setup.NoteRepository
            .Setup(r => r.CountFilteredAsync(
                "shop",
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(
                message: "Counted",
                data: 25,
                statusCode: HttpStatusCode.OK));

        // Act
        PaginatedResult<string[]> result = await setup.Service.GetPaginatedAsync<string>(
            2,
            10,
            "title",
            true,
            "shop",
            true,
            notes => [.. notes.Select(n => n.Title)],
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().Equal("Shopping list", "Shop opening hours");
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenRepositorySucceeds_ReturnsPaginationMetadata()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        setup.NoteRepository
            .Setup(r => r.GetPaginatedAsync(
                2,
                10,
                "title",
                true,
                "shop",
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note[]>.Success(
                message: "Notes retrieved",
                data: [NoteTestData.CreateNote(title: "Shopping list")],
                statusCode: HttpStatusCode.OK));
        setup.NoteRepository
            .Setup(r => r.CountFilteredAsync(
                "shop",
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(
                message: "Counted",
                data: 25,
                statusCode: HttpStatusCode.OK));

        // Act
        PaginatedResult<string[]> result = await setup.Service.GetPaginatedAsync<string>(
            2,
            10,
            "title",
            true,
            "shop",
            true,
            notes => [.. notes.Select(n => n.Title)],
            TestContext.Current.CancellationToken);

        // Assert
        result.Metadata.Should().NotBeNull();
        result.Metadata!.PageNumber.Should().Be(2);
        result.Metadata.PageSize.Should().Be(10);
        result.Metadata.TotalCount.Should().Be(25);
        result.Metadata.PageCount.Should().Be(3);
        result.Metadata.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenPageRetrievalFails_PropagatesFailureWithoutCounting()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        setup.NoteRepository
            .Setup(r => r.GetPaginatedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note[]>.Failure(
                message: "Query failed",
                statusCode: HttpStatusCode.InternalServerError));

        // Act
        PaginatedResult<string[]> result = await setup.Service.GetPaginatedAsync<string>(
            1,
            20,
            "title",
            false,
            null,
            null,
            notes => [.. notes.Select(n => n.Title)],
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Be("Query failed");
        setup.NoteRepository.Verify(
            r => r.CountFilteredAsync(
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenCountFails_PropagatesFailure()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        setup.NoteRepository
            .Setup(r => r.GetPaginatedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note[]>.Success(
                message: "Notes retrieved",
                data: [],
                statusCode: HttpStatusCode.OK));
        setup.NoteRepository
            .Setup(r => r.CountFilteredAsync(
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Failure(
                message: "Count failed",
                statusCode: HttpStatusCode.InternalServerError));

        // Act
        PaginatedResult<string[]> result = await setup.Service.GetPaginatedAsync<string>(
            1,
            20,
            "title",
            false,
            null,
            null,
            notes => [.. notes.Select(n => n.Title)],
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Be("Count failed");
    }
}
