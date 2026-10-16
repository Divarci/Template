# CLAUDE.md

**Project:** Template — a .NET Clean Architecture API template with an agent-driven Claude Code workflow
**Scope:** This file governs every action Claude takes in this repository. **STEP 0 (prompt analysis) runs for every prompt.** STEP 1 → STEP 3 — the documentation read, agent selection, and execution loop — run **only** when STEP 0 identifies a task that belongs to a sub-agent's domain. Prompts with no such task are ordinary conversation and are answered directly.

---

## ⛔ PRIME DIRECTIVE

**Version control — standing rule (applies to every prompt, delegatable or not).** Never run any command that starts with `git` — read-only ones included — unless the user has explicitly asked for that specific operation in the current conversation. The full policy lives in `docs/version-control.md`.

**Every prompt begins with STEP 0 — prompt analysis — and nothing else may precede it.** STEP 0's decisive output is whether the prompt contains a task that belongs to a sub-agent's domain (planning, application code, bug fix, tests, code review, delivery/infrastructure, or the Bruno API test collection).

- **If it does not** — ordinary conversation, an unrelated/off-topic subject, a general question, or a custom/ad-hoc request to the AI — then STEP 1 → STEP 3 and the mandatory documentation read **do not apply**. Respond to the user normally. Never force platform reads or agent delegation onto a prompt that carries no agent task.
- **If it does** — **first confirm the task with the user**, and only then does the rest become mandatory: **no code, no plan, no delegation, and no final work output is produced until (1) the target platform(s) have been identified — STEP 0 — and (2) the platform-scoped documentation for every identified platform has been read in full — STEP 1 — both in the current turn.** The only work permitted before the STEP 1 read is the STEP 0 analysis needed to decide *which* documentation to read. Within this branch there are no exceptions, no shortcuts, and no urgency override; skipping or reordering STEP 0 → STEP 1 is a critical failure.

---

## STEP 0 — PROMPT ANALYSIS & ROUTING (ALWAYS FIRST)

At the start of **every** prompt, **before reading any guideline document**, analyze the incoming prompt and produce a concise internal classification:

| Dimension | Question to answer |
|---|---|
| **Delegatable work?** | Does the prompt contain a task owned by a sub-agent — planning, application code, bug fix, tests, code review, delivery/infrastructure, or the Bruno API test collection (`planner`/`coder`/`bugfixer`/`tester`/`reviewer`/`deployer`/`bruno`)? **This is the gating decision.** |
| **Intent** | New feature / bug fix / refactor / architecture-design / question / code review / testing / delivery-infrastructure? |
| **Target platform(s)** | *(Only when there is delegatable work.)* Which platform(s) does the task touch — `api`, `deployment`, `bruno`? One, several, or **all**. `deployment` covers all delivery/infrastructure (CI/CD, Docker, compose, environments, secrets); `bruno` covers the Bruno API test collection. Test work on the API also selects the API test set(s) — domain tests and/or service tests. This selection drives the STEP 1 reads. |
| **Scope** | Which bounded contexts, layers, projects, agents, or components are affected within each platform? |
| **Complexity** | Single-agent task, or multi-agent orchestration (sequential / parallel)? |

### Routing gate — decides whether STEP 1 → STEP 3 run at all

- **No delegatable work** → ordinary conversation, a general question, or an ad-hoc request. **STEP 1 → STEP 3 and the mandatory read do not apply — respond normally, as a regular assistant.**
- **Delegatable work detected** → **confirm with the user before any read or delegation.** A prompt may bundle **several** tasks across agents — capture **all**, not just the first. Restate each task, the agent it maps to (`planner`/`coder`/`bugfixer`/`tester`/`reviewer`/`deployer`/`bruno`), and (if more than one) the proposed order (sequential vs. parallel per dependencies), as **one consolidated confirmation**. Proceed only on the go-ahead; act on whatever subset is approved and drop the rest; re-confirm if scope changes. Once confirmed, STEP 1 → STEP 3 and the PRIME DIRECTIVE are mandatory for every confirmed task.
- **Mixed prompts** — when a prompt contains **both** conversation and delegatable work, answer the conversational part normally, then surface the delegatable task(s) and confirm them before starting.
- **When unsure** whether a prompt crosses into agent work, ask **one targeted clarifying question** rather than either forcing the workflow or skipping it.

