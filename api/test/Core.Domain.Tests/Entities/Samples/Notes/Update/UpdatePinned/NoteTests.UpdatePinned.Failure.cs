using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed partial class NoteTests
{
    [Fact]
    public void UpdatePinned_WhenNoteIsDeleted_ReturnsConflictError()
    {
        // Arrange
        Note note = CreateDeletedNote();
        bool originalIsPinned = note.IsPinned;

        // Act
        Result result = note.UpdatePinned(!originalIsPinned);

        // Assert
        AssertFailureResult(result, HttpStatusCode.Conflict);
        note.IsPinned.Should().Be(originalIsPinned);
    }
}
