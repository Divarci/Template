# Event-Driven Design

How the backend communicates internally using domain events — publishing, persistence, and asynchronous processing. The pattern below is implemented end to end (EF Core / PostgreSQL outbox persistence, the Quartz.NET scheduler) and is the canonical rulebook for all eventing.

---

## Overview

The system implements the **Transactional Outbox** pattern. Domain events raised by entities are:

1. Collected in-memory during the request.
2. Serialised into `OutboxMessage` rows within the same database transaction as the business data.
3. Processed asynchronously by a background scheduler that dispatches them to registered handlers.

This guarantees domain events are **never lost** (they share the business data's transaction) and **eventually processed** (the scheduler picks them up) — without any distributed transaction.

---

## Domain Event Contract

All domain events derive from the abstract base record `DomainEvent`, which implements `IDomainEvent` (both in `Core.Library/Contracts/DomainEvents/Process/`). The base record provides a `Guid` identity (`Id`) and a UTC timestamp (`OccurredOnUtc`). Domain events are immutable C# **records** — they cannot be modified after creation. Because a record can only inherit from another record, the base type must stay an `abstract record`, never a class.

The entity id a concrete event carries is named `{Entity}Id` (e.g. `NoteCreatedDomainEvent(Guid NoteId)`) — never `Id`, which would hide the base event's own `Id`.

---

## Event Structure

Each entity defines **at most one** event per state change, and **only** when the feature being implemented calls for eventing (see the *Feature-driven rule* below). All events carry only the entity id — the handler loads any additional data it needs.

Events follow the naming pattern `{Entity}{Verb}DomainEvent` and live in a `DomainEvents/` folder under the entity's folder:

| Naming pattern | Raised by |
|---|---|
| `{Entity}CreatedDomainEvent` | the static `Create()` factory method |
| `{Entity}DeletedDomainEvent` | `SoftDelete()` or `Delete()` |
| `{Entity}{Property}UpdatedDomainEvent` | each individual update method |

---

## Publishing — How Events Are Raised

Entities raise events via the `Raise()` method inherited from `BaseEntity`. Events accumulate in the entity's `DomainEvents` collection during the request; they are **not dispatched inline** — nothing happens to them until the Unit of Work commits.

### ⚠ Feature-driven rule

Whether a state change emits a domain event is decided from the feature request itself. If the requirement describes an asynchronous reaction (notification, projection, downstream side effect, …), add the event class **and** its `IDomainEventHandler<…>` together in the same change, and call `Raise(...)` from the entity. If the feature does not call for eventing, do not create the event class and do not call `Raise(...)`. Event and handler are introduced together or not at all — never one without the other. Every raised event is treated as work by the pipeline below: serialised into an `OutboxMessage` on commit, picked up by the scheduler, and dispatched.

---

## Persistence — Transactional Outbox

When `UnitOfWork.CommitAsync()` is called at the end of a use case, it performs the following inside the active database transaction:

1. Iterates all tracked `BaseEntity` instances via the EF Core `ChangeTracker`.
2. For each entity, reads its `DomainEvents` collection and calls `ClearDomainEvents()` to prevent re-processing.
3. Serialises each event to JSON and creates an `OutboxMessage` row storing:
   - `Type` — the .NET assembly-qualified type name (for deserialisation).
   - `Content` — the full JSON payload.
   - `OccurredOnUtc` — the UTC timestamp.
4. Calls `SaveChangesAsync` and `CommitAsync` — business data and outbox messages persist in the **same transaction**. If anything fails, both roll back: no ghost events, no lost events.

### OutboxMessage fields

| Column | Type | Purpose |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `Type` | `string` (max 2000) | Assembly-qualified .NET type name |
| `Content` | `string` (max 100000) | Serialised JSON of the domain event |
| `OccurredOnUtc` | `DateTime` | When the event was raised |
| `ProcessedOnUtc` | `DateTime?` | When processed (`null` = unprocessed) |
| `Error` | `string?` | Error details if the handler failed |

---

## Processing — Background Scheduler

The `Adapter.Scheduler` project runs a Quartz.NET job (`ProcessOutboxJob`) on a configurable interval (`Outbox:IntervalInSeconds`, `Outbox:BatchSize`).

### Job execution flow

1. Fetch unprocessed `OutboxMessage` rows ordered by `OccurredOnUtc`, limited to `BatchSize`.
2. For each message:
   - Resolve the .NET `Type` from `OutboxMessage.Type`.
   - Deserialise `OutboxMessage.Content` into the concrete domain event.
   - Find all registered `IDomainEventHandler<T>` for that event type.
   - Call each handler's `HandleAsync()` method.
   - Mark the message processed (`ProcessedOnUtc = now`).
   - If a handler throws, log the error and store it in `OutboxMessage.Error`.
3. `SaveChanges` + `Commit` — processing is also wrapped in a transaction.

### Handler discovery

Handlers are discovered via reflection at startup from the `App.Processor` assembly and registered as `Scoped` by `App.Processor.ServiceRegistrar`. At runtime each handler is resolved from a fresh `IServiceScope` for correct DI lifetime management.

### Concurrency safety

`[DisallowConcurrentExecution]` on the job class ensures only one processor instance runs at a time, preventing duplicate event processing without distributed locks.

---

## Domain Event Handler Contract

Handlers extend `DomainEventHandler<T>` (from `Core.Library/Contracts/DomainEvents/Handle/`) and implement a single `HandleAsync(TDomainEvent, CancellationToken)` method.

Handlers live in `App.Processor/DomainEvents/<Context>/<Aggregate>/` and follow the naming pattern `{Entity}{Verb}DomainEventHandler`.

**Handlers are async use cases.** They follow the same transactional pattern as HTTP use cases:
- Inject `ITransactionalUnitOfWork`, the relevant entity services, and `ILogger<T>`.
- Wrap the entire body in `_unitOfWork.ExecuteTransactionAsync`.
- Call services, never repositories directly.
- Audit-log entries are written inside the entity services the handler calls (via the audit-log repository — hard rule 4), not by the handler. A service's audit-log write is non-fatal — it logs a warning with a trace id and returns the service result.

**DI registration:** `App.Processor.ServiceRegistrar` scans the `App.Processor` assembly, finds every `IDomainEventHandler`, and registers each with `TryAddScoped`. Adding a new handler file is enough — no explicit per-handler registration.

---

## Error Handling

- If a handler throws, the error is stored in `OutboxMessage.Error` and the message is marked processed — preventing infinite retry loops.
- If the batch fetch itself fails, the transaction rolls back and the job logs the failure via the `Core.Library` domain exception.
- If event serialisation fails during the UoW commit, the entire transaction (business data + outbox) rolls back.

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| Outbox in the same database | Eliminates distributed-transaction complexity; guarantees at-least-once delivery |
| JSON serialisation | Human-readable, easy to debug, sufficient for event payloads |
| Assembly-qualified type name | Enables polymorphic deserialisation without a separate type registry |
| Configurable batch size | Prevents overloading the system under high event volume |
| Separate scheduler process | Decouples event processing from API request latency |
| `DisallowConcurrentExecution` | Prevents duplicate processing without distributed locks |
