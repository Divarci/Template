# Philosophy

The guiding principles behind the **Bruno collection** — the runnable, version-controlled exercise of the API. Where `hard-rules.md` states what is non-negotiable, this document explains *why* the collection is shaped as it is.

---

## The API's behaviour, kept honest in git

Every endpoint the API exposes has a matching Bruno request that lives in the repository next to everything else. The collection is not a throwaway scratchpad in someone's local Bruno app — it is committed, reviewed, and evolves with the API. When an endpoint changes, its request changes in the same breath (`hard-rules.md` §2). This keeps a truthful, executable description of the API surface that anyone can pick up and run.

---

## One click to a working call

The measure of the collection is that a newcomer can open it, pick the environment, and run any request — or the whole flow — and get a green result, without hunting for a token, editing a URL, or pasting an id from one response into the next. That is why auth is configured once and inherited (`hard-rules.md` §3), why values are captured into variables (§6), and why the collection runs unattended end-to-end (§8). Friction removed once is friction removed for everyone.

---

## Happy path first, and — for now — only

The collection's job is to prove the intended flow works: create a thing, read it back, change it, remove it. It is a smoke test of the contract, not an exhaustive negative-testing suite. We deliberately keep it to the happy path (`hard-rules.md` §5) so it stays small, fast, and unambiguous. Exhaustive failure-mode coverage is not the API client collection's job.

---

## Mirror the API so nothing gets lost

The collection's shape follows the API's shape — same version, context, aggregate, operation (`hard-rules.md` §7). A convention that mirrors the thing it describes needs no separate map: you already know where a request lives because you know where its endpoint lives. Consistency here is what lets the collection grow to cover the whole API without becoming a maze.

---

## Secure by default, even in a test client

A tool that holds a bearer token to the application's data is a place secrets can leak. So the collection treats credentials the same way the rest of the system does: nothing secret is ever committed, only non-secret identifiers are literals, and tokens are never logged (`hard-rules.md` §4, `security.md`). Convenience never comes at the cost of a committed password.
