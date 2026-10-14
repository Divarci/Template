using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.Samples.Notes;

public sealed class NoteServiceGetByIdAsyncTests
{
    [Fact]
    public async Task GetByIdAsync_WhenNoteExists_ReturnsMappedNote()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote(title: "Shopping list");
        setup.NoteRepository
            .Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note>.Success(
                message: "Note retrieved",
                data: note,
                statusCode: HttpStatusCode.OK));

        // Act
        Result<string> result = await setup.Service.GetByIdAsync(
            note.Id,
            n => n.Title,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("Note retrieved");
        result.Data.Should().Be("Shopping list");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNoteIsNotFound_PropagatesFailureWithoutMapping()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Guid missingId = Guid.NewGuid();
        bool mapperInvoked = false;
        setup.GivenNoteMissing(missingId, "Note not found.");

        // Act
        Result<string> result = await setup.Service.GetByIdAsync(
            missingId,
            n =>
            {
                mapperInvoked = true;
                return n.Title;
            },
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Note not found.");
        result.Data.Should().BeNull();
        mapperInvoked.Should().BeFalse();
    }
}
