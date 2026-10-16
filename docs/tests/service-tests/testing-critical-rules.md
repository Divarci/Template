# Testing Critical Rules — Service Tests

## ✅ DO

1. **Always verify cache invalidation** (`docs/api/coding-standards.md` — Entity Service Structure):
   - **Create success** → `_cacheInvalidationServiceMock.Verify(c => c.InvalidateCollectionsAsync(), Times.Once)`.
   - **Update / delete success** → also `InvalidateEntityAsync` once with the entity's single key — computed into a local first (`string cacheKey = CacheKeyHelper.Single(nameof({Entity}), ("id", {entity}.Id));` then `Verify(c => c.InvalidateEntityAsync(cacheKey), Times.Once)`), because an expression tree cannot contain the tuple literal (`testing-moq-patterns.md`).
   - **Every failure** → all invalidation calls `Times.Never`.
   - **Reads (Single, Collection)** → all invalidation calls `Times.Never`.

2. **Always verify repository calls** — the staging call on success (`CreateAsync` / `Update`, `Times.Once`, with the right entity), and `Times.Never` on every failure.

3. **Always verify audit-log entries** — on success one `IAuditLogRepository.CreateAsync` per mutation (one per changed field on update), whose `Description` is the domain result's `Message` and whose `EntityId` is the entity's id; on failure `Times.Never`. A service without an audit log (`OutboxMessageService` — `docs/api/hard-rules.md` §4) has nothing to verify here.

4. **Always test entity state changes:**
   - After create: the returned entity's properties.
   - After update: the updated properties — and that unsupplied fields kept their values.
   - After delete: `IsDeleted == true`.

5. **Always assert status codes:**
   - Create → `HttpStatusCode.Created`.
   - Update / delete / other mutations → `HttpStatusCode.NoContent`.
   - Single / collection → `HttpStatusCode.OK` (the status the repository returned for a single read).
   - Failures → the status the domain or repository produced (`NotFound`, `Conflict`, `UnprocessableEntity`, …) — the service passes it through.

6. **Use assertion and verification helpers** from the base partial file to avoid duplication.

7. **Use `[Theory]` with `[InlineData]`** for several values of one scenario.

8. **Always make one test for domain validation** per write operation — status code only: `HttpStatusCode.UnprocessableEntity`. The rules themselves are covered by the domain tests.

9. **Critical: always resolve the domain business logic first:**
   - Read the entity, its `Validate` partial, the service's partial files, and the interfaces involved.
   - Extract the business rules and the order of checks, and create the helper methods they need.
   - Be able to build a helper for every entity and state the service handles.

10. **Always test that an audit-log failure is non-fatal** for at least one mutation: the repository returns a failure for the audit entry, the operation still succeeds, and a warning is logged.

## ❌ DON'T

1. **Don't skip cache-invalidation verification** — it is critical for cache coherence.
2. **Don't test domain logic** — it belongs to the domain tests.
3. **Don't use magic numbers** — use named constants or values the test arranges itself.
4. **Don't test several concerns per test** — one behaviour per test.
5. **Don't forget failure scenarios.**
6. **Don't use verbose `because` arguments** unless the assertion is genuinely unclear.
7. **Never skip reading and resolving the domain business** — it is what makes the helpers correct.
8. **Never create separate tests only to verify the absence of side effects on failure** (`When{Condition}_DoesNotCallRepository`, `When{Condition}_DoesNotInvalidateCache`). Every failure test verifies the error result **and** all `Times.Never` side effects together. When the failing step is the staging call itself (`WhenRepositoryCreateFails…`, `WhenRepositoryUpdateFails…`), that call is verified `Times.Once` and every side effect after it `Times.Never`.
9. **Don't expect a commit** — services never call the unit of work (`docs/api/hard-rules.md` §5); there is no `SaveChangesAsync` or transaction to verify.
10. **Don't test paging normalisation in the service** — the use case normalises page, page size, and sort before the service runs.
11. **Don't mock entities, models, or `Result`** — mock only interfaces (`Core.Domain` `Interfaces/`, `Core.Library/Contracts/`) and `ILogger<T>`.
