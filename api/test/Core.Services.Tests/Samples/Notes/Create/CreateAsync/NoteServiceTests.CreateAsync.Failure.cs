using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.Exceptions;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.Samples.Notes;

public sealed partial class NoteServiceTests
{
    [Fact]
    public async Task CreateAsync_WithInvalidData_ReturnsValidationError()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel(title: string.Empty);

        // Act
        Result<Note> result = await _sut.CreateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task CreateAsync_WhenRepositoryCreateFails_ReturnsRepositoryFailure()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel();
        const string failureMessage = "Failed to stage note.";
        GivenNoteCreateFails(failureMessage, HttpStatusCode.InternalServerError);

        // Act
        Result<Note> result = await _sut.CreateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be(failureMessage);
        _noteRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Once);
        VerifyNoAuditOrCacheSideEffects();
    }

    [Fact]
    public async Task CreateAsync_WithNullModel_ThrowsTemplateException()
    {
        // Arrange
        NoteCreateModel model = null!;

        // Act
        Func<Task> act = () => _sut.CreateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<TemplateException>();
        VerifyNoWriteSideEffects();
    }
}
