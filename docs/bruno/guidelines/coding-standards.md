# Coding Standards

How `.bru` files are written in the collection. `structure.md` governs where files live and how they are named; this document governs how each file is written. All conventions are derived from the collection layout and the hard rules.

---

## Request file anatomy

A request declares only the blocks it needs, in this order:

```
meta {
  name: Create Note
  type: http
  seq: 1
}

post {
  url: {{baseUrl}}/api/v1/notes
  body: json
  auth: inherit
}

headers {
  Content-Type: application/json
}

body:json {
  {
    "title": "Bruno happy-path note",
    "content": "Created by the Bruno collection",
    "isPinned": false
  }
}

vars:post-response {
  noteId: res.body.data.id
}

assert {
  res.status: eq 201
}

tests {
  test("returns 201 Created", function() {
    expect(res.status).to.equal(201);
  });
  test("returns a created id", function() {
    expect(res.body.data.id).to.be.a("string");
  });
}
```

Rules:
- **`auth: inherit` always** — never an `auth` / `auth:oauth2` block on a request (auth is collection-level — `hard-rules.md` §3).
- **URLs use `{{baseUrl}}`** and captured vars (`{{noteId}}`) — never a hardcoded host or id.
- **One request per file**; `meta.name` matches the file name; `seq` reflects the dependency chain.
- **Headers**: only request-specific ones; shared headers are inherited from the collection.
- **Query parameters**: a request with a query string (e.g. a list with paging/filters) carries a `params:query` block right after the method block, kept identical to the query string in the `url`.

---

## Capturing values for the chain

Every write whose output a later request needs captures it in `vars:post-response`, reading through the API's `Result` envelope:

```
vars:post-response {
  noteId: res.body.data.id
}
```

Downstream requests consume it in the URL: `{{baseUrl}}/api/v1/notes/{{noteId}}`. This is what lets the folder run create → single → list → update → delete unattended (`hard-rules.md` §8). Never paste an id between requests by hand.

---

## Assertions and tests (happy path)

Each request verifies its **success contract** — nothing about failure modes (`hard-rules.md` §5). At minimum:

- **Status code** — the operation's documented success status:

  | Operation | Success status | Body |
  |---|---|---|
  | Create (`POST`) | `201` | created resource (`Result<T>`) |
  | Single (`GET /{id}`) | `200` | resource (`Result<T>`) |
  | List (`GET`) | `200` | page (`PaginatedResult<T[]>`: items at `data`, paging info at `metadata`) |
  | Update (`PATCH /{id}`) | `204` | none |
  | Delete (`DELETE /{id}`) | `204` | none |

- **Essential shape** — for bodied responses, assert the key fields exist (e.g. the id on create; the `data` array and the `metadata` paging fields on list). For `204` responses, assert the status only (there is no body to inspect).

Use the declarative `assert` block for the status and simple field checks; use the `tests` block with `expect(...)` for anything that needs a scripted check. Keep assertions to the success contract — do not over-specify volatile values.

---

## Variables

- Reference variables with `{{var}}`; define environment values in `environments/*.bru`, shared constants in `collection.bru`, captured values in `vars:post-response`.
- Name variables in camelCase (`baseUrl`, `tokenUrl`, `clientId`, `noteId`, `auditLogId`).
- Never inline a value that belongs in a variable (host, token endpoint, client id, credentials, captured ids).

---

## Bodies

- `body: json` for writes; the JSON is the **happy-path** payload only (valid, complete, minimal).
- Payloads use the request model's field names exactly as the API expects (matching the `[JsonPropertyName]` on the request model).
- No commented-out alternative payloads, no negative-case variants.

---

## Collection & environment files

- `collection.bru` holds the OAuth 2.0 auth (`auth:oauth2`), any base headers, and shared scripts — defined once, inherited everywhere. All OAuth parameters are `{{vars}}`.
- `environments/local.bru` holds non-secret dev literals only (`baseUrl`, `authorizeUrl`, `tokenUrl`, `callbackUrl`, `clientId`); a secret, if ever needed, is referenced as `{{process.env.<NAME>}}` from `bruno/.env`, never written (`security.md`).
- `bruno.json` names the collection and Bru version and carries the ignore list (e.g. `.env`, `node_modules`).
