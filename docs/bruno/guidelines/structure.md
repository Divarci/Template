# Structure

Where the **Bruno collection** lives in the repository and how it is named. This layout is a convention: files that do not yet exist MUST follow the placement and naming here when they are added. The collection root is `bruno/` at the repository root (a sibling of `keycloak/` and the compose files).

---

## Repository layout

```
Template/                                   ← repo root
├── bruno/                                  ← Bruno collection root
│   ├── bruno.json                          ← collection manifest (name, Bru version, ignore list)
│   ├── collection.bru                      ← collection-level: OAuth 2.0 auth, base headers, shared vars/scripts
│   ├── .env                                ← secret values, if any — GIT-IGNORED, never committed
│   ├── environments/
│   │   └── local.bru                       ← dev values: baseUrl, authorizeUrl, tokenUrl, callbackUrl, clientId
│   └── v<N>/                               ← one folder per API version (VersionOne → v1)
│       └── <Context>/                      ← bounded context (mirrors the API)
│           └── <Aggregate>/                ← aggregate / resource (plural, e.g. Notes)
│               ├── folder.bru              ← (optional) folder metadata
│               ├── Create <Entity>.bru     ← singular entity name, e.g. Create Note
│               ├── Get <Entity> By Id.bru
│               ├── List <Aggregate>.bru    ← plural, e.g. List Notes
│               ├── Update <Entity>.bru
│               └── Delete <Entity>.bru
└── docs/
    └── bruno/
        └── guidelines/                     ← these documents
```

Only `bruno/` (including its git-ignored `.env`) and these guideline docs are in scope for the `bruno` domain.

---

## Placement rules

| Asset | Location | Rule |
|---|---|---|
| **Collection manifest** | `bruno/bruno.json` | One per collection; names the collection and Bru version. |
| **Collection root** | `bruno/collection.bru` | The single home of OAuth 2.0 auth and any shared headers/vars/scripts inherited by every request. |
| **Environments** | `bruno/environments/<name>.bru` | One file per environment; `local` targets the dev stack. Non-secret literals only; a secret is referenced as `{{process.env.<NAME>}}`, never written. |
| **Request** | `bruno/v<N>/<Context>/<Aggregate>/<Operation> <Entity or Aggregate>.bru` | One request per endpoint operation, at the path mirroring the endpoint (naming below). |
| **Folder metadata** | `bruno/…/<Aggregate>/folder.bru` | Optional; folder-scoped settings/vars only. |
| **Secret values** | `bruno/.env` (collection root) | Git-ignored (repo `.gitignore` and `bruno.json` ignore list); Bruno loads it and exposes values as `{{process.env.<NAME>}}`. Never committed. |

---

## Mirroring the API

The path under `bruno/` matches the endpoint's own location so a request is trivially findable:

| API endpoint | Bruno request file |
|---|---|
| `Adapter.Restful/Controllers/VersionOne/<Context>/<Aggregate>/…` → `POST /api/v1/<resource>` | `bruno/v1/<Context>/<Aggregate>/Create <Entity>.bru` |
| `GET /api/v1/<resource>/{id}` | `bruno/v1/<Context>/<Aggregate>/Get <Entity> By Id.bru` |
| `GET /api/v1/<resource>` | `bruno/v1/<Context>/<Aggregate>/List <Aggregate>.bru` |
| `PATCH /api/v1/<resource>/{id}` | `bruno/v1/<Context>/<Aggregate>/Update <Entity>.bru` |
| `DELETE /api/v1/<resource>/{id}` | `bruno/v1/<Context>/<Aggregate>/Delete <Entity>.bru` |

`v<N>` folds the API's `Version<N>` (`VersionOne` → `v1`). Context and aggregate names match the API's bounded-context / aggregate folders exactly.

---

## Naming

- **Request `name`** (in `meta`) is human-readable Title Case — the singular entity name for single-resource operations and the plural aggregate name for the list: `Create Note`, `Get Note By Id`, `List Notes`, `Update Note`, `Delete Note`. The file name matches the request name.
- **Sequence** (`seq` in `meta`) orders requests within a folder along the dependency chain: create (1) → list/single (2–3) → update (4) → delete (5). A read-only aggregate with no create endpoint (e.g. `AuditLogs`) orders list (1) → single (2), the single using the id the list captures.
- **Cross-folder dependencies.** Folders carry no `folder.bru` sequence, so Bruno runs them in alphabetical order. A folder that consumes a variable captured in another folder (e.g. `AuditLogs` filtering by `{{noteId}}` from `Notes`) must therefore sort after its producer; running it on its own requires the producing request to have run earlier in the same Bruno session.
- **Variables** use camelCase: environment vars (`baseUrl`, `authorizeUrl`, `tokenUrl`, `callbackUrl`, `clientId`), captured runtime vars (`noteId`, `auditLogId`).

---

## What does NOT belong here

- Application source, Dockerfiles, compose files, and pipelines are owned by other domains — never placed under `bruno/`.
- Real secret values never appear in any committed file under `bruno/`; they live only in the git-ignored `bruno/.env` (`security.md`).
- These guideline documents are the only files under `docs/bruno/`.
