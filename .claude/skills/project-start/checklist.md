# Project Start — change checklist

The fixed, known set of changes that turns the template into a named project. `SKILL.md` fills the placeholders from the user's inputs, shows the filled rows for approval, and dispatches each owner with "apply your rows". Nothing outside this file changes during a conversion; a `template` hit outside it is reported (see Verification), never improvised.

## Placeholders

| Placeholder | Source | Example |
|---|---|---|
| `{Name}` | input — project name, PascalCase | `Acme` |
| `{name}` | derived — `{Name}` with a lowercase first letter (C# identifier prefix) | `acme` |
| `{slug}` | input — lowercase, digits, hyphens | `acme` |
| `{Description}` | input — one line | `Order management API for Acme` |
| `{DeployBranch}` | input — keep `master` or change | `master` |

Fixed — never asked, never changed by this skill: `AGENT_POOL` stays `HomerServer` (the one self-hosted pool every project deploys to); `REPLACE_WITH_PROD_KEYCLOAK_HOST` stays a placeholder until production is provisioned (Step 7 manual steps); `PROJECT_DIR` is derived as `/srv/{slug}`.

Derived identifiers: `/srv/{slug}` (checkout on the self-hosted machine), `{Name}Exception`, `{Name}.slnx`, `realm-{slug}.json`, realm `{slug}`, client `{slug}-public`, audience `{slug}-api`, audience mapper `{slug}-api-audience`, network `{slug}-network`, database `{slug}`, Compose project `{slug}`, volume prefix `{slug}_`.

---

## Owner: main agent (governance files — no sub-agent owns them)

| # | File | Change |
|---|---|---|
| M1 | `CLAUDE.md` | Line 3: `**Project:** Template — a .NET Clean Architecture API template with an agent-driven Claude Code workflow` → `**Project:** {Name} — {Description}` |
| M2 | `.claude/agents/reviewer.md` | `dotnet test Template.slnx` → `dotnet test {Name}.slnx` |
| M3 | `.claude/agents/tester.md` | Every `Template.slnx` → `{Name}.slnx` (2 lines) |
| M4 | `docs/api/philosophy.md` | Insert the approved brief as `## What this project is` directly after the intro paragraph; rename `## A foundation, not a product` → `## A foundation for the product` and reword its first sentence from "The codebase does not yet carry the **product domain**. It provides …" to "The codebase provides …; {Name}'s product domain (above) is built on top of them." Keep every other section intact. |
| M5 | `docs/api/structure.md` | `Template.slnx` → `{Name}.slnx` |
| M6 | `docs/api/tech-stack.md` | Only if the brief names an integration or platform not yet in the stack: add a `○` (planned) row in the matching section — one row per named item, wording from the brief, no vendor guesses. |
| M7 | `docs/api/coding-standards.md`, `domain-driven-design.md`, `event-driven-design.md`, `security.md` | `TemplateException` → `{Name}Exception` |
| M8 | `docs/tests/domain-tests/*.md`, `docs/tests/service-tests/*.md` | `TemplateException` → `{Name}Exception`; `Template.slnx` → `{Name}.slnx` |
| M9 | `docs/bruno/structure.md` | Tree root `Template/` → `{Name}/` (keep the column alignment of the `←` comments) |
| M10 | `docs/bruno/tech-stack.md` | dev realm `template` → `{slug}`, public client `template-public` → `{slug}-public` |
| M11 | `docs/deployment/architecture.md` | `template_postgres-data` → `{slug}_postgres-data` |
| M12 | `docs/deployment/coding-standards.md` | `template-network` → `{slug}-network` (2 places), `name: template` → `name: {slug}` |
| M13 | `docs/deployment/structure.md` | Tree root `Template/` → `{Name}/`; `Template.slnx` → `{Name}.slnx`; `realm-template.json` → `realm-{slug}.json` (tree + placement table); `Template-Api` → `{Name}-Api` |
| M14 | `docs/deployment/tech-stack.md` | `Template.slnx` → `{Name}.slnx` |
| M15 | `Template.slnx` (content only — the rename is C2) | `keycloak/realm-template.json` → `keycloak/realm-{slug}.json` |

## Owner: `coder` (application code)

| # | File | Change |
|---|---|---|
| C1 | `api/src/core/Core.Library/Exceptions/TemplateException.cs` | Rename file and class to `{Name}Exception`; update `className: nameof(…)` inside if present |
| C2 | `Template.slnx` | Rename the file to `{Name}.slnx` (content change M15 is the main agent's; coordinate: coder renames after M15 is applied) |
| C3 | Every `api/src` file that uses the exception — currently: `Adapter.Restful/AspNetCore/Diagnostics/GlobalExceptionHandler.cs` (also the local `templateException` → `{name}Exception`), `Adapter.Restful/AspNetCore/Helpers/ApiResultHelper.cs`, `Adapter.Restful/Controllers/BaseController.cs`, `Adapter.Scheduler/Jobs/ProcessOutboxJob.cs`, `App.Processor/DomainEvents/OutboxProcessor.Private.cs`, `App.Processor/DomainEvents/Samples/Notes/NoteCreatedDomainEventHandler.cs`, `Core.Domain/Entities/Samples/Notes/Note.cs`, `Core.Library/ResultPattern/Metadata.cs`, `Core.Library/ResultPattern/ResultExtensions.cs`, `Core.Services/System/OutboxMessages/OutboxMessageService.Update.cs`, `Infra.Caching/ServiceRegistrar.cs` | `TemplateException` → `{Name}Exception`. Confirm the list with a grep before editing; a file not listed here is still in scope (it is the same rename), report it so the checklist is updated. |

Validation: `dotnet build {Name}.slnx` — 0 errors.

## Owner: `tester` (test code)

| # | File | Change |
|---|---|---|
| T1 | Every `api/test` file that uses the exception — currently: `Core.Domain.Tests/Entities/Samples/Notes/Create/NoteTests.Create.Failure.cs`, `Core.Library.Tests/ResultPattern/Metadata/Constructor/MetadataTests.Constructor.Failure.cs`, `Core.Library.Tests/ResultPattern/PaginatedResultOfT/Success/PaginatedResultOfTTests.Success.Failure.cs`, `Core.Library.Tests/ResultPattern/ResultExtensions/MapTo/ResultExtensionsTests.MapTo.Failure.cs`, `Core.Services.Tests/Samples/Notes/Create/CreateAsync/NoteServiceTests.CreateAsync.Failure.cs`, `Core.Services.Tests/System/OutboxMessages/Update/MarkProcessed/OutboxMessageServiceTests.MarkProcessed.Failure.cs` | `TemplateException` → `{Name}Exception`, including test method names such as `…_ThrowsTemplateException` → `…_Throws{Name}Exception`. Confirm the list with a grep; report unlisted files. |

Validation: `dotnet test {Name}.slnx` — all tests pass.

## Owner: `deployer` (delivery assets)

| # | File | Change |
|---|---|---|
| D1 | `docker-compose.yml` | `name: template` → `name: {slug}`; `/realms/template` → `/realms/{slug}` (2); `template-api` → `{slug}-api` (2); `template-network` → `{slug}-network` (3: two services + top-level network). `REPLACE_WITH_PROD_KEYCLOAK_HOST` stays. |
| D2 | `docker-compose.override.yml` | `Database=template` → `Database={slug}` (2); `/realms/template` → `/realms/{slug}` (2 env values + 1 comment); `template-network` → `{slug}-network` (4); `POSTGRES_DB: template` → `POSTGRES_DB: {slug}`; `pg_isready -U postgres -d template` → `… -d {slug}` |
| D3 | `keycloak/realm-template.json` | Rename to `realm-{slug}.json`; `"realm": "template"` → `"{slug}"`; `"displayName": "Template"` → `"{Name}"`; `"clientId": "template-public"` → `"{slug}-public"`; `"name": "Template Public Client (PKCE + direct grant)"` → `"{Name} Public Client (PKCE + direct grant)"`; mapper `"template-api-audience"` → `"{slug}-api-audience"`; `"included.custom.audience": "template-api"` → `"{slug}-api"` |
| D4 | `pipelines/azure-pipelines-api-build-and-test.yml` | `solution: 'Template.slnx'` → `'{Name}.slnx'`; `displayName: 'Restore, build, test Template.slnx'` → `'… {Name}.slnx'` |
| D5 | `pipelines/azure-pipelines-ci-cd.yml` | `PROJECT_DIR: '/srv/template'` → `'/srv/{slug}'`; `displayName: 'Deploy Template'` → `'Deploy {Name}'`; if `{DeployBranch}` changed: `DEPLOY_BRANCH` **and** the literal under `trigger: branches: include:` (Azure DevOps does not expand variables in triggers). `AGENT_POOL: 'HomerServer'` stays. |

Validation: `docker compose config` (dev merge) and `docker compose -f docker-compose.yml config` (prod) pass; the realm file is valid JSON; no destructive Docker command.

## Owner: `bruno` (API collection)

| # | File | Change |
|---|---|---|
| B1 | `bruno/bruno.json` | `"name": "Template"` → `"{Name}"` |
| B2 | `bruno/environments/local.bru` | `/realms/template/` → `/realms/{slug}/` (2); `clientId: template-public` → `{slug}-public` |

Validation: every `{{var}}` used by the collection is still defined in `local.bru`; no request route changes.

---

## Leave unchanged — the word means something else

| File | Text |
|---|---|
| `.claude/agents/deployer.md` | "environment templates" |
| `docs/deployment/hard-rules.md` §1 | "environment templates" |
| `pipelines/azure-pipelines-ci-cd.yml` | "Template expression (compile time)" (Azure Pipelines term) |
| `docs/tests/service-tests/testing-index.md` | "base partial file template" |
| `api/src/infrastructures/Infra.Persistence/Migrations/**` | never edited (contain no project name) |
| `.claude/skills/project-start/**` | this skill |

---

## Verification (after all owners finish)

1. Marker check (SKILL.md Step 1): every marker must be in the new-project state.
2. `grep -ri template` over the repo (excluding `.git`, `.vs`, `bin`, `obj`, `Migrations`, this skill): the only hits allowed are the **Leave unchanged** rows above. Any other hit means the template gained an occurrence this checklist does not know — report it as a checklist gap; do not fix it ad hoc.
3. `dotnet build {Name}.slnx` and `dotnet test {Name}.slnx` pass; both compose validations pass.
4. `docs/api/philosophy.md` carries the approved brief verbatim; no other doc change was made beyond rows M1–M15.
