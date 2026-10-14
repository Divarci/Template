using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed class NoteUpdatePinnedTests
{
    [Fact]
    public void UpdatePinned_WhenNoteIsActive_ReturnsOk()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();

        // Act
        Result result = note.UpdatePinned(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("Note pin state updated successfully");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdatePinned_WhenNoteIsActive_SetsPinState(bool isPinned)
    {
        // Arrange
        Note note = NoteTestData.CreateNote(isPinned: !isPinned);

        // Act
        note.UpdatePinned(isPinned);

        // Assert
        note.IsPinned.Should().Be(isPinned);
    }

    [Fact]
    public void UpdatePinned_WhenNoteIsDeleted_ReturnsConflict()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();
        note.SoftDelete();

        // Act
        Result result = note.UpdatePinned(true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.Message.Should().Be("Note is already deleted.");
    }

    [Fact]
    public void UpdatePinned_WhenNoteIsDeleted_LeavesPinStateUnchanged()
    {
        // Arrange
        Note note = NoteTestData.CreateNote(isPinned: false);
        note.SoftDelete();

        // Act
        note.UpdatePinned(true);

        // Assert
        note.IsPinned.Should().BeFalse();
    }
}
