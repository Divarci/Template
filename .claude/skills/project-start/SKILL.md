---
name: project-start
description: Turns this repository from the generic "Template" into a concrete project — gives it its own name/identity across code, tests, solution, Docker Compose, Keycloak realm, Bruno collection, pipelines, CLAUDE.md and docs, and records the project's subject, scope and planned work in the API philosophy. Follows a fixed checklist (checklist.md). Use when the user wants to start a new project from the template, "give the project an identity", rename the template, or invokes /project-start. Refuses to run on a repository that has already been converted.
---

# Project Start — instantiate the template

This skill converts the repository from the generic **Template** into a named project by applying the fixed change set in **`checklist.md`** (same folder). It does not discover what to change — the checklist already knows. It asks the user only for the inputs, the project brief, one plan approval, and (separately) the fresh-history approval. Talk to the user in their language.

The skill does not replace `CLAUDE.md`: the work is delegatable, so STEP 0 → STEP 3 (exclusive domains, delegation briefs, version control) apply in full. The approved plan from Step 3 is the user's STEP 0 confirmation.

**Version control:** never run any command starting with `git` (`docs/version-control.md`), with one exception — the fresh-history step (Step 6), only after the user approves the exact command list shown at that moment. The skill never commits, adds a remote, or pushes.

---

## Step 1 — Is this repository still the template?

Check these **markers** (read-only):

| # | Marker | Template state |
|---|---|---|
| M1 | Solution file at repo root | `Template.slnx` exists |
| M2 | Keycloak realm file | `keycloak/realm-template.json` exists |
| M3 | Domain exception | `api/src/core/Core.Library/Exceptions/TemplateException.cs` exists |
| M4 | `CLAUDE.md` project line | starts with `**Project:** Template` |
| M5 | Compose project name | `docker-compose.yml` has top-level `name: template` |

- **All in template state** → continue.
- **None in template state** → already converted. Tell the user the project name found (solution file / `CLAUDE.md` / compose `name:`), explain the skill runs only on the untouched template, and **stop**. No re-run, no re-rename.
- **Mixed** → partial conversion. List which markers are template and which are not (with values), say the state is inconsistent, and **stop**; the user decides.

---

## Step 2 — Inputs (ask, never infer silently)

Ask for the four inputs in `checklist.md` → Placeholders: project name, slug, one-line description, deploy branch. You may propose a derived value (e.g. the slug from the name); it is used only after the user confirms it. Do **not** ask about the agent pool, the production Keycloak host, or the project directory — they are fixed or derived (`AGENT_POOL` = `HomerServer`, Keycloak host stays a placeholder, `PROJECT_DIR` = `/srv/{slug}`).

Validate: `{Name}` is a valid C# identifier starting uppercase and is not `Template`; `{slug}` is lowercase letters/digits/hyphens starting with a letter; `{DeployBranch}` changes the `trigger:` literal too (row D5). If `{Name}.slnx` or `keycloak/realm-{slug}.json` already exists, stop and ask.

Then read `checklist.md` in full and fill every placeholder — this filled checklist is the plan.

---

## Step 3 — Project brief (short interview)

A name alone is not an identity. Ask these five questions; never fill a gap from the name, the description, or general knowledge. "Not decided yet" is a valid answer only when the user says it.

| Item | Question |
|---|---|
| Subject / purpose | What is the project about — which problem, for whom? |
| Scope — in | Which business capabilities / bounded contexts are inside? |
| Scope — out | What is explicitly not part of it? |
| Planned work | What will be built first — main features, first release slice? |
| Constraints & decisions | Tenancy, per-user ownership, extra identity flows, integrations, regulatory/performance needs? |

Write the `## What this project is` section from the answers in the user's own words — short prose, no embellishment — and show it. The user approves, edits, or rejects; re-present after every change. The approved text is row **M4**; any integration the brief names becomes a row-**M6** item.

If an answer conflicts with a `hard-rules.md` (e.g. an anonymous endpoint, multi-tenancy — `docs/api/hard-rules.md` §10/§11), flag the doc and section and ask how to proceed; do not resolve it yourself.

