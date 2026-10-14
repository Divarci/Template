# API — Project Structure

Single source of truth for how the `api/` codebase is laid out. The tree is intentionally **generic**: every part that grows with the domain — bounded contexts, aggregates, entities, value objects, API versions, infrastructure capabilities, operations — is shown as a `<Placeholder>`, not a concrete name. Adding a feature therefore does **not** require editing this file; you only touch it when a genuinely new *kind* of building block appears.

## Placeholder legend

| Token | Means | Examples |
|---|---|---|
| `<Context>` | A bounded context (top-level domain grouping) | `Samples`, `System`, `Billing` |
| `<Aggregate>` | An aggregate / feature area inside a context | `Notes`, `AuditLogs`, `Invoices` |
| `<Entity>` | An entity type within an aggregate | `Note`, `Invoice` |
| `<ValueObject>` | A value-object name (folder suffixed `ValueObject`) | `Address`, `Fullname` |
| `<Operation>` | A behavior/operation slice of a partial class | `Create`, `Single`, `Update`, `Collection`, `Delete` |
| `<Event>` | A domain event | `NoteCreated`, `InvoicePaid` |
| `<Capability>` | An infrastructure capability / contract category (one folder or project each) | `Persistence`, `Caching`, `Email`, `Identity` |
| `Version<N>` | An API version folder | `VersionOne`, `VersionTwo` |
| `<Project>` | A source project name (for its test mirror) | `Core.Domain`, `Core.Services` |
| `<Type>` | The type under test | `Note`, `NoteService`, `NoteMapper` |
| `<Subject>` | The entity of an aggregate, or the type under test when it is not an entity (test data helpers) | `Note`, `AuditLog`, `DateTimeValidator` |
| `<Method>` | A specific method under test | `Create`, `CreateAsync`, `ToResponse` |
| `<Path>` | A folder path inside a source project, relative to its root | `Entities/Samples/Notes`, `Samples/Notes` |

## Layout

