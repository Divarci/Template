# Philosophy

The guiding principles behind delivery for this project. Where `hard-rules.md` states what is non-negotiable, this document explains *why* the model is shaped as it is. Every concrete convention in `architecture`, `tech-stack`, `structure`, `coding-standards`, and `security` traces back to one of these principles.

---

## Simplicity over orchestration complexity

The system deploys as a set of containers to a **single self-hosted machine** (a home server / on-prem PC — not a cloud VM), coordinated by Docker Compose. There is no Kubernetes, no service mesh, no container registry, no multi-node scheduler. This is a deliberate choice, not a temporary shortcut: the operational surface stays small enough that one engineer can reason about the whole system, and every moving part earns its place. Complexity is added only when a concrete need forces it — never speculatively.

---

## Git-based, reproducible delivery

The Git repository is the single source of truth for *what runs*. A deploy is nothing more than making the self-hosted machine's checkout match the deploy branch on `origin` (`DEPLOY_BRANCH`, default `master`) and rebuilding from it. Because images are built on that machine from that exact commit — not pulled from a registry — the running system is always reconstructible from a commit hash and a `.env` file. There is no drift between "what's tagged in a registry" and "what's in the repo," because there is no registry.

The corollary: rollback is a Git operation. Revert the commit, let the pipeline rebuild. No separate artifact store to reconcile.

---

## Infrastructure defined as configuration in the repo

Everything about how the system runs — service topology, networks, volumes, build steps, environment key names, pipeline stages — lives as version-controlled files in the repository (`docker-compose.yml`, `Dockerfile`s, `pipelines/azure-pipelines-*.yml`, `.env.example`). Nothing is configured by hand on the self-hosted machine and left undocumented. If it matters to how the system runs, it is a reviewed file in the repo.

---

## Secrets externalized, never committed

Configuration that is *non-secret* — including non-secret production values like the environment name, bind URL, ports, and non-secret endpoint identifiers (an OIDC issuer/authority URL, an audience id) — lives in the repo as literals in the Compose files; only configuration that is *sensitive* (connection strings, credentials, keys, tokens, passwords, or any URL that embeds a credential) is injected at runtime. Production secrets never enter Git in any form. They are delivered to the self-hosted machine through Azure DevOps Secure Files as a `.env` file, consumed by Compose at `up` time, and removed immediately afterward. The repository describes the **shape** of the secrets the system needs (`.env.example` key names); the secret **values** live only in the secret store and, transiently, on the machine during a deploy.

Local development is the one relaxation: `docker-compose.override.yml` may inline **throwaway** development values so a developer can `docker compose up` with zero setup — but only values that unlock nothing real.

---

## Fail-safe cleanup

Deploy steps assume the previous run may have failed midway. The pipeline prunes aggressively before building (a clean machine every deploy) and removes the transient `.env` with `condition: always()` so a secret file never survives a failed deploy. The default posture is: leave the system in a known-clean state even when something goes wrong.

---

## Quality gates before merge

Nothing reaches the self-hosted machine that has not first passed automated build and test on a pull request. CI is a gate, not a formality: it runs on every PR against the same solution and test projects the developers run locally, and a red gate blocks the merge. CD is intentionally dumb — it trusts that anything on `master` already passed the gate.

---

## Environment parity is explicit

There are exactly two environments — **Development** (local) and **Production** (the self-hosted machine) — and the differences between them are visible, not accidental. The base Compose file is the production definition; the override file is the *documented, reviewable* set of development deltas. A developer can read both files side by side and see exactly how local differs from production. No hidden per-environment behavior, no implicit toggles.
