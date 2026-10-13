# Coding Standards

The code-quality and consistency rules used across the `api` solution. All rules are derived from the actual codebase and its conventions; `structure.md` governs where things live, this document governs how they are written.

---

## Naming Conventions

### General

**CRITICAL RULE:** Never use space padding for alignment. All naming is based on the natural length of the identifier, with no extra spaces.

> **ABSOLUTE PROHIBITION — enforced on every `.cs` file without exception:**
>
> Space-padding for column alignment is **strictly forbidden** across the entire codebase.
> Never insert extra spaces to make `=` signs, type names, or parameter names line up in
> columns. The rule applies to:
> - Object initialisers (`Title    = ...` ← forbidden; `Title = ...` ← correct)
> - Primary constructor parameters
> - Field declarations
> - Method parameter lists in both interface and implementation files
> - Local variable declarations
>
> A single space between the identifier and `=` (or between type and name) is the only
> permitted whitespace. Any alignment beyond that is a violation, enforced at code-review
> time.

| Element | Convention | Example |
|---|---|---|
| Namespace | PascalCase, mirrors folder path | `Core.Domain.Entities.<Context>.<Aggregate>` |
| Class / Record | PascalCase | `{Entity}Service`, `{Entity}CreateModel` |
| Interface | `I` prefix + PascalCase | `I{Entity}Repository`, `ICacheProvider` |
| Method | PascalCase | `Create{Entity}Async`, `GetPaginated{Entities}Async` |
| Property | PascalCase | `Id`, `IsDeleted`, `OccurredOnUtc` |
| Private field | `_camelCase` | `_{entity}Repository`, `_cacheInvalidationService` |
| Parameter | camelCase | `createModel`, `cancellationToken` |
| Constant | PascalCase | `PolicyNames.AllRoles`, `ApiVersion.V1` |
| Static readonly | PascalCase | `SerializerOptions.Instance` |
| Generic type parameter | `T` prefix | `TDto`, `TEntity`, `TProperty` |

### Async Methods

All async methods are suffixed with `Async`.

```csharp
Task<Result<{Entity}>> Create{Entity}Async(...)
Task<PaginatedResult<TDto[]>> GetPaginated{Entities}Async<TDto>(...)
```

### File Naming

Partial-class files use the pattern `ClassName.Operation.cs`:

```
{Entity}Service.cs                   // constructor + fields
{Entity}Service.Collection.cs        // GetPaginated...
{Entity}Service.Create.cs            // Create...
{Entity}Service.Update.cs            // Update...
{Entity}Service.Delete.cs            // Delete...
{Entity}Service.Single.cs            // GetById...
```

Entity validation goes into `{Entity}.Validate.cs`:

```
{Entity}.cs
{Entity}.Validate.cs
```

---

## Vertical Alignment Code Writing

Code is written with a deliberate vertical rhythm. Each logical unit occupies its own line; statements are never compressed onto a single line to save space. The goal is consistent top-to-bottom scanability.

### One Statement Per Line

```csharp
// CORRECT
Result<{Entity}> createResult = {Entity}.Create(createModel);

if (createResult.IsFailureAndNoData)
    return createResult;

await _{entity}Repository.AddAsync(createResult.Data!, cancellationToken);

// INCORRECT — multiple concerns on one line
var result = {Entity}.Create(model); if (result.IsFailureAndNoData) return result;
```

### Constructor Parameters — One Per Line

```csharp
// CORRECT
internal sealed partial class {Entity}Service(
    I{Entity}Repository {entity}Repository,
    ICacheInvalidationService cacheInvalidationService) : I{Entity}Service

// INCORRECT
internal sealed partial class {Entity}Service(I{Entity}Repository {entity}Repository, ICacheInvalidationService cacheInvalidationService) : I{Entity}Service
```

### Field Assignments — One Per Line

Each injected dependency assignment occupies its own line with consistent indentation. No horizontal alignment padding — a single space separates the type from the name.

