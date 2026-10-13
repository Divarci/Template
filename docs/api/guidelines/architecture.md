# Architecture

A high-level summary of the `api` architecture: structural patterns, layer boundaries, dependency rules, scalability, and runtime topology. `structure.md` shows the concrete folder/file layout that realises this architecture.

---

## Hybrid Architecture — Clean Architecture + Vertical Slice

The backend combines two complementary patterns.

**Clean Architecture** governs the macro structure: code is organised into concentric layers where dependencies always point inward. The domain and shared kernel sit at the centre; infrastructure and adapters sit at the edges and depend on inner layers — never the reverse.

**Vertical Slice Architecture** governs the micro structure inside each layer: code is grouped by domain concept (`<Context>/<Aggregate>/`), not by technical type. Every aggregate owns its folder with its interfaces, models, domain events, and configuration co-located.

The result: each feature is easy to locate (vertical slices), while cross-layer boundaries stay clean and testable (clean architecture).

---

## Layers and Their Responsibilities

The solution is divided into four layers, each with a distinct responsibility boundary.

```
┌──────────────────────────────────────────────────┐
│                   Adapter Layer                    │
│      Adapter.Restful   ·   Adapter.Scheduler       │
├──────────────────────────────────────────────────┤
│                 Application Layer                  │
│        App.UseCases    ·   App.Processor           │
├──────────────────────────────────────────────────┤
│                Infrastructure Layer                │
│   Infra.Persistence  ·  Infra.Caching  ·  …        │
├──────────────────────────────────────────────────┤
│                     Core Layer                     │
│   Core.Domain  ·  Core.Library  ·  Core.Services   │
└──────────────────────────────────────────────────┘
```

| Layer | Projects | Responsibility |
|---|---|---|
| **Core** | `Core.Library`, `Core.Domain`, `Core.Services` | Foundation **and** business logic. `Core.Library` holds shared abstractions, the result pattern, validators, and cross-cutting contracts — zero business logic. `Core.Domain` holds entities, value objects, domain events, and the interfaces upper layers must implement. `Core.Services` implements the entity services (domain orchestration, repository calls, cache invalidation) and depends only on `Core.Domain`. |
| **Application** | `App.UseCases`, `App.Processor` | Orchestration. `App.UseCases` wraps service calls in the unit-of-work transaction and serves read operations from cache — triggered by HTTP requests. `App.Processor` wraps domain-event reactions in the same transactional pattern — triggered asynchronously by the outbox processor. Audit-log entries are written by the entity services these call (hard rule 4), not by the orchestration layer. |
| **Infrastructure** | `Infra.Persistence`, `Infra.Caching` (+ `Infra.Identity`, `Infra.Email`, … as added) | Technical concerns. `Infra.Persistence` provides EF Core repositories, the unit of work, and outbox persistence over **PostgreSQL**. Every other `Infra.*` project implements a `Core.Library` contract for one external system. |
| **Adapter** | `Adapter.Restful`, `Adapter.Scheduler` | Entry points. `Adapter.Restful` is the HTTP host (authentication, routing, validation, error handling). `Adapter.Scheduler` is the background worker that processes outbox messages via Quartz.NET. |

> **Note — services live in Core.** In this solution the entity services sit in the **Core** layer (`Core.Services`), not the Application layer. Their dependency shape is unchanged: a service depends only on `Core.Domain` and implements the `I{Entity}Service` interface declared there. Use cases still call services; services still call repositories.

---

## Layer Dependencies

Dependencies are strictly unidirectional — outer layers depend on inner layers, never the reverse.

```
Adapter.Restful ───┐
                   ├──► App.UseCases ───► Core.Domain ──► Core.Library
Adapter.Scheduler ─┤   App.Processor ───► Core.Domain ──► Core.Library
                   ├──► Core.Services ──► Core.Domain ──► Core.Library
                   ├──► Infra.Persistence ─► Core.Domain
                   └──► Infra.Caching ─────► Core.Domain
```