```
api/
└─ src/
   ├─ adapters/                              # inbound driving adapters (entry points)
   │  ├─ Adapter.Restful/                    # HTTP/REST entry point (ASP.NET Core)
   │  │  ├─ Adapter.Restful.csproj
   │  │  ├─ Adapter.Restful.http
   │  │  ├─ Program.cs
   │  │  ├─ ServiceRegistrar.cs              # DI wiring / composition for this adapter
   │  │  ├─ Dockerfile
   │  │  ├─ appsettings.json
   │  │  ├─ appsettings.Development.json
   │  │  ├─ Properties/
   │  │  │  └─ launchSettings.json
   │  │  ├─ AspNetCore/                      # framework cross-cutting concerns
   │  │  │  ├─ Authentication/               # JWT options, policy names, roles
   │  │  │  ├─ Diagnostics/                  # global exception handler, …
   │  │  │  ├─ Extensions/
   │  │  │  ├─ Filters/                      # action filters, authorization attributes
   │  │  │  └─ Helpers/
   │  │  ├─ Controllers/
   │  │  │  ├─ Shared/                       # cross-context/version shared pieces
   │  │  │  │  ├─ <ValueObject>ValueObject/  # <ValueObject>Mapper.cs + Requests/ + Responses/
   │  │  │  │  ├─ Mappers/                   # shared mappers (PropertyMapper, …)
   │  │  │  │  └─ Models/                    # shared models (PaginationFilterModel, …)
   │  │  │  └─ Version<N>/                   # one folder per API version
   │  │  │     └─ <Context>/
   │  │  │        └─ <Aggregate>/
   │  │  │           ├─ <Aggregate>Controller.cs
   │  │  │           ├─ <Aggregate>Controller.<Operation>.cs   # (optional) partial split
   │  │  │           ├─ <Aggregate>Mapper.cs
   │  │  │           └─ Models/
   │  │  │              ├─ <Aggregate>FilterModel.cs           # (optional) query / filter model
   │  │  │              ├─ Requests/         # <Operation><Aggregate>Request.cs
   │  │  │              └─ Responses/        # <Aggregate>Response.cs
   │  │  └─ Hubs/                            # (optional) SignalR real-time hubs
   │  └─ Adapter.Scheduler/                  # background / cron entry point (Worker Service)
   │     ├─ Adapter.Scheduler.csproj
   │     ├─ Program.cs
   │     ├─ ServiceRegistrar.cs
   │     ├─ Worker.cs                        # host worker (if used)
   │     ├─ Dockerfile
   │     ├─ appsettings.json
   │     ├─ appsettings.Development.json
   │     ├─ Properties/
   │     │  └─ launchSettings.json
   │     ├─ Configuration/                   # <Name>Options.cs
   │     └─ Jobs/                            # <Name>Job.cs
   │
   ├─ applications/                          # orchestration layer (no domain rules of its own)
   │  ├─ App.UseCases/                       # use cases: flow / transaction orchestration
   │  │  ├─ App.UseCases.csproj
   │  │  └─ <Context>/
   │  │     └─ <Aggregate>/
   │  │        ├─ <Aggregate>UseCase.cs               # partial: ctor + shared members
   │  │        └─ <Aggregate>UseCase.<Operation>.cs   # one partial per operation
   │  └─ App.Processor/                      # domain-event / message processors
   │     ├─ App.Processor.csproj
   │     ├─ DomainEvents/
   │     │  └─ <Context>/
   │     │     └─ <Aggregate>/
   │     │        └─ <Event>DomainEventHandler.cs
   │     └─ Shared/
   │        └─ Mappers/
   │
   ├─ core/                                  # domain heart + shared kernel (no outward deps)
   │  ├─ Core.Domain/                        # entities, value objects, domain events
   │  │  ├─ Core.Domain.csproj
   │  │  ├─ Entities/
   │  │  │  └─ <Context>/
   │  │  │     └─ <Aggregate>/
   │  │  │        ├─ <Entity>.cs                     # partial: state + behavior
   │  │  │        ├─ <Entity>.Validate.cs            # partial: invariants / validation
   │  │  │        ├─ DomainEvents/                   # <Event>DomainEvent.cs
   │  │  │        ├─ Interfaces/                     # I<Entity>Repository / I<Entity>Service / I<Entity>UseCase
   │  │  │        ├─ Models/                         # <Entity><Operation>Model.cs (Create, Update, …)
   │  │  │        └─ Enums/                          # (optional) <Enum>.cs
   │  │  └─ ValueObjects/
   │  │     └─ <ValueObject>ValueObject/
   │  │        ├─ <ValueObject>.cs
   │  │        ├─ <ValueObject>.Validation.cs
   │  │        └─ <ValueObject>Model.cs
   │  ├─ Core.Services/                      # business/application services
   │  │  ├─ Core.Services.csproj
   │  │  └─ <Context>/
   │  │     └─ <Aggregate>/
   │  │        ├─ <Aggregate>Service.cs
   │  │        └─ <Aggregate>Service.<Operation>.cs  # one partial per operation
   │  └─ Core.Library/                       # shared kernel: contracts, primitives, helpers
   │     ├─ Core.Library.csproj
   │     ├─ Abstractions/                    # base types (+ Interfaces/)
   │     ├─ Attributes/                      # framework-agnostic attributes (e.g. AutoSeedData)
   │     ├─ Contracts/                       # cross-cutting interfaces, one folder per capability
   │     │  ├─ Caching/
   │     │  ├─ DomainEvents/                 # Handle/ (IDomainEventHandler) + Process/ (IDomainEvent, DomainEvent)
   │     │  ├─ GenericRepositories/          # IRepository<T> — the generic repository contract
   │     │  ├─ UnitOfWorks/
   │     │  └─ <Capability>/                 # add one folder per new contract category
   │     ├─ Exceptions/
   │     ├─ Helpers/
   │     ├─ Models/
   │     ├─ ResultPattern/
   │     └─ Validators/                      # (+ Enums/)
   │
   └─ infrastructures/                       # outbound driven adapters (contract implementations)
      ├─ Infra.Persistence/                  # EF Core persistence
      │  ├─ Infra.Persistence.csproj
      │  ├─ Context/
      │  │  ├─ AppDbContext.cs
      │  │  └─ Conventions/                  # global value converters (UTC DateTime, …)
      │  ├─ Configurations/                  # IEntityTypeConfiguration
      │  │  └─ <Context>/                    # <Entity>Config.cs
      │  ├─ Repositories/
      │  │  ├─ Generics/                     # generic repository base
      │  │  └─ <Context>/
      │  │     └─ <Aggregate>/
      │  │        ├─ <Aggregate>Repository.cs
      │  │        └─ <Aggregate>Repository.<Operation>.cs
      │  ├─ Migrations/                      # EF Core migrations + AppDbContextModelSnapshot.cs
      │  ├─ UnitOfWorks/                     # UnitOfWork.cs
      │  └─ Extensions/
      └─ Infra.<Capability>/                 # one per capability (Caching, Email, Identity, …)
         ├─ Infra.<Capability>.csproj
         ├─ Configuration/                   # <Capability>Options.cs
         └─ Services/                        # implements a Core.Library contract
```

