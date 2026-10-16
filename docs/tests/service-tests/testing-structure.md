# Project Structure — Service Tests

Service tests follow **operation-folder** organisation: the service's partial files (`{Entity}Service.{Operation}.cs`) each become a folder, and every service method gets its own folder inside it.

## Example structure

### Production code
```
api/src/core/Core.Services/
└── <Context>/
    └── <Aggregate>/
        ├── {Entity}Service.cs                 # ctor + fields
        ├── {Entity}Service.Create.cs          # CreateAsync
        ├── {Entity}Service.Update.cs          # UpdateAsync
        ├── {Entity}Service.Delete.cs          # DeleteAsync
        ├── {Entity}Service.Single.cs          # GetByIdAsync
        ├── {Entity}Service.Collection.cs      # GetPaginatedAsync
        └── {Entity}Service.Private.cs         # private helpers — tested through the public methods
```

### Test code
```
api/test/Core.Services.Tests/
└── <Context>/
    └── <Aggregate>/                                              # same path as the service
        ├── {Entity}ServiceTests.cs                               # base partial: mocks + SUT + helpers
        ├── Create/                                               # from {Entity}Service.Create.cs
        │   ├── CreateAsync/
        │   │   ├── {Entity}ServiceTests.CreateAsync.Success.cs
        │   │   └── {Entity}ServiceTests.CreateAsync.Failure.cs
        │   └── {NestedOperation}/                                # ⚠️ another method in the same partial
        │       ├── {Entity}ServiceTests.{NestedOperation}.Success.cs
        │       └── {Entity}ServiceTests.{NestedOperation}.Failure.cs
        ├── Update/                                               # from {Entity}Service.Update.cs
        │   └── UpdateAsync/
        │       ├── {Entity}ServiceTests.UpdateAsync.Success.cs
        │       └── {Entity}ServiceTests.UpdateAsync.Failure.cs
        ├── Delete/                                               # from {Entity}Service.Delete.cs
        │   └── DeleteAsync/
        │       ├── {Entity}ServiceTests.DeleteAsync.Success.cs
        │       └── {Entity}ServiceTests.DeleteAsync.Failure.cs
        ├── Single/                                               # from {Entity}Service.Single.cs
        │   └── {Entity}ServiceTests.GetByIdAsync.cs              # ⚠️ SINGLE FILE
        └── Collection/                                           # from {Entity}Service.Collection.cs
            └── GetPaginatedAsync/
                └── {Entity}ServiceTests.GetPaginatedAsync.cs     # ⚠️ SINGLE FILE
```

Concrete examples:

```
api/test/Core.Services.Tests/Samples/Notes/
├── NoteServiceTests.cs
├── Create/CreateAsync/        NoteServiceTests.CreateAsync.Success.cs · .Failure.cs
├── Update/UpdateAsync/        NoteServiceTests.UpdateAsync.Success.cs · .Failure.cs
├── Delete/DeleteAsync/        NoteServiceTests.DeleteAsync.Success.cs · .Failure.cs
├── Single/                    NoteServiceTests.GetByIdAsync.cs
└── Collection/GetPaginatedAsync/ NoteServiceTests.GetPaginatedAsync.cs

api/test/Core.Services.Tests/System/OutboxMessages/
├── OutboxMessageServiceTests.cs
├── Update/MarkProcessed/      OutboxMessageServiceTests.MarkProcessed.Success.cs · .Failure.cs
├── Single/                    OutboxMessageServiceTests.GetByIdAsync.cs
└── Collection/GetUnprocessedMessagesAsync/ OutboxMessageServiceTests.GetUnprocessedMessagesAsync.cs
```

## Structure rules

**CRITICAL:** each partial file of a service is a folder; the tests of that partial's methods live inside the folder of the same name (**NESTED OPERATION**).

1. **Write operations (Create, Update, Delete, and any other mutating method):** split into **Success** and **Failure** files.
   - **Only `Success.cs` and `Failure.cs` are allowed.**
   - ❌ **Never** create `Additional.cs`, `PropertyVerification.cs`, or any other name.
   - ✅ Every success test goes in `Success.cs` — property checks, repository calls, audit entries, cache invalidation.
   - ✅ Every failure test goes in `Failure.cs` — not found, conflict, validation error, every error scenario.

2. **Read operations (Single, Collection):** **one file** per method holding all its tests.

3. **Method-level folders** — each service method gets its own folder (Single's one file sits directly in `Single/`).

4. **Base partial file** — all mock setup and helper methods; no tests.

5. **Operation files** hold only `[Fact]` / `[Theory]` methods — no fields, helpers, or constants.

6. **One partial type, one namespace.** Every file declares `public sealed partial class {Entity}ServiceTests` in the **base file's** namespace — it mirrors the base file's folder (`Core.Services.Tests.Samples.Notes`), not the operation sub-folders.

**Why read operations stay in a single file**
- Read tests are simpler — mostly the happy path, a not-found or repository failure, and the no-side-effect check.
- Fewer validation scenarios.
- Easier to review together, less file overhead.

**Why only Success.cs and Failure.cs?**
- **Consistency** — every operation follows the same pattern.
- **Clarity** — it is immediately clear where a test lives.
- **Maintenance and review** — predictable files are easy to locate and review.
- **No ambiguity** — extra file names blur where a test belongs.

**Need more organisation?** Order the tests within the file and separate groups with a blank line. `#region` blocks and decorative comment banners are not used (`docs/api/coding-standards.md` — XML Documentation). Do **not** add further partial files.