---

## Step 4 — Present the plan and get one approval

Show the filled checklist grouped by owner — main agent, `coder`, `tester`, `deployer`, `bruno` — with old → new values, the final philosophy text (M4), any M6 rows, and the order:

1. Main agent — rows M1–M15 (docs first, so sub-agents read the new names).
2. `coder` (C1–C3) ∥ `deployer` (D1–D5) — independent, run in parallel.
3. `tester` (T1) — after `coder`, because the test code references the renamed exception and solution.
4. `bruno` (B1–B2) — after `deployer`, because it mirrors the realm/client names.
5. Verification (Step 5).
6. Fresh git history (Step 6) — last, own approval.

Ask for explicit approval. The user may approve all, a subset, or ask for changes; re-present after any change. **Do not start Step 5 without approval.**

---

## Step 5 — Apply and verify

1. **Main agent:** apply M1–M15 exactly as approved — nothing else.
2. **Dispatch** per `CLAUDE.md` STEP 2. Every brief contains: the owner's checklist rows with the filled old → new values (quote them — the agent's context is isolated); the docs the agent must read (`coder`/`tester` → `docs/api/*` core set, `tester` also `docs/tests/*`; `deployer` → `docs/deployment/*`; `bruno` → `docs/bruno/*` + `docs/api/hard-rules.md`; everyone `docs/version-control.md`); the statement that the task is approved and fully sequenced; **no `git` command**; the owner's validation line from the checklist; and the instruction to grep-confirm its file list and report any unlisted file rather than skipping it. Handle Impact Reports / Delegation Requests per `CLAUDE.md` STEP 3.
3. **Verify** per `checklist.md` → Verification. If anything fails or could not run, say so plainly and **stop before Step 6**.
4. **Report** what changed per owner, the leftover `template` hits (must all be in the Leave-unchanged list), and any checklist gap an owner reported — a gap is a follow-up to update `checklist.md`, confirmed separately.

---

## Step 6 — Fresh git history (own approval)

The new project starts with an empty history: the template's commits must not travel to the new remote, and the repo must no longer point at the template's remote.

1. **Warn:** deleting `.git` permanently removes local history, branches, stashes, and unpushed commits. Offer (do not run unasked) a read-only check — `git status`, `git stash list`, `git branch -vv`, `git log --branches --not --remotes --oneline` — run only on explicit approval of that list.
2. **Show the exact commands and ask:** delete `.git` (e.g. PowerShell `Remove-Item -Recurse -Force .git`), then `git init -b {DeployBranch}`, then `git branch --show-current` to confirm. Approval covers only this list.
3. Run only after approval. Do **not** stage, commit, add a remote, or push. `.gitignore` / `.gitattributes` stay.

If the user declines, leave `.git` untouched and say the repo still carries the template's history and remote.

---

## Step 7 — Manual steps for the user

- **Git:** create the new remote, then `git add -A`, `git commit -m "Initial commit"`, `git remote add origin <url>`, `git push -u origin {DeployBranch}` — the branch name must equal `{DeployBranch}` or CD will not trigger.
- **Keycloak (dev):** `docker compose up -d --force-recreate keycloak` to import the new realm; set `dev-user`'s password in the admin console.
- **Docker volumes:** new `{slug}_*` volumes are created; the old `template_*` volumes are orphaned — `docker volume rm` them only if their data is no longer needed.
- **Bruno:** select `local` and log in again (new realm/client).
- **Self-hosted machine:** the CD pipeline expects the checkout at `/srv/{slug}` (`PROJECT_DIR`) and the agent registered in the `HomerServer` pool.
- **Production (when relevant):** import the realm into production Keycloak, set HTTPS redirect URIs, replace the remaining placeholders in `docker-compose.yml` (`REPLACE_WITH_PROD_KEYCLOAK_HOST`, `REPLACE_WITH_PROD_SEQ_HOST`).
- **Folder name (optional):** the repo folder can be renamed to `{Name}`; nothing depends on it.

The skill stays in the repository; Step 1 makes any later run refuse safely.
