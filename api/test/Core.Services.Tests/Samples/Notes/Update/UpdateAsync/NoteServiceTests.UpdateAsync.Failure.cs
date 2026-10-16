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
    public async Task UpdateAsync_WhenNoteNotFound_ReturnsNotFoundError()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        NoteUpdateModel model = CreateNoteUpdateModel(title: "Updated title");
        GivenNoteNotFound(id);

        // Act
        Result result = await _sut.UpdateAsync(id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.NotFound);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task UpdateAsync_WhenNoteIsDeleted_ReturnsConflictError()
    {
        // Arrange
        Note note = CreateDeletedNote();
        NoteUpdateModel model = CreateNoteUpdateModel(title: "Updated title");
        GivenNoteFound(note);

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.Conflict);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidData_ReturnsValidationError()
    {
        // Arrange
        Note note = CreateValidNote();
        NoteUpdateModel model = CreateNoteUpdateModel(title: string.Empty);
        GivenNoteFound(note);

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task UpdateAsync_WhenSecondFieldIsInvalid_WritesNoAuditLog()
    {
        // Arrange
        Note note = CreateValidNote();
        NoteUpdateModel model = CreateNoteUpdateModel(
            title: "Updated title",
            content: CreateStringOfLength(ContentMaxLength + 1));
        GivenNoteFound(note);

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task UpdateAsync_WhenRepositoryUpdateFails_ReturnsRepositoryFailure()
    {
        // Arrange
        Note note = CreateValidNote();
        NoteUpdateModel model = CreateNoteUpdateModel(title: "Updated title");
        const string failureMessage = "Failed to stage note.";
        GivenNoteFound(note);
        GivenNoteUpdateFails(failureMessage, HttpStatusCode.InternalServerError);

        // Act
        Result result = await _sut.UpdateAsync(note.Id, model, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be(failureMessage);
        _noteRepositoryMock.Verify(r => r.Update(note), Times.Once);
        VerifyNoAuditOrCacheSideEffects();
    }
}
