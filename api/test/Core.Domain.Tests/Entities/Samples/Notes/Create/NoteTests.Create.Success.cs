using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.DomainEvents;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed partial class NoteTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel(
            title: "Shopping list",
            content: "Milk, eggs",
            isPinned: true);

        // Act
        Result<Note> result = Note.Create(model);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.Created);
        result.Message.Should().Be("Note created successfully");
        result.Data!.Id.Should().NotBeEmpty();
        result.Data.Title.Should().Be("Shopping list");
        result.Data.Content.Should().Be("Milk, eggs");
        result.Data.IsPinned.Should().BeTrue();
        result.Data.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_WithValidData_RaisesNoteCreatedDomainEvent()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel();

        // Act
        Result<Note> result = Note.Create(model);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.Created);
        result.Data!.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<NoteCreatedDomainEvent>()
            .Which.NoteId.Should().Be(result.Data.Id);
    }

    [Fact]
    public void Create_CalledTwiceWithSameData_AssignsDistinctIds()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel();

        // Act
        Result<Note> firstResult = Note.Create(model);
        Result<Note> secondResult = Note.Create(model);

        // Assert
        AssertSuccessResult(firstResult, HttpStatusCode.Created);
        AssertSuccessResult(secondResult, HttpStatusCode.Created);
        secondResult.Data!.Id.Should().NotBe(firstResult.Data!.Id);
    }
}
