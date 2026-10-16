using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.Samples.Notes;

public sealed partial class NoteServiceTests
{
    [Fact]
    public async Task GetPaginatedAsync_WithValidParameters_ReturnsSuccessResult()
    {
        // Arrange
        Note[] notes = [CreateValidNote(title: "First"), CreateValidNote(title: "Second")];
        GivenNotePageFound(notes);
        GivenNoteCountFound(notes.Length);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            Page,
            PageSize,
            SortBy,
            true,
            null,
            null,
            MapToTitles,
            TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Message.Should().Be("List retrieved successfully");
    }

    [Fact]
    public async Task GetPaginatedAsync_WithValidParameters_ReturnsMappedPage()
    {
        // Arrange
        Note[] notes = [CreateValidNote(title: "First"), CreateValidNote(title: "Second")];
        GivenNotePageFound(notes);
        GivenNoteCountFound(notes.Length);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            Page,
            PageSize,
            SortBy,
            true,
            null,
            null,
            MapToTitles,
            TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Data.Should().Equal("First", "Second");
    }

    [Fact]
    public async Task GetPaginatedAsync_WithValidParameters_ReturnsPaginationMetadata()
    {
        // Arrange
        int page = 2;
        int pageSize = 10;
        int totalCount = 25;
        Note[] notes = [CreateValidNote()];
        GivenNotePageFound(notes);
        GivenNoteCountFound(totalCount);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            page,
            pageSize,
            SortBy,
            true,
            null,
            null,
            MapToTitles,
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
        int page = 2;
        int pageSize = 10;
        string sortBy = "isPinned";
        bool isAscending = true;
        string keyword = "milk";
        bool isPinned = true;
        Note[] notes = [CreateValidNote()];
        GivenNotePageFound(notes);
        GivenNoteCountFound(notes.Length);

        // Act
        await _sut.GetPaginatedAsync(
            page,
            pageSize,
            sortBy,
            isAscending,
            keyword,
            isPinned,
            MapToTitles,
            TestContext.Current.CancellationToken);

        // Assert
        _noteRepositoryMock.Verify(
            r => r.GetPaginatedAsync(page, pageSize, sortBy, isAscending, keyword, isPinned, It.IsAny<CancellationToken>()),
            Times.Once);
        _noteRepositoryMock.Verify(
            r => r.CountFilteredAsync(keyword, isPinned, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithEmptyResult_ReturnsSuccessWithEmptyArray()
    {
        // Arrange
        Note[] notes = [];
        GivenNotePageFound(notes);
        GivenNoteCountFound(0);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            Page,
            PageSize,
            SortBy,
            true,
            null,
            null,
            MapToTitles,
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
        const string failureMessage = "Failed to retrieve notes.";
        GivenNotePageFails(failureMessage, HttpStatusCode.InternalServerError);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            Page,
            PageSize,
            SortBy,
            true,
            null,
            null,
            MapToTitles,
            TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be(failureMessage);
        _noteRepositoryMock.Verify(
            r => r.CountFilteredAsync(It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()),
            Times.Never);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenCountFails_ReturnsCountFailure()
    {
        // Arrange
        const string failureMessage = "Failed to count notes.";
        Note[] notes = [CreateValidNote()];
        GivenNotePageFound(notes);
        GivenNoteCountFails(failureMessage, HttpStatusCode.InternalServerError);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            Page,
            PageSize,
            SortBy,
            true,
            null,
            null,
            MapToTitles,
            TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be(failureMessage);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task GetPaginatedAsync_WithValidParameters_DoesNotInvalidateCache()
    {
        // Arrange
        Note[] notes = [CreateValidNote()];
        GivenNotePageFound(notes);
        GivenNoteCountFound(notes.Length);

        // Act
        PaginatedResult<string[]> result = await _sut.GetPaginatedAsync(
            Page,
            PageSize,
            SortBy,
            true,
            null,
            null,
            MapToTitles,
            TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        VerifyNoWriteSideEffects();
    }
}
