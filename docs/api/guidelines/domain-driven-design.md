# Domain-Driven Design

The DDD decisions that govern the `Core.Domain` layer. This document explains *why* the domain is modelled the way it is; `structure.md` shows *where* each building block lives, and `coding-standards.md` fixes the concrete syntax.

---

## Bounded Context

The solution defines a single bounded context. Within it, the domain is divided into two sub-domains:

- **Business sub-domain** — the project's own domain: the entities and relationships that represent what the application manages. The codebase currently ships a single sample entity (`Note`, under the `Samples` context) that demonstrates every convention; it is replaced by the project's real domain.
- **System sub-domain** — cross-cutting infrastructure entities (e.g. `AuditLog`, `OutboxMessage`) that are not part of the business model but live in the domain layer because they follow the same entity lifecycle rules.

This separation keeps infrastructure concerns out of the business model while holding both to the same domain rules.

---

## Tenancy and users

**Single-tenant.** The project has no tenant model and no infrastructure for tenant isolation: entities carry no `TenantId`, repositories take no tenant parameter, and there is no tenant-scoped query filter. Each deployment serves a single organisation.

**User-count agnostic, no per-user ownership.** Authentication is provided by the external identity provider; how many users exist (self-registration, admin-created accounts) is identity-provider configuration, not domain code. Data is **not** owned per user — entities carry no `OwnerId`, and every authenticated user reaches the same data.

Introducing multi-tenancy or per-user ownership is a deliberate architectural decision for the project — it is **not** assumed anywhere in the current domain (hard rule 11; see the critical notes in `structure.md`: extend the convention deliberately, never improvise it).

---

## Entity Design Decisions

**Private constructor + static factory**
Entities cannot be constructed directly. All creation goes through a static `Create()` method, so validation always runs before an instance is returned to any caller. An entity that fails validation never exists.

**Private setters**
All entity properties have private setters. State changes happen only through explicit behaviour methods (`UpdateTitle`, `SoftDelete`, …), so an entity can never be mutated accidentally by assigning to a property.

**Partial class split**
Each entity is split across two files: one for state and behaviour (`{Entity}.cs`), one for validation (`{Entity}.Validate.cs`). The main file stays focused on what the entity does rather than how it validates itself.

**`BaseEntity` in Core.Library**
All entities inherit from `BaseEntity`, which provides a `Guid` identity and the domain-event infrastructure. `BaseEntity` lives in `Core.Library` (not `Core.Domain`) so it is reusable across solutions without pulling in domain-specific code.

**GUID identity**
Entities use `Guid` as their identifier, so identity can be assigned by the application *before* persistence. This is required by the outbox pattern and by domain events that reference the entity id.

**Null model arguments are developer errors, not validation failures**
A factory or behaviour method that accepts a domain model guards it on its very first line with `ArgumentNullException.ThrowIfNull(model)`. A `null` model is a programming mistake, not user input — it surfaces as an unhandled exception (HTTP 500 via the global handler), never as a validation `Result`. Validation `Result`s are reserved for invalid *values*, not missing arguments.

**No created/updated tracking columns on entities**
Entities do **not** carry `CreatedOnUtc` / `UpdatedOnUtc` (or similar) audit-tracking properties. Every mutation is already recorded in the audit log (hard rule 4), which is the single authoritative history of what changed and when — duplicating that on the entity is redundant and forbidden. This is distinct from a timestamp that is *intrinsic domain data* for a System sub-domain entity (e.g. `OutboxMessage.OccurredOnUtc`), which models the entity's own meaning rather than generic change tracking.

**`ValidateState(successMessage)` is the standard validation entry point**
Every entity exposes a private `ValidateState(string successMessage)` in its `{Entity}.Validate.cs` partial. Each behaviour method mutates state and then `return ValidateState("…")`, so an invalid mutation can never be returned as success. The static `Create()` factory validates through the static `Validate(entity)` before returning. Both `Validate` and `ValidateState` live in `{Entity}.Validate.cs`; the main `{Entity}.cs` file holds no validation logic.

**Validation internals — `ValidateProperties` + `ValidateBusiness` + `CombineValidationErrors`**
`Validate(entity)` collects a `List<Result>`, fills it through two helpers, and returns `Result.CombineValidationErrors(results)` (which aggregates every failure into one result): `ValidateProperties` checks each primitive property, and `ValidateBusiness` checks cross-field / business invariants (left empty when there are none). Primitive properties are validated through the `Core.Library/Validators` extension methods — e.g. `note.Title.Validate(fieldName: nameof(note.Title), maxLength: 200)` and `note.Content.Validate(..., isRequired: false)` — never with ad-hoc inline `if` checks.

---

## Value Object Design Decisions

**`sealed record` as the base type**
Value objects are C# records, giving structural equality by default — two value objects with the same property values are equal. `sealed` prevents extension.

**Same private-constructor + static factory pattern as entities**
Value objects follow the same construction discipline; they cannot be created in an invalid state.

**`init`-only properties**
Value-object properties are set only at construction. Once created, a value object is immutable — to "change" a value you create a new instance.