### Tests

Tests live under `api/test/`. There are two kinds of test project: **unit test projects**, each mirroring one source project, and a single **`Architecture.Tests`** project that enforces the conventions of these guidelines across every `api/src` assembly. Approved packages: `tech-stack.md` — Testing.

```
api/
└─ test/
   ├─ Architecture.Tests/                    # architecture rules over every api/src assembly
   │  ├─ Architecture.Tests.csproj
   │  ├─ ApiArchitecture.cs                  # shared loaded architecture + assembly sets
   │  ├─ AssemblyDiscovery.cs                # discovers the api/src assemblies
   │  ├─ AssemblyDiscoveryApiAssemblyNamesTests.cs   # guard: discovery finds every api assembly
   │  ├─ ArchitectureRules.cs                # shared rule factories / predicates
   │  ├─ Layers/                             # project-dependency direction
   │  ├─ ThirdParty/                         # third-party packages confined to their owning project
   │  ├─ Naming/                             # type-name suffixes / prefixes
   │  ├─ Domain/                             # entity, domain-event, and model shape
   │  ├─ Visibility/                         # access modifiers, interface placement
   │  └─ Calls/                              # one-level-down call rules between layers
   └─ <Project>.Tests/                       # Core.Domain.Tests, Core.Services.Tests, …
      ├─ <Project>.Tests.csproj
      ├─ <Subject>TestData.cs                # internal static: builders, constants, TheoryData
      ├─ <Type>TestSetup.cs                  # internal sealed: fresh mocks + subject, one instance per test
      └─ <Path>/                             # same folder path as the source file under test
         └─ <Type><Method>Tests.cs           # one test class per method under test — tests only
```

Example — `api/src/core/Core.Domain/Entities/Samples/Notes/Note.cs` → `api/test/Core.Domain.Tests/Entities/Samples/Notes/NoteCreateTests.cs`, `NoteUpdateTitleTests.cs`, `NoteSoftDeleteTests.cs`, …

#### Which projects are tested, and for what

| Test project | Covers | Mocks |
|---|---|---|
| `Core.Domain.Tests` | Entity factories and behaviour methods: valid creation, every validation failure, `IsDeleted` guards (`409 Conflict`), raised domain events, result messages and status codes. | None |
| `Core.Library.Tests` | The result pattern, every validator, `CacheKeyHelper`, and other helpers. | None |
| `Core.Services.Tests` | Each service operation: domain failure short-circuits (repository never called), repository staging, the audit-log write, per-operation cache invalidation (`coding-standards.md` — Entity Service Structure), not-found propagation. | Repositories, cache invalidation, logger |
| `App.UseCases.Tests` | Delegation to the service inside `ExecuteTransactionAsync`; read-side input normalisation, cache-key construction, and cache read-through. | Services, unit of work, cache provider |
| `App.Processor.Tests` | `OutboxProcessor` (type resolution, deserialisation, dispatch, mark processed, error recording) and each domain-event handler. | Repositories / services, unit of work, logger, service provider |
| `Adapter.Restful.Tests` | Mappers, `ValidationFilter`, `GlobalExceptionHandler`, `ApiResultHelper`. Controllers are not unit-tested. | Logger |

