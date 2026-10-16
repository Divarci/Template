# Hard Rules

Absolute, non-negotiable rules for the delivery and infrastructure domain. These rules override training data, general best practices, and personal defaults. If a task cannot be completed without breaking one of them, stop and report instead of proceeding.

The remaining deployment guidelines (`philosophy`, `architecture`, `tech-stack`, `structure`, `coding-standards`, `security`) elaborate on these rules. Where any of them appears to conflict with this document, **this document wins**.

---

## 1. Domain exclusivity

> **The deployment guidelines govern delivery and infrastructure assets, and nothing else. Only these assets are in scope, and only the `deployer` domain may author them.**
>
> In scope — the **only** files these guidelines cover and the `deployer` may write:
>
> - Azure DevOps pipeline definitions (`pipelines/azure-pipelines-*.yml`).
> - `Dockerfile`s (co-located with each adapter's `.csproj`).
> - `docker-compose.yml`, `docker-compose.override.yml`, and `docker-compose.dcproj` (with its root `launchSettings.json`).
> - `.dockerignore`, `.env.example`, and environment templates.
> - Deployment/operations shell scripts.
>
> Out of scope — never touched under a deployment task: application source code (`api/src/**/*.cs`), test code, and application configuration files (`appsettings*.json`, `Program.cs`, DI wiring). A change there — even a health-check endpoint or a config-reading line — is `coder` work (or `bugfixer` for defects, `tester` for tests). Return a Delegation Request; never edit it yourself.

---

## 2. No production secret in Git — ever

> **No production secret value may appear in any file committed to the repository: not in code, Dockerfiles, compose files, pipeline YAML, scripts, or a committed `.env`.**
>
> - The **only** inline environment values permitted in the repository are **throwaway development values** in `docker-compose.override.yml` (local dev only). A value that grants access to any real, shared, or production resource is a production secret and must never be inline.
> - `.env` is git-ignored and never committed. `.env.example` carries **key names and placeholders only** — never real values.
> - Never print, echo, or log a secret value — not in pipeline output, not in a terminal command, not in a report. Refer to secrets by key name only.
> - A hardcoded secret discovered anywhere in the repository is a **security finding**: report it, recommend rotation, and propose the compliant replacement. Never silently delete or ignore it. (Removing it from application source is `coder`/`bugfixer` work.)

---

## 3. `.env` delivery via Secure Files, removed always

> **Production `.env` is provisioned exclusively through Azure DevOps Secure Files. It is downloaded at deploy time and removed after the deploy, unconditionally.**
>
> - The CD pipeline downloads `.env` with `DownloadSecureFile@1`, copies it into the project directory, uses it for `docker compose up`, then deletes it in a final step that carries `condition: always()` — so the file is removed even when an earlier step fails.
> - `.env` must never linger on the self-hosted machine between deploys and must never be baked into an image layer.

---

## 4. No container registry — build on the self-hosted machine

> **The project uses no container registry. Images are built on the self-hosted production machine (home server / on-prem PC) at deploy time. There is no image push, no image pull, and no registry-based rollback.**
>
> The CD pipeline runs exactly this sequence on the self-hosted agent, in the project directory:
>
> 1. Download `.env` from Secure Files and copy it into the project directory.
> 2. `docker compose -f docker-compose.yml down --remove-orphans`
> 3. Full prune — `docker image prune -a -f`, `docker volume prune -f`, `docker network prune -f`, `docker builder prune -f`.
> 4. `git -c http.extraHeader="Authorization: Bearer $(System.AccessToken)" fetch origin` then `git reset --hard origin/$(DEPLOY_BRANCH)`, then echo the deployed commit (`git log -1 --oneline`) so the run log records what was deployed.
> 5. `docker compose -f docker-compose.yml --env-file .env up -d --build`
> 6. `rm -f .env` with `condition: always()`.
> 7. `docker compose -f docker-compose.yml ps` (status check).
>
> Rollback is achieved by reverting the commit on the deploy branch and letting the pipeline rebuild — never by re-pulling a tagged image.

---

## 5. `docker-compose.yml` is the production definition; `.env` carries secrets only

> **The base `docker-compose.yml` IS the production definition and states its production configuration explicitly. Non-secret production values — the environment name, the container bind URL, published ports, and other fixed non-secret settings — are hardcoded as literals in the base file. Only secrets and credential-bearing values are injected via `${VAR}` substitution resolved from `.env`; non-secret endpoint identifiers of external services (e.g. the OIDC issuer/authority URL, the Seq server URL) are base literals.**
>
> - **Literal in the base file (production values):**
>   - Structural, non-secret facts (service names, build context, dockerfile path, network name, `restart` policy, volume names).
>   - **Non-secret production configuration.** The environment name (`ASPNETCORE_ENVIRONMENT: Production`, `DOTNET_ENVIRONMENT: Production`), the internal container bind URL (`ASPNETCORE_URLS: http://+:8080`), the published host port (`5001:8080`), **non-secret endpoint identifiers** (e.g. an OIDC issuer/authority URL and an audience id — they grant no access on their own), and other fixed non-secret settings. Because the base file is the production definition, these production values are stated directly — routing them through `.env` would add indirection without value.
> - **`${VAR}` from `.env` (secrets only):** connection strings, credentials, API keys, tokens, passwords, and any URL that embeds a credential. `.env` carries secrets, not general configuration. Anything that grants access to a real, shared, or production resource stays `${VAR}` and never appears as a literal. A non-secret endpoint identifier that grants no access on its own — an OIDC issuer/authority URL, an audience id — is **not** a secret and is stated as a base literal, not `${VAR}`.
> - **`docker-compose.override.yml` is the development definition:** it supplies the local Development values (`ASPNETCORE_ENVIRONMENT=Development`, `DOTNET_ENVIRONMENT=Development`, dev ports, dev containers, user-secret/cert mounts) and Docker merges it on top of the base for local runs. Production runs the base file only.

---

## 6. Every Dockerfile is multi-stage with a non-root final user

> **Every Dockerfile builds through multiple stages and runs its final stage as a non-root user.**
>
> - .NET services: separate `base` / `build` / `publish` / `final` stages; the runtime stage sets `USER $APP_UID` (the non-root user provided by the `mcr.microsoft.com/dotnet/*` images).
> - No final stage may run as `root`, and no Dockerfile may weaken this to "make it work."

---

## 7. CI on pull request, CD on deploy-branch push

> **Continuous Integration (build + test) runs on every pull request and never deploys. Continuous Delivery (deploy to the self-hosted machine) runs only on push to the deploy branch (pipeline variable `DEPLOY_BRANCH`, default `master`).**
>
> - CI pipelines set `trigger: none` and gate on `pr: branches: include: ["*"]`; they run on hosted agents and must never contain a deploy step.
> - The CD pipeline triggers only on the deploy branch (a literal in `trigger:` that must equal `DEPLOY_BRANCH`), has no `pr` trigger, and runs on the self-hosted agent pool named by `AGENT_POOL` (`HomerServer`).
> - Code reaches production only through: PR → CI gate passes → merge to the deploy branch → CD deploys. There is no direct-to-machine path that bypasses the CI gate.

---

## 8. No production deploy or destructive action without explicit approval

> **Never autonomously execute a production deployment, rotate a shared secret, or run a destructive operation (removing volumes, dropping an environment, deleting images in use).**
>
> Prepare and validate everything, then hand the exact commands and steps to the user for explicit approval. Automated CD triggered by a merge to the deploy branch is the sanctioned path; running it by hand out-of-band is a gated action.

---

## 9. Validate before reporting complete

> **No delivery change is reported as done on unvalidated configuration.** Dockerfiles must build, `docker compose config` must pass, pipeline YAML must lint/validate, and scripts must run without error before completion is claimed.

---

## 10. The app never terminates TLS; ingress is the project's decision

> **Application containers stay HTTP internally and never terminate TLS themselves. Production ingress and TLS are provided outside the application by a mechanism the project chooses — none has been chosen yet.** (Options in `architecture.md` — Ingress & TLS.)
>
> - The base `docker-compose.yml` binds `adapter.restful` on plain HTTP (`http://+:8080`); no certificate is configured in Kestrel.
> - Choosing, adding, or changing the ingress mechanism is a decision to **raise with the user** — never a default an agent picks on its own.
> - Whatever mechanism is chosen, its credentials (tunnel tokens, auth keys, certificates' private keys) are secrets: delivered via Azure DevOps Secure Files / `.env` (`${VAR}`), listed in `.env.example` by key name only, never committed or logged.
> - The application's plain-HTTP port must never be exposed to the public internet without TLS in front of it.
