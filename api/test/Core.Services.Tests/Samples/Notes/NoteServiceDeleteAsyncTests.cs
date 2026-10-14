using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.Samples.Notes;

public sealed class NoteServiceDeleteAsyncTests
{
    [Fact]
    public async Task DeleteAsync_WhenNoteExists_ReturnsNoContent()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenExistingNote(note);

        // Act
        Result result = await setup.Service.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result.Message.Should().Be("Note deleted successfully");
    }

    [Fact]
    public async Task DeleteAsync_WhenNoteExists_SoftDeletesAndStagesNote()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenExistingNote(note);

        // Act
        await setup.Service.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        note.IsDeleted.Should().BeTrue();
        setup.NoteRepository.Verify(r => r.Update(note), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNoteExists_WritesAuditLogWithDomainResultMessage()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenExistingNote(note);

        // Act
        await setup.Service.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        setup.AuditLogRepository.Verify(
            r => r.CreateAsync(
                It.Is<AuditLog>(a => a.EntityId == note.Id && a.Description == "Note soft-deleted successfully"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNoteExists_InvalidatesSingleEntityAndCollections()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenExistingNote(note);
        string expectedKey = $"note:id({note.Id})";

        // Act
        await setup.Service.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        setup.CacheInvalidationService.Verify(c => c.InvalidateEntityAsync(expectedKey), Times.Once);
        setup.CacheInvalidationService.Verify(c => c.InvalidateCollectionsAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNoteIsNotFound_PropagatesNotFound()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Guid missingId = Guid.NewGuid();
        setup.GivenNoteMissing(missingId, "Note not found.");

        // Act
        Result result = await setup.Service.DeleteAsync(missingId, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Note not found.");
        setup.NoteRepository.Verify(r => r.Update(It.IsAny<Note>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenNoteIsAlreadyDeleted_ReturnsConflict()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        note.SoftDelete();
        setup.GivenExistingNote(note);

        // Act
        Result result = await setup.Service.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteAsync_WhenNoteIsAlreadyDeleted_DoesNotStageAuditOrInvalidate()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        note.SoftDelete();
        setup.GivenExistingNote(note);

        // Act
        await setup.Service.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        setup.NoteRepository.Verify(r => r.Update(It.IsAny<Note>()), Times.Never);
        setup.AuditLogRepository.Verify(
            r => r.CreateAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()),
            Times.Never);
        setup.CacheInvalidationService.Verify(c => c.InvalidateCollectionsAsync(), Times.Never);
        setup.CacheInvalidationService.Verify(c => c.InvalidateEntityAsync(It.IsAny<string>()), Times.Never);
    }
}
