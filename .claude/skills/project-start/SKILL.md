---
name: project-start
description: Turns this repository from the generic "Template" into a concrete project — gives it its own name/identity across code, solution, Docker Compose, Keycloak realm, Bruno collection, CD pipeline, CLAUDE.md and docs, and records the project's subject, scope and planned work in the philosophy docs. Use when the user wants to start a new project from the template, "give the project an identity", rename the template, or invokes /project-start. Refuses to run on a repository that has already been converted.
---

# Project Start — instantiate the template

This skill converts the repository from the generic **Template** into a named project. It runs in four steps, strictly in order. **Never assume or presume anything:** every value, every ambiguous occurrence, and every conflict is asked to the user. Talk to the user in their language.

The skill does not replace `CLAUDE.md`: the work it produces is delegatable work, so the orchestration rules in `CLAUDE.md` (STEP 0 → STEP 3, exclusive domains, delegation briefs, version control) apply in full. The approved plan from Step 3 of this skill is the user's confirmation required by `CLAUDE.md` STEP 0.

**Version control:** never run any command starting with `git` (`docs/version-control.md`) — with one exception: the fresh-history step (4.5), and only after the user has explicitly approved the exact command list shown at that moment. The skill never commits, never adds a remote, and never pushes; it tells the user how to do that at the end.

---

## Step 1 — Is this repository still the template?

Check these **markers** (read-only — use Glob/Grep/Read):

| # | Marker | Template state |
|---|---|---|
| M1 | Solution file at repo root | `Template.slnx` exists |
| M2 | Keycloak realm file | `keycloak/realm-template.json` exists |
| M3 | Domain exception | `api/src/core/Core.Library/Exceptions/TemplateException.cs` exists |
| M4 | `CLAUDE.md` project line | starts with `**Project:** Template` |
| M5 | Compose project name | `docker-compose.yml` has top-level `name: template` |

Decide:

- **All markers in template state** → the repo is the template. Continue to Step 2.
- **No marker in template state** → the repo has already been converted. Tell the user which project name it was converted to (read it from the solution file name / `CLAUDE.md` project line / compose `name:`), explain that this skill only runs on the untouched template, and **stop**. Do not offer to "re-run" or "re-rename" — a second conversion is out of scope.
- **Some markers in template state, some not** → a partial or manual conversion. List exactly which markers are still template and which are not (with the values found), explain that the state is inconsistent, and **stop**. Do not guess how to finish it; the user decides what to do next.

---

## Step 2 — Collect inputs and build the change plan

### 2.1 Inputs — ask, never infer silently

Ask the user for each value. You may *propose* a derived value, but it is used only after the user confirms it.

| Input | Example | Rules |
|---|---|---|
| Project name (PascalCase) | `Acme` | Required. Valid C# identifier, starts with an uppercase letter, no spaces. Must not be `Template`. |
| Slug | `acme` | Required. Lowercase letters, digits, hyphens; starts with a letter. Propose a lowercase/kebab form of the name and ask for confirmation. |
| One-line project description | `Order management API for Acme` | Required — replaces the template description in `CLAUDE.md`. |
| `AGENT_POOL` | `Default` | Ask whether to keep the current value or change it. |
| `DEPLOY_BRANCH` | `master` / `main` | Ask whether to keep or change. If changed, the literal in `trigger:` changes with it (Azure DevOps does not expand variables in triggers). |
| Production Keycloak host | `auth.acme.com` | Ask whether to set it now or keep the `REPLACE_WITH_PROD_KEYCLOAK_HOST` placeholder. |
| Pipeline `PROJECT_DIR` | `/srv/acme` | Ask whether to set the real checkout path now or keep the `/path/to/<Name>` placeholder. |

Derived identifiers (show them to the user as part of the plan): `{Name}Exception`, `{Name}.slnx`, `keycloak/realm-{slug}.json`, realm `{slug}`, public client `{slug}-public`, audience `{slug}-api`, audience mapper `{slug}-api-audience`, network `{slug}-network`, dev database `{slug}`, Compose project name `{slug}`, pattern variable `{camelName}Ex`.

### 2.1a Project brief — subject, scope, planned work (mandatory)

A name alone does not give the project an identity. **Always** ask the user the questions below — even if the one-line description seems to answer them — and never fill a gap from the name, the description, or general knowledge. Ask follow-up questions until every item has an answer the user has stated (or explicitly marked as "not decided yet").

