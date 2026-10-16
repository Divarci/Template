# Testing Setup — Service Tests

## Base partial file — mocks, system under test, helpers

The base partial file (`{Entity}ServiceTests.cs`) creates every mock, builds the **system under test** (`_sut`), and holds the helper methods every operation file shares. xUnit creates a new test-class instance per test, so the fields are fresh for every test — no state leaks between tests.

**CRITICAL RULE:** the examples below are the baseline. When the domain has its own business rules, investigate the entity and service **in depth** and add the helpers those rules need (entities in specific states, related data, repository arrangements).

### Example — entity in a specific state
```csharp
private static Note CreateDeletedNote(string title = "Test Note")
{
    Note note = CreateValidNote(title: title);

    note.SoftDelete();

    return note;
}
```

### Example — complete base file (`NoteService`)
```csharp
using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Domain.Entities.System.AuditLogs;
using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Core.Library.Contracts.Caching;
using Core.Library.ResultPattern;
using Core.Services.Samples.Notes;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace Core.Services.Tests.Samples.Notes;

public sealed partial class NoteServiceTests
{
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

    private void GivenNoteUpdateSucceeds()
    {
        _noteRepositoryMock
            .Setup(r => r.Update(It.IsAny<Note>()))
            .Returns(Result.Success(
                message: "Note staged",
                statusCode: HttpStatusCode.OK));
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

    private void VerifyNoWriteSideEffects()
    {
        _noteRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Never);
        _noteRepositoryMock.Verify(r => r.Update(It.IsAny<Note>()), Times.Never);
        _auditLogRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheInvalidationServiceMock.Verify(c => c.InvalidateEntityAsync(It.IsAny<string>()), Times.Never);
        _cacheInvalidationServiceMock.Verify(c => c.InvalidateCollectionsAsync(), Times.Never);
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
```

## Helper groups

| Group | Purpose | Shape |
|---|---|---|
| **Mocks and SUT** | One `Mock<T>` per constructor dependency, plus `_sut` | `private readonly` fields, built in the constructor |
| **Models** | Valid `CreateModel` / `UpdateModel` instances | `private static`, optional named parameters with valid defaults; an `UpdateModel` builder defaults every field to `null` ("not supplied") |
| **Entities** | Valid entities and entities in specific states | `private static`, always through `Create()` and behaviour methods — never reflection; ids come from the created entity (`note.Id`) |
| **Mock arrangements** | Repository results the scenario needs (`GivenNoteFound`, `GivenNoteNotFound`, `GivenNoteCreateSucceeds`, `GivenAuditLogWriteSucceeds`, …) | `private` instance methods named `Given…` |
| **Verifications** | Repeated verification blocks (`VerifyNoWriteSideEffects`) | `private` instance methods named `Verify…` |
| **Assertions** | Repeated result assertions | `private static` methods named `Assert…` |

Rules:
- No XML documentation and no `#region` blocks in helpers (`docs/api/coding-standards.md` — XML Documentation).
- Explicit types; no `var`.
- Every helper is used by at least one test of its class (`docs/api/coding-standards.md` — Unused Code).
- Scenario values the test asserts on are written in the test (or passed to the helper by the test) — a test never asserts a builder default it did not choose.
- No real secrets anywhere (`docs/api/security.md`).
