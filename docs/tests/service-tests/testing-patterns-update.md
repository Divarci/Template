# Update Operation

**CRITICAL:** the scenarios below are the baseline for every service and are implemented unless the service has genuinely different behaviour. When the entity carries further domain logic, add the matching scenarios.

Reference implementation: `NoteService.UpdateAsync` — load by id → apply each **supplied** (non-null) field through its behaviour method, collecting one domain message per field → **no field supplied: return `NoContent` immediately, with no side effect** → otherwise `_repository.Update(entity)` once → one audit entry per collected message → `InvalidateEntityAsync(single key)` + `InvalidateCollectionsAsync()` → `NoContent` (`docs/api/coding-standards.md` — Entity Service Structure, partial update).

## Success tests (`{Entity}ServiceTests.UpdateAsync.Success.cs`)

- `WithValid{Field}_ReturnsNoContent` — status `HttpStatusCode.NoContent`.
- `WithValid{Field}_Updates{Field}` — the entity's property changed; one test per updatable field.
- `WithSingleField_LeavesOtherFieldsUnchanged` — unsupplied fields keep their values.
- `WithValidData_StagesEntityOnRepository` — `_repository.Update(entity)` once.
- `WithSeveralFields_WritesOneAuditLogPerField` — one audit entry per supplied field, each described by that field's domain message.
- `WithValidData_InvalidatesCache` — `InvalidateEntityAsync(CacheKeyHelper.Single(nameof({Entity}), ("id", id)))` once **and** `InvalidateCollectionsAsync` once.
- `WithEmptyModel_ReturnsNoContentWithoutSideEffects` — every field `null`: the result is `NoContent`, and nothing is staged, audited, or invalidated (`VerifyNoWriteSideEffects()`).
  - This deliberately differs from "touch the cache anyway": our convention returns before any side effect when nothing was supplied.
- `WithEmptyModel_LeavesEntityUnchanged` — the entity's properties keep their values.

## Failure tests (`{Entity}ServiceTests.UpdateAsync.Failure.cs`)

**CRITICAL:** every failure test verifies the error result **and**, in the same test, that nothing was staged, audited, or invalidated. Never split those verifications.

- `When{Entity}NotFound_ReturnsNotFoundError` — the repository's `NotFound` is returned.
- `When{Entity}IsDeleted_ReturnsConflictError` — the domain's state guard (`Conflict`) is returned.
- `WithInvalidData_ReturnsValidationError` — one domain-validation scenario; `HttpStatusCode.UnprocessableEntity`.
- `WhenSecondFieldIsInvalid_WritesNoAuditLog` — a valid first field followed by an invalid one returns the validation error and leaves no audit trace for the first field (audit entries are written only after every field passed).
- `WhenRepositoryUpdateFails_ReturnsRepositoryFailure` — the staging failure is returned; no audit entry, no invalidation.

## Understanding update behaviour

**Partial updates**
- Update operations allow partial updates; a `null` field means "not supplied — do not change".
- The entity keeps its existing value for every unsupplied field.

**Property verification**
- After calling the service, verify the entity's state directly — the test holds the same instance the repository mock returned.
- Don't check only the return value; a service can succeed while the domain did not change anything.

**Recommendation**
1. One success test per updatable field.
2. Verify both the returned result and the entity state.
3. Test the empty model to prove there are no unintended changes or side effects.

## Complete failure test example

```csharp
[Fact]
public async Task UpdateAsync_WhenNoteNotFound_ReturnsNotFoundError()
{
    // Arrange
    Guid id = Guid.NewGuid();
    NoteUpdateModel model = CreateNoteUpdateModel(title: "Updated title");
    GivenNoteNotFound(id);

    // Act
    Result result = await _sut.UpdateAsync(id, model, TestContext.Current.CancellationToken);

    // Assert
    AssertFailureResult(result, HttpStatusCode.NotFound);
    VerifyNoWriteSideEffects();
}
```

**Remember:** the `Times.Never` verifications prove the service short-circuits on errors. They always live in the same test as the error assertion.
