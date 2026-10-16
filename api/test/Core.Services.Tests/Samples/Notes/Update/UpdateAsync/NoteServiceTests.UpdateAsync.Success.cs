using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.Samples.Notes;

public sealed partial class NoteServiceTests
{
    [Fact]
    public async Task UpdateAsync_WithValidTitle_ReturnsNoContent()
    {
        // Arrange
        Note note = CreateValidNote();
        NoteUpdateModel model = CreateNoteUpdateModel(title: "Updated title");
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        result.Message.Should().Be("Note updated successfully");
    }

    [Fact]
    public async Task UpdateAsync_WithValidTitle_UpdatesTitle()
    {
        // Arrange
        Note note = CreateValidNote(title: "Original title");
        NoteUpdateModel model = CreateNoteUpdateModel(title: "Updated title");
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        note.Title.Should().Be("Updated title");
    }

    [Fact]
    public async Task UpdateAsync_WithValidContent_UpdatesContent()
    {
        // Arrange
        Note note = CreateValidNote(content: "Original content");
        NoteUpdateModel model = CreateNoteUpdateModel(content: "Updated content");
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        note.Content.Should().Be("Updated content");
    }

    [Fact]
    public async Task UpdateAsync_WithValidPinState_UpdatesIsPinned()
    {
        // Arrange
        Note note = CreateValidNote(isPinned: false);
        NoteUpdateModel model = CreateNoteUpdateModel(isPinned: true);
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        note.IsPinned.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WithSingleField_LeavesOtherFieldsUnchanged()
    {
        // Arrange
        Note note = CreateValidNote(title: "Original title", content: "Original content", isPinned: true);
        NoteUpdateModel model = CreateNoteUpdateModel(title: "Updated title");
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        note.Content.Should().Be("Original content");
        note.IsPinned.Should().BeTrue();
        note.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_StagesEntityOnRepository()
    {
        // Arrange
        Note note = CreateValidNote();
        NoteUpdateModel model = CreateNoteUpdateModel(title: "Updated title");
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        _noteRepositoryMock.Verify(r => r.Update(note), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithSeveralFields_WritesOneAuditLogPerField()
    {
        // Arrange
        Note note = CreateValidNote(isPinned: false);
        NoteUpdateModel model = CreateNoteUpdateModel(
            title: "Updated title",
            content: "Updated content",
            isPinned: true);
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        VerifyAuditLogWritten(note.Id, "Note title updated successfully");
        VerifyAuditLogWritten(note.Id, "Note content updated successfully");
        VerifyAuditLogWritten(note.Id, "Note pin state updated successfully");
        VerifyAuditLogCount(3);
    }

    [Fact]
    public async Task UpdateAsync_WithSeveralFields_StagesEntityOnce()
    {
        // Arrange
        Note note = CreateValidNote(isPinned: false);
        NoteUpdateModel model = CreateNoteUpdateModel(
            title: "Updated title",
            content: "Updated content",
            isPinned: true);
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        _noteRepositoryMock.Verify(r => r.Update(note), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_InvalidatesCache()
    {
        // Arrange
        Note note = CreateValidNote();
        NoteUpdateModel model = CreateNoteUpdateModel(title: "Updated title");
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        VerifyCacheInvalidated(note.Id);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyModel_ReturnsNoContentWithoutSideEffects()
    {
        // Arrange
        Note note = CreateValidNote();
        NoteUpdateModel model = CreateNoteUpdateModel();
        GivenNoteFound(note);

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        result.Message.Should().Be("No changes were applied to the note");
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyModel_LeavesEntityUnchanged()
    {
        // Arrange
        Note note = CreateValidNote(title: "Original title", content: "Original content", isPinned: true);
        NoteUpdateModel model = CreateNoteUpdateModel();
        GivenNoteFound(note);

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        note.Title.Should().Be("Original title");
        note.Content.Should().Be("Original content");
        note.IsPinned.Should().BeTrue();
        note.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WhenAuditLogWriteFails_StillReturnsNoContent()
    {
        // Arrange
        Note note = CreateValidNote();
        NoteUpdateModel model = CreateNoteUpdateModel(title: "Updated title");
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteFails();

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        VerifyWarningLogged();
    }
}
