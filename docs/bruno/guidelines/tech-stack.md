# Tech Stack

Approved tooling for the **Bruno collection**. This document is the source of truth for *which* tools are allowed; it does not pin exact versions (the installed Bruno app / CLI is the source of truth for that).

**Status:** `✓` = in use today · `○` = planned / optional.

---

## Client & format

| Technology | Purpose | Status |
|---|---|---|
| **Bruno** (open-source API client) | Author and run the collection from the desktop app | ✓ |
| **Bru** language (`.bru` files) | Git-native, plain-text request/collection format — one request per file | ✓ |
| **Bruno CLI** (`bru run`) | Run the collection headless (local, and optionally in CI) | ○ |

> Bruno is chosen because the collection is **plain-text and git-native**: `.bru` files diff and review like source, unlike an opaque exported JSON blob. No account, cloud sync, or proprietary format is used.

## Authentication

| Technology | Purpose | Status |
|---|---|---|
| **OAuth 2.0 — Authorization Code + PKCE** (Bruno built-in auth, collection scope) | Obtain a bearer token (one-time browser login) and attach it to every request; Password grant is the headless alternative | ✓ |
| **Keycloak** (dev realm `template`, public client `template-public`) | The identity provider the OAuth flow targets | ✓ |

> Auth is configured once at the collection level and inherited by every request (`hard-rules.md` §3). The concrete OAuth parameters and how secrets are handled are defined in `architecture.md` and `security.md`; the working dev values live in the `local` environment.

## Variables & scripting

| Technology | Purpose | Status |
|---|---|---|
| **Bruno variables** (environment, collection, runtime) | Parameterise URLs/ids; carry captured values between requests | ✓ |
| **Bruno assertions** (`assert` block) | Declarative success-contract checks | ✓ |
| **Bruno tests** (`tests` block, `expect` API) | Scripted assertions on status and response shape | ✓ |
| **Post-response vars** (`vars:post-response`) | Capture ids/tokens from a response into runtime variables | ✓ |

## Out of scope

- No external test runner, no data-driven fuzzing, no negative-case matrices (happy path only — `hard-rules.md` §5).
- No secret manager beyond Bruno secret vars / `.env` (`security.md`).
- No proprietary Postman/Insomnia formats; the collection is Bruno `.bru` only.
