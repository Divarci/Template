using Adapter.Restful.Controllers.VersionOne.Samples.Notes;
using Adapter.Restful.Controllers.VersionOne.Samples.Notes.Models.Responses;
using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Xunit;

namespace Adapter.Restful.Tests.Controllers.VersionOne.Samples.Notes;

public sealed class NoteMapperToResponseTests
{
    [Fact]
    public void ToResponse_WithNote_MapsEveryField()
    {
        // Arrange
        Note note = NoteTestData.CreateNote(
            title: "Shopping list",
            content: "Milk, eggs",
            isPinned: true);

        // Act
        NoteResponse response = NoteMapper.ToResponse(note);

        // Assert
        response.Id.Should().Be(note.Id);
        response.Title.Should().Be("Shopping list");
        response.Content.Should().Be("Milk, eggs");
        response.IsPinned.Should().BeTrue();
    }
}
