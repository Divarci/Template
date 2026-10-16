# Project Structure — Domain Tests

Domain tests follow **vertical-slice** organisation: one **partial test class** per subject, one **folder per method under test**, and inside it exactly one **Success** and one **Failure** file. The test tree mirrors the production tree, so a test is found by the path of the code it covers.

## Complete example — entity

### Production code
```
api/src/core/Core.Domain/
└── Entities/
    └── <Context>/
        └── <Aggregate>/
            ├── {Entity}.cs                        # state + behaviour (Create, Update{Field}, SoftDelete, …)
            ├── {Entity}.Validate.cs               # private validation — tested through the public methods
            ├── {Entity}.{Concern}.cs              # (optional) further behaviour partial, grouped by business concern
            ├── DomainEvents/
            │   └── {Entity}CreatedDomainEvent.cs
            ├── Interfaces/
            └── Models/
                ├── {Entity}CreateModel.cs
                └── {Entity}UpdateModel.cs
```

### Test code
```
api/test/Core.Domain.Tests/
└── Entities/
    └── <Context>/
        └── <Aggregate>/                                        # same path as the entity
            ├── {Entity}Tests.cs                                # base partial: helpers + constants only
            ├── Create/                                         # from {Entity}.cs
            │   ├── {Entity}Tests.Create.Success.cs
            │   └── {Entity}Tests.Create.Failure.cs
            ├── Update/                                         # from {Entity}.cs — one folder per update method
            │   ├── Update{Field1}/
            │   │   ├── {Entity}Tests.Update{Field1}.Success.cs
            │   │   └── {Entity}Tests.Update{Field1}.Failure.cs
            │   └── Update{Field2}/
            │       ├── {Entity}Tests.Update{Field2}.Success.cs
            │       └── {Entity}Tests.Update{Field2}.Failure.cs
            ├── SoftDelete/                                     # from {Entity}.cs
            │   ├── {Entity}Tests.SoftDelete.Success.cs
            │   └── {Entity}Tests.SoftDelete.Failure.cs
            ├── {Method}/                                       # any other behaviour method (e.g. MarkProcessed)
            │   ├── {Entity}Tests.{Method}.Success.cs
            │   └── {Entity}Tests.{Method}.Failure.cs
            └── {Concern}/                                      # from {Entity}.{Concern}.cs (partial), when present
                ├── Add{Item}/
                │   ├── {Entity}Tests.Add{Item}.Success.cs
                │   └── {Entity}Tests.Add{Item}.Failure.cs
                └── Remove{Item}/
                    ├── {Entity}Tests.Remove{Item}.Success.cs
                    └── {Entity}Tests.Remove{Item}.Failure.cs
```

Concrete example — `Note` (`Core.Domain/Entities/Samples/Notes/Note.cs`):

```
api/test/Core.Domain.Tests/Entities/Samples/Notes/
├── NoteTests.cs
├── Create/            NoteTests.Create.Success.cs · NoteTests.Create.Failure.cs
├── Update/
│   ├── UpdateTitle/   NoteTests.UpdateTitle.Success.cs · NoteTests.UpdateTitle.Failure.cs
│   ├── UpdateContent/ NoteTests.UpdateContent.Success.cs · NoteTests.UpdateContent.Failure.cs
│   └── UpdatePinned/  NoteTests.UpdatePinned.Success.cs · NoteTests.UpdatePinned.Failure.cs
└── SoftDelete/        NoteTests.SoftDelete.Success.cs · NoteTests.SoftDelete.Failure.cs
```

## Complete example — value object

### Production code
```
api/src/core/Core.Domain/
└── ValueObjects/
    └── <ValueObject>ValueObject/
        ├── {ValueObject}.cs
        ├── {ValueObject}.Validation.cs
        └── {ValueObject}Model.cs
```

### Test code
```
api/test/Core.Domain.Tests/
└── ValueObjects/
    └── <ValueObject>ValueObject/
        ├── {ValueObject}Tests.cs                          # base partial: helpers + constants
        └── Create/
            ├── {ValueObject}Tests.Create.Success.cs
            └── {ValueObject}Tests.Create.Failure.cs
```

## Complete example — validator and result pattern (`Core.Library.Tests`)

```
api/test/Core.Library.Tests/
├── Validators/                                            # mirrors Core.Library/Validators/
│   └── {Type}Validator/                                   # e.g. StringValidator, GuidValidator, DateTimeValidator
│       ├── {Type}ValidatorTests.cs                        # base partial: helpers + constants
│       └── Validate/
│           ├── {Type}ValidatorTests.Validate.Success.cs
│           └── {Type}ValidatorTests.Validate.Failure.cs
└── ResultPattern/                                         # mirrors Core.Library/ResultPattern/
    └── {Type}/                                            # Result, ResultOfT, PaginatedResultOfT, ResultExtensions, ErrorDetails, Metadata
        ├── {Type}Tests.cs                                 # base partial: helpers + constants
        └── {Method}/                                      # e.g. Success, Failure, CombineValidationErrors, MapTo
            ├── {Type}Tests.{Method}.Success.cs
            └── {Type}Tests.{Method}.Failure.cs
```

A generic type uses the `OfT` file-name suffix in its folder and class name (`ResultOfT/ResultOfTTests.cs`, class `ResultOfTTests`), matching the production file name (`docs/api/coding-standards.md` — One Top-Level Type Per File). Overloads of one method share its folder and files. A constructor under test uses `Constructor` as its folder and method-name prefix (`Constructor/MetadataTests.Constructor.Failure.cs`, `Constructor_WithNonPositivePageSize_ThrowsTemplateException`).

