# Structure

Where deployment assets live in the repository and how they are named. This layout is a convention: assets that do not yet exist (e.g. the api CI pipeline, additional Dockerfiles) MUST follow the placement and naming described here when they are added.

---

## Repository layout

```
Template/                                   ← repo root
├── Template.slnx                           ← solution (built by CI)
├── docker-compose.yml                      ← base / production service definitions
├── docker-compose.override.yml             ← development overrides (auto-merged locally)
├── docker-compose.dcproj                   ← VS Compose project wrapper
├── .dockerignore                           ← build-context exclusions (root)
├── .env                                    ← runtime secrets — GIT-IGNORED, never committed
├── .env.example                            ← required key names + placeholders (committed)
├── pipelines/
│   ├── azure-pipelines-ci-cd.yml                    ← CD: build on self-hosted machine + deploy
│   └── azure-pipelines-api-build-and-test.yml       ← CI: api PR gate                            [to add]
├── keycloak/                               ← realm-as-code (IdP config); imported by the dev Keycloak container
│   └── realm-template.json                 ← Keycloak realm export — committed source of truth, NO real secrets
├── api/
│   └── src/
│       └── adapters/
│           ├── Adapter.Restful/
│           │   ├── Adapter.Restful.csproj
│           │   └── Dockerfile              ← co-located with its csproj
│           └── Adapter.Scheduler/
│               ├── Adapter.Scheduler.csproj
│               └── Dockerfile              ← co-located with its csproj
└── docs/
    └── deployment/
        └── guidelines/                     ← these documents
```

Entries marked `[to add]` are conventions for assets not yet present; create them in these exact locations.

---

## Placement rules

| Asset | Location | Rule |
|---|---|---|
| **Dockerfile** | Alongside the owning adapter's `.csproj` (e.g. `api/src/adapters/Adapter.Restful/Dockerfile`) | One Dockerfile per runnable host, co-located with its project. Never centralised in a `docker/` folder. |
| **`docker-compose.yml`** | Repo root | Single base file for all services. Build **context is the repo root**; the `dockerfile:` key holds the path to the co-located Dockerfile. |
| **`docker-compose.override.yml`** | Repo root | Development deltas only; merged automatically by `docker compose` when present. |
| **`docker-compose.dcproj`** | Repo root | Visual Studio Compose tooling wrapper; leave in place. |
| **Pipeline definitions** | `pipelines/` | Named `azure-pipelines-*.yml` (see naming below). |
| **`.dockerignore`** | Repo root | Single root file governing every build context. |
| **`.env` / `.env.example`** | Repo root | `.env` git-ignored; `.env.example` committed with keys only. |
| **Deploy/ops scripts** | Repo root or a `scripts/` folder | Kept in the repo, reviewed like any asset; never hand-run steps that live nowhere. |
| **Keycloak realm-as-code** | Repo root `keycloak/` (e.g. `keycloak/realm-template.json`) | Committed IdP/realm config — the single source of truth, imported by dev (Keycloak container `start-dev --import-realm`) and, once provisioned, prod (external Keycloak). Contains **no real secret** (confidential-client secrets are auto-generated on import; passkeys and SMTP are per-instance). Environment-specific values (redirect URIs, issuer/host URL) differ per environment and are set per instance. Sync workflow: **edit the JSON → commit → each environment re-imports**; if tuned in a Keycloak admin UI, **export and re-commit** to keep the JSON authoritative. |

---

## Pipeline naming

Pipelines are named by role so their purpose is obvious in the Azure DevOps list. All reside in the `pipelines/` directory; the table below lists their bare filenames.

There is one CD pipeline and **one CI pipeline per application platform**. `api` is the only application platform today.

| File | Role | Trigger |
|---|---|---|
| `azure-pipelines-ci-cd.yml` | CD — build on the self-hosted machine and deploy | push to the deploy branch (`DEPLOY_BRANCH`, default `master`) |
| `azure-pipelines-api-build-and-test.yml` | CI — api build + test gate | pull request (`*`) |

If another application platform is added later, its CI gate follows the same pattern under the `pipelines/` directory: `pipelines/azure-pipelines-{platform}-build-and-test.yml`.

---

## Dockerfile / service naming

- **Compose service name** matches the adapter, lowercased with a dot: `adapter.restful`, `adapter.scheduler`. Stock supporting services use a short functional name (e.g. a cache or database service).
- **Image name** for a custom build is a bare lowercase compact name (e.g. `adapterrestful`, `adapterscheduler`). No registry prefix is used — there is no container registry (see `hard-rules.md` §4); images are built locally and never pushed or pulled.
- **`container_name`** (when set) uses a readable PascalCase project prefix (e.g. `Template-Api`), added when it aids operability; it is optional for single-instance services.

---

## What does NOT belong here

- Application source, `appsettings*.json`, and `Program.cs` live under `api/src/**` and are owned by the `api` platform — not deployment assets.
- Test projects live under the solution's test tree and are owned by `tester`.
- These guideline documents are the only files under `docs/deployment/`.
