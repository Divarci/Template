using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed partial class NoteTests
{
    [Fact]
    public void UpdateContent_WhenNoteIsDeleted_ReturnsConflictError()
    {
        // Arrange
        Note note = CreateDeletedNote();
        string? originalContent = note.Content;

        // Act
        Result result = note.UpdateContent("New content");

        // Assert
        AssertFailureResult(result, HttpStatusCode.Conflict);
        note.Content.Should().Be(originalContent);
    }

    [Fact]
    public void UpdateContent_WhenNoteIsDeletedAndContentIsTooLong_ReturnsConflictError()
    {
        // Arrange
        Note note = CreateDeletedNote();
        string? originalContent = note.Content;

        // Act
        Result result = note.UpdateContent(CreateStringOfLength(ContentMaxLength + 1));

        // Assert
        AssertFailureResult(result, HttpStatusCode.Conflict);
        note.Content.Should().Be(originalContent);
    }

    [Fact]
    public void UpdateContent_WithTooLongContent_ReturnsValidationError()
    {
        // Arrange
        Note note = CreateValidNote();

        // Act
        Result result = note.UpdateContent(CreateStringOfLength(ContentMaxLength + 1));

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }
}
