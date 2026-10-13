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
| `<Method>` | A specific method under test | `CreateAsync` |

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

### Tests (deferred)

Tests live under `api/test/` and **mirror** the source layout — one `<Project>.Tests` project per testable source project, then the same `<Context>/<Aggregate>` grouping, narrowed to the behavior under test:

```
api/
└─ test/                                     # (deferred — no test projects exist yet)
   └─ <Project>.Tests/                       # Core.Domain.Tests, Core.Services.Tests, …
      └─ <Context>/
         └─ <Aggregate>/
            └─ <Operation>/
               └─ <Method>/                  # test classes for that method
```

## Conventions the tree encodes

- **Four layers, fixed names:** `adapters` (driving), `applications` (orchestration), `core` (domain + shared kernel), `infrastructures` (driven). Project names are `<Adapter|App|Core|Infra>.<Name>`.
- **Context → Aggregate nesting** is the repeating unit across every layer; the *same* `<Context>/<Aggregate>` path is used in controllers, use cases, services, domain entities, and repositories.
- **Partial classes split by operation:** a type's core lives in `<Type>.cs`; each behavior gets its own partial file `<Type>.<Operation>.cs` (services, use cases, repositories) or a purpose partial (`<Entity>.Validate.cs`).
- **Interfaces are declared in `Core.Domain`** (per aggregate, under `Interfaces/`); implementations live in their owning layer (`Core.Services`, `App.UseCases`, `Infra.Persistence`).
- **Contracts vs. implementations:** cross-cutting contracts live in `Core.Library/Contracts/`; each `Infra.<Capability>` project implements one.

## Critical structure notes

1. **A new node inherits its layer's rules.** Anything added to a layer (a new adapter, use case, service, entity, repository, contract, infra project, …) must obey exactly the same rules and conventions as the sibling nodes already in that layer. There are no per-node exceptions — the layer's existing shape is the contract.
2. **Every new third-party dependency enters through infrastructure, behind a Core.Library abstraction.** A newly introduced 3rd-party library/integration must live in an `infrastructures/Infra.<Capability>` project and be consumed only through an abstraction (contract) declared in `Core.Library/Contracts/`. No layer above infrastructure may reference the third-party package directly — it depends on the contract, and the `Infra.<Capability>` project implements it.
3. **Follow the existing convention; never invent a new one.** Always conform to the conventions already established here. If the existing convention does not cover a new case, **stop and ask the user** — never improvise an ad-hoc convention or write arbitrary code to fill the gap.
4. **Adding an entity is configuration-driven — there are no `DbSet`s.** `AppDbContext` exposes no `DbSet<T>` properties; `OnModelCreating` calls `ApplyConfigurationsFromAssembly` (and `ConfigureConventions` applies global UTC `DateTime` conversion), and repositories reach entities via `Set<T>()`. To add an entity you add its `{Entity}Config : IEntityTypeConfiguration<{Entity}>` (auto-discovered) and a migration — **never edit `AppDbContext`**. (Details in `coding-standards.md` — Persistence Conventions.)
5. **Services, use cases, and repositories are wired by explicit DI registration — one line each in their layer's `ServiceRegistrar`.** Adding the file is *not* enough — unlike domain-event handlers, which are assembly-scanned (`event-driven-design.md`). A new entity adds `AddScoped<I{Entity}Repository, {Entity}Repository>()` (Infra.Persistence), `AddScoped<I{Entity}Service, {Entity}Service>()` (Core.Services), and `AddScoped<I{Entity}UseCase, {Entity}UseCase>()` (App.UseCases) to the respective registrars; the generic repository is registered once as an open generic (`AddScoped(typeof(IRepository<>), typeof(Repository<>))`).