| Item | What to ask |
|---|---|
| **Subject / purpose** | What is the project about? Which problem does it solve, and for whom (users, actors, consumers of the API)? |
| **Scope — in** | Which business capabilities / bounded contexts are inside the project? |
| **Scope — out** | What is explicitly *not* part of the project (non-goals)? |
| **Planned work** | What will be built — the main features / use cases, and (if known) their priority or first release slice? |
| **Constraints & decisions** | Anything that shapes the design: tenancy, per-user data ownership, extra identity flows, integrations, regulatory or performance needs. Ask explicitly; "none" is a valid answer only when the user says it. |

Then write a **draft philosophy section** from the answers (see 2.5) and show it to the user. The user approves, edits, or rejects it; re-present after every change. Use the user's own words for facts — do not embellish, add features, or invent goals.

If an answer conflicts with a `hard-rules.md` of any platform (e.g. a requirement that an endpoint be anonymous), flag it with the doc and section and ask how to proceed — do not resolve it yourself.

### 2.2 Inventory — find every occurrence

Search the whole repository **excluding** `.git/`, `.vs/`, `bin/`, `obj/`, and `api/src/infrastructures/Infra.Persistence/Migrations/` (migrations contain no project name and must never be edited). Also exclude this skill's own folder (`.claude/skills/project-start/`).

Search case-insensitively for `template` and classify **every** hit into exactly one class:

| Class | Meaning | Examples | Action |
|---|---|---|---|
| **A — identifier** | A concrete name of this project | `TemplateException`, `templateEx`, `Template.slnx`, `realm-template.json`, realm `template`, `/realms/template`, `template-public`, `template-api`, `template-api-audience`, `template-network`, `Database=template`, `POSTGRES_DB=template`, `pg_isready … -d template`, `name: template`, `template_postgres-data`, `"name": "Template"` (Bruno), `"displayName": "Template"`, `'Deploy Template'`, `/path/to/Template`, the `CLAUDE.md` project line | Replace with the derived identifier |
| **B — prose about "the template"** | Sentences in `CLAUDE.md` / `docs/**` that describe the repo *as a template*. The template's docs are written project-neutral ("the project …"), so this class is normally **empty**; a hit means project-specific wording slipped back into the template | "The template does not prescribe ingress", "A project built from the template replaces the sample" | Propose a rewritten sentence (see 2.3) |
| **C — unrelated meaning** | The word "template" used for something else | "template expression `${{ }}`" (Azure Pipelines), "environment templates", Keycloak/VS "templates" | Leave unchanged |

If a hit does not clearly fit one class, it is a **conflict** — ask the user (2.4). Never decide it yourself.

### 2.3 Prose rewrites (class B) — user approval per sentence

Replacing "template" with "project" word-for-word breaks many sentences (e.g. "A project built from the project…"). For each class-B sentence:

1. Show the **current** sentence with its file and line.
2. Show a **proposed** rewrite that restores the meaning for a concrete project (e.g. "The template does not prescribe how production traffic reaches the application" → "The project has not yet chosen how production traffic reaches the application"). Rewrite the minimal span; keep rules, references, and section anchors intact.
3. Mark sentences that should arguably be **removed** rather than rewritten (e.g. sentences that only make sense for a template, such as "a project replaces the sample with its real domain") — propose removal explicitly.

The user approves, edits, or rejects each one. Nothing in class B is changed without that approval. Group them by file so the user can review efficiently.

If class B is not empty, also tell the user that the template's docs have drifted from the project-neutral wording, so they can fix the wording in the template itself and the next conversion does not hit it again.

### 2.4 Conflicts — always ask

Collect and ask about every conflict before presenting the final plan. At minimum check:

- A target path already exists (e.g. `{Name}.slnx`, `keycloak/realm-{slug}.json`).
- The slug collides with another identifier in the repo or is not a valid realm/DB/network name.
- `DEPLOY_BRANCH` and the `trigger:` literal would differ.
- The repo folder name differs from the project name (informational — Compose no longer depends on it because of `name:`; renaming the folder is the user's choice).
- Any class-A/B/C ambiguity from 2.2.
- Anything in the repo that contradicts the docs (report it; do not fix it as part of this skill unless the user adds it to the plan).
- Anything in the project brief (2.1a) that conflicts with a hard rule or with another doc (see 2.6).

### 2.5 Record the brief in the philosophy

The approved brief becomes part of `docs/api/guidelines/philosophy.md` — the API philosophy is where the project explains *what it is and why*. Add a new section directly after the introduction and before the first existing principle, e.g. `## What this project is`, containing: subject/purpose, scope in, scope out, planned work, and the stated constraints & decisions. Keep the existing tone (short prose, principles over lists where possible) and the existing sections and anchors intact.

Ask the user whether the brief should also be reflected in `docs/deployment/guidelines/philosophy.md` and/or `docs/bruno/guidelines/philosophy.md` (e.g. a one-line pointer to the API philosophy). Change them only if the user says so.

### 2.6 Doc impact of the brief — ask before every change

With the approved brief in hand, re-read **every** guideline doc (`docs/api|deployment|bruno/guidelines/*.md`), `docs/version-control.md`, and `CLAUDE.md`, and look for statements the brief makes outdated, wrong, or incomplete. Typical candidates:

- `philosophy.md` "A foundation, not a product" — the project now has a product domain.
- Statements of what the project deliberately does **not** model (tenancy, per-user ownership, identity flows) when the brief decides one of them.
- `domain-driven-design.md` / `architecture.md` — bounded contexts named in the brief; the `Samples` / `Note` sample context.
- `tech-stack.md` / `security.md` / deployment docs — integrations or constraints named in the brief.

For **each** candidate, show: file and line, the current text, the proposed change (or proposed removal), and *which part of the brief* triggers it. The user approves, edits, or rejects each item. **No doc change derived from the brief is made without that explicit approval**, and you must not propose changes the brief does not justify. Group items by file.

Rules:

- A change that would weaken or contradict a `hard-rules.md` is never proposed as a doc edit — flag the conflict and let the user decide.
- Doc changes only; if an approved doc change implies code, compose, pipeline, or Bruno work (e.g. removing the `Note` sample), do **not** add it to this skill's plan — list it at the end of the report (4.4) as a follow-up task for the owning agent, to be confirmed separately under `CLAUDE.md` STEP 0.
- If the user rejects every item, say so in the plan; the philosophy section from 2.5 is still added.

---

## Step 3 — Present the plan, split by agent

Present one consolidated plan. Split it **by owner** so the user sees who does what (ownership per `CLAUDE.md` STEP 2):

| Owner | Scope in this skill |
|---|---|
| **Main agent (this context)** | `CLAUDE.md` project line + description; `docs/**` class-A identifiers and approved class-B rewrites; the approved project-brief section in `docs/api/guidelines/philosophy.md` (2.5, plus the other philosophy files only if the user chose so); every approved doc-impact item (2.6). (Docs are governance files — no sub-agent owns them.) |
| **`coder`** | Rename `TemplateException` → `{Name}Exception` (file + class + all usages, pattern variable `templateEx` → `{camelName}Ex`); rename `Template.slnx` → `{Name}.slnx` and update its solution-item path for the realm file. |
| **`deployer`** | `docker-compose.yml` (`name:`, network, realm URLs, audience, optional prod Keycloak host); `docker-compose.override.yml` (DB name, healthcheck, realm URLs, audience, network, comments); rename `keycloak/realm-template.json` → `realm-{slug}.json` and update realm, display name, client id/name, audience mapper; `pipelines/azure-pipelines-ci-cd.yml` (`PROJECT_DIR`, `displayName`, optional `AGENT_POOL` / `DEPLOY_BRANCH` + `trigger:` literal). |
| **`bruno`** | `bruno/bruno.json` collection name; `bruno/environments/local.bru` realm URLs, `realm`, `clientId`. No request routes change. |

For every owner list the exact files and the old → new values. For the main agent also show the final philosophy section text and every approved doc-impact item (file, current → new). Show the order:

1. Main agent — docs and `CLAUDE.md` (first, so sub-agents read the new names in the docs).
2. `coder` ∥ `deployer` (independent — run in parallel).
3. `bruno` (after `deployer`, because it mirrors the realm/client names).
4. Verification (Step 4.3).
5. Fresh git history (Step 4.5) — main agent, last, only if verification passed; it needs its own explicit approval at that moment in addition to the plan approval.

Ask for explicit approval. The user may approve all, a subset, or request changes; re-present the plan after any change. **Do not start Step 4 without approval.**

---

## Step 4 — Apply

### 4.1 Main agent work
Apply the docs and `CLAUDE.md` changes exactly as approved: identifiers, approved class-B rewrites, the approved philosophy section (2.5), and the approved doc-impact items (2.6) — nothing else.

### 4.2 Dispatch sub-agents
Dispatch per `CLAUDE.md` STEP 2 — every brief must contain:
- The precise task with the exact old → new values from the approved plan, and the files in scope / out of scope.
- The docs the agent must read itself: its platform core set(s) in full (`docs/api/guidelines/` for `coder`; `docs/deployment/guidelines/` for `deployer`; `docs/bruno/guidelines/` + `docs/api/guidelines/hard-rules.md` for `bruno`) and `docs/version-control.md`.
- The statement that the task is approved and fully sequenced (execute directly), that **no `git` command** may be run, and that tests are out of scope unless the docs say otherwise.
- Validation the agent must run: `coder` → `dotnet build {Name}.slnx`; `deployer` → `docker compose config` (dev merge) and `docker compose -f docker-compose.yml config` (prod) plus JSON validity of the realm file, no destructive Docker commands; `bruno` → static check that every `{{var}}` is defined.

Handle every Impact Report / Delegation Request per `CLAUDE.md` STEP 3.

### 4.3 Verify
After all agents finish:
- Re-run the Step 1 marker check — **every** marker must now be in the new-project state.
- Search again for `template` (same exclusions as 2.2): only class-C hits and hits the user explicitly chose to keep may remain. List them.
- Confirm the reported build and compose validations passed; if any could not run, say so plainly.
- Confirm the project-brief section is present in `docs/api/guidelines/philosophy.md` (and in the other philosophy files, if chosen) exactly as approved, that every approved 2.6 item was applied, and that no unapproved doc change was made.

### 4.4 Report
Report what changed per owner. List any follow-up tasks surfaced by 2.6 (work for `coder` / `deployer` / `bruno` / `tester` that the brief implies) — this skill does not execute them; each needs its own confirmation under `CLAUDE.md` STEP 0. If any verification in 4.3 failed or could not run, say so plainly and **do not** proceed to 4.5 — stop and let the user decide.

### 4.5 Fresh git history (the template's history is not carried over)
The new project starts with an empty history: the template's commits (and anything they contain) must not travel to the new project's remote, and the local repository must no longer point at the template's remote.

1. **Warn first.** Tell the user that deleting `.git` permanently removes the template's local history, all local branches, stashes, and any commits that were never pushed — this cannot be undone. Offer (do not run unasked) a read-only check first, listing the exact commands, e.g. `git status`, `git stash list`, `git branch -vv`, `git log --branches --not --remotes --oneline`. Run them only if the user explicitly approves that list; report the result.
2. **Show the exact commands and ask for explicit approval** (approval covers only this list):
   - delete the `.git` directory at the repo root (e.g. PowerShell `Remove-Item -Recurse -Force .git`);
   - `git init -b <DEPLOY_BRANCH>` — the initial branch name equals the pipeline's `DEPLOY_BRANCH`, so the CD trigger matches.
3. **Run them only after approval.** Then confirm `.git` was recreated and the current branch is `<DEPLOY_BRANCH>` (e.g. `git branch --show-current` — include it in the approved list). Do **not** stage, commit, add a remote, or push.
4. Keep `.gitignore` / `.gitattributes` as they are (they are files, not history).

If the user declines, leave `.git` untouched and tell them the repository is still linked to the template's remote and still carries its history.

### 4.6 Manual steps for the user
Tell the user what they must do themselves:
- **Git:** create the new remote repository, then: `git add -A`, `git commit -m "Initial commit"`, `git remote add origin <new-repo-url>`, `git push -u origin <DEPLOY_BRANCH>`. The pushed branch must be named exactly `<DEPLOY_BRANCH>` or the CD pipeline will not trigger.
- **Keycloak (dev):** the realm name changed — recreate the container so the new realm is imported (`docker compose up -d --force-recreate keycloak`), and set a password for `dev-user` in the admin console.
- **Docker volumes:** the Compose project name changed from `template` to `{slug}`, so Compose creates new, empty volumes (`{slug}_postgres-data`, …) and the dev database `{slug}` is created on first start. The old `template_*` volumes are orphaned; remove them with `docker volume rm` only if they no longer need the data.
- **Bruno:** select the `local` environment and log in again (new realm/client).
- **Production (when relevant):** import the new realm file into the production Keycloak, set the HTTPS redirect URIs, replace remaining placeholders.
- **Folder name (optional):** the repository folder can be renamed to `{Name}`; nothing depends on it.

The skill stays in the repository after the conversion; Step 1 makes any later run refuse safely.