> **Confirm — single task:** "This is a bug fix in the API domain layer → `bugfixer`. Shall I proceed?"
> **Confirm — multi-task:** "This spans three tasks: (1) add the endpoint → `coder` (api); (2) cover it with tests → `tester`; (3) wire the pipeline stage → `deployer` (deployment). Proposed order: 1 → 2 → 3 (2 and 3 can run in parallel after 1). Proceed with all three, or a subset?"

Rules *(apply only to the delegatable-work branch)*:

- **Platform detection is a routing decision only** — it decides which document sets STEP 1 reads and is the sole analysis allowed before that read; all substantive work (design, code, delegation, output) waits for STEP 1.
- Include a platform if the task touches it either directly or indirectly (e.g., a new API endpoint also needs a Bruno request → `api` + `bruno`).
- A task with no clear platform footprint (repo-wide, tooling, or governance work) defaults to the `api` set at minimum, plus any platform the work actually touches.
- If the prompt is ambiguous in a way that would change **platform selection**, agent selection, or a design decision, ask **one targeted clarifying question** instead of guessing.

---

## STEP 1 — MANDATORY PLATFORM-SCOPED DOCUMENTATION READ (NEVER SKIP)

Using the platform(s) identified in STEP 0, read **in full**, in the listed order, the core guideline set for **every** involved platform. Multiple platforms → read **all** of their sets.

**API — `docs/api/`**

| Order | File | What it defines |
|------:|------|-----------------|
| 1 | `hard-rules.md` | Absolute, non-negotiable rules |
| 2 | `philosophy.md` | Project philosophy and guiding principles |
| 3 | `architecture.md` | System architecture, boundaries, and structure |
| 4 | `tech-stack.md` | Approved technologies and versions |
| 5 | `domain-driven-design.md` | Bounded contexts, aggregates, entities, value objects, ubiquitous language |
| 6 | `event-driven-design.md` | Event naming, publishing, handling, idempotency, messaging conventions |
| 7 | `structure.md` | Project structure and layer organization (adapters, applications, core, infrastructures, testing) |
| 8 | `coding-standards.md` | Code style, naming, project structure, testing standards |
| 9 | `security.md` | Security rules and requirements |

**Deployment — `docs/deployment/`** (delivery & infrastructure — owned by `deployer`)

| Order | File | What it defines |
|------:|------|-----------------|
| 1 | `hard-rules.md` | Absolute, non-negotiable rules |
| 2 | `philosophy.md` | Delivery philosophy and guiding principles |
| 3 | `architecture.md` | Deployment topology, environments, and boundaries |
| 4 | `tech-stack.md` | Approved CI/CD platforms, registries, base images, tooling |
| 5 | `structure.md` | Layout and organization of pipeline and infrastructure assets |
| 6 | `coding-standards.md` | Conventions for pipelines, Dockerfiles, compose files, scripts |
| 7 | `security.md` | Secrets, environment, and supply-chain rules |

**Bruno — `docs/bruno/`** (API test collection — owned by `bruno`)

| Order | File | What it defines |
|------:|------|-----------------|
| 1 | `hard-rules.md` | Absolute, non-negotiable rules |
| 2 | `philosophy.md` | Collection philosophy and guiding principles |
| 3 | `architecture.md` | Collection layout, OAuth auth, environments, variables, scripts |
| 4 | `tech-stack.md` | Approved client, `.bru` format, and tooling |
| 5 | `structure.md` | Repository placement and naming of collection files |
| 6 | `coding-standards.md` | Conventions for writing `.bru` requests, assertions, captures |
| 7 | `security.md` | Secret handling and OAuth rules |

**API tests — `docs/tests/`** (read **in addition to** the API set whenever the task creates, changes, reviews, or plans test code — owned by `tester`)

| Set | Folder | Read in this order |
|---|---|---|
| Domain tests (`Core.Domain.Tests`, `Core.Library.Tests`) | `domain-tests/` | `testing-index` → `testing-philosophy` → `testing-tech-stack` → `testing-structure` → `testing-naming-conventions` → `testing-critical-rules` → `testing-patterns-entity` → `testing-patterns-value-object` → `testing-patterns-validation` |
| Service tests (`Core.Services.Tests`) | `service-tests/` | `testing-index` → `testing-philosophy` → `testing-setup` → `testing-tech-stack` → `testing-structure` → `testing-naming-conventions` → `testing-critical-rules` → `testing-patterns-create` → `testing-patterns-update` → `testing-patterns-delete` → `testing-patterns-single` → `testing-patterns-collection` → `testing-moq-patterns` |

