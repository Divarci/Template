# Delete Operation

**CRITICAL:** the scenarios below are the baseline for every service and are implemented unless the service has genuinely different behaviour. When the entity carries further domain logic, add the matching scenarios.

Reference implementation: `NoteService.DeleteAsync` — load by id → `entity.SoftDelete()` → `_repository.Update(entity)` → one audit entry with the domain message → `InvalidateEntityAsync(single key)` + `InvalidateCollectionsAsync()` → `NoContent`.

## Success tests (`{Entity}ServiceTests.DeleteAsync.Success.cs`)

- `WithExisting{Entity}_ReturnsNoContent` — status `HttpStatusCode.NoContent`.
- `WithExisting{Entity}_Marks{Entity}AsDeleted` — `IsDeleted == true`.
- `WithExisting{Entity}_StagesEntityOnRepository` — `_repository.Update(entity)` once (soft delete is an update).
- `WithExisting{Entity}_WritesAuditLog` — one audit entry carrying the soft-delete domain message.
- `WithExisting{Entity}_InvalidatesCache` — `InvalidateEntityAsync(single key)` once and `InvalidateCollectionsAsync` once.

## Failure tests (`{Entity}ServiceTests.DeleteAsync.Failure.cs`)

**CRITICAL:** every failure test verifies the error result **and**, in the same test, that nothing was staged, audited, or invalidated. Never split those verifications.

- `When{Entity}NotFound_ReturnsNotFoundError`.
- `When{Entity}AlreadyDeleted_ReturnsConflictError` — the domain's double-delete guard.
- `WhenRepositoryUpdateFails_ReturnsRepositoryFailure` — no audit entry, no invalidation.

### Domain-specific business-rule failures — **variable by domain logic**

**Analysis required:**
- Read the entity's `SoftDelete()` (or `Delete()`) and the service's delete operation to find **every** rule that can block the delete.
- Each rule = one to three tests, depending on where and how it is checked.
- The status varies (`Conflict`, `NotFound`, `UnprocessableEntity`, …); the test name states the blocking condition.

## Understanding delete behaviour

**Soft delete vs hard delete**
- **Soft delete** — sets `IsDeleted = true` through the domain and stages it with `Update()`.
- **Hard delete** — a join/relationship entity without `IHaveSoftDelete` is removed with the repository's `Delete()`; verify that call instead of `Update()`.

**Where the business rule sits**
- **In the service** — checked before the entity is touched; the repository is never called on failure.
- **In the entity** — the entity method runs and refuses; nothing is staged.

**Recommendation**
1. Read the entity's delete method and the service's delete operation.
2. Find whether each rule is checked before or during deletion.
3. Add only the conditional tests that apply.

## Complete failure test example

```csharp
[Fact]
public async Task DeleteAsync_WhenNoteNotFound_ReturnsNotFoundError()
{
    // Arrange
    Guid id = Guid.NewGuid();
    GivenNoteNotFound(id);

    // Act
    Result result = await _sut.DeleteAsync(id, TestContext.Current.CancellationToken);

    // Assert
    AssertFailureResult(result, HttpStatusCode.NotFound);
    VerifyNoWriteSideEffects();
}
```

**Remember:** the `Times.Never` verifications prove the service short-circuits on errors. They always live in the same test as the error assertion.
