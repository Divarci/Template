using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Domain.Entities.System.AuditLogs;
using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Core.Library.Contracts.Caching;
using Core.Library.Helpers;
using Core.Library.ResultPattern;
using Core.Services.Samples.Notes;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace Core.Services.Tests.Samples.Notes;

public sealed partial class NoteServiceTests
{
    private const int ContentMaxLength = 5000;
    private const int Page = 1;
    private const int PageSize = 10;
    private const string SortBy = "title";

    private readonly Mock<INoteRepository> _noteRepositoryMock = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();
    private readonly Mock<ICacheInvalidationService> _cacheInvalidationServiceMock = new();
    private readonly Mock<ILogger<NoteService>> _loggerMock = new();
    private readonly NoteService _sut;

    public NoteServiceTests()
    {
        _sut = new NoteService(
            _noteRepositoryMock.Object,
            _auditLogRepositoryMock.Object,
            _cacheInvalidationServiceMock.Object,
            _loggerMock.Object);
    }

    private static NoteCreateModel CreateValidNoteCreateModel(
        string title = "Test Note",
        string? content = "Test content",
        bool isPinned = false)
    {
        return new NoteCreateModel
        {
            Title = title,
            Content = content,
            IsPinned = isPinned
        };
    }

    private static NoteUpdateModel CreateNoteUpdateModel(
        string? title = null,
        string? content = null,
        bool? isPinned = null)
    {
        return new NoteUpdateModel
        {
            Title = title,
            Content = content,
            IsPinned = isPinned
        };
    }

    private static Note CreateValidNote(
        string title = "Test Note",
        string? content = "Test content",
        bool isPinned = false)
    {
        Result<Note> result = Note.Create(CreateValidNoteCreateModel(title, content, isPinned));

        return result.Data!;
    }

    private static Note CreateDeletedNote()
    {
        Note note = CreateValidNote();

        note.SoftDelete();

        return note;
    }

    private static string CreateStringOfLength(int length)
    {
        return new string('A', length);
    }

    private static string[] MapToTitles(Note[] notes)
    {
        return [.. notes.Select(note => note.Title)];
    }

    private void GivenNoteFound(Note note)
    {
        _noteRepositoryMock
            .Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note>.Success(
                message: "Note retrieved successfully",
                data: note,
                statusCode: HttpStatusCode.OK));
    }

    private void GivenNoteNotFound(Guid id)
    {
        _noteRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note>.Failure(
                message: "Note not found.",
                statusCode: HttpStatusCode.NotFound));
    }

    private void GivenNoteCreateSucceeds()
    {
        _noteRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note note, CancellationToken _) => Result<Note>.Success(
                message: "Note staged",
                data: note,
                statusCode: HttpStatusCode.Created));
    }

    private void GivenNoteCreateFails(string message, HttpStatusCode statusCode)
    {
        _noteRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note>.Failure(
                message: message,
                statusCode: statusCode));
    }

    private void GivenNoteUpdateSucceeds()
    {
        _noteRepositoryMock
            .Setup(r => r.Update(It.IsAny<Note>()))
            .Returns(Result.Success(
                message: "Note staged",
                statusCode: HttpStatusCode.OK));
    }

    private void GivenNoteUpdateFails(string message, HttpStatusCode statusCode)
    {
        _noteRepositoryMock
            .Setup(r => r.Update(It.IsAny<Note>()))
            .Returns(Result.Failure(
                message: message,
                statusCode: statusCode));
    }

    private void GivenNotePageFound(Note[] notes)
    {
        _noteRepositoryMock
            .Setup(r => r.GetPaginatedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note[]>.Success(
                message: "Notes retrieved successfully",
                data: notes,
                statusCode: HttpStatusCode.OK));
    }

    private void GivenNotePageFails(string message, HttpStatusCode statusCode)
    {
        _noteRepositoryMock
            .Setup(r => r.GetPaginatedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note[]>.Failure(
                message: message,
                statusCode: statusCode));
    }

    private void GivenNoteCountFound(int totalCount)
    {
        _noteRepositoryMock
            .Setup(r => r.CountFilteredAsync(
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(
                message: "Notes counted successfully",
                data: totalCount,
                statusCode: HttpStatusCode.OK));
    }

    private void GivenNoteCountFails(string message, HttpStatusCode statusCode)
    {
        _noteRepositoryMock
            .Setup(r => r.CountFilteredAsync(
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Failure(
                message: message,
                statusCode: statusCode));
    }

    private void GivenAuditLogWriteSucceeds()
    {
        _auditLogRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditLog auditLog, CancellationToken _) => Result<AuditLog>.Success(
                message: "Audit log staged",
                data: auditLog,
                statusCode: HttpStatusCode.Created));
    }

    private void GivenAuditLogWriteFails()
    {
        _auditLogRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuditLog>.Failure(
                message: "Failed to stage audit log.",
                statusCode: HttpStatusCode.InternalServerError));
    }

    private void VerifyAuditLogWritten(Guid entityId, string description)
    {
        _auditLogRepositoryMock.Verify(
            r => r.CreateAsync(
                It.Is<AuditLog>(a => a.EntityId == entityId && a.Description == description),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void VerifyAuditLogCount(int expectedCount)
    {
        _auditLogRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()),
            Times.Exactly(expectedCount));
    }

    private void VerifyCacheInvalidated(Guid id)
    {
        string cacheKey = CacheKeyHelper.Single(nameof(Note), ("id", id));

        _cacheInvalidationServiceMock.Verify(c => c.InvalidateEntityAsync(cacheKey), Times.Once);
        _cacheInvalidationServiceMock.Verify(c => c.InvalidateCollectionsAsync(), Times.Once);
    }

    private void VerifyWarningLogged()
    {
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void VerifyNoAuditOrCacheSideEffects()
    {
        _auditLogRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheInvalidationServiceMock.Verify(c => c.InvalidateEntityAsync(It.IsAny<string>()), Times.Never);
        _cacheInvalidationServiceMock.Verify(c => c.InvalidateCollectionsAsync(), Times.Never);
    }

    private void VerifyNoWriteSideEffects()
    {
        _noteRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Never);
        _noteRepositoryMock.Verify(r => r.Update(It.IsAny<Note>()), Times.Never);
        VerifyNoAuditOrCacheSideEffects();
    }

    private static void AssertSuccessResult(Result result, HttpStatusCode expectedStatusCode)
    {
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }

    private static void AssertSuccessResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Data.Should().NotBeNull();
    }

    private static void AssertFailureResult(Result result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }

    private static void AssertFailureResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailureAndNoData.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }
}
