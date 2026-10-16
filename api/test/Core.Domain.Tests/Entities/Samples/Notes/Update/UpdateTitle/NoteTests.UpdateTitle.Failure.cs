using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed partial class NoteTests
{
    [Fact]
    public void UpdateTitle_WhenNoteIsDeleted_ReturnsConflictError()
    {
        // Arrange
        Note note = CreateDeletedNote();
        string originalTitle = note.Title;

        // Act
        Result result = note.UpdateTitle("New title");

        // Assert
        AssertFailureResult(result, HttpStatusCode.Conflict);
        note.Title.Should().Be(originalTitle);
    }

    [Fact]
    public void UpdateTitle_WhenNoteIsDeletedAndTitleIsInvalid_ReturnsConflictError()
    {
        // Arrange
        Note note = CreateDeletedNote();
        string originalTitle = note.Title;

        // Act
        Result result = note.UpdateTitle(string.Empty);

        // Assert
        AssertFailureResult(result, HttpStatusCode.Conflict);
        note.Title.Should().Be(originalTitle);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateTitle_WithNullOrWhitespaceTitle_ReturnsValidationError(string? title)
    {
        // Arrange
        Note note = CreateValidNote();

        // Act
        Result result = note.UpdateTitle(title!);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void UpdateTitle_WithTooLongTitle_ReturnsValidationError()
    {
        // Arrange
        Note note = CreateValidNote();

        // Act
        Result result = note.UpdateTitle(CreateStringOfLength(TitleMaxLength + 1));

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }
}