No unit test project exists for `Adapter.Scheduler` (thin job wrapper) or for `Infra.*` (repositories, `UnitOfWork`, `CacheService`, EF configuration) — those need a real database / cache and belong to integration testing, which is deferred (`tech-stack.md` — Testing). A new source project with testable logic gets its own `<Project>.Tests` mirror (critical note 1).

#### Unit test conventions

- **Mirror exactly.** A test file sits at the same `<Path>` inside `<Project>.Tests` as its subject inside `<Project>`; the namespace follows the folder path (`Core.Domain.Tests.Entities.Samples.Notes`).
- **One test class per method under test**, named `<Type><Method>Tests` (`NoteCreateTests`, `NoteServiceCreateAsyncTests`, `NoteMapperToResponseTests`). Overloads of one method share the class.
- **Test method name:** `<Method>_<Scenario>_<ExpectedResult>` — e.g. `UpdateTitle_WhenNoteIsDeleted_ReturnsConflict`. Use the ubiquitous language of `domain-driven-design.md`.
- **`[Fact]` for a single case; `[Theory]` + `[InlineData]`** for value tables (boundary lengths, invalid inputs).
- **Arrange / Act / Assert.** Every test body is split by the three comments `// Arrange`, `// Act`, `// Assert` (merge as `// Act & Assert` only for exception assertions). These markers are mandatory and are the only comments test code carries. One Act per test.
- **Assertions use AwesomeAssertions `.Should()` only** — never xUnit's `Assert.*`.
- **Mocks (Moq) only for interfaces** declared in `Core.Domain` (`Interfaces/`) or `Core.Library/Contracts/`, plus `ILogger<T>` and the Microsoft DI abstractions (`IServiceProvider`, `IServiceScope`, `IServiceScopeFactory`). Never mock entities, value objects, models, `Result`, or the subject itself. Interactions are verified (`Verify(…, Times.Once / Times.Never)`) only when the interaction *is* the documented behaviour (audit written, cache invalidated, repository not called on failure).
- **Entities are built through their `Create()` factory** — never via reflection or by bypassing validation.
- **Deterministic and isolated.** No network, database, file system, clock sleeps, or shared mutable state between tests; no real secrets anywhere (`security.md`). Mocks exist only on a per-test `TestSetup` instance; static members hold only immutable values.
- **Test code obeys `coding-standards.md` in full** — one top-level type per file, no space-padding, no XML documentation, `public sealed class` for test classes.

#### Test helpers

- **A test class contains only tests.** A `<Type><Method>Tests` class declares only `[Fact]` / `[Theory]` methods — no fields, properties, constants, constructors, private methods, nested types, or `TheoryData` members. Everything a test needs besides its own body lives in a helper at the **root** of the test project. This applies to every test project, `Architecture.Tests` included.
- **Two helper kinds, one type per file, at the project root:**
  - **`<Subject>TestData`** (`internal static class`) — entity builders (always through `Create()`), model builders, constants and sample values, immutable shared delegates, and `TheoryData` sources (consumed with `[MemberData(nameof(…), MemberType = typeof(<Subject>TestData))]`). It holds no mutable state and no mocks. Builders take optional named parameters whose defaults are valid, unremarkable values.
  - **`<Type>TestSetup`** (`internal sealed class`) — one per type under test that has mocked dependencies or needs per-test context (e.g. an `HttpContext`). Its constructor creates fresh mocks, applies the background setups every test of that type needs, and builds the subject. It exposes the mocks and the subject as get-only properties, plus named `Given…` setup methods and, where a verification repeats, `Verify…` methods. **Each test creates its own instance in `// Arrange`** (`NoteServiceTestSetup setup = new();`); a `TestSetup` is never stored in a field or a static member.
  - `Architecture.Tests` keeps its shared rule factories and predicates in a root `ArchitectureRules` (`internal static class`), next to `ApiArchitecture` and `AssemblyDiscovery`.