Key rules:
- `Core.Library` has **zero** project references — fully self-contained.
- `Core.Domain` depends only on `Core.Library`.
- `Core.Services` depends only on `Core.Domain` — it implements the entity-service interfaces declared there.
- `App.UseCases` and `App.Processor` depend only on `Core.Domain` — they consume services through the `Core.Domain` interfaces and are unaware of infrastructure and of the concrete `Core.Services`.
- `Infra.*` projects depend only on `Core.Domain` — they implement its contracts.
- Adapter projects reference all layers — they are the composition roots that wire everything via DI.

---

## Scalability

The architecture grows horizontally without modifying existing projects.

**Adapter layer** accepts any number of new entry-point projects, each named `Adapter.{ChannelName}` (synchronous channels) or `Adapter.{WorkerName}` (background workers). A new adapter adds a project, wires DI, and delegates all logic inward — no existing adapter or application code is touched.

| Pattern | Example additions |
|---|---|
| `Adapter.{ChannelName}` | `Adapter.Grpc`, `Adapter.GraphQL` |
| `Adapter.{WorkerName}` | `Adapter.EventConsumer`, `Adapter.DataSync` |

**Infrastructure layer** accepts any number of new provider projects, each named `Infra.{ProviderName}`, depending only on `Core.Domain`, registering through its own `ServiceRegistrar`. Adding a provider requires no changes to any existing project.

| Pattern | Example additions |
|---|---|
| `Infra.{ProviderName}` | `Infra.Email`, `Infra.Storage`, `Infra.Search` |

**Core and Application layers** stay untouched when new adapters or infrastructure providers are added — they define contracts, not delivery mechanisms.

---

## Use Case Routing Rule — Hard Rule

> **A use case's call path depends on what kind of work the operation does. Two routes; one decision; no exceptions.**
>
> ### Route 1 — Local DB + domain work goes through the service
>
> If a use-case operation reads or writes a domain entity in the local database, the call **must** go `UseCase → Service → Repository`. The service is non-skippable:
>
> - The service owns the per-entity domain orchestration: it calls the domain factory or domain method, validates, and produces the `Result` shape callers depend on.
> - Business rules that don't fit on the entity (cross-row pre-condition checks, cache-invalidation policy) live on the service. Bypassing it silently strips those rules.
> - The cache-invalidation discipline (`InvalidateEntity`, `InvalidateCollections`) lives on the service. Going around it leaves stale cache entries.
>
> Use cases never call repositories directly, never call `SaveChangesAsync`, never invoke domain factory methods themselves.
>
> ### Route 2 — Pure third-party calls go through the infrastructure port directly
>
> If a use-case operation only delegates to an external system (identity provider, message bus, search index, LLM/AI provider, …) and produces **no local entity change**, the call goes `UseCase → IInfraPort` directly. **No service wrapper.**
>
> - There is no domain entity to load, no factory to invoke, no repository to call, no cache to invalidate. A service method here would be a one-line pass-through adding zero behaviour and one layer of indirection.
> - The infrastructure port (an `I{Function}Provider`-style contract in `Core.Library/Contracts/{Function}/`) is the abstraction the use case depends on; wrapping it in a service-named pass-through hides nothing the port doesn't already hide.
>
> The use case still wraps the call in `ExecuteTransactionAsync` so that any local writes stay atomic with the operation — even though its *primary* effect is remote. Transaction ownership is always the use case's, never the port's.
>
> ### Reactor Rule — Domain Event Handlers
>
> Handlers in `App.Processor` follow the same pattern as use cases: wrap the body in `_unitOfWork.ExecuteTransactionAsync` and call services (never repositories directly). Audit-log entries are written inside the entity services the handler calls (hard rule 4), not by the handler itself. The only difference from HTTP use cases is the trigger — the outbox processor in response to a domain event, not an HTTP request. Routes 1 and 2 apply equally to handlers.
>
> ### Mixed operations — both routes in one transaction
>
> Operations that touch both local state and an external system invoke both routes inside the same `ExecuteTransactionAsync` block. **DB write first, third-party call second** — an upstream failure rolls the local transaction back so both stores stay consistent.
>
> Example — `{Entity}UseCase.UpdateAsync`:
>
> 1. `_{entity}Service.UpdateAsync(...)` — DB write through the service (Route 1).
> 2. `_externalProvider.UpdateAsync(...)` — external system update through the port (Route 2).
> 3. If step 2 fails, the surrounding transaction rolls back step 1.
>
> ### Forbidden
>
> - **Wrapping a third-party port in a service method that adds no behaviour.** A service whose body is `return await _port.X(...)` is a Route-2 operation in Route-1 clothing — delete the wrapper, call the port from the use case.
> - **Calling a repository or `SaveChangesAsync` from a use case.** Always Route 1 → service.
> - **Skipping the service to avoid writing one for a Route-1 operation.** If the operation is Route 1, the service exists; create it.
> - **Calling a peer in the same layer** — a use case invoking another use case, or a service invoking another service. Calls only ever go one layer down; cross-entity coordination is the use case's job (hard rule 6).