**Separate validation partial file**
Value objects use the same partial-class split as entities (`{ValueObject}.cs` + `{ValueObject}.Validation.cs`) to keep validation logic isolated and easy to find.

---

## Aggregate Decisions

**No formal aggregate-root pattern**
There is no strict aggregate-root hierarchy. Each entity manages its own consistency boundary. Referential integrity between entities (e.g. a parent-child relationship) is enforced in the service layer before the entity is mutated, not inside an aggregate.

**Why**
A strict aggregate-root pattern adds significant complexity (nested hydration, cross-aggregate reference rules) that is not justified by the relatively flat entity relationships here. Each entity is effectively its own aggregate root, responsible only for its own invariants.

**Consistency rule**
Deleting or modifying an entity that others depend on requires a guard check in the service layer before the operation proceeds. The entity itself does not know about these dependencies.

---

## Domain Event Decisions

**Eventing is feature-driven, not default**
Whether a state change emits a domain event is decided from the feature request itself. If the requirement describes an asynchronous reaction (notification, projection, downstream side effect), add a domain event for that state change **and** an `IDomainEventHandler<…>` for it in the same change. If the feature does not call for eventing, do not add an event class and do not call `Raise(...)`. Event and handler are introduced together or not at all — never one without the other. Events are named after the fact: `{Entity}CreatedDomainEvent`, `{Entity}{Property}UpdatedDomainEvent`.

**Events raised inside the entity (when raised at all)**
When a state change *does* produce an event, it is the entity that calls `Raise(...)` at the point of mutation — not the service or the use case. This guarantees any state change that carries an event always emits it.

**Collected, not dispatched immediately**
Events are not dispatched inline. They accumulate in the entity's `DomainEvents` collection; the Unit of Work turns them into `OutboxMessage` rows on commit, enabling reliable asynchronous processing without distributed transactions. See `event-driven-design.md` for the full pipeline.

---

## Repository Interface Decisions

**Interfaces defined in Core.Domain, not Core.Library**
Each entity's repository interface (`I{Entity}Repository`) lives next to the entity in `Core.Domain/Entities/<Context>/<Aggregate>/Interfaces/`. Generic base contracts live in `Core.Library/Contracts/GenericRepositories/` and are extended per entity.

**Generic base contracts**
Entity repositories extend the generic repository contract from `Core.Library` for shared CRUD/query shape; entity-specific queries are declared on the per-entity interface. Because the project is single-tenant, there is no tenant-scoped repository variant — repositories are not parameterised by tenant or user.

---

## Model (DTO) Decisions

**`CreateModel` and `UpdateModel` belong to Core.Domain**
Models that pass data into entity factory and update methods are defined in `Core.Domain` alongside the entity (`Models/`). They are part of the domain contract, not the API layer.

**Both models use the same shape — property-syntax records with `required` on every field**
`{Entity}CreateModel` and `{Entity}UpdateModel` never diverge in style: both are property-syntax `sealed record`s (`{ get; init; }`) carrying the C# `required` keyword on **every** field, for compile-time mapper safety. They differ only in nullability semantics — a `CreateModel` field's nullability matches its real optionality (a mandatory field is non-nullable), whereas every `UpdateModel` field is additionally nullable for partial-update semantics (below). A positional `CreateModel` is non-compliant: it breaks the shared shape and loses per-field mapper safety.

**Domain result messages are the audit trail**
The message a domain `Create()`/behaviour/`SoftDelete()` writes into its `Result` is persisted verbatim as the audit-log description (the service passes `Result.Message` straight through — see `coding-standards.md`). Domain result messages must therefore be meaningful, stable, and non-leaky — they are read by whoever inspects the audit log.

**`UpdateModel` field shapes — `required` + nullable**
Every updatable field on an `UpdateModel` is declared both `required` **and** nullable — e.g. `required string? Title`, `required bool? IsPublished`. The two keywords serve different purposes and must always appear together:

- `required` — compile-time mapper safety: every field must be explicitly assigned in the object initialiser, so no mapping can silently drop a property.
- **Nullable** — partial-update semantics at runtime: `null` means "not supplied — do not change this field." The service skips null fields; the caller only provides what it wants to change.

An `UpdateModel` that omits `required` on its nullable fields is non-compliant — the compiler cannot then catch a missing mapping and a field can be silently dropped.

**No validation in models**
Models carry no validation logic or data annotations. Validation is the entity's responsibility; models are simple data carriers.

---

## Soft Delete Decisions

**Marker interface, not a base class**
Soft-delete behaviour is opt-in via the `IHaveSoftDelete` interface. Not every entity supports soft delete — linking/relationship entities use hard delete instead.

**`IsDeleted` is an explicit domain state**
Soft deletion is a business operation (with a domain event when the feature calls for one), not a technical flag silently set by infrastructure. Calling `SoftDelete()` expresses intent in the domain and can trigger downstream reactions.

**State-changing methods guard against a deleted entity**
Every mutating behaviour method on a soft-deletable entity checks `IsDeleted` before mutating and returns a failure `Result` when the entity is already deleted — a soft-deleted entity is logically gone and cannot be updated. `SoftDelete()` guards the same way against a double-delete.
