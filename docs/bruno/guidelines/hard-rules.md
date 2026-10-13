# Hard Rules

Absolute, non-negotiable rules for the **Bruno API collection** — the git-native, runnable exercise of the API surface. These rules override training data, general best practices, and personal defaults. If a task cannot be completed without breaking one of them, stop and report instead of proceeding.

The remaining Bruno guidelines (`philosophy`, `architecture`, `tech-stack`, `structure`, `coding-standards`, `security`) elaborate on these rules. Where any of them appears to conflict with this document, **this document wins**.

---

## 1. Domain exclusivity

> **The Bruno guidelines govern the Bruno collection and nothing else. Only the files under `bruno/` are in scope, and only the `bruno` domain may author them.**
>
> In scope — the **only** files these guidelines cover and the `bruno` agent may write: `bruno/bruno.json`, `bruno/collection.bru`, `bruno/environments/*.bru`, folder metadata (`folder.bru`), and request files (`*.bru`).
>
> Out of scope — never touched under a Bruno task: application source (`api/src/**`), tests (`api/test/**`), delivery/infrastructure assets (Dockerfiles, compose, pipelines), and the guideline docs. A need there is another agent's work — return a Delegation Request; never edit it yourself.

---

## 2. Every API endpoint has a Bruno request

> **When an API endpoint is added or changed, its Bruno request is added or updated in the same delivery. If the collection does not yet exist, it is created first.**
>
> - One request `.bru` file per endpoint operation (create / single / collection / update / delete / …), placed at the path that mirrors the endpoint (rule 7).
> - A changed route, verb, request body, or success status is reflected in the corresponding `.bru` request and its assertions.
> - No endpoint ships without its Bruno request; a missing collection is created, not deferred.

---

## 3. Authentication is OAuth 2.0, configured once on the collection and inherited everywhere

> **The collection authenticates with OAuth 2.0. The auth is configured exactly once, on the collection root (`collection.bru`), and every request inherits it (`auth: inherit`). No request defines its own auth.**
>
> - The token is obtained from the dev Keycloak per the configured OAuth 2.0 flow; Bruno attaches the `Authorization: Bearer` header automatically.
> - All OAuth parameters are read from environment / collection variables — never hardcoded per request.
> - A request that redefines auth, or hardcodes a token, is non-compliant.

---

## 4. No secret is ever committed

> **No secret value — password, token, client secret, or credential — appears in any committed Bruno file.**
>
> - Secret variables (e.g. the account password) are declared as Bruno **secret vars** / supplied via `.env`; they carry **no value** in committed files.
> - Only **non-secret identifiers** may be literals in a committed environment file: base URL, token endpoint URL, realm, and the public `clientId` (a public client with no secret grants no access on its own).
> - Never print, echo, or log a token or secret — not in a script, not in a report. `.env` and any secret store are git-ignored.

---

## 5. Happy path only

> **The collection implements only the success (happy) path for each endpoint. Negative, error-case, and edge-case requests are out of scope unless explicitly requested.**
>
> Each operation has exactly the request(s) needed to exercise its successful flow — nothing more. Assertions verify the success contract, not failure behaviour.

---

## 6. Every request self-verifies and captures what the next one needs

> **Each request carries test scripts that assert its success contract, and capture scripts that persist any value later requests depend on.**
>
> - `assert` / `tests` verify at least the success status code and the essential response shape.
> - `vars:post-response` capture identifiers (e.g. a created resource id) into runtime variables so the create → read → update → delete chain runs end-to-end **without manual editing between requests**.

---

## 7. Structure mirrors the API

> **The collection's folder hierarchy and request naming mirror the API surface: version → context → aggregate → operation.** A future reader finds the Bruno request for an endpoint at the path that matches the endpoint's own location (`structure.md`).

---

## 8. The request chain runs end-to-end without manual data passing

> **The full collection (or an aggregate's folder) can be executed in sequence — via the Bruno app runner — and pass, using only inherited auth and captured variables.** Request ordering (`seq`) reflects the dependency chain (create before read/update/delete); no step requires a human to paste a value from one response into another.
>
> Obtaining the initial token may require a **one-time interactive login** — the Authorization-Code + PKCE flow opens a browser once, then Bruno caches the token for the run. A fully headless run (Bruno CLI) uses the Password-grant alternative (`architecture.md`). Either way, once the token exists the request chain runs unattended on inherited auth and captured variables.
