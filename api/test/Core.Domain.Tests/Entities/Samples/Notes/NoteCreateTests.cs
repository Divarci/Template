using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.DomainEvents;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed class NoteCreateTests
{
    [Fact]
    public void Create_WhenModelIsValid_ReturnsCreatedNote()
    {
        // Arrange
        NoteCreateModel createModel = NoteTestData.CreateModel();

        // Act
        Result<Note> result = Note.Create(createModel);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Message.Should().Be("Note created successfully");
    }

    [Fact]
    public void Create_WhenModelIsValid_CopiesModelValuesOntoNote()
    {
        // Arrange
        NoteCreateModel createModel = NoteTestData.CreateModel(
            title: "Shopping list",
            content: "Milk, eggs",
            isPinned: true);

        // Act
        Result<Note> result = Note.Create(createModel);

        // Assert
        Note note = result.Data!;
        note.Id.Should().NotBe(Guid.Empty);
        note.Title.Should().Be("Shopping list");
        note.Content.Should().Be("Milk, eggs");
        note.IsPinned.Should().BeTrue();
        note.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_WhenModelIsValid_RaisesNoteCreatedDomainEventWithNoteId()
    {
        // Arrange
        NoteCreateModel createModel = NoteTestData.CreateModel();

        // Act
        Result<Note> result = Note.Create(createModel);

        // Assert
        result.Data!.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<NoteCreatedDomainEvent>()
            .Which.NoteId.Should().Be(result.Data.Id);
    }

    [Fact]
    public void Create_WhenContentIsNull_ReturnsCreatedNote()
    {
        // Arrange
        NoteCreateModel createModel = NoteTestData.CreateModel(content: null);

        // Act
        Result<Note> result = Note.Create(createModel);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Content.Should().BeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(200)]
    public void Create_WhenTitleLengthIsWithinLimit_ReturnsCreatedNote(int length)
    {
        // Arrange
        NoteCreateModel createModel = NoteTestData.CreateModel(title: new string('a', length));

        // Act
        Result<Note> result = Note.Create(createModel);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenTitleIsEmptyOrWhitespace_ReturnsUnprocessableEntity(string title)
    {
        // Arrange
        NoteCreateModel createModel = NoteTestData.CreateModel(title: title);

        // Act
        Result<Note> result = Note.Create(createModel);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.Message.Should().Be("Validation failed");
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Title cannot be null or empty.");
    }

    [Fact]
    public void Create_WhenTitleExceedsMaximumLength_ReturnsUnprocessableEntity()
    {
        // Arrange
        NoteCreateModel createModel = NoteTestData.CreateModel(title: new string('a', 201));

        // Act
        Result<Note> result = Note.Create(createModel);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Title must be 200 characters or less.");
    }

    [Fact]
    public void Create_WhenContentIsAtMaximumLength_ReturnsCreatedNote()
    {
        // Arrange
        NoteCreateModel createModel = NoteTestData.CreateModel(content: new string('a', 5000));

        // Act
        Result<Note> result = Note.Create(createModel);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WhenContentExceedsMaximumLength_ReturnsUnprocessableEntity()
    {
        // Arrange
        NoteCreateModel createModel = NoteTestData.CreateModel(content: new string('a', 5001));

        // Act
        Result<Note> result = Note.Create(createModel);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Content must be 5000 characters or less.");
    }

    [Fact]
    public void Create_WhenSeveralFieldsAreInvalid_ReturnsEveryError()
    {
        // Arrange
        NoteCreateModel createModel = NoteTestData.CreateModel(title: string.Empty, content: new string('a', 5001));

        // Act
        Result<Note> result = Note.Create(createModel);

        // Assert
        result.ErrorDetails!.Errors.Should().BeEquivalentTo(
            "Title cannot be null or empty.",
            "Content must be 5000 characters or less.");
    }

    [Fact]
    public void Create_WhenModelIsInvalid_ReturnsNoNote()
    {
        // Arrange
        NoteCreateModel createModel = NoteTestData.CreateModel(title: string.Empty);

        // Act
        Result<Note> result = Note.Create(createModel);

        // Assert
        result.Data.Should().BeNull();
        result.IsFailureAndNoData.Should().BeTrue();
    }

    [Fact]
    public void Create_WhenModelIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        NoteCreateModel createModel = null!;
        Action act = () => Note.Create(createModel);

        // Act & Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
