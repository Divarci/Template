using Adapter.Restful.Controllers.VersionOne.Samples.Notes;
using Adapter.Restful.Controllers.VersionOne.Samples.Notes.Models.Requests;
using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes.Models;
using Xunit;

namespace Adapter.Restful.Tests.Controllers.VersionOne.Samples.Notes;

public sealed class NoteMapperToUpdateModelTests
{
    [Fact]
    public void ToUpdateModel_WithFullRequest_MapsEveryField()
    {
        // Arrange
        UpdateNoteRequest request = new()
        {
            Title = "New title",
            Content = "New content",
            IsPinned = true,
        };

        // Act
        NoteUpdateModel model = NoteMapper.ToUpdateModel(request);

        // Assert
        model.Title.Should().Be("New title");
        model.Content.Should().Be("New content");
        model.IsPinned.Should().BeTrue();
    }

    [Fact]
    public void ToUpdateModel_WithEmptyRequest_KeepsEveryFieldNull()
    {
        // Arrange
        UpdateNoteRequest request = new();

        // Act
        NoteUpdateModel model = NoteMapper.ToUpdateModel(request);

        // Assert
        model.Title.Should().BeNull();
        model.Content.Should().BeNull();
        model.IsPinned.Should().BeNull();
    }
}
