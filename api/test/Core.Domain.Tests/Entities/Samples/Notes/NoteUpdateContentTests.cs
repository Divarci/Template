using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed class NoteUpdateContentTests
{
    [Fact]
    public void UpdateContent_WhenContentIsValid_ReturnsOk()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();

        // Act
        Result result = note.UpdateContent("New content");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("Note content updated successfully");
    }

    [Fact]
    public void UpdateContent_WhenContentIsValid_ChangesContent()
    {
        // Arrange
        Note note = NoteTestData.CreateNote(content: "Original content");

        // Act
        note.UpdateContent("New content");

        // Assert
        note.Content.Should().Be("New content");
    }

    [Fact]
    public void UpdateContent_WhenContentIsEmpty_ReturnsOk()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();

        // Act
        Result result = note.UpdateContent(string.Empty);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void UpdateContent_WhenContentIsAtMaximumLength_ReturnsOk()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();

        // Act
        Result result = note.UpdateContent(new string('a', 5000));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void UpdateContent_WhenContentExceedsMaximumLength_ReturnsUnprocessableEntity()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();

        // Act
        Result result = note.UpdateContent(new string('a', 5001));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Content must be 5000 characters or less.");
    }

    [Fact]
    public void UpdateContent_WhenNoteIsDeleted_ReturnsConflict()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();
        note.SoftDelete();

        // Act
        Result result = note.UpdateContent("New content");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.Message.Should().Be("Note is already deleted.");
    }

    [Fact]
    public void UpdateContent_WhenNoteIsDeleted_LeavesContentUnchanged()
    {
        // Arrange
        Note note = NoteTestData.CreateNote(content: "Original content");
        note.SoftDelete();

        // Act
        note.UpdateContent("New content");

        // Assert
        note.Content.Should().Be("Original content");
    }
}
