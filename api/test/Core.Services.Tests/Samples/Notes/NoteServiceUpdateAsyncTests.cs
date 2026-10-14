using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.Samples.Notes;

public sealed class NoteServiceUpdateAsyncTests
{
    [Fact]
    public async Task UpdateAsync_WhenAllFieldsSupplied_AppliesEveryField()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote(
            title: "Original title",
            content: "Original content",
            isPinned: false);
        setup.GivenExistingNote(note);
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(
            title: "New title",
            content: "New content",
            isPinned: true);

        // Act
        await setup.Service.UpdateAsync(
            note.Id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        note.Title.Should().Be("New title");
        note.Content.Should().Be("New content");
        note.IsPinned.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WhenUpdateSucceeds_ReturnsNoContent()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenExistingNote(note);
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(title: "New title");

        // Act
        Result result = await setup.Service.UpdateAsync(
            note.Id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result.Message.Should().Be("Note updated successfully");
    }

    [Fact]
    public async Task UpdateAsync_WhenFieldsAreNull_LeavesThemUnchanged()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote(
            title: "Original title",
            content: "Original content",
            isPinned: false);
        setup.GivenExistingNote(note);
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(isPinned: true);

        // Act
        await setup.Service.UpdateAsync(
            note.Id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        note.Title.Should().Be("Original title");
        note.Content.Should().Be("Original content");
        note.IsPinned.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WhenUpdateSucceeds_StagesNoteOnRepository()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenExistingNote(note);
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(title: "New title");

        // Act
        await setup.Service.UpdateAsync(
            note.Id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        setup.NoteRepository.Verify(r => r.Update(note), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenUpdateSucceeds_WritesAuditLogWithResultMessage()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenExistingNote(note);
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(title: "New title");

        // Act
        await setup.Service.UpdateAsync(
            note.Id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        setup.AuditLogRepository.Verify(
            r => r.CreateAsync(
                It.Is<AuditLog>(a => a.EntityId == note.Id && a.Description == "Note updated successfully"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenUpdateSucceeds_InvalidatesSingleEntityAndCollections()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenExistingNote(note);
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(title: "New title");
        string expectedKey = $"note:id({note.Id})";

        // Act
        await setup.Service.UpdateAsync(
            note.Id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        setup.CacheInvalidationService.Verify(c => c.InvalidateEntityAsync(expectedKey), Times.Once);
        setup.CacheInvalidationService.Verify(c => c.InvalidateCollectionsAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNoteIsNotFound_PropagatesNotFound()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Guid missingId = Guid.NewGuid();
        setup.GivenNoteMissing(missingId, "Note not found.");
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(title: "New title");

        // Act
        Result result = await setup.Service.UpdateAsync(
            missingId,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Note not found.");
        setup.NoteRepository.Verify(r => r.Update(It.IsAny<Note>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenTitleIsInvalid_ReturnsUnprocessableEntity()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenExistingNote(note);
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(title: new string('a', 201));

        // Act
        Result result = await setup.Service.UpdateAsync(
            note.Id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateAsync_WhenTitleIsInvalid_DoesNotStageAuditOrInvalidate()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenExistingNote(note);
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(title: new string('a', 201));

        // Act
        await setup.Service.UpdateAsync(
            note.Id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        setup.NoteRepository.Verify(r => r.Update(It.IsAny<Note>()), Times.Never);
        setup.AuditLogRepository.Verify(
            r => r.CreateAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()),
            Times.Never);
        setup.CacheInvalidationService.Verify(c => c.InvalidateCollectionsAsync(), Times.Never);
        setup.CacheInvalidationService.Verify(c => c.InvalidateEntityAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenContentIsInvalid_ReturnsUnprocessableEntity()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenExistingNote(note);
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(content: new string('a', 5001));

        // Act
        Result result = await setup.Service.UpdateAsync(
            note.Id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        setup.NoteRepository.Verify(r => r.Update(It.IsAny<Note>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNoteIsDeleted_ReturnsConflict()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        note.SoftDelete();
        setup.GivenExistingNote(note);
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(title: "New title");

        // Act
        Result result = await setup.Service.UpdateAsync(
            note.Id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        setup.NoteRepository.Verify(r => r.Update(It.IsAny<Note>()), Times.Never);
    }
}