- **Members** of a helper are `public` (the class itself is `internal`); a helper's own implementation details are `private`.
- **Scenario values stay in the test.** Every value the Act depends on or the Assert checks is written in the test — as a literal, as the same named constant used in its Arrange, or read back from the arranged object (e.g. `note.Id`). A test never asserts a builder default, and a `TestSetup` never hard-codes such a value — it takes it as a parameter. Background behaviour that is not the scenario (an audit write succeeds, the transaction runs the operation) belongs in the `TestSetup`.
- **Starting state is written in the Arrange.** When the Assert checks state that the Act changes — or must leave unchanged — the starting value of that state is written in the test's Arrange (e.g. `NoteTestData.CreateNote(isPinned: !isPinned)`). A test must fail if the Act were a no-op.
- **Helpers are per project.** Each unit test project keeps its own helpers and still references only its source project; helpers are never shared across test projects.
- **No unused helper members** — every member is used by at least one test in its project.

#### Test project setup

- `Microsoft.NET.Sdk`, `net10.0`, `Nullable` + `ImplicitUsings` enabled, `IsPackable=false`, `OutputType=Exe` (xUnit v3), `IsTestingPlatformApplication=false` (tests run through classic VSTest `dotnet test`; there is no `global.json` switching to Microsoft Testing Platform). Package versions are pinned in the `.csproj`.
- A unit test project references **only** its source project (plus what that project transitively needs). `Architecture.Tests` references every `api/src` project.
- Every test project is registered in `Template.slnx` under the `/api/test/` folder.
- **Internal access.** Implementations are `internal sealed` (`coding-standards.md` — Access Modifiers), so every source project with a test mirror declares `<InternalsVisibleTo Include="<Project>.Tests" />` in its `.csproj` — plus `DynamicProxyGenAssembly2` when Moq must proxy an `internal` interface or an interface whose type argument is `internal` (e.g. `ILogger<NoteService>`). This is the only change a source project makes for testability; it is application code, owned by `coder`.

#### Architecture rules

`Architecture.Tests` turns the rules below into executable tests (ArchUnitNET). Every rule traces to a guideline; a rule that fails exposes a code violation, which is fixed in the code — never by weakening the rule. Adding a rule requires a guideline that states it.

| Folder | Rule | Source |
|---|---|---|
| `Layers/` | `Core.Library` depends on no other `api/src` assembly. `Core.Domain` depends only on `Core.Library`. `Core.Services`, `App.UseCases`, `App.Processor` depend on no `Infra.*`, no `Adapter.*`, and not on each other. `Infra.*` depend on no `App.*`, `Adapter.*`, `Core.Services`, or other `Infra.*`. | `architecture.md` — Layer Dependencies, Infrastructure Layer |
| `ThirdParty/` | EF Core / Npgsql types are used only in `Infra.Persistence`; StackExchange.Redis only in `Infra.Caching`; Quartz only in `Adapter.Scheduler`; ASP.NET Core only in `Adapter.Restful`. `Core.*` and `App.*` use no third-party types beyond `Microsoft.Extensions.*.Abstractions`. | `hard-rules.md` §2 |
| `Naming/` | Interfaces start with `I`. Implementations of `I…Service` / `I…UseCase` / `I…Repository` end with `Service` / `UseCase` / `Repository`; controllers end with `Controller`; domain events with `DomainEvent`; domain-event handlers with `DomainEventHandler`; EF configurations with `Config`; async methods with `Async` — the only exception is a method implementing a third-party interface whose name we do not control (e.g. Quartz `IJob.Execute`). | `coding-standards.md` — Naming; `event-driven-design.md` |
| `Domain/` | Entities (`BaseEntity` subclasses) are `public sealed`, expose no public constructor, and no public property setter. Domain events, `CreateModel`s, and `UpdateModel`s are `sealed` records. | `hard-rules.md` §7; `domain-driven-design.md`; `coding-standards.md` — Record Design |
| `Visibility/` | Service, use-case, repository, and domain-event-handler implementations are `internal sealed` — abstract classes included for `internal`. Two named base classes are the only exceptions: the generic `Repository<TEntity>` (`Infra.Persistence/Repositories/Generics/`) is `internal` but not sealed because every entity repository inherits from it; `DomainEventHandler<TDomainEvent>` (`Core.Library/Contracts/DomainEvents/Handle/`) is `public abstract` because it is the contract base every handler extends. Entity repository / service / use-case interfaces live in `Core.Domain.Entities.<Context>.<Aggregate>.Interfaces`. Each `Infra.*` project exposes no public type other than its `ServiceRegistrar` (EF Core-generated migrations under `Migrations/` excepted). | `coding-standards.md` — Access Modifiers; `architecture.md` — Infrastructure Layer |
| `Calls/` | Controllers depend on no service or repository interface. Use cases depend on no repository interface and no other use-case interface. Services depend on no other service, no use case, and not on `ITransactionalUnitOfWork`. Domain-event handlers depend on no repository interface. | `hard-rules.md` §3–§6; `architecture.md` — Use Case Routing |

