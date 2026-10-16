# Entity Testing Patterns

**CRITICAL:** these are example patterns. A real entity may have more methods and rules than shown — every public method gets its tests.

Examples use the sample `Note` entity (`Core.Domain/Entities/Samples/Notes/`); substitute `{Entity}` and its fields for any other entity.

---

## Failure patterns

**Note:** Failure files must also contain the **validation** scenarios of the method — one test per rule (empty, whitespace, too long, invalid range, …). They are not listed one by one here because they depend on the entity; they are mandatory.

### Create — failure
Every entity whose `Create()` takes a model has the null-model test, plus one test per validation rule.

```csharp
[Fact]
public void Create_WithNullModel_ThrowsTemplateException()
{
    // Arrange
    NoteCreateModel model = null!;

    // Act
    Action act = () => Note.Create(model);

    // Assert
    act.Should().Throw<TemplateException>();
}

[Theory]
[InlineData("")]
[InlineData("   ")]
public void Create_WithEmptyTitle_ReturnsValidationError(string title)
{
    // Arrange
    NoteCreateModel model = CreateValidNoteCreateModel(title: title);

    // Act
    Result<Note> result = Note.Create(model);

    // Assert
    AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
}

[Fact]
public void Create_WithTooLongTitle_ReturnsValidationError()
{
    // Arrange
    NoteCreateModel model = CreateValidNoteCreateModel(title: CreateStringOfLength(TitleMaxLength + 1));

    // Act
    Result<Note> result = Note.Create(model);

    // Assert
    AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
}
```

### Update — failure
Every update method of a soft-deletable entity has the deleted-state test, plus one test per validation rule of the field.

```csharp
[Fact]
public void UpdateTitle_WhenNoteIsDeleted_ReturnsConflictError()
{
    // Arrange
    Note note = CreateDeletedNote();
    string originalTitle = note.Title;

    // Act
    Result result = note.UpdateTitle("New title");

    // Assert
    AssertFailureResult(result, HttpStatusCode.Conflict);
    note.Title.Should().Be(originalTitle);
}

[Fact]
public void UpdateTitle_WithTooLongTitle_ReturnsValidationError()
{
    // Arrange
    Note note = CreateValidNote();

    // Act
    Result result = note.UpdateTitle(CreateStringOfLength(TitleMaxLength + 1));

    // Assert
    AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
}
```

### SoftDelete — failure
```csharp
[Fact]
public void SoftDelete_WhenAlreadyDeleted_ReturnsConflictError()
{
    // Arrange
    Note note = CreateDeletedNote();

    // Act
    Result result = note.SoftDelete();

    // Assert
    AssertFailureResult(result, HttpStatusCode.Conflict);
    note.IsDeleted.Should().BeTrue();
}
```

### Entity-specific method — failure
Any other behaviour method gets the failures its own guards produce.

```csharp
[Fact]
public void {Method}_When{Condition}_Returns{Status}Error()
{
    // Arrange
    {Entity} {entity} = Create{State}{Entity}();

    // Act
    Result result = {entity}.{Method}();

    // Assert
    AssertFailureResult(result, HttpStatusCode.{Status});
}
```

---

## Success patterns

**Note:** Success files **never** contain validation scenarios — the Failure file covers the boundaries.

### Create — success
```csharp
[Fact]
public void Create_WithValidData_ReturnsSuccessResult()
{
    // Arrange
    NoteCreateModel model = CreateValidNoteCreateModel(
        title: "Shopping list",
        content: "Milk, eggs",
        isPinned: true);

    // Act
    Result<Note> result = Note.Create(model);

    // Assert
    AssertSuccessResult(result, HttpStatusCode.Created);
    result.Data!.Id.Should().NotBeEmpty();
    result.Data.Title.Should().Be("Shopping list");
    result.Data.Content.Should().Be("Milk, eggs");
    result.Data.IsPinned.Should().BeTrue();
    result.Data.IsDeleted.Should().BeFalse();
}

[Fact]
public void Create_WithValidData_RaisesNoteCreatedDomainEvent()
{
    // Arrange
    NoteCreateModel model = CreateValidNoteCreateModel();

    // Act
    Result<Note> result = Note.Create(model);

    // Assert
    result.Data!.DomainEvents.Should().ContainSingle()
        .Which.Should().BeOfType<NoteCreatedDomainEvent>()
        .Which.NoteId.Should().Be(result.Data.Id);
}
```

### Update — success
```csharp
[Fact]
public void UpdateTitle_WithValidTitle_ReturnsSuccessAndUpdatesTitle()
{
    // Arrange
    Note note = CreateValidNote(title: "Original title");
    const string newTitle = "Updated title";

    // Act
    Result result = note.UpdateTitle(newTitle);

    // Assert
    AssertSuccessResult(result, HttpStatusCode.OK);
    note.Title.Should().Be(newTitle);
}

[Fact]
public void UpdateTitle_WithValidTitle_DoesNotChangeOtherProperties()
{
    // Arrange
    Note note = CreateValidNote(content: "Original content", isPinned: true);

    // Act
    Result result = note.UpdateTitle("Updated title");

    // Assert
    AssertSuccessResult(result, HttpStatusCode.OK);
    note.Content.Should().Be("Original content");
    note.IsPinned.Should().BeTrue();
}
```

### SoftDelete — success
```csharp
[Fact]
public void SoftDelete_WithActiveNote_ReturnsSuccessAndSetsIsDeleted()
{
    // Arrange
    Note note = CreateValidNote();

    // Act
    Result result = note.SoftDelete();

    // Assert
    AssertSuccessResult(result, HttpStatusCode.OK);
    note.IsDeleted.Should().BeTrue();
}

[Fact]
public void SoftDelete_WithActiveNote_DoesNotChangeOtherProperties()
{
    // Arrange
    Note note = CreateValidNote(title: "Original title");
    Guid originalId = note.Id;

    // Act
    Result result = note.SoftDelete();

    // Assert
    AssertSuccessResult(result, HttpStatusCode.OK);
    note.Id.Should().Be(originalId);
    note.Title.Should().Be("Original title");
}
```

### Entity-specific method — success
```csharp
[Fact]
public void MarkProcessed_WithoutError_ReturnsSuccessAndSetsProcessedOnUtc()
{
    // Arrange
    OutboxMessage message = CreateValidOutboxMessage();

    // Act
    Result result = message.MarkProcessed(null);

    // Assert
    AssertSuccessResult(result, HttpStatusCode.OK);
    message.ProcessedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    message.Error.Should().BeNull();
}
```

---

## Assertion helpers (base partial file)

```csharp
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
```

Only the helpers a class actually uses are declared (`docs/api/coding-standards.md` — Unused Code).