---

## Infrastructure Layer — Hard Rule

> **The Infrastructure layer is the designated home for every third-party integration in the solution. There is no other place for third-party-specific code.**
>
> Every new third party — every database engine, cache provider, identity provider, message bus, email service, blob store, search index, observability backend, LLM/AI provider, or any other external system — gets its own dedicated project named **`Infra.{ThirdPartyFunction}`**. The `{ThirdPartyFunction}` describes what the third party *does for the application* (its function), never the vendor name, so a vendor can be swapped without renaming the project.
>
> | Project | Third party | Function |
> |---|---|---|
> | `Infra.Persistence` | PostgreSQL (via EF Core) | Relational persistence |
> | `Infra.Caching` | Redis (via StackExchange.Redis) | Distributed caching |
> | `Infra.{Function}` (as added, e.g. `Infra.Email`) | e.g. an SMTP service | e.g. outbound email |
>
> **Constraints that apply to every `Infra.*` project:**
>
> - Depends only on `Core.Library` and `Core.Domain`. Never references another `Infra.*` project, `Core.Services`, `App.*`, or any `Adapter.*`.
> - Implements interfaces declared in `Core.Library/Contracts/{Function}/`. Those interfaces are the **only** types that leak out of the project.
> - All other types (options, payloads, mappers, internal services, the implementation class itself) stay `internal sealed`.
> - Owns its DI wiring via a `ServiceRegistrar.Register{Function}Layer(IServiceCollection, IConfiguration)` extension method. The composition root (`Adapter.Restful/Program.cs`) is the only place that calls these registrars.
>
> **Forbidden alternatives:** putting third-party code inside `Core.Services`, `App.*`, `Adapter.*`, or an `Infra.*` project that already exists for a different function. A new third party = a new `Infra.{Function}` project, full stop. No exceptions, no shortcuts, no "we'll move it later." (This is the concrete form of critical note 2 in `structure.md`.)

---

## Runtime Topology

The API runs as two containerised processes that share one database and one cache:

| Process | Project | Role |
|---|---|---|
| REST API host | `Adapter.Restful` | Serves HTTP requests. |
| Scheduler / outbox processor | `Adapter.Scheduler` | Background worker draining `OutboxMessage` rows and dispatching domain events. |

Both are defined as Docker Compose services (`adapter.restful`, `adapter.scheduler`). In development, PostgreSQL, Redis, Seq, and Keycloak run as additional services on the shared network (from the override file); in production they are external/managed services. The **deployment guidelines** (`docs/deployment/guidelines/`) own the authoritative detail of Compose files, environments, and secrets — this document only states the shape.
