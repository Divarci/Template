using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed class NoteUpdateTitleTests
{
    [Fact]
    public void UpdateTitle_WhenTitleIsValid_ReturnsOk()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();

        // Act
        Result result = note.UpdateTitle("New title");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("Note title updated successfully");
    }

    [Fact]
    public void UpdateTitle_WhenTitleIsValid_ChangesTitle()
    {
        // Arrange
        Note note = NoteTestData.CreateNote(title: "Original title");

        // Act
        note.UpdateTitle("New title");

        // Assert
        note.Title.Should().Be("New title");
    }

    [Fact]
    public void UpdateTitle_WhenTitleIsAtMaximumLength_ReturnsOk()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();

        // Act
        Result result = note.UpdateTitle(new string('a', 200));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void UpdateTitle_WhenTitleExceedsMaximumLength_ReturnsUnprocessableEntity()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();

        // Act
        Result result = note.UpdateTitle(new string('a', 201));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Title must be 200 characters or less.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateTitle_WhenTitleIsEmptyOrWhitespace_ReturnsUnprocessableEntity(string title)
    {
        // Arrange
        Note note = NoteTestData.CreateNote();

        // Act
        Result result = note.UpdateTitle(title);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void UpdateTitle_WhenNoteIsDeleted_ReturnsConflict()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();
        note.SoftDelete();

        // Act
        Result result = note.UpdateTitle("New title");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.Message.Should().Be("Note is already deleted.");
    }

    [Fact]
    public void UpdateTitle_WhenNoteIsDeleted_LeavesTitleUnchanged()
    {
        // Arrange
        Note note = NoteTestData.CreateNote(title: "Original title");
        note.SoftDelete();

        // Act
        note.UpdateTitle("New title");

        // Assert
        note.Title.Should().Be("Original title");
    }
}