```csharp
// CORRECT
private readonly I{Entity}Repository _{entity}Repository = {entity}Repository;
private readonly ICacheInvalidationService _cacheInvalidationService = cacheInvalidationService;

// INCORRECT — extra spaces added for column alignment
private readonly I{Entity}Repository          _{entity}Repository           = {entity}Repository;
private readonly ICacheInvalidationService _cacheInvalidationService = cacheInvalidationService;
```

The same no-padding rule applies to property declarations, constructor-body assignments, method parameter lists, and local variable declarations in test Arrange blocks.

### Method Chain — One Call Per Line

Fluent chains (LINQ, builder patterns, service registration) break after each method call.

```csharp
// CORRECT
builder.Services
    .RegisterRestfulLayer()
    .RegisterUseCaseLayer()
    .RegisterServiceLayer()
    .RegisterCachingLayer(builder.Configuration)
    .RegisterPersistenceLayer(builder.Configuration);

// INCORRECT
builder.Services.RegisterRestfulLayer().RegisterUseCaseLayer().RegisterServiceLayer();
```

### Conditional Bodies

Single-statement `if` bodies may omit braces but must sit on their own line, indented below the condition. Multi-statement bodies always use braces.

```csharp
// CORRECT — single statement, own line
if (createResult.IsFailureAndNoData)
    return createResult;

// CORRECT — multi-statement, braces required
if (updateModel.Title is not null)
{
    Result titleResult = {entity}.UpdateTitle(updateModel.Title);

    if (titleResult.IsFailure)
        return titleResult;
}
```

### Blank Lines Between Logical Blocks

A single blank line separates distinct logical blocks within a method body (guard clauses, core logic, persistence, return).

```csharp
public async Task<Result<{Entity}>> Create{Entity}Async(
    {Entity}CreateModel createModel,
    CancellationToken cancellationToken)
{
    Result<{Entity}> createResult = {Entity}.Create(createModel);

    if (createResult.IsFailureAndNoData)
        return createResult;

    await _{entity}Repository.AddAsync(createResult.Data!, cancellationToken);

    return createResult;
}
```

---

## Namespace Imports

Always import by namespace (`using Full.Namespace;`) so types can be referenced by their short name. Never use `global::` qualifiers inline.

```csharp
// CORRECT
using Core.Domain.Entities.<Context>.<Aggregate>;
// Usage:
Result<{Entity}> {entity}Result = ...

// INCORRECT — using alias when no naming conflict exists
using {Entity} = Core.Domain.Entities.<Context>.<Aggregate>.{Entity};
```

Use a **type alias** only when two types with the same short name are both needed in one file and a conflict cannot be avoided any other way:

```csharp
// ONLY when a genuine name collision exists in the same file:
using SysTask = global::System.Threading.Tasks.Task;
```

Every `using` directive must reference at least one type that actually appears in the file.

---

## Unused Code

The codebase must contain **zero unused artefacts**. Dead code is a maintenance liability and obscures intent.

| Category | Rule |
|---|---|
| `using` directives | Remove any `using` not referenced in the file. |
| Parameters | Every method/constructor parameter must be used. Discard unused parameters rather than keeping them "for future use". |
| Variables & locals | Every declared local must be read at least once after assignment. |
| Private fields | Every injected or assigned private field must be used in at least one method. |
| Private methods | Every private method must be called from within the class. |
| Classes & interfaces | Every type must be referenced from at least one other type. |
| `.csproj` exclusions | Obsolete source folders are removed via `<Compile Remove="..." />` rather than left as dead files. |

Enforced at code-review time. IDE warnings for unused symbols must be resolved — not suppressed — before a change is committed.

---

## Class Design

### One Top-Level Type Per File

**ABSOLUTE RULE:** every `.cs` file declares **exactly one** top-level type — one class, record, interface, or enum. Multiple unrelated top-level types in one file are forbidden, regardless of size or relationship. The file name matches the type name.

Two clarifications that do not violate this rule:

- **Partial classes** split a single type across multiple files. Each file still declares exactly one type — the same type, a different fragment (`{Entity}.cs` + `{Entity}.Validate.cs`). This is the canonical pattern for entities, services, use cases, and repositories.
- **Nested types** (a class or entity-scoped enum declared inside another class) are allowed; the host class is still the file's single top-level type.

