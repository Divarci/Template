# Security

Application- and runtime-level security rules for the `api` platform. Subordinate only to `hard-rules.md`.

**Scope boundary.** *Delivery-time* security — secret storage, the Azure DevOps Secure Files workflow, `.env`/`.env.example` discipline, container hardening, `.dockerignore`, pipeline authentication, and environment TLS configuration — lives in `docs/deployment/guidelines/security.md` and is **not** repeated here. This document governs how the running application authenticates, authorises, validates, fails, persists, and logs.

---

## Authentication

- **Secure by default.** Every endpoint requires an authenticated principal. Authorisation is on globally (a fallback authenticated-user policy); anonymous access is opt-in per endpoint and reserved for a tiny set (e.g. health checks). A new endpoint is authenticated unless someone deliberately and visibly opts out.
- **JWT Bearer + external IdP.** Authentication is JWT Bearer tokens issued by the external identity provider (Keycloak, see `tech-stack.md`). Token-validation settings (authority, issuer, audience, `RequireHttpsMetadata`) are bound from configuration via a typed options class (`JwtBearerConfigurationOptions`), never hard-coded in a registrar lambda — so every parameter is environment-driven.
- **HTTPS metadata is required in production.** `RequireHttpsMetadata` may be relaxed only in local development; production must fetch identity metadata over HTTPS (the environment split is enforced in the deployment security doc).
- **Identity is read from claims, never from the request body.** When an operation needs the caller's identity, it reads it from the validated token's claims (e.g. `sub`) at the adapter boundary — never from a client-supplied field.

---

## Authorization

- **Authenticated, not partitioned.** The project is single-tenant and models no per-user data ownership (hard rule 11, `domain-driven-design.md`). Every authenticated user may read and act on the same data; there is no tenant or owner filter to enforce. If tenant isolation or per-user ownership is needed, it is introduced as a deliberate architectural decision.
- **No magic strings.** If a privileged or role-gated surface is introduced, its policy identifiers are centralised as constants (a `PolicyNames`-style class) and referenced by name on actions — never inline string literals scattered across controllers.
- **Authorisation is never enforced in the model.** The domain has no knowledge of authentication; authZ decisions live at the adapter (HTTP) boundary.

---

## Input Validation

- **Validate at the boundary, then trust nothing downstream anyway.** Request models carry data-annotation validation; a globally registered validation filter (`ValidationFilter`) short-circuits invalid `ModelState` with a `422 Unprocessable Entity` `ProblemDetails` before any action code runs.
- **Defence in depth.** Boundary validation does not replace domain validation — entities still validate their own invariants in their `Create()`/behaviour methods (`domain-driven-design.md`). Both layers validate; neither assumes the other did.
- **All external input is untrusted** — HTTP request bodies, query parameters, tokens, and anything returned by an external system.

---

## Error Handling & Output

- **A single global boundary.** `GlobalExceptionHandler` catches every unhandled exception, emits a `500` `ProblemDetails`, attaches a `traceId`, and logs the failure. A `TemplateException` — an exception the application throws deliberately (`coding-standards.md` — Exceptions) — is logged with its structured context (assembly, class, method) **and** the exception itself (stack trace, inner exception); any other exception is logged as unhandled with its full stack trace. The two use distinct log templates, so "thrown by us" and "unexpected" stay distinguishable. Stack traces stay in server logs, never in responses.
- **Never leak internals to the client.** Responses must not carry stack traces, inner-exception text, connection strings, or any secret. The `traceId` correlates a terse client message with the detailed server log; `Result.Message` stays concise and non-leaky, and failed `Result`s are translated to RFC 7807 `ProblemDetails` via the shared result helper.
- **Fail closed.** On any ambiguity about whether an operation is permitted or an input is valid, the request is rejected — not allowed through.

---

## Data Protection

- **Treat the datastore as sensitive by default.** There is no "public" tier; all stored data is protected behind authentication.
- **Least-privilege database access.** The application connects to PostgreSQL with an account scoped to only the schemas/operations it needs; the connection string is a secret supplied via environment (deployment security doc), never in `appsettings.json` or code.
- **Secrets never live in the application.** No credential, token, API key, or connection string appears in source, `appsettings*.json`, logs, or responses. Runtime configuration is bound from environment into `internal sealed record` options types — or, for a framework-owned options type such as `JwtBearerOptions`, through an `internal sealed class` implementing `IConfigureNamedOptions<T>` (`JwtBearerConfigurationOptions`); secret *values* come only from the environment (local user-secrets in dev, `.env` in prod).

---

## Logging & Privacy

- **Never log a secret.** No credential, token, or connection string in any log — in any environment.
- **Minimise personal content in logs.** Log identifiers, `traceId`s, and structured metadata for diagnosis; do not dump full personal payloads into logs by default. Diagnostic detail lives in server logs, never in client responses.
- **Correlation over verbosity.** The `traceId` links a client-visible failure to its server-side detail, so logs can stay lean without losing traceability.

---

## Dependencies

- **Approved technologies only.** Use only the packages sanctioned in `tech-stack.md`; introducing a new third-party dependency is an architectural decision (and, if it wraps an external system, an `Infra.{Function}` project — `architecture.md`).
- **Pinned.** Package versions are pinned in the `.csproj` files; track advisories and breaking changes when upgrading.
