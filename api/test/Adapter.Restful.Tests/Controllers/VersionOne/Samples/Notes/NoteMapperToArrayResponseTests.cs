using Adapter.Restful.Controllers.VersionOne.Samples.Notes;
using Adapter.Restful.Controllers.VersionOne.Samples.Notes.Models.Responses;
using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Xunit;

namespace Adapter.Restful.Tests.Controllers.VersionOne.Samples.Notes;

public sealed class NoteMapperToArrayResponseTests
{
    [Fact]
    public void ToArrayResponse_WithNotes_MapsEachNoteInOrder()
    {
        // Arrange
        Note first = NoteTestData.CreateNote(title: "First");
        Note second = NoteTestData.CreateNote(title: "Second");

        // Act
        NoteResponse[] responses = NoteMapper.ToArrayResponse([first, second]);

        // Assert
        responses.Select(r => r.Id).Should().Equal(first.Id, second.Id);
        responses.Select(r => r.Title).Should().Equal("First", "Second");
    }

    [Fact]
    public void ToArrayResponse_WithNoNotes_ReturnsEmptyArray()
    {
        // Arrange
        Note[] notes = [];

        // Act
        NoteResponse[] responses = NoteMapper.ToArrayResponse(notes);

        // Assert
        responses.Should().BeEmpty();
    }
}
