# Architecture

How the **Bruno collection** is put together — the collection root, environments, auth, folders, and the anatomy of a request, including where scripts, tests, and variables live. `structure.md` shows the concrete file layout; this document explains how the pieces fit and behave.

---

## The three layers of a collection

```
collection root (collection.bru)        ← auth (OAuth 2.0), base headers, collection vars, shared scripts
        │  inherited by everything below
        ▼
environments (environments/*.bru)       ← per-environment values: baseUrl, token endpoint, realm, clientId, secret password
        │  selected at run time
        ▼
requests (*.bru, in mirrored folders)   ← one endpoint each: method+url, body, auth:inherit, assertions, tests, captured vars
```

Everything an individual request needs that is *not* specific to that one call — authentication, the base URL, shared headers — is defined **once** higher up and flows down. A request file states only what makes it that request.

---

## Collection root — `collection.bru`

The collection root is where cross-cutting behaviour is defined once:

- **Auth (OAuth 2.0).** The single auth definition for the whole collection. Every request sets `auth: inherit` and therefore sends the bearer token Bruno obtains from this definition (`hard-rules.md` §3). The flow is **Authorization Code + PKCE** — the verified setup for the dev Keycloak public client. Bruno performs a **one-time interactive browser login** (a dev user account), then caches the token, so the request chain runs without further prompts. **No credential is stored in the collection.** All parameters come from variables:

  ```
  auth {
    mode: oauth2
  }
  auth:oauth2 {
    grant_type: authorization_code
    callback_url: {{callbackUrl}}
    authorization_url: {{authorizeUrl}}
    access_token_url: {{tokenUrl}}
    client_id: {{clientId}}
    scope: openid profile email
    pkce: true
    credentials_placement: body
    token_placement: header
    token_header_prefix: Bearer
    auto_fetch_token: true
    auto_refresh_token: false
  }
  ```

  **The token-attachment fields are mandatory, not optional.** `token_placement: header` + `token_header_prefix: Bearer` are what make Bruno emit `Authorization: Bearer <token>`. If `token_header_prefix` is omitted, current Bruno defaults it to an **empty string** and sends `Authorization: <token>` with no scheme — which a JWT bearer API rejects with **401** even though the login succeeded and the token is valid. Always include these fields.

  The **Password (Resource Owner) grant** is the documented alternative for a fully headless run (e.g. Bruno CLI in CI), where a browser login is not possible: it replaces the browser step with a stored `password` secret var (`grant_type: password`, `username`/`password` from vars). Use it only when interactive login is unavailable.

- **Base headers / collection vars / shared scripts** — anything every request should carry (e.g. a default `Accept`), or a collection-scoped variable, or a pre-/post-request script shared across the collection.

No secret literal ever appears here — `{{password}}` resolves from a secret variable (`security.md`).

---

## Environments — `environments/*.bru`

An environment supplies the concrete values the collection root and requests reference. The committed `local` environment targets the dev stack:

- **Non-secret literals (committed):** `baseUrl` (the API host), `tokenUrl` (Keycloak token endpoint), `realm`, `clientId` (the public client), `username`.
- **Secret vars (declared, value not committed):** `password` — a Bruno secret var supplied at runtime / via `.env`.

Selecting an environment rebinds every `{{var}}` in the collection, so the same requests run against dev today and another environment later without edits.

---

## Variable scopes and the capture chain

| Scope | Set where | Lives for | Used for |
|---|---|---|---|
| **Environment var** | `environments/*.bru` | The selected environment | Host, endpoints, client id, credentials |
| **Collection var** | `collection.bru` | The whole run | Shared constants |
| **Runtime var** (captured) | `vars:post-response` on a request | The rest of the run | Ids/tokens produced by one request and consumed by later ones |

The **capture chain** is what makes the collection run unattended (`hard-rules.md` §8): a `Create` request captures the new resource id into a runtime var (`vars:post-response { noteId: res.body.data.id }`), and the following `Get` / `Update` / `Delete` requests use `{{noteId}}` in their URLs. No value is ever pasted by hand between requests.

---

## Anatomy of a request

Each request `.bru` contains only the blocks it needs:

- **`meta`** — `name`, `type: http`, and `seq` (ordering within its folder; reflects the dependency chain).
- **method block** (`get`/`post`/`patch`/`delete`) — the `url` built from `{{baseUrl}}` and captured vars; `body: json` and `auth: inherit` where relevant.
- **`headers`** — request-specific headers only (shared ones are inherited).
- **`body:json`** — the happy-path payload for writes.
- **`vars:post-response`** — captures values later requests depend on.
- **`assert`** — declarative success checks (at least the status code).
- **`tests`** — scripted `expect` assertions on status and essential response shape.

Auth is **not** a per-request block — it is inherited (`hard-rules.md` §3).

---

## How it maps to the API

The collection is a mirror of the API surface. A folder path in `bruno/` matches the endpoint's own location — API version → bounded context → aggregate — and each operation (create, single, collection, update, delete) is one request file. Because the API returns the shared `Result` / `PaginatedResult` envelope, captures and assertions read the response through that envelope (e.g. the created id at `res.body.data.id`, a list at `res.body.data`). See `structure.md` for the exact layout and `coding-standards.md` for the request-writing conventions.

---

## Running the collection

The collection is runnable two ways, both using only inherited auth and captured variables:

- **Bruno app runner** — pick the `local` environment, run a folder or the whole collection top to bottom.
- **Bruno CLI** (`bru run`, optional) — the same, headless; suitable for a future CI smoke stage. Because auth and every dependency are inherited/captured, no interactive input is required.
