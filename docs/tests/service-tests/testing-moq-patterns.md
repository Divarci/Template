# Moq Patterns — Service Tests

Mocks are created with `new Mock<T>()` in the base partial file. Mock **only** interfaces — `Core.Domain` `Interfaces/`, `Core.Library/Contracts/` — and `ILogger<T>`; never entities, models, `Result`, or the service itself.

## Setup — returning values

```csharp
// Asynchronous return
_noteRepositoryMock
    .Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result<Note>.Success(
        message: "Note retrieved successfully",
        data: note,
        statusCode: HttpStatusCode.OK));

// Synchronous return (staging)
_noteRepositoryMock
    .Setup(r => r.Update(It.IsAny<Note>()))
    .Returns(Result.Success(
        message: "Note staged",
        statusCode: HttpStatusCode.OK));

// Return built from the arguments (echo the staged entity back)
_noteRepositoryMock
    .Setup(r => r.CreateAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((Note note, CancellationToken _) => Result<Note>.Success(
        message: "Note staged",
        data: note,
        statusCode: HttpStatusCode.Created));

// Failure return
_noteRepositoryMock
    .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result<Note>.Failure(
        message: "Note not found.",
        statusCode: HttpStatusCode.NotFound));
```

A method returning a plain `Task` (`InvalidateEntityAsync`, `InvalidateCollectionsAsync`) needs no setup — Moq returns a completed task. Any method returning `Task<Result…>` or `Result` that the scenario reaches **must** be set up; an unset call returns `null` and the service fails with a `NullReferenceException` instead of the scenario under test.

## Verify — method calls

```csharp
// Called once
_noteRepositoryMock.Verify(
    r => r.CreateAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()),
    Times.Once);

// Never called
_noteRepositoryMock.Verify(
    r => r.Update(It.IsAny<Note>()),
    Times.Never);

// Called with specific arguments
_noteRepositoryMock.Verify(
    r => r.CreateAsync(
        It.Is<Note>(n => n.Title == "Shopping list" && !n.IsDeleted),
        It.IsAny<CancellationToken>()),
    Times.Once);

// Audit entry carrying the domain message
_auditLogRepositoryMock.Verify(
    r => r.CreateAsync(
        It.Is<AuditLog>(a => a.EntityId == note.Id && a.Description == "Note title updated successfully"),
        It.IsAny<CancellationToken>()),
    Times.Once);

// Exact cache key — compute it outside the lambda: an expression tree
// cannot contain a tuple literal (CS8143), so CacheKeyHelper.Single(…, ("id", …))
// cannot be written inside Verify(...)
string cacheKey = CacheKeyHelper.Single(nameof(Note), ("id", note.Id));

_cacheInvalidationServiceMock.Verify(
    c => c.InvalidateEntityAsync(cacheKey),
    Times.Once);
```

## Verify — mapper delegates and pass-through parameters

Read operations take a mapper (`Func<{Entity}, TDto>` / `Func<{Entity}[], TDto[]>`). Pass a real lambda from the test and assert on its output — no mock is needed for a delegate.

```csharp
Func<Note[], string[]> mapper = notes => [.. notes.Select(n => n.Title)];

_noteRepositoryMock.Verify(
    r => r.GetPaginatedAsync(2, 10, "title", true, "milk", true, It.IsAny<CancellationToken>()),
    Times.Once);
```

## Verify — logging

`ILogger<T>.LogWarning(...)` is an extension method; verify the underlying `Log` call:

```csharp
_loggerMock.Verify(
    l => l.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception?>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    Times.Once);
```