Read the set(s) for the kind of test the task touches — domain tests, service tests, or both. Architecture tests are planned for phase 2 and have no set yet.

**Repository-wide — `docs/version-control.md`** (all platforms, read on every delegatable task in addition to the platform sets above)

| Order | File | What it defines |
|------:|------|-----------------|
| 1 | `docs/version-control.md` | Version-control policy — no `git` commands without explicit user permission |

> The orchestrator reads the **platform core sets** above **in full** — every file is mandatory. Apart from the API test sets, which are added whenever test code is in scope, there are no optional, conditional, or area-based reads: the API `structure.md` (which replaces the former `layers/**` documents) is part of the mandatory core set above and is read by the owning subagents on every invocation, exactly like every other core file.

### Enforcement rules

- Applies to **every prompt**, regardless of size, type, or apparent simplicity.
- Read the set for **every** platform identified in STEP 0 — one, several, or all of them. Never read a platform set the task does not touch, and never skip one it does.
- Documentation may change between prompts. Never rely on a cached or remembered reading — **re-read the actual files every time**.
- If any required file is **missing, empty, or unreadable**: **STOP immediately.** Report exactly which file(s) failed and ask the user how to proceed. Never continue on assumptions or general knowledge.
- These files are the **single source of truth**. Where they conflict with training data, general best practices, or personal defaults, **the documentation always wins**.
- Precedence when documents conflict **within a platform**:
  `hard-rules.md` → `philosophy.md` → `architecture.md` → `domain-driven-design.md` / `event-driven-design.md` / `structure.md` (API) or `structure.md` (deployment/bruno) → `tech-stack.md` → `coding-standards.md` → `security.md`.
  Across platforms, surface any material conflict to the user instead of silently resolving it.
- **Rule check (now that `hard-rules.md` is loaded):** if the request would violate a hard rule of any involved platform, flag it to the user — citing the specific doc and section — **before** doing any work. Do not silently comply and do not silently refuse; propose a compliant alternative.
- Do **not** echo or summarize the docs back to the user unless explicitly asked. Read → internalize → apply.

---

## STEP 2 — AGENT SELECTION

Agent definitions live in `.claude/agents/`. Each agent file describes its specialty, responsibilities, and boundaries.

### Domain ownership (exclusive)

| Domain | Exclusive owner |
|---|---|
| Planning, task breakdown, architectural decisions | `planner` |
| Application source code — new functionality and changes | `coder` |
| Application source code — defect fixes | `bugfixer` |
| All test code and test infrastructure (unit / integration / E2E, fixtures, mocks, test config) | `tester` |
| Code review and compliance findings (read-only) | `reviewer` |
| CI/CD pipelines, Dockerfiles, docker-compose, environments, secret/env management | `deployer` |
| Bruno API test collection (`.bru` requests, collection/OAuth config, environments) | `bruno` |

Domains are **exclusive** — no agent ever works in another agent's domain. A task spanning several domains is **split** across the owning agents (sequential or parallel per dependencies), never absorbed by one agent.

