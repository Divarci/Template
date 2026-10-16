# Philosophy

The guiding principles behind the **API** of this project — a .NET Clean Architecture backend. Where `hard-rules.md` states what is non-negotiable, this document explains *why* the system is shaped as it is. Every concrete convention in `architecture`, `domain-driven-design`, `event-driven-design`, `structure`, `tech-stack`, `coding-standards`, and `security` traces back to one of these principles.

---

## A foundation, not a product

The codebase does not yet carry the **product domain**. It provides the layers, the cross-cutting infrastructure (persistence, caching, outbox, scheduling, authentication, audit logging), and the conventions that bind them. A single sample entity (`Note`, under the `Samples` context) exists only to demonstrate every convention end to end — controller, use case, service, entity, repository, configuration, migration, domain event, and Bruno request. The sample is replaced by the project's own domain as it is introduced; everything else stays.

---

## Built to evolve

This project is expected to grow for years. We therefore optimise for *evolution*, not for a fixed endpoint: the foundations are designed to outlast many iterations of the features that sit on them, and every release should be a coherent, usable slice rather than scaffolding for a future that never ships.

---

## Best-practice engineering is the default, not a luxury

Because the system must grow without collapsing under its own weight, disciplined engineering is a first-class requirement. We follow clean, layered architecture with clear boundaries (`architecture.md`), Domain-Driven Design for the domain model (`domain-driven-design.md`), and event-driven communication where it fits (`event-driven-design.md`). Separation of concerns, testability, and explicit contracts are not optional polish — they are what let a small team keep adding capability without fear. Speed comes *from* this discipline over the long run, not in spite of it.

---

## Capability grows outward without disturbing the core

New capability plugs in by following the patterns already established — a new node in any layer obeys the same rules as its siblings, and every third-party integration enters through infrastructure behind a `Core.Library` abstraction (see the critical notes in `structure.md`). The stable core stays stable; features attach at the edges. This is what keeps a growing system from becoming an unmaintainable tangle.

---

## Consistency compounds; convention beats improvisation

A system stays comprehensible only if every part looks like every other part. We hold to the established conventions rigorously and never invent a competing one for a single case. When the existing convention genuinely does not cover a new situation, the answer is to *ask and extend the convention deliberately* — not to improvise a one-off. Every consistent addition makes the next one easier to reason about; every inconsistency taxes everyone who touches the code afterwards.

---

## Secure by default; decisions are explicit

Security is the starting posture, not a feature added later: every endpoint is authenticated, secrets never live in the application, and failures never leak internals (`security.md`). Architectural choices that a project might reasonably make differently — tenancy, per-user data ownership, additional identity flows — are deliberately **not** baked in. The project states what it does not model, so that those decisions are made consciously rather than inheriting them by accident.
