# Test Naming Conventions — Service Tests

## Test classes and files

| Element | Pattern | Example |
|---|---|---|
| Test class (all partial files) | `{Entity}ServiceTests` — `public sealed partial class` | `NoteServiceTests` |
| Base partial file | `{Entity}ServiceTests.cs` | `NoteServiceTests.cs` |
| Write-operation file | `{Entity}ServiceTests.{Method}.Success.cs` / `.Failure.cs` | `NoteServiceTests.UpdateAsync.Failure.cs` |
| Read-operation file | `{Entity}ServiceTests.{Method}.cs` | `NoteServiceTests.GetPaginatedAsync.cs` |
| Namespace | the base file's folder | `Core.Services.Tests.Samples.Notes` |

## Test methods

**Pattern:** `{MethodUnderTest}_{Scenario}_{ExpectedResult}` — the method name is the service method's real name (`CreateAsync`, `GetByIdAsync`, `MarkProcessed`, …).

**Examples:**
```csharp
// Create
CreateAsync_WithValidData_ReturnsCreatedNote()
CreateAsync_WithValidData_InvalidatesCollections()
CreateAsync_WithInvalidData_ReturnsValidationError()

// Single
GetByIdAsync_WithExistingId_ReturnsMappedNote()
GetByIdAsync_WhenNoteNotFound_ReturnsNotFoundError()

// Update
UpdateAsync_WithValidTitle_UpdatesTitle()
UpdateAsync_WithEmptyModel_ReturnsNoContentWithoutSideEffects()
UpdateAsync_WhenNoteNotFound_ReturnsNotFoundError()

// Delete
DeleteAsync_WithExistingNote_MarksNoteAsDeleted()
DeleteAsync_WhenNoteAlreadyDeleted_ReturnsConflictError()

// Collection
GetPaginatedAsync_WithValidParameters_ReturnsMappedPage()
GetPaginatedAsync_WhenCountFails_ReturnsCountFailure()
```

The DO / DON'T rules of [`../domain-tests/testing-naming-conventions.md`](../domain-tests/testing-naming-conventions.md) apply unchanged: descriptive scenarios, the state or condition in the name, a specific outcome, one consistent order, no abbreviations, no "Test" in the name.
