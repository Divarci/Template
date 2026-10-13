# Structure

Where the **Bruno collection** lives in the repository and how it is named. This layout is a convention: files that do not yet exist MUST follow the placement and naming here when they are added. The collection root is `bruno/` at the repository root (a sibling of `keycloak/` and the compose files).

---

## Repository layout

```
Template/                                   ← repo root
├── bruno/                                  ← Bruno collection root
│   ├── bruno.json                          ← collection manifest (name, Bru version, ignore list)
│   ├── collection.bru                      ← collection-level: OAuth 2.0 auth, base headers, shared vars/scripts
│   ├── environments/
│   │   └── local.bru                       ← dev values: baseUrl, tokenUrl, realm, clientId, username (+ secret: password)
│   └── v<N>/                               ← one folder per API version (VersionOne → v1)
│       └── <Context>/                      ← bounded context (mirrors the API)
│           └── <Aggregate>/                ← aggregate / resource
│               ├── folder.bru              ← (optional) folder metadata
│               ├── Create <Aggregate>.bru
│               ├── Get <Aggregate> By Id.bru
│               ├── List <Aggregate>.bru
│               ├── Update <Aggregate>.bru
│               └── Delete <Aggregate>.bru
├── .env                                    ← Bruno secret values (e.g. password) — GIT-IGNORED, never committed
└── docs/
    └── bruno/
        └── guidelines/                     ← these documents
```

Only `bruno/` (plus the git-ignored `.env` for secret values) and these guideline docs are in scope for the `bruno` domain.

---

## Placement rules

| Asset | Location | Rule |
|---|---|---|
| **Collection manifest** | `bruno/bruno.json` | One per collection; names the collection and Bru version. |
| **Collection root** | `bruno/collection.bru` | The single home of OAuth 2.0 auth and any shared headers/vars/scripts inherited by every request. |
| **Environments** | `bruno/environments/<name>.bru` | One file per environment; `local` targets the dev stack. Non-secret literals only; secret vars declared without values. |
| **Request** | `bruno/v<N>/<Context>/<Aggregate>/<Operation> <Aggregate>.bru` | One request per endpoint operation, at the path mirroring the endpoint. |
| **Folder metadata** | `bruno/…/<Aggregate>/folder.bru` | Optional; folder-scoped settings/vars only. |
| **Secret values** | `.env` (repo root) | Git-ignored; supplies secret var values (e.g. `password`) at run time. Never committed. |

---

## Mirroring the API

The path under `bruno/` matches the endpoint's own location so a request is trivially findable:

| API endpoint | Bruno request file |
|---|---|
| `Adapter.Restful/Controllers/VersionOne/<Context>/<Aggregate>/…` → `POST /api/v1/<resource>` | `bruno/v1/<Context>/<Aggregate>/Create <Aggregate>.bru` |
| `GET /api/v1/<resource>/{id}` | `bruno/v1/<Context>/<Aggregate>/Get <Aggregate> By Id.bru` |
| `GET /api/v1/<resource>` | `bruno/v1/<Context>/<Aggregate>/List <Aggregate>.bru` |
| `PATCH /api/v1/<resource>/{id}` | `bruno/v1/<Context>/<Aggregate>/Update <Aggregate>.bru` |
| `DELETE /api/v1/<resource>/{id}` | `bruno/v1/<Context>/<Aggregate>/Delete <Aggregate>.bru` |

`v<N>` folds the API's `Version<N>` (`VersionOne` → `v1`). Context and aggregate names match the API's bounded-context / aggregate folders exactly.

---

## Naming

- **Request `name`** (in `meta`) is human-readable Title Case: `Create Note`, `Get Note By Id`, `List Notes`, `Update Note`, `Delete Note`. The file name matches the request name.
- **Sequence** (`seq` in `meta`) orders requests within a folder along the dependency chain: create (1) → list/single (2–3) → update (4) → delete (5).
- **Variables** use camelCase: environment vars (`baseUrl`, `tokenUrl`, `clientId`), captured runtime vars (`noteId`).

---

## What does NOT belong here

- Application source, tests, Dockerfiles, compose files, and pipelines are owned by other domains — never placed under `bruno/`.
- Real secret values never appear in any committed file under `bruno/`; they live only in the git-ignored `.env` (`security.md`).
- These guideline documents are the only files under `docs/bruno/`.
