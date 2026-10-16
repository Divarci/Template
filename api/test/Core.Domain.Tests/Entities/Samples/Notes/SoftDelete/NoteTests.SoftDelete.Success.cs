using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed partial class NoteTests
{
    [Fact]
    public void SoftDelete_WithActiveNote_ReturnsSuccessAndSetsIsDeleted()
    {
        // Arrange
        Note note = CreateValidNote();

        // Act
        Result result = note.SoftDelete();

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Message.Should().Be("Note soft-deleted successfully");
        note.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_WithActiveNote_DoesNotChangeOtherProperties()
    {
        // Arrange
        Note note = CreateValidNote(title: "Original title", content: "Original content", isPinned: true);
        Guid originalId = note.Id;

        // Act
        Result result = note.SoftDelete();

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        note.Id.Should().Be(originalId);
        note.Title.Should().Be("Original title");
        note.Content.Should().Be("Original content");
        note.IsPinned.Should().BeTrue();
    }
}