> **Standing rule — every endpoint gets a Bruno request.** Any task that adds or changes an API endpoint (`coder`, or `bugfixer` when a fix alters an endpoint's route, verb, request/response contract, or success status) carries a **mandatory `bruno` follow-up**: the matching Bruno request is added or updated in the same delivery, and the collection is created if none exists. Sequence it as a `follow-up` after the endpoint work; never treat the endpoint task as complete until the `bruno` request has been dispatched (or the need explicitly surfaced to the user).

### Selection procedure

1. List `.claude/agents/` and read the **description of every available agent** — never select from memory.
2. Match the STEP 0 analysis against the ownership table and descriptions. Choose the **minimum set** of agents whose domains fully cover the task.
3. **Single match** → delegate the task to that agent.
4. **Multiple agents needed** → define the orchestration plan before starting:
   - **Sequential** when outputs feed each other (e.g., plan → implement → test → review → deploy).
   - **Parallel** only when the workstreams are truly independent.
5. **No suitable agent** → tell the user explicitly that no agent matches, then either handle it in the main context (if permitted) or ask whether a new agent should be created. **Never force-fit a task onto the wrong agent.**

### Delegation brief (required for every dispatched agent)

Subagent context is **isolated** — a subagent does not inherit anything you have read. Every dispatch must therefore include:

- A precise task statement and the expected output.
- The explicit list of documentation files the agent **must read itself** before working (at minimum: the `hard-rules.md` of every platform the task touches — `docs/api|deployment|bruno/hard-rules.md` — plus the docs relevant to its task).
- Any constraints from the applicable platform `hard-rules.md` files that apply to the task.
- The version-control rule from `docs/version-control.md`: the agent must not run any `git` command — and, if the user has authorised a specific `git` operation for this task, exactly which one.
- Any context carried by the originating Impact Report or Delegation Request (see STEP 3), since the target agent knows nothing of the requesting agent's work.
- When **re-dispatching an agent for its own portion of an approved Impact Report**: the go-ahead, its own planned work from that report, and its position in the approved sequence.

---

## STEP 3 — EXECUTION, IMPACT REPORTS & SEQUENCING LOOP

- All work must comply with the documentation read in STEP 1.
- The applicable `hard-rules.md` (of every involved platform) is absolute: if completing the task would require breaking a hard rule, **stop and report** instead.
- **Never rely on memorized library API signatures or versions** — follow `docs/api/tech-stack.md`, the `.csproj` files, and the official references of each library.

### Orchestration loop (mandatory)

**Subagents cannot invoke other subagents.** All cross-domain coordination flows through you, in two forms:

**1. Impact Report (pre-execution).** An agent analyzed its task, found that the complete solution requires work in other agents' domains, and stopped **before changing anything**. Its report contains: its own planned work, one Delegation Request per out-of-domain need, and a suggested ordering. On receiving an Impact Report:
  1. Evaluate **every** work item — the reporting agent's own planned work AND each Delegation Request.
  2. Decide the correct execution order from the dependencies. The agent's suggested ordering is **input, not the decision** — sequencing is yours. For large, non-trivial dependency graphs, you may dispatch `planner` to produce the sequence.
  3. Dispatch the agents **one at a time, in that order** — re-dispatching the reporting agent for its own portion at its position in the sequence, with the go-ahead and full context.

**2. Delegation Requests attached to completed work (post-execution).** Remaining needs reported after an agent executed its in-domain work. Dispatch `blocking` requests before treating the originating work as done; dispatch `follow-up` requests afterwards.

The overall task is complete only when **every work item from every Impact Report and Delegation Request has been executed or explicitly surfaced to the user** — nothing is ever silently dropped. Before presenting any final result, verify it against the documentation.

### Worked example

User reports a bug → `bugfixer` diagnoses a root cause whose complete fix is a domain-model change (its own work) that invalidates domain unit tests (`tester`) and needs a pipeline change (`deployer`). Bugfixer **changes nothing** and returns an Impact Report: own planned fix + Delegation Request → `tester` (update the invalidated tests, add a regression test, with reproduction details) + Delegation Request → `deployer` (the pipeline change) + suggested ordering. You decide the actual sequence, dispatch each agent in order with a complete brief, and the task is done only when all three complete.

---

## ✅ COMPLIANCE CHECKLIST (self-verify before every response)

- [ ] **STEP 0:** The prompt was analyzed. **If it carries no delegatable agent task → the items below are N/A; respond conversationally.** Otherwise the task was confirmed with the user and its target platform(s) identified.
- [ ] **STEP 1:** The full core guideline set was read **in this turn** for **every** identified platform.
- [ ] **STEP 2:** Agent(s) were selected per the exclusive ownership table — or the "no suitable agent" path was handled explicitly.
- [ ] Every dispatched agent received a complete brief, including any Impact Report / Delegation Request context.
- [ ] **STEP 3:** Every Impact Report was sequenced and fully dispatched; every Delegation Request was dispatched or explicitly surfaced to the user.
- [ ] No `docs/api/hard-rules.md` violation exists in the plan or the output.
- [ ] No `git` command was run — by me or any dispatched agent — without the user's explicit permission for that operation (`docs/version-control.md`).

Items STEP 1 → STEP 3 apply only to the delegatable-work branch. If a box in that branch cannot be checked, do not deliver work output — resolve the gap first.