When two related types feel like they "belong together," put them in sibling files inside the same folder — locality through folder structure, not file fusion.

### Access Modifiers

| Type | Modifier |
|---|---|
| Service implementation | `internal sealed partial class` |
| Use case implementation | `internal sealed partial class` |
| Repository implementation | `internal sealed partial class` |
| Domain entity | `public sealed partial class` |
| Value object | `public sealed record` |
| Domain event | `public sealed record` |
| Model (DTO) | `public sealed record` |
| Request / Response (API) | `public record` / `public class` |
| Controller | `public class` |
| Static helper | `internal static class` / `public static class` |
| Domain event handler | `internal sealed class` |
| Exception | `public sealed class` |

### Sealed by Default

All non-abstract implementation classes are `sealed` to prevent unintended inheritance.

### Primary Constructors

Services, use cases, repositories, controllers, and handlers use **primary constructors** with explicit field assignments:

```csharp
internal sealed partial class {Entity}Service(
    I{Entity}Repository {entity}Repository,
    ICacheInvalidationService cacheInvalidationService) : I{Entity}Service
{
    private readonly I{Entity}Repository _{entity}Repository = {entity}Repository;
    private readonly ICacheInvalidationService _cacheInvalidationService = cacheInvalidationService;
}
```

---

## Record Design

Records are the exclusive type for immutable data transfer across layer boundaries and within the domain.

| Use case | Type |
|---|---|
| Value object | `public sealed record` |
| Domain event | `public sealed record` |
| Application model (DTO) | `public sealed record` |
| Configuration binding | `public sealed record` |
| API request model | `public record` |
| API response model | `public record` |

### Positional vs Property Syntax

**Domain models (`{Entity}CreateModel` / `{Entity}UpdateModel`) always use property syntax with `required` on every field** — the two never diverge in shape. Property syntax + `required` gives compile-time mapper safety: every field must be explicitly assigned in the object initialiser, so a mapper can never silently drop one.

- **CreateModel** — every field `required`; nullability matches the field's real optionality (a mandatory field is non-nullable, a genuinely optional one is nullable).
- **UpdateModel** — every field `required` **and** nullable (partial-update: `null` = "not supplied, leave unchanged").

```csharp
public sealed record {Entity}CreateModel
{
    public required string Title { get; init; }
    public required string? Description { get; init; }
    public required bool IsPinned { get; init; }
}

public sealed record {Entity}UpdateModel
{
    public required string? Title { get; init; }
    public required string? Description { get; init; }
    public required bool? IsPinned { get; init; }
}
```

**Positional records** remain the norm for simple immutable payloads that are *not* mapper-built — domain events (`{Entity}CreatedDomainEvent(Guid {Entity}Id) : DomainEvent`), value objects, and configuration-binding records.

**API request models** use property syntax with data-annotation attributes (they are deserialised from client input, not mapper-built):

```csharp
public record Create{Entity}Request
{
    [Required]
    [MaxLength(100)]
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
```

### `required` on every view-model (Response) property

**ABSOLUTE RULE:** every property on a **view model** — the API **response** records returned to the client — is declared with the C# `required` keyword, **whether or not the property is nullable**. `required Guid Id`, `required string? Title`, and `required bool? IsPublished` all carry it.

```csharp
public record {Entity}Response
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("title")]
    public required string? Title { get; init; }

    [JsonPropertyName("description")]
    public required string? Description { get; init; }
}
```

The purpose is **compile-time mapping safety**: `required` forces every property to be explicitly assigned in the object initialiser, so forgetting to map a field when building the response is a **compiler error** rather than a silently dropped value. Nullability is an independent concern — it says whether the value itself may be `null`, not whether the property must be set; it never removes the need for `required`.

