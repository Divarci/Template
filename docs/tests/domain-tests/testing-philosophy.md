# Testing Philosophy — Domain Tests

## Core principles

1. **Test domain logic first** — the domain layer is the heart of the application and is tested thoroughly.
2. **Pure unit testing** — no mocking; domain logic is tested in isolation.
3. **Fast execution** — no database, network, file system, or clock waits.
4. **Readable** — tests document the business rules and domain behaviour.
5. **Maintainable** — easy to update when a business requirement changes.
6. **Comprehensive** — success paths, edge cases, validation, and business-rule violations.
7. **Result-pattern focus** — every operation returns `Result` or `Result<T>`; tests verify success/failure **and** `StatusCode` (`HttpStatusCode`).

## What to test at the domain layer

### Entities (`Core.Domain/Entities/<Context>/<Aggregate>/`)
- **Creation logic** — the static `Create()` factory.
- **Behaviour methods** — every public state change (`UpdateTitle`, `UpdatePinned`, `SoftDelete`, `MarkProcessed`, …).
- **Validation** — the validation that a factory or behaviour method runs (`Validate` / `ValidateState`), tested **through** the public method.
- **State transitions** — e.g. active → soft-deleted; processed / unprocessed.
- **Business rules** — invariants and guards (e.g. a deleted entity cannot be updated → `409 Conflict`).
- **Soft deletion** — `IsDeleted` management and the guards that depend on it.
- **Relationships** — adding/removing related items, when an entity has them.
- **Domain events** — the events a method raises (`DomainEvents` collection), when the entity raises any.
- **Null-model guard** — a `null` model argument throws `TemplateException` (`docs/api/domain-driven-design.md`).

### Value objects (`Core.Domain/ValueObjects/<ValueObject>ValueObject/`, once added)
- **Creation logic** and its **validation**.
- **Immutability** — properties cannot change after creation.
- **Equality** — value-based equality (`Equals`, `GetHashCode`, `==`).
- **Value-object business rules.**

### Validators & result pattern (`Core.Library/Validators/`, `Core.Library/ResultPattern/`)
- **Validators** — every rule each validator enforces (required, length, format, range, restriction, empty Guid, …); investigate each validator type and define the cases its rules imply.
- **Result pattern** — the factories (`Success` / `Failure`), `IsFailureAndNoData`, `CombineValidationErrors`, `PaginatedResult` metadata, and the `MapTo` extensions.

## What NOT to test at the domain layer

❌ **Infrastructure concerns** — database access, repositories, external services.
❌ **Service orchestration** — that is the service tests' job.
❌ **HTTP/API layer** — endpoints, request/response mapping.
❌ **Framework code** — .NET or library behaviour.
❌ **Private methods** — always test through the public surface.
❌ **Configuration** — app settings, environment variables.
❌ **Cross-cutting concerns** — logging, caching, authentication.

---

**Remember:** domain tests are the foundation of the suite. They are fast, reliable, and document the business logic — invest in them.
