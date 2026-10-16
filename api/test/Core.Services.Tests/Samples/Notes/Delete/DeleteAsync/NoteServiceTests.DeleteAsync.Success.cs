using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.Samples.Notes;

public sealed partial class NoteServiceTests
{
    [Fact]
    public async Task DeleteAsync_WithExistingNote_ReturnsNoContent()
    {
        // Arrange
        Note note = CreateValidNote();
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        result.Message.Should().Be("Note deleted successfully");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingNote_MarksNoteAsDeleted()
    {
        // Arrange
        Note note = CreateValidNote();
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        note.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingNote_StagesEntityOnRepository()
    {
        // Arrange
        Note note = CreateValidNote();
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        _noteRepositoryMock.Verify(r => r.Update(note), Times.Once);
        _noteRepositoryMock.Verify(r => r.Delete(It.IsAny<Note>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingNote_WritesAuditLog()
    {
        // Arrange
        Note note = CreateValidNote();
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        VerifyAuditLogWritten(note.Id, "Note soft-deleted successfully");
        VerifyAuditLogCount(1);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingNote_InvalidatesCache()
    {
        // Arrange
        Note note = CreateValidNote();
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result result = await _sut.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        VerifyCacheInvalidated(note.Id);
    }

    [Fact]
    public async Task DeleteAsync_WhenAuditLogWriteFails_StillReturnsNoContent()
    {
        // Arrange
        Note note = CreateValidNote();
        GivenNoteFound(note);
        GivenNoteUpdateSucceeds();
        GivenAuditLogWriteFails();

        // Act
        Result result = await _sut.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.NoContent);
        VerifyWarningLogged();
    }
}
