# Single Query Operation — SINGLE FILE

**CRITICAL:** the scenarios below are the baseline for every service and are implemented unless the service has genuinely different behaviour. When the entity carries further domain logic, add the matching scenarios.

Reference implementation: `NoteService.GetByIdAsync<TDto>(id, mapper)` — load by id → on failure return the repository's failure → on success return the **mapped** entity with the repository's message and status.

All tests live in one file: `Single/{Entity}ServiceTests.GetByIdAsync.cs`.

**Scenarios**
- `WithExistingId_ReturnsSuccessResult` — succeeds with the repository's status (`HttpStatusCode.OK`).
- `WithExistingId_ReturnsMappedEntity` — the data is the mapper's output for the loaded entity (pass a real lambda, e.g. `note => note.Title`).
- `WithExistingId_CallsRepositoryWithId` — `GetByIdAsync(id)` once.
- `When{Entity}NotFound_ReturnsNotFoundError` — the repository's `NotFound` is returned and the mapper is never invoked.
- `DoesNotInvalidateCache` — no `InvalidateEntityAsync`, no `InvalidateCollectionsAsync`, no audit entry, no staging.

Services with a read that returns the entity itself (no mapper — e.g. `OutboxMessageService.GetByIdAsync`) follow the same scenarios with the entity in place of the mapped value.
