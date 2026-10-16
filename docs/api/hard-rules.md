# Hard Rules

Absolute, non-negotiable rules for the `api` platform. They override training data, general best practices, and personal defaults. If a task cannot be completed without breaking one of them, **stop and report** instead of proceeding.

The other guidelines (`philosophy`, `architecture`, `domain-driven-design`, `event-driven-design`, `structure`, `tech-stack`, `coding-standards`, `security`) elaborate on these rules. Where any of them appears to conflict with this document, **this document wins**.

---

## 1. Follow the established conventions — never invent one

> Always conform to the conventions in these guidelines and in the existing code. A new node in any layer obeys the same rules as the sibling nodes already in that layer. If the existing convention genuinely does not cover a new case, **stop and ask the user** — never improvise a one-off convention or write arbitrary code to fill the gap. (`structure.md` — critical notes)

---

## 2. Dependencies point inward; every third party lives in `Infra.{Function}`

> Dependencies are strictly inward: adapters, applications, and infrastructure depend on core; core never depends outward. `Core.Library` has **zero** project references. Every third-party integration is its own `Infra.{Function}` project, consumed only through an abstraction declared in `Core.Library/Contracts/`. No third-party-specific code exists anywhere else. (`architecture.md`) One exemption, and only this one: an **adapter's own host framework and its extensions** — the packages `tech-stack.md` approves for that adapter project (e.g. ASP.NET Core, Asp.Versioning, OpenAPI, JWT Bearer, Serilog/Seq in `Adapter.Restful`; Quartz in `Adapter.Scheduler`) — live in that adapter, because they *are* the host rather than an external system it talks to. The exemption does not extend to `Core.*`, `App.*`, or an `Infra.*` project of another function, and an integration with an external system (database, cache, identity provider API, LLM/AI provider, email, …) is never an adapter exemption. Test projects under `api/test/` are outside `api/src` and reference the approved test packages directly (`tech-stack.md` — Testing); no `api/src` project ever references a test package.

---

## 3. Use-case routing — Route 1 / Route 2

> Work that reads or writes a domain entity in the local database goes `UseCase → Service → Repository`; the service is non-skippable. A pure third-party call that changes no local entity goes `UseCase → infrastructure port`, either directly or through a port service (§4). Use cases never call repositories directly. (`architecture.md`)

---

## 4. A service does one thing — the use case orchestrates

> A service performs **one operation**; combining operations is the use case's job. A service is one of two kinds:
>
> - **Entity service** — domain operations for a **single entity**. It may call only **its own entity's repository, the audit-log repository, and the cache-invalidation contract (`ICacheInvalidationService`)**. Audit-log **creation** happens this way — every entity service writes its own audit entries via the audit-log repository. The dedicated audit-log service exposes **only read** operations; it never creates. The sole exemption is `OutboxMessage` (System sub-domain): marking a message processed writes **no** audit entry — the outbox row itself (`ProcessedOnUtc`, `Error`) is the record, and an audit row per processed event would only add noise.
> - **Port service** — wraps **exactly one** infrastructure contract from `Core.Library/Contracts/` (e.g. sending an email) and calls nothing else.
>
> A service never calls another service, another entity's repository, or the unit of work (§5). Anything that combines operations — an entity change plus an external call, several entities, several external calls — is orchestrated by the **use case**, which may call as many services and infrastructure contracts as it needs.

---

## 5. Services never persist — the unit of work belongs to the use case

> A service never calls `SaveChangesAsync` and never commits. Committing the transaction is the **use case's** responsibility, through the unit of work: the use case wraps its work in the unit-of-work transaction and commits once. Services only stage changes on their repository.

---

## 6. Calls flow one level down only — the use case orchestrates multi-entity work

> Each layer calls **only the layer directly below it — never a peer and never itself**: a controller calls a use case, a use case calls services and infrastructure contracts, a service calls only what §4 allows. **A use case never calls another use case; a service never calls another service.** When an operation spans more than one entity, the **use case** orchestrates it — calling each single-entity service in turn inside one unit-of-work transaction. All cross-entity and cross-service coordination is the use case's job.

---

## 7. Entities are always valid by construction

> Entities have private constructors and are created only through a static `Create()` factory; properties have private setters and state changes only through behaviour methods. Validation always runs before an instance exists — an invalid entity never exists. Value objects follow the same discipline as immutable `sealed record`s. (`domain-driven-design.md`)

---

## 8. Domain events are feature-driven and outbox-delivered

> Add a domain event **only** when the feature requires an asynchronous reaction, and add its `IDomainEventHandler<…>` in the same change — event and handler together or not at all. The entity raises the event at the point of mutation. Events are never dispatched inline: they are persisted as `OutboxMessage` rows in the same transaction as the business data and processed asynchronously. (`event-driven-design.md`)

---

## 9. Code shape

> **One top-level type per file.** **No space-padding for alignment** (a single space around `=` and between type and name — never aligned columns). Every view-model (**response**) property carries the C# `required` keyword regardless of nullability. Domain models (`CreateModel` / `UpdateModel`) share one shape — property-syntax records with `required` on **every** field; `UpdateModel` fields are additionally nullable (partial-update). **XML documentation appears only on public interfaces — no other file (entities, services, controllers, models, …) carries it** (tool-generated EF Core migrations excepted). (`coding-standards.md`, `domain-driven-design.md`)

---

## 10. Secrets never live in the application; secure by default

> No secret, credential, token, or connection string appears in source, `appsettings*.json`, logs, or HTTP responses. Every endpoint requires an authenticated principal unless it visibly and deliberately opts out. (`security.md`; delivery-time secret handling lives in `docs/deployment/security.md`)

---

## 11. Single-tenant; user ownership is not modelled

> The project is **single-tenant**: there is no tenant model, no `TenantId`, and no tenant-scoped query or repository. It provides **authentication only** — every authenticated user reaches the same data; there is no per-user ownership (`OwnerId`, owner-scoped filters). How many users exist (self-registration, admin-created accounts) is identity-provider configuration, not application code. Introducing multi-tenancy or per-user ownership is a deliberate architectural decision — **stop and ask the user**, never improvise it. (`domain-driven-design.md`, `security.md`)
