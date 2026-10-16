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
    public async Task GetByIdAsync_WithExistingId_ReturnsSuccessResult()
    {
        // Arrange
        Note note = CreateValidNote();
        GivenNoteFound(note);

        // Act
        Result<string> result = await _sut.GetByIdAsync(note.Id, n => n.Title, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsMappedNote()
    {
        // Arrange
        Note note = CreateValidNote(title: "Shopping list");
        GivenNoteFound(note);

        // Act
        Result<string> result = await _sut.GetByIdAsync(note.Id, n => n.Title, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Data.Should().Be("Shopping list");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_CallsRepositoryWithId()
    {
        // Arrange
        Note note = CreateValidNote();
        GivenNoteFound(note);

        // Act
        await _sut.GetByIdAsync(note.Id, n => n.Title, TestContext.Current.CancellationToken);

        // Assert
        _noteRepositoryMock.Verify(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNoteNotFound_ReturnsNotFoundError()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        bool isMapperInvoked = false;
        Func<Note, string> mapper = n =>
        {
            isMapperInvoked = true;

            return n.Title;
        };
        GivenNoteNotFound(id);

        // Act
        Result<string> result = await _sut.GetByIdAsync(id, mapper, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.NotFound);
        isMapperInvoked.Should().BeFalse();
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_DoesNotInvalidateCache()
    {
        // Arrange
        Note note = CreateValidNote();
        GivenNoteFound(note);

        // Act
        Result<string> result = await _sut.GetByIdAsync(note.Id, n => n.Title, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        VerifyNoWriteSideEffects();
    }
}
