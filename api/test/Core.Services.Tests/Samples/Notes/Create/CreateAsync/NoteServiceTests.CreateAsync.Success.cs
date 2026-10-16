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
    public async Task CreateAsync_WithValidData_ReturnsCreatedNote()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel(
            title: "Shopping list",
            content: "Milk, eggs",
            isPinned: true);
        GivenNoteCreateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result<Note> result = await _sut.CreateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.Created);
        result.Message.Should().Be("Note created successfully");
        result.Data!.Title.Should().Be("Shopping list");
        result.Data.Content.Should().Be("Milk, eggs");
        result.Data.IsPinned.Should().BeTrue();
        result.Data.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_StagesEntityOnRepository()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel();
        GivenNoteCreateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result<Note> result = await _sut.CreateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.Created);
        Note createdNote = result.Data!;
        _noteRepositoryMock.Verify(
            r => r.CreateAsync(createdNote, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_WritesAuditLog()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel();
        GivenNoteCreateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        Result<Note> result = await _sut.CreateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.Created);
        VerifyAuditLogWritten(result.Data!.Id, "Note created successfully");
        VerifyAuditLogCount(1);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_InvalidatesCollections()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel();
        GivenNoteCreateSucceeds();
        GivenAuditLogWriteSucceeds();

        // Act
        await _sut.CreateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        _cacheInvalidationServiceMock.Verify(c => c.InvalidateCollectionsAsync(), Times.Once);
        _cacheInvalidationServiceMock.Verify(c => c.InvalidateEntityAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenAuditLogWriteFails_StillReturnsCreatedNote()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel();
        GivenNoteCreateSucceeds();
        GivenAuditLogWriteFails();

        // Act
        Result<Note> result = await _sut.CreateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.Created);
        VerifyWarningLogged();
    }
}
