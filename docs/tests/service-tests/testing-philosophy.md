# Testing Philosophy — Service Tests

## Core principles

1. **Test service orchestration** — a service coordinates the domain entity, its repository, the audit log, and cache invalidation (`docs/api/coding-standards.md` — Entity Service Structure).
2. **Isolation through mocking** — every collaborator is a mock; no database, cache, or log sink is touched.
3. **Fast execution** — tests complete quickly, with no external services.
4. **Readable** — tests document the service's behaviour and workflow.
5. **Maintainable** — easy to update when service logic changes.
6. **Comprehensive** — success paths, edge cases, validation integration, and error short-circuits.

## What to test at the service layer

- Orchestration logic of every service method.
- **Repository interaction** — the right method is called with the right arguments, exactly when it should be (`Verify … Times.Once` / `Times.Never`).
- **Staging, not persisting** — services stage changes on the repository (`CreateAsync`, `Update`); committing is the use case's job, so no commit is ever expected (`docs/api/hard-rules.md` §5).
- **Audit-log entries** — one entry per successful mutation (one per changed field on update), described by the domain result's `Message`; an audit failure is non-fatal and only logs a warning.
- **Cache invalidation** — create invalidates collections; update and delete invalidate the single-entity key **and** collections; reads and failures invalidate nothing.
- **Domain validation integration** — a domain failure is returned unchanged and short-circuits every side effect.
- **Result pattern and status codes** — `Created` / `NoContent` / `OK`, and the failure status the domain or repository produced.
- **Failure propagation** — a repository failure (not found, count failure) is returned to the caller.

## What NOT to test at the service layer

❌ **Domain rules in depth** — every validation rule is covered by the domain tests; a service test proves the integration with **one** validation scenario.
❌ **Input normalisation** — paging defaults and sort defaults are normalised by the use case (`docs/api/coding-standards.md` — API Surface Conventions), not the service.
❌ **Repositories, the unit of work, the cache, or logging implementations** — they are infrastructure and are mocked.
❌ **Use cases, the outbox processor, adapters** — out of scope for this suite.
