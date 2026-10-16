using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed partial class NoteTests
{
    [Fact]
    public void UpdateTitle_WithValidTitle_ReturnsSuccessAndUpdatesTitle()
    {
        // Arrange
        Note note = CreateValidNote(title: "Original title");
        const string newTitle = "Updated title";

        // Act
        Result result = note.UpdateTitle(newTitle);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Message.Should().Be("Note title updated successfully");
        note.Title.Should().Be(newTitle);
    }

    [Fact]
    public void UpdateTitle_WithValidTitle_DoesNotChangeOtherProperties()
    {
        // Arrange
        Note note = CreateValidNote(content: "Original content", isPinned: true);
        Guid originalId = note.Id;

        // Act
        Result result = note.UpdateTitle("Updated title");

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        note.Id.Should().Be(originalId);
        note.Content.Should().Be("Original content");
        note.IsPinned.Should().BeTrue();
        note.IsDeleted.Should().BeFalse();
    }
}
