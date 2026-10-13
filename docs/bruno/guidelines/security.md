# Security

Security rules for the **Bruno collection**. Subordinate only to `hard-rules.md`. Because the collection holds a bearer token to the application's data, it is treated with the same secret-hygiene discipline as the rest of the system.

---

## Secrets are never committed

- **No secret value in any committed Bruno file** — not a password, token, client secret, or credential, in `collection.bru`, an environment file, or a request (`hard-rules.md` §4).
- Secret variables are declared as Bruno **secret vars** and/or supplied through a git-ignored `.env` at the repo root; committed files carry the **name only**, never the value.
- `.env` (and any secret store) is git-ignored. If a secret value is ever found committed, treat it as a security finding: report it, recommend rotation, and replace it with a secret var — never silently leave or delete it.

---

## What may be a committed literal (non-secret identifiers)

The dev OAuth setup exposes several identifiers that grant **no access on their own** and are safe as committed literals in `environments/local.bru`:

- `baseUrl` — the API host (e.g. the local dev URL).
- `authorizeUrl` — the Keycloak authorization endpoint.
- `tokenUrl` — the Keycloak token endpoint.
- `callbackUrl` — the local OAuth redirect/callback URL Bruno listens on.
- `realm` — the Keycloak realm name.
- `clientId` — the **public** OAuth client (no client secret; a public client id is not a credential).

With **Authorization Code + PKCE** there is **no stored credential** — the user logs in through the browser and Bruno holds only the resulting token; nothing secret is committed or even needed at rest. The Password-grant alternative (headless only) additionally needs `username` (non-secret) and the account **password** as a secret var — valueless in git, provided at run time.

---

## OAuth 2.0 handling

- Auth is defined once on the collection and inherited (`hard-rules.md` §3); the token is fetched by Bruno from the dev Keycloak using the configured grant. Tokens are held only in Bruno's runtime, never written to a committed file.
- The flow is **Authorization Code + PKCE** against a **public client** (no client secret), so no credential is stored: the user authenticates in the browser and Bruno keeps only the resulting token. The Password-grant alternative (headless only) needs the account password as a secret var and requires no browser.
- Never place a token, `Authorization` header value, or password in a request, a script `console.log`, or a report. Refer to credentials by variable name only.

---

## Scripts and logging

- Pre-/post-request and test scripts must never print secrets or tokens (`console.log` of a bearer token is forbidden).
- Captured runtime variables (`vars:post-response`) hold non-secret resource ids, not credentials.

---

## Scope and blast radius

- The collection exercises the API only through its public HTTP surface, as an authenticated user — it never reaches a database, cache, or internal service directly.
- Happy-path only (`hard-rules.md` §5): the collection does not probe for authorization bypasses or error leakage — that is the province of the automated test suite and `reviewer`, not the API client collection.
- Point the collection only at environments the operator is authorized to exercise; the committed `local` environment targets the dev stack.
