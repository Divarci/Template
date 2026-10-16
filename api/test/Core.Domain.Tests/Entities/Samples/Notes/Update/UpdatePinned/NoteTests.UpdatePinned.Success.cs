using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed partial class NoteTests
{
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void UpdatePinned_WithNewPinState_ReturnsSuccessAndChangesIsPinned(bool initialIsPinned, bool newIsPinned)
    {
        // Arrange
        Note note = CreateValidNote(isPinned: initialIsPinned);

        // Act
        Result result = note.UpdatePinned(newIsPinned);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Message.Should().Be("Note pin state updated successfully");
        note.IsPinned.Should().Be(newIsPinned);
    }

    [Fact]
    public void UpdatePinned_WithNewPinState_DoesNotChangeOtherProperties()
    {
        // Arrange
        Note note = CreateValidNote(title: "Original title", content: "Original content", isPinned: false);
        Guid originalId = note.Id;

        // Act
        Result result = note.UpdatePinned(true);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        note.Id.Should().Be(originalId);
        note.Title.Should().Be("Original title");
        note.Content.Should().Be("Original content");
        note.IsDeleted.Should().BeFalse();
    }
}
