using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed class NoteSoftDeleteTests
{
    [Fact]
    public void SoftDelete_WhenNoteIsActive_ReturnsOk()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();

        // Act
        Result result = note.SoftDelete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("Note soft-deleted successfully");
    }

    [Fact]
    public void SoftDelete_WhenNoteIsActive_MarksNoteAsDeleted()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();

        // Act
        note.SoftDelete();

        // Assert
        note.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_WhenNoteIsAlreadyDeleted_ReturnsConflict()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();
        note.SoftDelete();

        // Act
        Result result = note.SoftDelete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.Message.Should().Be("Note is already deleted.");
    }

    [Fact]
    public void SoftDelete_WhenNoteIsAlreadyDeleted_KeepsNoteDeleted()
    {
        // Arrange
        Note note = NoteTestData.CreateNote();
        note.SoftDelete();

        // Act
        note.SoftDelete();

        // Assert
        note.IsDeleted.Should().BeTrue();
    }
}