**Request models are not affected by this rule** — they keep their existing shape (nullable properties with validation attributes such as `[Required]`/`[MaxLength]` as needed), because they are deserialised from client input rather than constructed by our mappers. `[Required]` (a runtime validation attribute) and `required` (a compile-time C# keyword) are different tools for different jobs.

### Immutability

All record properties use `init`-only setters. Records are never mutated after construction — a new instance is produced instead.

### `sealed` on Domain Records

Domain records (value objects, domain events, models) are always `sealed`. API records (request, response) omit `sealed` to allow test or mapping inheritance where needed.

---

## Entity Service Structure

An entity service is an `internal sealed partial class` split by operation, exactly like the rest of the codebase. Beyond the file-naming rule above, services follow these conventions:

- **`{Entity}Service.cs` holds only the primary constructor and field declarations** — no method bodies. It is the definition file, nothing else.
- **Private helpers live in `{Entity}Service.Private.cs`.** A helper that adds genuine behaviour and is shared across operations — the audit-log writer, for example — is a private member in this partial, never duplicated across operation files and never pushed into a base class (services do not use method-bearing base classes). A method that merely forwards to a single repository call (e.g. wrapping `GetByIdAsync`, which already returns a `Result<T>`) adds nothing: it is a forbidden pass-through — each operation calls the repository directly.
- **Guard `Result<T>` reads with `IsFailureAndNoData`, not `IsFailure`.** After an early return on `IsFailureAndNoData`, `.Data` is known to be present, so downstream code uses it directly — never `?? fallback` and never the `!` null-forgiving operator on result data.
- **The audit-log description is the domain operation's `Result.Message`.** The service passes the message returned by the entity's `Create()`/behaviour/`SoftDelete()` result straight into the audit-log entry — it does **not** hand-write a separate description string. (See `domain-driven-design.md` — domain result messages are the audit trail.)
- **Partial update = skip null fields.** An update operation applies each `UpdateModel` field only when it is non-null — `if (updateModel.X is not null) { Result r = entity.UpdateX(updateModel.X); if (r.IsFailure) return r; }` — then stages the entity via the repository. A null field means "not supplied — leave unchanged".
- **Per-operation cache invalidation.** Create invalidates collections only (`InvalidateCollections()`); update and delete invalidate **both** the single-entity key (`InvalidateEntity(CacheKeyHelper.Single(nameof(Entity), ("id", id)))`) **and** collections. A freshly created entity has no cached single entry yet; a mutation may invalidate an existing one and always affects list results.

---

## XML Documentation

### Mandatory on All Public Interfaces

**ABSOLUTE REQUIREMENT:** every `public` interface must carry complete XML documentation — `<summary>`, `<typeparam>` (if generic), `<param>`, and `<returns>` on every member. This includes entity repository/service/use-case interfaces, marker interfaces (`IHaveSoftDelete`, …), generic base contracts (`IRepository<T>`, …), and infrastructure contracts (`ICacheProvider`, `ITransactionalUnitOfWork`, …).

```csharp
/// <summary>
/// Brief one-line description of what the interface contract defines.
/// </summary>
public interface I{Entity}UseCase
{
    /// <summary>
    /// Creates a new {entity} within a transaction and records an audit log entry.
    /// </summary>
    /// <param name="createModel">The data required to create the {entity}.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the newly created <see cref="{Entity}"/>.</returns>
    Task<Result<{Entity}>> Create{Entity}Async(
        {Entity}CreateModel createModel,
        CancellationToken cancellationToken = default);
}
```

Omitting XML documentation from a public interface is a compliance violation.

### No XML documentation anywhere else

XML documentation lives **only** on public interfaces (and their members). **No other file carries XML documentation** — not entities, services, use cases, repositories, controllers, models, requests, responses, mappers, event handlers, or configurations. An implementation expresses intent through clear naming and structure; its contract is documented once, on the interface it implements.

The same restraint applies to ordinary comments: non-interface files avoid narrative `//` comment blocks and decorative section banners (`// Properties`, `// Methods`, …). A short inline comment is justified only for genuinely non-obvious rationale the code cannot express itself — not for restating what the code plainly does.

---

## Controller Result Handling — `HandleResult`

Controllers never build `ActionResult`s by hand. Every action ends by delegating its `Result` / `PaginatedResult` to a `HandleResult` overload on `BaseController`, which centralises the success-vs-failure decision: map and return the payload on success, or emit an RFC 7807 `ProblemDetails` carrying the result's status code on failure. `BaseController` exposes exactly these overloads:

| Overload | Use for |
|---|---|
| `HandleResult(Result result)` | Operations with **no response body** (update, delete). On success it emits an empty **204 No Content** when the result's status is `NoContent`; otherwise the status the result carries. |
| `HandleResult<T>(Result<T> result)` | A result that is **already mapped** to a response — reads where the use case took the mapper. |
| `HandleResult<T, M>(Result<T> result, Func<T, M> mapper)` | A result that still carries the **domain type** and must be mapped to a response here — writes that return a body (e.g. create). Maps on success only; on failure returns the problem without mapping. |
| `HandleResult<T>(PaginatedResult<T> result)` | A paginated (already-mapped) collection. |

- **Never hand-roll the map/branch in the action.** `if (result.IsFailureAndNoData) return HandleResult(result); return HandleResult(result.MapTo(mapper));` is exactly what the `HandleResult<T, M>` overload already does — call `return HandleResult(result, {Entity}Mapper.ToResponse);` instead.
- **Reads map inside the use case** — the action passes the mapper (`Func<TEntity, TResponse>`) into the use-case call and receives an already-mapped `Result<TResponse>` / `PaginatedResult<TResponse[]>`, so reads use the already-mapped overloads. This deliberately differs from writes, for two reasons: (1) reads are **cached**, and the cache must store the serialization-friendly response DTO — a domain entity (private constructor/setters, domain-event infrastructure) cannot round-trip through the JSON cache; (2) passing the mapper in from the adapter keeps presentation mapping out of the application layer, which cannot reference the adapter's mappers. Writes (create) are **not** cached, so they return the domain `Result<TEntity>` and map in the controller via the `HandleResult<T, M>` overload.

---

## Persistence Conventions

`Infra.Persistence` realises the repository contracts over EF Core / PostgreSQL. A new entity follows these exactly.

### Repository shape
- A generic `Repository<TEntity> : IRepository<TEntity>` (registered once as an open generic) provides shared CRUD/query operations, split by operation into partials (`Repository.Create.cs`, `.Update.cs`, `.Delete.cs`, `.Single.cs`, `.Exist.cs`, `.Count.cs`).
- A per-entity `{Entity}Repository : Repository<{Entity}>, I{Entity}Repository` adds entity-specific queries, one partial per operation (`{Entity}Repository.Collection.cs`, `.Count.cs`, …).
- **A missing or soft-deleted row is returned as a failure `Result<T>` with an HTTP status (`NotFound`) — never `null`, never a thrown exception.** Callers guard with `IsFailureAndNoData`.

### Read-list implementation (the canonical paginated query)
Filtered/sorted/paged reads use `AsNoTracking()` + the `WhereIf(condition, predicate)` extension (`Infra.Persistence/Extensions/IQueryableExtensions.cs`) — one `WhereIf` per optional filter — then ordering, then `Skip((page - 1) * pageSize).Take(pageSize)`:
- **Keyword match** uses PostgreSQL case-insensitive `EF.Functions.ILike(x.Field, $"%{keyword}%")`.
- **Sorting** maps the string `sortBy` to a column via `string.Equals(sortBy, "<field>", StringComparison.OrdinalIgnoreCase)` and picks `OrderBy` / `OrderByDescending` by `isAscending`. `sortBy` only ever names a real, indexed column.
- **The total count is a separate `CountFilteredAsync` partial that repeats the exact same `WhereIf` predicates** as the list query (without ordering/paging) — the count and the page must filter identically.

### EF configuration
- `internal sealed class {Entity}Config : IEntityTypeConfiguration<{Entity}>` under `Configurations/<Context>/`.
- **String lengths are triple-sourced and must match:** the entity validator's `maxLength`, the EF `HasMaxLength(n)`, and the request model's `[MaxLength(n)]` are the same number.
- **Soft delete is enforced in persistence** via `builder.HasQueryFilter(c => !c.IsDeleted)` — every query transparently excludes soft-deleted rows (the domain expresses intent through `IHaveSoftDelete`; the query filter enforces it).
- **Indexes are named** `IX_{Entity}_{Col1}_{Col2}` via `HasIndex(...).HasDatabaseName(...)`, mirroring the sort/filter columns.
- **All `DateTime`/`DateTime?` columns are stored UTC automatically** — `AppDbContext.ConfigureConventions` applies the `UtcDateTimeConverter` / `NullableUtcDateTimeConverter` globally; configs never repeat this per property.

### Adding an entity to the context
`AppDbContext` declares **no `DbSet<T>` properties**. `OnModelCreating` calls `ApplyConfigurationsFromAssembly(...)`, and repositories reach entities via `Set<T>()`. Adding an entity therefore means: add its `{Entity}Config` (auto-discovered) and a migration — **never edit `AppDbContext`**. Each entity gets its own migration (`dotnet ef migrations add Add{Entity}`), applied at startup via `MigratePersistence` (`Database.Migrate()`). (See also `structure.md` — Critical structure notes.)

---

## Caching Conventions

Reads are cached through `ICacheProvider`; writes invalidate. `Infra.Caching` (Redis, `System.Text.Json`) implements both `ICacheProvider` and `ICacheInvalidationService`.

- **Keys come only from `CacheKeyHelper`** (never hand-built): `Single(entity, (label, value)…)` → `"{entity}:{label}({value})"`; `Collection(entity, method, params…)` → `"collection_{entity}:{method}_{p1_p2_…}"`. Both lower-cased.
- **Every collection key starts with `collection_`, because `InvalidateCollections()` pattern-deletes `collection_*`.** A list key not built through `CacheKeyHelper.Collection` will never be invalidated — always use the helper.
- **Read-through shape.** A read use case calls `_cacheProvider.GetSingleAsync(...)` / `GetPaginatedCollection(...)` with `useCache: true`, a `CacheKeyHelper` key, and an expiry from a `_timeout` field on the use case (default `TimeSpan.FromMinutes(30)`). The service delegate runs only on a cache miss.
- **The cached value is the response DTO, not the entity** (see "Controller Result Handling") — the mapper is threaded adapter → use case → service so the cache stores a serialization-friendly `Result<TResponse>` / `PaginatedResult<TResponse[]>`.
- **Caching is best-effort.** Cache read/write failures are logged (warning + trace id) and degrade to the source — a cache fault never fails the request.

---

## API Surface Conventions

- **Mapper.** Each aggregate has an `internal static class {Entity}Mapper` exposing the fixed quartet `ToCreateModel(request)`, `ToUpdateModel(request)`, `ToResponse(entity)`, `ToArrayResponse(entity[])`. Request→model mapping uses the null-forgiving operator (`request.Title!`) on fields the request already guards with `[Required]`; the mapper performs no validation itself.
- **Filter model.** A collection endpoint binds `[FromQuery] {Entity}FilterModel`, where `{Entity}FilterModel : PaginationFilterModel` (the shared base supplies `Page`, `PageSize`, `SortBy`, `IsAscending`) and adds entity-specific filters as `[FromQuery(Name = "…")]` properties.
- **Input normalisation lives in the use case, not the controller/filter model.** The filter model binds raw nullable query values; the use case normalises them (`page < 1 → 1`, `pageSize` default, `sortBy` default to a real column, `isAscending ?? false`) before building the cache key and calling the service.
- **Controller attributes.** A versioned controller carries `[ApiVersion(ApiVersion.V1)]`, `[Route("api/v{version:apiVersion}/{resource}")]` (plural, lower-case resource), `[ApiController]`, and `[Authorize]` (secure by default). Every action enumerates its outcomes with one `[ProducesResponseType]` per status — the success type/code plus each `ProblemDetails` failure code it can return.
- **Write endpoints that return no resource use 204 No Content.** A partial-update (`PATCH` — partial semantics; never `PUT`, which implies full-resource replacement) and a (soft-)delete (`DELETE`) both return a **non-generic `Result`** whose success status is `NoContent` (no body) — the action calls `HandleResult(result)`. By contrast, create (`POST`) returns `201 Created` + the created resource (via the mapper overload), and reads (`GET`) return `200 OK` + body.
- Every overload guards a `null` result by throwing the domain exception, and takes the HTTP status from `result.StatusCode` — the status is decided in the domain/service, never hard-coded in the controller.
