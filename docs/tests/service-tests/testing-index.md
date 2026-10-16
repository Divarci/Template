# Service Tests — Index

**Purpose:** the complete reference for **service tests** — unit tests of the entity services in `Core.Services`, with every collaborator mocked.

**Project:** `api/test/Core.Services.Tests`.

---

**General rules**
- Write unit tests covering every public method of each service interface (`I{Entity}Service`).
- Always investigate the domain and its business rules first, so the helper methods build realistic entities and states.
- Test every business-logic scenario — success and failure.
- Mock every collaborator the service takes: its own repository, the audit-log repository, `ICacheInvalidationService`, and `ILogger<T>`. Services never touch the unit of work (`docs/api/hard-rules.md` §5), so there is nothing transactional to mock.
- Follow the Arrange-Act-Assert (AAA) pattern.
- Test edge cases and the short-circuit behaviour on every failure.
- Verify the result pattern and the status codes.

---

**CRITICAL:** read **every** document of this set in full before writing or changing a service test. These documents extend the API guidelines (`docs/api/`) and rank below them — `docs/api/hard-rules.md` and `docs/api/coding-standards.md` win over anything here.

## Table of contents

### Foundation & philosophy
- **[Testing Philosophy](./testing-philosophy.md)** — core principles, what to test, testing mindset.
- **[Testing Setup](./testing-setup.md)** — the base partial file: mock setup, system under test, helper methods.
- **[Testing Tech Stack](./testing-tech-stack.md)** — xUnit v3, AwesomeAssertions, Moq, .NET 10; project setup.

### Project structure & naming
- **[Testing Structure](./testing-structure.md)** — folder organisation, partial test classes, file naming.
- **[Naming Conventions](./testing-naming-conventions.md)** — `{MethodUnderTest}_{Scenario}_{ExpectedResult}`.

### Critical rules
- **[Testing Critical Rules](./testing-critical-rules.md)** — DO / DON'T list, side-effect verification, status codes.

### Test patterns (CRUD & collections)
Patterns are organised by operation, each with its Success and Failure scenarios.

**Critical note:** these are the baseline for every service. When a domain has its own business rules, investigate it in depth and add the scenarios (and helpers) those rules need.

- **[Create Pattern](./testing-patterns-create.md)** — success: entity returned, repository create, audit entry, collections invalidated. Failure: validation error, no side effects.
- **[Update Pattern](./testing-patterns-update.md)** — success: fields updated, repository update, one audit entry per field, cache invalidated; empty model is a no-op. Failure: not found, conflict, validation error.
- **[Delete Pattern](./testing-patterns-delete.md)** — success: soft-delete flag, repository update, audit entry, cache invalidated. Failure: not found, already deleted.
- **[Single Query Pattern](./testing-patterns-single.md)** — success: mapped entity, no side effects. Failure: not found.
- **[Collection Pattern](./testing-patterns-collection.md)** — pagination in a single file: parameters passed through, mapping, metadata, failures, no side effects.

### Mocking & verification
- **[Moq Patterns](./testing-moq-patterns.md)** — `Setup` / `Returns` / `ReturnsAsync`, `Verify` syntax, `It.IsAny<T>`, `It.Is<T>`, callbacks.

---

## How to use this index

1. Start with **[Testing Philosophy](./testing-philosophy.md)** for the testing mindset.
2. Review **[Testing Setup](./testing-setup.md)** for the base partial file template.
3. Check **[Testing Structure](./testing-structure.md)** for file organisation.
4. Read **[Naming Conventions](./testing-naming-conventions.md)** for test method names.
5. Study the **test patterns** ([Create](./testing-patterns-create.md), [Update](./testing-patterns-update.md), [Delete](./testing-patterns-delete.md), [Single](./testing-patterns-single.md), [Collection](./testing-patterns-collection.md)).
6. Use **[Moq Patterns](./testing-moq-patterns.md)** for mocking examples.
7. Always check **[Testing Critical Rules](./testing-critical-rules.md)** before writing tests.

Domain tests have their own set: [`../domain-tests/testing-index.md`](../domain-tests/testing-index.md). Use-case, processor, adapter, and infrastructure tests are out of scope; architecture tests are planned for **phase 2**.
