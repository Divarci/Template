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
    public async Task DeleteAsync_WhenNoteNotFound_ReturnsNotFoundError()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        GivenNoteNotFound(id);

        // Act
        Result result = await _sut.DeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.NotFound);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task DeleteAsync_WhenNoteAlreadyDeleted_ReturnsConflictError()
    {
        // Arrange
        Note note = CreateDeletedNote();
        GivenNoteFound(note);

        // Act
        Result result = await _sut.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.Conflict);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task DeleteAsync_WhenRepositoryUpdateFails_ReturnsRepositoryFailure()
    {
        // Arrange
        Note note = CreateValidNote();
        const string failureMessage = "Failed to stage note.";
        GivenNoteFound(note);
        GivenNoteUpdateFails(failureMessage, HttpStatusCode.InternalServerError);

        // Act
        Result result = await _sut.DeleteAsync(note.Id, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be(failureMessage);
        _noteRepositoryMock.Verify(r => r.Update(note), Times.Once);
        VerifyNoAuditOrCacheSideEffects();
    }
}
