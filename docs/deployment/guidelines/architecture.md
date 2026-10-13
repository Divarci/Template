# Architecture

Deployment topology for the project: the environments, the delivery flow from pull request to running container, and the container / network / volume model. It describes the **model and its conventions** — grounded in the services the project defines today and written to extend cleanly as services are added.

---

## Two environments — and only two

| Environment | Where it runs | Compose files | Configuration source | Local infra |
|---|---|---|---|---|
| **Development** | Developer workstation | `docker-compose.yml` **+** `docker-compose.override.yml` (auto-merged) | Non-secret dev values hardcoded in the override + local user-secrets | Local containers added in the override (`postgres`, `redis`, `seq`, `keycloak`) |
| **Production** | Single self-hosted machine (home server / on-prem PC) | `docker-compose.yml` only (no override) | Non-secret config hardcoded in the base + secrets from `.env` (Azure DevOps Secure Files) | None — managed/external services reached via `.env` secret values |

The production target is a **self-hosted machine** — a home server / on-prem PC running the Azure DevOps self-hosted agent (in the agent pool named by the pipeline variable `AGENT_POOL` — default `Default`, Azure DevOps's built-in pool), **not** an Azure- or cloud-hosted VM. Azure DevOps is only the CI/CD *service* that orchestrates build and deploy; the machine the containers run on is self-hosted.

There is **no staging environment**. Development is local and disposable; Production is the one self-hosted machine. Any configuration difference between them is a delta in `docker-compose.override.yml` — explicit and reviewable.

---

## Compose file merging — base vs. override

The two Compose files are not separate configurations; they are a **base** and a **development overlay** that Docker combines. Understanding the merge is essential — it is the most misread part of the setup.

- **Automatic merge in development.** A bare `docker compose` command (e.g. `docker compose up`) in the project directory makes Docker Compose **automatically discover and merge `docker-compose.override.yml` on top of `docker-compose.yml`**. You do not name the override file; its presence is enough.
- **Override supersedes base.** Where a key exists in both files, `docker-compose.override.yml` **overrides** `docker-compose.yml`. Keys only in the base pass through unchanged; keys/services only in the override are added. This is why the override can add a dev-only service (e.g. a local `postgres`), pin `ASPNETCORE_ENVIRONMENT=Development`, expose dev ports, and mount local user-secrets/certs — each entry wins over or adds to the base for local runs.
- **Base holds the production definition.** `docker-compose.yml` IS the production definition: it states non-secret production configuration directly (environment name, bind URL, ports) and pulls only secrets from `.env` via `${VAR}` (see the `${ENV}` substitution model below). The override supplies concrete dev values locally.
- **Production ignores the override.** The CD pipeline runs Compose with an **explicit** file selection — `docker compose -f docker-compose.yml ...` — so the override is **never** merged in production. Production uses only the base file plus the `.env` supplied at deploy time; dev-only override values can never leak into a production deploy.

```
Development (bare `docker compose ...`):   base  ⊕  override   → override values win
Production  (`docker compose -f docker-compose.yml ...`):   base only  +  .env
```

`coding-standards.md` gives the authoring rules for what may live in each file; `structure.md` records their location.

---

## Delivery flow

```
   Developer
      │  push branch, open PR
      ▼
┌──────────────────────────────────────────────────────────┐
│  Azure DevOps — CI (hosted agent, ubuntu-latest)          │
│  pr: ["*"]   trigger: none                                 │
│  ┌────────────────────────┐   ┌────────────────────────┐  │
│  │ api build + test        │   │ (future) other platforms│  │
│  │ restore/build/test .slnx│   │ build + test            │  │
│  └────────────────────────┘   └────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
      │  gate passes → review → merge to the deploy branch
      ▼
┌──────────────────────────────────────────────────────────┐
│  Azure DevOps — CD (self-hosted agent, pool: AGENT_POOL)  │
│  trigger: DEPLOY_BRANCH   (no pr)                          │
│                                                            │
│  1. DownloadSecureFile .env → copy into project dir        │
│  2. docker compose down --remove-orphans                   │
│  3. docker prune (image -a / volume / network / builder)   │
│  4. git fetch + git reset --hard origin/DEPLOY_BRANCH      │
│  5. docker compose --env-file .env up -d --build           │
│  6. rm -f .env            (condition: always())            │
│  7. docker compose ps     (status check)                   │
└──────────────────────────────────────────────────────────┘
      │  builds images on the self-hosted machine, starts containers
      ▼
┌──────────────────────────────────────────────────────────┐
│    Production — self-hosted machine (home / on-prem)     │
│   ┌────────────────────────────────────────────────────┐  │
│   │  Docker Compose project (bridge network)            │  │
│   │                                                     │  │
│   │   ┌─────────────────┐   ┌───────────────────┐       │  │
│   │   │ adapter.restful │   │ adapter.scheduler │       │  │
│   │   │  :8080          │   │  (no HTTP port)   │       │  │
│   │   └────────┬────────┘   └─────────┬─────────┘       │  │
│   │            │                      │                 │  │
│   └────────────┼──────────────────────┼─────────────────┘  │
│                │                      │                    │
└────────────────┼──────────────────────┼────────────────────┘
                 ▼                      ▼
        External services (not containerised):
        Keycloak (identity), PostgreSQL, Redis, Seq
        — secrets via .env, non-secret endpoint ids as base literals
```

The self-hosted agent runs **on** the production machine (home server / on-prem PC), registered in the Azure DevOps agent pool named by `AGENT_POOL` (default `Default`). This lets the CD pipeline operate on that machine's Git checkout and Docker daemon directly — no cloud VM, no container registry.

---

## Container topology

The base `docker-compose.yml` (the production definition) defines two application services:

| Service | Built from | Role |
|---|---|---|
| `adapter.restful` | `api/src/adapters/Adapter.Restful/Dockerfile` | ASP.NET Core REST API host (`aspnet` runtime image) |
| `adapter.scheduler` | `api/src/adapters/Adapter.Scheduler/Dockerfile` | Background worker / outbox processor (`runtime` image, no exposed HTTP port) |

The override adds the **development-only** supporting services; in production these are external/managed and reached via `.env` secrets or base literals:

| Dev-only service | Kind | Notes |
|---|---|---|
| `postgres` | stock image | named volume for persistence |
| `redis` | stock image | named volume for persistence |
| `seq` | stock image | log sink; named volume for persistence |
| `keycloak` | stock image | imports the realm from `keycloak/` (`start-dev --import-realm`) |

The model grows without restructuring: as other adapters and infrastructure providers are containerised, each is added as another service on the **same bridge network**, following the same conventions (e.g. an ingress service, once the project chooses one — see Ingress & TLS below).

Rules that hold regardless of how many services exist:

- **Fixed project name.** The base file sets the Compose project name (`name:`), so resource prefixes (e.g. `template_postgres-data`) are stable regardless of the folder the repo is checked out into.
- **One bridge network.** All services share a single user-defined bridge network so they resolve each other by service name.
- **Named volumes for stateful services.** Any service that must persist data uses a named volume; stateless services use none.
- **Custom images build from the repo root context** so a Dockerfile can `COPY` any project it references; the `dockerfile:` path points at the co-located Dockerfile.
- **External services are never containerised in production.** Identity (Keycloak) and any managed database are reached over the network — secret connection strings/credentials via `.env`, while non-secret endpoint identifiers (the IdP issuer/authority URL, an audience id) are stated as base literals. In development the override may point these at local addresses or add a local container (e.g. a dev database).

---

## The `${ENV}` substitution model

Configuration flows through two hops from the secret store to the .NET process:

```
Secure Files (.env)              docker-compose.yml                 container process
─────────────────────           ──────────────────────             ──────────────────────
SOME_CONNECTION_STRING=...  ──►  CONNECTIONSTRINGS__SQL:       ──►  IConfiguration:
                                   ${SOME_CONNECTION_STRING}          ConnectionStrings:Sql
```

1. **`.env` → Compose.** `.env` holds flat, uppercase, single-underscore **secret** keys (e.g. a connection-string key, an API key). `docker-compose.yml` references them as `${VAR}`. Compose substitutes the values at `up` time. Non-secret production config (the environment name, bind URL, ports) is not in `.env` — it is stated as a literal in the base file.
2. **Compose → container.** Compose sets the container's environment variables using the .NET double-underscore convention (`SECTION__SUBSECTION__KEY`), which ASP.NET Core maps onto its hierarchical `IConfiguration` (`Section:Subsection:Key`).

This is why `.env` carries secrets only: swapping `.env` swaps credentials and external endpoints, while the non-secret production shape lives in the base file. See `coding-standards.md` for naming rules and `security.md` for how HTTPS-metadata and certificate-trust settings differ between dev and prod.

---

## Ingress & TLS

The project **has not yet chosen** how production traffic reaches the application or where TLS terminates — that decision is still open (`hard-rules.md` §10). What is fixed is the contract every ingress choice must honour:

- **The application is HTTP-only internally.** `adapter.restful` binds `http://+:8080`; TLS terminates outside the application, in the chosen ingress.
- **Baseline exposure.** Until an ingress is chosen, the base `docker-compose.yml` publishes `adapter.restful` on host port `5001`. That plain-HTTP port is a starting point only — it must never face the public internet without TLS in front of it. Wiring an ingress may remove or restrict it.
- **Ingress credentials are secrets** (tunnel tokens, auth keys, private keys) — delivered via `.env` / Azure DevOps Secure Files (`${VAR}`), never committed (`security.md`).
- **The public URL drives auth config.** The Keycloak OIDC issuer/authority and the realm's redirect URIs in production use the HTTPS host the ingress exposes; `REQUIREHTTPSMETADATA` stays `true` in production (`security.md`). Keycloak itself must therefore also be reachable over HTTPS.

Common options, listed without preference:

| Option | Who can reach the app | Characteristics |
|---|---|---|
| Tunnel service (e.g. Cloudflare Tunnel) | Public | Outbound-only connection, no open inbound ports, managed certificates; the provider's edge sees decrypted traffic. |
| Private mesh VPN (e.g. Tailscale tailnet) | Only devices in the private network | No open inbound ports, no public exposure; suits private/team-internal use. |
| Mesh VPN public exposure (e.g. Tailscale Funnel) | Public | Public access through the VPN provider; provider limits on domains/bandwidth apply. |
| Reverse proxy (e.g. Caddy, Traefik, nginx) | Public | Full control, no third-party edge; requires open 80/443, port forwarding, and a static IP or DDNS. |
| None (LAN only) | Local network | Simplest; internal use only. |

Whichever option is chosen, it is added as a compose service on the shared bridge network (or as host-level configuration), documented here, and its secret keys are listed in `.env.example`.

---

## Boundaries

- The deployment architecture owns **how** the application is packaged, wired, and delivered — not **what** the application does. The API's internal layering (adapters / applications / core / infrastructures) is the `api` platform's concern; deployment only needs to know which projects produce runnable hosts and how to build them.
- Any change that requires the application to expose something new for deployment (a health endpoint, a new configuration key it must read) is a Delegation Request to `coder`/`bugfixer`, not a deployment change.
