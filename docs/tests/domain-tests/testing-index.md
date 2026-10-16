# Domain Tests — Index

**Purpose:** the complete reference for **domain tests** — pure unit tests of the code that holds business rules without collaborators: the entities (and, once added, value objects) in `Core.Domain`, and the validators and result pattern in `Core.Library`.

**Projects:** `api/test/Core.Domain.Tests` (entities, value objects) · `api/test/Core.Library.Tests` (validators, result pattern).

---

**General rules**
- Write unit tests covering every public method of every entity, value object, validator, and result-pattern type.
- Test every business-logic scenario — success and failure.
- Follow the Arrange-Act-Assert (AAA) pattern.
- Test edge cases and validation boundaries.
- Verify the result pattern (`Result` / `Result<T>`) **and** the resulting entity state.

---

**CRITICAL:** read **every** document of this set in full before writing or changing a domain test. These documents extend the API guidelines (`docs/api/`) and rank below them — `docs/api/hard-rules.md` and `docs/api/coding-standards.md` win over anything here.

## Table of contents

### Foundation & philosophy
- **[Testing Philosophy](./testing-philosophy.md)** — core principles, what to test, what not to test.
- **[Testing Tech Stack](./testing-tech-stack.md)** — xUnit v3, AwesomeAssertions, .NET 10; project setup.

### Project structure & naming
- **[Testing Structure](./testing-structure.md)** — folder organisation, partial test classes, Success/Failure files.
- **[Naming Conventions](./testing-naming-conventions.md)** — `{MethodUnderTest}_{Scenario}_{ExpectedResult}`.

### Critical rules
- **[Testing Critical Rules](./testing-critical-rules.md)** — DO / DON'T list, state checks, status codes.

### Test patterns
Patterns are organised by building block, each with Success and Failure scenarios.

**Critical note:** the patterns are the baseline. When an entity has its own business rules, investigate the entity in depth and add the tests (and helper methods) those rules need.

- **Entity patterns:** [testing-patterns-entity.md](./testing-patterns-entity.md)
- **Value object patterns:** [testing-patterns-value-object.md](./testing-patterns-value-object.md)
- **Validator & result-pattern patterns:** [testing-patterns-validation.md](./testing-patterns-validation.md)

---

## How to use this index

1. Start with **[Testing Philosophy](./testing-philosophy.md)** for the testing mindset.
2. Check **[Testing Structure](./testing-structure.md)** for file organisation.
3. Read **[Naming Conventions](./testing-naming-conventions.md)** for test method names.
4. Study the **test patterns** linked above for the Success/Failure split and the expected scenarios.
5. Always check **[Testing Critical Rules](./testing-critical-rules.md)** before writing tests.

Service tests have their own set: [`../service-tests/testing-index.md`](../service-tests/testing-index.md). Architecture tests are planned for **phase 2** and do not exist yet.
