using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed partial class NoteTests
{
    [Fact]
    public void UpdateContent_WithValidContent_ReturnsSuccessAndUpdatesContent()
    {
        // Arrange
        Note note = CreateValidNote(content: "Original content");
        const string newContent = "Updated content";

        // Act
        Result result = note.UpdateContent(newContent);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Message.Should().Be("Note content updated successfully");
        note.Content.Should().Be(newContent);
    }

    [Fact]
    public void UpdateContent_WithValidContent_DoesNotChangeOtherProperties()
    {
        // Arrange
        Note note = CreateValidNote(title: "Original title", isPinned: true);
        Guid originalId = note.Id;

        // Act
        Result result = note.UpdateContent("Updated content");

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        note.Id.Should().Be(originalId);
        note.Title.Should().Be("Original title");
        note.IsPinned.Should().BeTrue();
        note.IsDeleted.Should().BeFalse();
    }
}
