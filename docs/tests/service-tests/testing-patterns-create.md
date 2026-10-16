# Create Operation

**CRITICAL:** the scenarios below are the baseline for every service and are implemented unless the service has genuinely different behaviour. When the entity carries further domain logic, add the matching scenarios.

Reference implementation: `NoteService.CreateAsync` — `{Entity}.Create(model)` → `_repository.CreateAsync(entity)` → one audit entry with the domain message → `InvalidateCollectionsAsync()` → the domain result (`Created`).

## Success tests — typical scenarios (`{Entity}ServiceTests.CreateAsync.Success.cs`)

- `WithValidData_ReturnsCreatedEntity` — result succeeds with `HttpStatusCode.Created`; the returned entity carries the model's values.
- `WithValidData_StagesEntityOnRepository` — `_repository.CreateAsync` called once with the created entity.
- `WithValidData_WritesAuditLog` — one audit entry, `EntityId` = the entity's id, `Description` = the domain result's `Message`.
- `WithValidData_InvalidatesCollections` — `InvalidateCollectionsAsync` once; `InvalidateEntityAsync` never (a new entity has no cached single entry).
- `WhenAuditLogWriteFails_StillReturnsCreatedEntity` — the audit write is non-fatal: success is returned and a warning is logged.

## Failure tests — typical scenarios (`{Entity}ServiceTests.CreateAsync.Failure.cs`)

**CRITICAL:** every failure test verifies the error result **and**, in the same test, that nothing was staged, audited, or invalidated (`VerifyNoWriteSideEffects()`). Never split those verifications into separate tests.

- `WithInvalidData_ReturnsValidationError` — one domain-validation scenario; status `HttpStatusCode.UnprocessableEntity`.
  - **Note:** if the entity checks state **before** validating, the first failing check decides the status (e.g. `Conflict`). Study the entity and assert accordingly.
- `WhenRepositoryCreateFails_ReturnsRepositoryFailure` — the repository's failure is returned; no audit entry, no invalidation.
- `WithNullModel_ThrowsTemplateException` — the domain guard surfaces through the service unchanged.

## Understanding domain complexity

**CRITICAL:** domain entities may carry business rules that change the scenarios.

### Common domain checks (in order of execution)
1. **State checks** — `IsDeleted`, processed / unprocessed, … → typically `Conflict`; they run **before** validation.
2. **Relationship checks** — a parent exists, a limit is reached → `Conflict` or `NotFound`; checked in the service before the entity is mutated (`docs/api/domain-driven-design.md` — Aggregate Decisions).
3. **Validation checks** — required fields, lengths, ranges → `HttpStatusCode.UnprocessableEntity`; they run last.

### Impact on tests
- An invalid input may produce `Conflict` when a state check fails first.
- A domain check may stop the service before the repository is reached even with a valid model.
- The entity's state decides which scenarios exist.

**Recommendation:**
1. Study the entity implementation first.
2. Understand the order of checks.
3. Assert the `HttpStatusCode` the first failing check produces.
4. Add state-specific scenarios where the entity has them.

## Complete failure test example

**CRITICAL RULE:** every failure test is ONE test method that verifies:
1. the correct error result;
2. the repository staging method was NOT called (`Times.Never`);
3. no audit entry was written and no cache entry was invalidated (`Times.Never`).

```csharp
[Fact]
public async Task CreateAsync_WithInvalidData_ReturnsValidationError()
{
    // Arrange
    NoteCreateModel model = CreateValidNoteCreateModel(title: string.Empty);

    // Act
    Result<Note> result = await _sut.CreateAsync(model, TestContext.Current.CancellationToken);

    // Assert
    AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    VerifyNoWriteSideEffects();
}
```

## Complete success test example

```csharp
[Fact]
public async Task CreateAsync_WithValidData_InvalidatesCollections()
{
    // Arrange
    NoteCreateModel model = CreateValidNoteCreateModel();
    GivenNoteCreateSucceeds();
    GivenAuditLogWriteSucceeds();

    // Act
    await _sut.CreateAsync(model, TestContext.Current.CancellationToken);

    // Assert
    _cacheInvalidationServiceMock.Verify(c => c.InvalidateCollectionsAsync(), Times.Once);
    _cacheInvalidationServiceMock.Verify(c => c.InvalidateEntityAsync(It.IsAny<string>()), Times.Never);
}
```

**Remember:** the `Times.Never` verifications prove the service short-circuits on errors and causes no unintended side effects. They always live in the same test as the error assertion.