## Conventions the tree encodes

- **Four layers, fixed names:** `adapters` (driving), `applications` (orchestration), `core` (domain + shared kernel), `infrastructures` (driven). Project names are `<Adapter|App|Core|Infra>.<Name>`.
- **Context → Aggregate nesting** is the repeating unit across every layer; the *same* `<Context>/<Aggregate>` path is used in controllers, use cases, services, domain entities, and repositories.
- **Partial classes split by operation:** a type's core lives in `<Type>.cs`; each behavior gets its own partial file `<Type>.<Operation>.cs` (services, use cases, repositories) or a purpose partial (`<Entity>.Validate.cs`).
- **Interfaces are declared in `Core.Domain`** (per aggregate, under `Interfaces/`); implementations live in their owning layer (`Core.Services`, `App.UseCases`, `Infra.Persistence`).
- **Contracts vs. implementations:** cross-cutting contracts live in `Core.Library/Contracts/`; each `Infra.<Capability>` project implements one.

## Critical structure notes

1. **A new node inherits its layer's rules.** Anything added to a layer (a new adapter, use case, service, entity, repository, contract, infra project, …) must obey exactly the same rules and conventions as the sibling nodes already in that layer. There are no per-node exceptions — the layer's existing shape is the contract.
2. **Every new third-party dependency enters through infrastructure, behind a Core.Library abstraction.** A newly introduced 3rd-party library/integration must live in an `infrastructures/Infra.<Capability>` project and be consumed only through an abstraction (contract) declared in `Core.Library/Contracts/`. No layer above infrastructure may reference the third-party package directly — it depends on the contract, and the `Infra.<Capability>` project implements it. Test projects under `api/test/` are exempt for the approved test packages only (`tech-stack.md` — Testing).
3. **Follow the existing convention; never invent a new one.** Always conform to the conventions already established here. If the existing convention does not cover a new case, **stop and ask the user** — never improvise an ad-hoc convention or write arbitrary code to fill the gap.
4. **Adding an entity is configuration-driven — there are no `DbSet`s.** `AppDbContext` exposes no `DbSet<T>` properties; `OnModelCreating` calls `ApplyConfigurationsFromAssembly` (and `ConfigureConventions` applies global UTC `DateTime` conversion), and repositories reach entities via `Set<T>()`. To add an entity you add its `{Entity}Config : IEntityTypeConfiguration<{Entity}>` (auto-discovered) and a migration — **never edit `AppDbContext`**. (Details in `coding-standards.md` — Persistence Conventions.)
5. **Services, use cases, and repositories are wired by explicit DI registration — one line each in their layer's `ServiceRegistrar`.** Adding the file is *not* enough — unlike domain-event handlers, which are assembly-scanned (`event-driven-design.md`). A new entity adds `AddScoped<I{Entity}Repository, {Entity}Repository>()` (Infra.Persistence), `AddScoped<I{Entity}Service, {Entity}Service>()` (Core.Services), and `AddScoped<I{Entity}UseCase, {Entity}UseCase>()` (App.UseCases) to the respective registrars; the generic repository is registered once as an open generic (`AddScoped(typeof(IRepository<>), typeof(Repository<>))`).

