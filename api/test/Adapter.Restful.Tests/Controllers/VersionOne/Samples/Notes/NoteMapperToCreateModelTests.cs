using Adapter.Restful.Controllers.VersionOne.Samples.Notes;
using Adapter.Restful.Controllers.VersionOne.Samples.Notes.Models.Requests;
using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes.Models;
using Xunit;

namespace Adapter.Restful.Tests.Controllers.VersionOne.Samples.Notes;

public sealed class NoteMapperToCreateModelTests
{
    [Fact]
    public void ToCreateModel_WithFullRequest_MapsEveryField()
    {
        // Arrange
        CreateNoteRequest request = new()
        {
            Title = "Shopping list",
            Content = "Milk, eggs",
            IsPinned = true,
        };

        // Act
        NoteCreateModel model = NoteMapper.ToCreateModel(request);

        // Assert
        model.Title.Should().Be("Shopping list");
        model.Content.Should().Be("Milk, eggs");
        model.IsPinned.Should().BeTrue();
    }

    [Fact]
    public void ToCreateModel_WithoutContent_MapsContentAsNull()
    {
        // Arrange
        CreateNoteRequest request = new()
        {
            Title = "Shopping list",
            Content = null,
            IsPinned = false,
        };

        // Act
        NoteCreateModel model = NoteMapper.ToCreateModel(request);

        // Assert
        model.Content.Should().BeNull();
        model.IsPinned.Should().BeFalse();
    }
}
