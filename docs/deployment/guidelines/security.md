# Security

Secrets, environment, and supply-chain rules for delivery. Subordinate only to `hard-rules.md`, which states the absolute constraints these rules implement. Whenever a task touches secrets, credentials, tokens, certificates, or environment variables, this document is in force.

---

## Secret handling — the golden rules

1. **No production secret in Git, ever.** Not in code, Dockerfiles, Compose files, pipeline YAML, scripts, or a committed `.env`. See `hard-rules.md` §2.
2. **Development throwaway values only, inline.** The single permitted exception is throwaway development values in `docker-compose.override.yml` — values that unlock nothing real, shared, or production. If a value would grant access to a real resource, it is a production secret and is forbidden inline.
3. **Reference secrets by key name only.** Never print, echo, log, or paste a secret value — not in pipeline output, not in a terminal command, not in a report or commit message. Refer to `SOME_SECRET_KEY`, never its value.
4. **Discovered hardcoded secret = security finding.** Report it, recommend rotation of the exposed credential, and propose the compliant replacement. Never silently delete or ignore it. Editing application source to remove it is `coder`/`bugfixer` work — raise a Delegation Request.

---

## The Secure Files workflow

Production configuration reaches the self-hosted machine as a `.env` file delivered through **Azure DevOps Secure Files** — never through the repository.

```
Azure DevOps Library ──(DownloadSecureFile@1)──► agent workspace ──(cp)──► PROJECT_DIR/.env
        │                                                                        │
        │                                                          docker compose --env-file .env up
        │                                                                        │
        └───────────────── rotate value here, no code change ───────────  rm -f .env  (condition: always())
```

Rules for the workflow — the full `.env` lifecycle on the self-hosted machine is **download → copy → use → remove**, and the removal step is mandatory:

- The CD pipeline downloads `.env` with `DownloadSecureFile@1` (the secure file must be named `.env` in the Library) and copies it into `PROJECT_DIR` on the self-hosted machine.
- It is used for the deploy via `docker compose --env-file .env up -d --build`.
- **Explicit rule — `.env` must be removed at the end of every deploy in a step that runs `condition: always()`.** The `always()` condition guarantees deletion even when an earlier pipeline step fails, so a real secret file never lingers on the machine after a deploy — successful or not. This removal step is non-optional; a CD pipeline without it is non-compliant.
- `.env` must be listed in both `.gitignore` (never committed) and `.dockerignore` (never enters a build context / image layer).
- Rotating a secret is a Library/Secure-File change plus a redeploy — never a repository edit.

---

## `.env.example` discipline

- `.env.example` is committed and lists **every** environment key the system requires, with **empty or placeholder values only**.
- When a service gains a new environment key, `.env.example` is updated in the same change so the required shape stays complete and reviewable.
- Group keys by service with comment headers; keep names identical to the `${VAR}` references in `docker-compose.yml`.

---

## Container security

- **Non-root final stage** for every image — `.NET` via `USER $APP_UID`. See `hard-rules.md` §6 and `coding-standards.md`. Never `USER root` in a runtime stage.
- **Minimal, pinned base images.** Application services use the official `mcr.microsoft.com/dotnet/*:10.0` images pinned to a major version; stock supporting services are pinned to a specific tag rather than `latest` when added. No unofficial base images.
- **No secret baked into an image layer.** Runtime configuration arrives as container `environment` at `up` time from `.env`; build-time `ARG`s must not carry production secrets, and `.env` is excluded from the build context by `.dockerignore`.
- **Least exposure.** Only publish the ports a service actually needs. Do not expose management/diagnostic ports of supporting services to the host in production unless required.

---

## `.dockerignore` — supply-chain hygiene

The root `.dockerignore` keeps secrets, VCS history, and build detritus out of every build context. It **must** exclude at minimum:

| Pattern | Reason |
|---|---|
| `**/.env` | Never let a secret file enter a build context or image layer. |
| `**/.git` | No VCS history / credentials in images. |
| `**/.vs`, `**/.vscode` | Local IDE state. |
| `**/bin`, `**/obj` | Host build output — rebuilt inside the image. |
| `**/node_modules` | Reinstalled inside the image. |
| `**/secrets.dev.yaml`, `**/*.*proj.user` | Local secret/user files. |
| `**/docker-compose*`, `**/Dockerfile*` | Build inputs, not build content. |

---

## Pipeline authentication

- **`System.AccessToken`** is the only credential the CD pipeline uses for Git, injected as an `http.extraHeader` bearer on `git fetch`:

  ```bash
  git -c http.extraHeader="Authorization: Bearer $(System.AccessToken)" fetch origin
  git reset --hard origin/$(DEPLOY_BRANCH)
  ```

  It is a pipeline-scoped token; do not add personal access tokens or long-lived credentials to a pipeline.
- Do not store deploy credentials in the repo. The self-hosted agent's access to the machine's Docker daemon is a property of where the agent runs, not a committed secret.

---

## Transport / TLS by environment

Configuration that relaxes transport security is **development-only** and must never be carried into production values:

| Setting | Development | Production |
|---|---|---|
| PostgreSQL `SSL Mode` / `Trust Server Certificate` | SSL may be disabled for the local dev `postgres` container | TLS as required by the managed database (e.g. `SSL Mode=Require`), never blind certificate trust — via the `.env` connection string |
| Auth `REQUIREHTTPSMETADATA` (identity metadata over HTTP) | May be `false` locally | Must be `true` — HTTPS metadata required |
| Identity/base URLs | Local/dev addresses inline in the override | Real HTTPS issuer/authority URL as a base literal (non-secret endpoint id; only credential-embedding URLs use `.env`) |

These relaxations live only in `docker-compose.override.yml` (dev) or local user-secrets. The base `docker-compose.yml` states the production posture directly — non-secret settings such as `REQUIREHTTPSMETADATA: "true"` and the HTTPS endpoint URLs as base literals, credentials via `${VAR}` — and must never hardcode a security-weakening value. Never disable TLS verification, open unnecessary ports, or run a container as root to "make something work" — report the conflict instead of shipping it (`hard-rules.md`).

**Production TLS terminates outside the application**, in the ingress mechanism the project chooses (`architecture.md` — Ingress & TLS; `hard-rules.md` §10). The app stays HTTP internally, and its plain-HTTP port never faces the public internet without TLS in front of it. The ingress's credentials (tunnel tokens, auth keys, private keys) are secrets — delivered via Secure Files / `.env` `${VAR}`, listed in `.env.example` by key name only, never committed or logged. If the chosen ingress lets a third party see decrypted traffic (e.g. a tunnel provider's edge), that trade-off is recorded explicitly when the choice is made.

---

## Local development secrets

- Local secrets use .NET **user-secrets**, mounted read-only into the dev container by the override (`${APPDATA}/Microsoft/UserSecrets` → `/home/app/.microsoft/usersecrets`, the non-root user's home). They never enter Git and never apply in production.
- Development is HTTP-only, so no HTTPS dev certificate is mounted. The application never holds a certificate in any environment: production TLS terminates in the ingress (`hard-rules.md` §10), and production endpoint URLs are base literals in `docker-compose.yml`.