**Name clash.** When a subject folder has the same name as the type under test (`ResultPattern/Result/`, `ResultPattern/ErrorDetails/`, `ResultPattern/Metadata/`), the folder's namespace segment shadows the type inside the test files. Each such file declares a type alias directly after its file-scoped `namespace` line (`using Result = Core.Library.ResultPattern.Result;`) — the genuine-collision case `docs/api/coding-standards.md` — Namespace Imports allows.

## When to use business-concern folders

A **business-concern folder** (`{Concern}/`) is used when the entity itself is split into a behaviour partial by concern (`{Entity}.{Concern}.cs`):

1. ✅ The entity has such a partial.
2. ✅ The methods in it are cohesively related.
3. ✅ The grouping improves navigation.

```
Entity partial class               →  Test folder
───────────────────────────────────────────────────────────────
{Entity}.cs (main)                 →  {Entity}/Create/, Update/Update{Field}/, SoftDelete/, …
{Entity}.{Concern}.cs (partial)    →  {Entity}/{Concern}/Add{Item}/, {Concern}/Remove{Item}/, …
{Entity}.Validate.cs               →  (no folder — private; covered through the public methods)
```

## Join-table entities

A **join-table entity** (`{EntityA}{EntityB}`) is a standalone entity: it gets its own test folder at the same level as any other entity of its context — never nested under one of the joined entities — and follows the same structure (Create, Update, SoftDelete, Restore, …) for whatever public methods it has. A simple join table may have only `Create/`.

## Structure rules

**CRITICAL:** one structure only — the one above.

1. **Base partial file** (`{Subject}Tests.cs`) holds helper methods and constants only — no tests.
2. **Operation files** (`*.Success.cs`, `*.Failure.cs`) hold only `[Fact]` / `[Theory]` methods — no fields, helpers, or constants.
3. **Method-level folders** — every public method under test gets its own folder.
4. **Only `Success.cs` and `Failure.cs`.** Never create `Additional.cs`, `PropertyVerification.cs`, or any other name. A method with no reachable failure path (for example, a behaviour method whose only outcome on a valid entity is success) has only its `Success.cs`; every other method has both.
5. **Pure tests** — no mocks in entity, value-object, validator, or result-pattern tests.
6. **One partial type, one namespace.** Every file of a test class declares `public sealed partial class {Subject}Tests` in the **base file's** namespace — the namespace mirrors the base file's folder (`Core.Domain.Tests.Entities.Samples.Notes`), not the `Create/` / `Update/` sub-folders, because all fragments are the same type.

**Why only Success.cs and Failure.cs?**
- **Consistency** — every operation follows the same pattern.
- **Clarity** — it is immediately clear where a test lives.
- **Maintenance and review** — predictable files are easier to locate, update, and review.
- **No ambiguity** — extra file names blur where a test belongs.

**Need more organisation?** Order the tests within the file and separate groups with a blank line. `#region` blocks and decorative comment banners are not used (`docs/api/coding-standards.md` — XML Documentation). Do **not** add further partial files.

---

## Domain architecture background

### Why partial classes?
The production entities are partial classes split by purpose (`{Entity}.cs` for state and behaviour, `{Entity}.Validate.cs` for validation, and optionally `{Entity}.{Concern}.cs` per business concern — `docs/api/domain-driven-design.md`). The test class mirrors that split with one partial file per method and outcome, so large subjects stay navigable and parallel work does not collide.

### Vertical slice
Production code is organised by business capability (`<Context>/<Aggregate>/`), not by technical type; the test tree uses the same path so related tests sit together and the suite grows with the domain.

### Code style and patterns

All test code follows `docs/api/coding-standards.md` in full — one top-level type per file (partials are one type), no space padding, explicit types (no `var`), one statement per line, no XML documentation — with one addition: every test body is split by the mandatory `// Arrange`, `// Act`, `// Assert` comments (the only comments test code carries).

```csharp
[Fact]
public void Create_WithValidData_ReturnsSuccessResult()
{
    // Arrange
    NoteCreateModel model = CreateValidNoteCreateModel();

    // Act
    Result<Note> result = Note.Create(model);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.StatusCode.Should().Be(HttpStatusCode.Created);
    result.Data.Should().NotBeNull();
    result.Data!.Title.Should().Be(model.Title);
}
```

A test with nothing to arrange omits `// Arrange` and starts at `// Act`.

### Helper methods (base partial file)

The base partial file holds `private static` helpers and `private const` values that every operation file of the class shares:

- builders for valid models and entities — always through the production factory (`Create()`) and behaviour methods, never via reflection or by bypassing validation;
- builders for specific states (`CreateDeletedNote()`, `CreateProcessedOutboxMessage()`);
- `CreateStringOfLength(int length)` and named constants for boundaries (`TitleMaxLength`);
- assertion helpers (`AssertSuccessResult`, `AssertFailureResult`) that remove repeated assertion blocks;
- `TheoryData` sources consumed with `[MemberData(nameof(…))]`.

Builders take optional named parameters whose defaults are valid, unremarkable values.

```csharp
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.ResultPattern;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed partial class NoteTests
{
    private const int TitleMaxLength = 200;
    private const int ContentMaxLength = 5000;

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
}
```

## Structure consistency

**CRITICAL:** every domain test follows the same structure:

1. **One base partial file** per entity / value object / validator / result-pattern type.
2. **Method-level folders** for each public method.
3. **Success.cs and Failure.cs** split per method.
4. **No additional partial files.**
5. **The test tree mirrors the production tree.**
