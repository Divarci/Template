using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed partial class NoteTests
{
    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ReturnsConflictError()
    {
        // Arrange
        Note note = CreateDeletedNote();

        // Act
        Result result = note.SoftDelete();

        // Assert
        AssertFailureResult(result, HttpStatusCode.Conflict);
        note.IsDeleted.Should().BeTrue();
    }
}
