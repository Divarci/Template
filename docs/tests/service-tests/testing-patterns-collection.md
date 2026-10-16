# Collection Operations

**CRITICAL:** the scenarios below are the baseline for every service and are implemented unless the service has genuinely different behaviour. When the entity carries further domain logic, add the matching scenarios.

## Pagination — SINGLE FILE

Reference implementation: `NoteService.GetPaginatedAsync<TDto>(page, pageSize, sortBy, isAscending, keyword, isPinned, mapper)` — repository page → repository filtered count → `PaginatedResult` with the mapped page and its metadata. The parameters arrive **already normalised by the use case** (page ≥ 1, a positive page size, a real sort column), so the service passes them through unchanged.

All tests live in one file: `Collection/GetPaginatedAsync/{Entity}ServiceTests.GetPaginatedAsync.cs`.

**Scenarios**
- `WithValidParameters_ReturnsSuccessResult` — succeeds with `HttpStatusCode.OK`.
- `WithValidParameters_ReturnsMappedPage` — the data is the mapper's output for the repository page.
- `WithValidParameters_ReturnsPaginationMetadata` — `PageNumber`, `PageSize`, `TotalCount` (the count result), and `PageCount` = `ceiling(totalCount / pageSize)`.
- `WithFilters_PassesParametersToRepository` — `GetPaginatedAsync` receives page, page size, sort, direction, and every filter exactly as given; `CountFilteredAsync` receives the same filters.
- `WithEmptyResult_ReturnsSuccessWithEmptyArray` — an empty page succeeds with an empty array and `TotalCount` 0.
- `WhenRepositoryFails_ReturnsRepositoryFailure` — the page query's failure is returned; the count is never queried.
- `WhenCountFails_ReturnsCountFailure` — the count query's failure is returned.
- `DoesNotInvalidateCache` — ⚠️ CRITICAL: no invalidation, no audit entry, no staging.

Page-number and page-size defaulting (`null`, `0`, negative values) is **not** a service scenario — the use case normalises those values before calling the service.

## Other collection reads — SINGLE FILE

A collection read without paging (e.g. `OutboxMessageService.GetUnprocessedMessagesAsync(batchSize)`) gets one file in its own method folder under `Collection/`, with:
- `WithValidParameters_ReturnsSuccessResult` — the repository's result is returned.
- `WithValidParameters_CallsRepositoryWithParameters` — parameters passed through unchanged.
- One test per guard the service itself applies (e.g. a non-positive batch size returns a failure and never reaches the repository).
- `WithEmptyResult_ReturnsSuccessWithEmptyCollection`.
- `DoesNotInvalidateCache` / no staging, when the service holds such collaborators.
