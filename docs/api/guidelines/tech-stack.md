# Tech Stack

Approved technologies for the `api` platform, grouped by concern and mapped to the owning project. This document is the source of truth for **which** technologies are allowed; the `.csproj` files remain the source of truth for **exact versions** (do not duplicate version numbers here).

**Status:** `✓` = referenced in the repo today · `○` = planned (part of the target stack, not yet wired).

---

## Runtime & Language

| Technology | Project | Status |
|---|---|---|
| **.NET 10** (`net10.0`) | All projects | ✓ |
| **C#** — `Nullable` + `ImplicitUsings` enabled | All projects | ✓ |

## Project SDKs

| SDK | Project | Status |
|---|---|---|
| **Microsoft.NET.Sdk.Web** | `Adapter.Restful` | ✓ |
| **Microsoft.NET.Sdk.Worker** | `Adapter.Scheduler` | ✓ |
| **Microsoft.NET.Sdk** | `App.*`, `Core.*`, `Infra.*` | ✓ |

## Web & API

| Technology | Project | Status |
|---|---|---|
| **ASP.NET Core** (Web SDK host) | `Adapter.Restful` | ✓ |
| **.NET Generic Host** (Worker SDK host) | `Adapter.Scheduler` | ✓ |
| **Asp.Versioning.Mvc** (API versioning) | `Adapter.Restful` | ✓ |
| **OpenAPI** (`Microsoft.AspNetCore.OpenApi`, `Microsoft.OpenApi`) | `Adapter.Restful` | ✓ |

## Authentication & Identity

| Technology | Project | Status |
|---|---|---|
| **JWT Bearer** (`Microsoft.AspNetCore.Authentication.JwtBearer`) — validates tokens issued by Keycloak | `Adapter.Restful` | ✓ |
| **Keycloak** — external identity provider (OIDC); realm-as-code under `keycloak/` | external service | ✓ |

## Data Access

| Technology | Project | Status |
|---|---|---|
| **Entity Framework Core** | `Infra.Persistence` | ✓ |
| **PostgreSQL** (`Npgsql.EntityFrameworkCore.PostgreSQL`) | `Infra.Persistence` | ✓ |
| **EF Core Design** (migrations tooling) | `Adapter.Restful` | ✓ |

## Caching

| Technology | Project | Status |
|---|---|---|
| **Redis** / **StackExchange.Redis** | `Infra.Caching` | ✓ |

## Background Processing

| Technology | Project | Status |
|---|---|---|
| **Quartz.NET** (`Quartz`, `Quartz.Extensions.Hosting`) | `Adapter.Scheduler` | ✓ |

## Observability & Logging

| Technology | Project | Status |
|---|---|---|
| **OpenTelemetry** (`OpenTelemetry.Api`) | `Adapter.Restful` | ✓ |
| **Serilog** (`Serilog.AspNetCore`) | `Adapter.Restful` | ✓ |
| **Seq** (`Serilog.Sinks.Seq`) | `Adapter.Restful` | ✓ |

## Containerisation & Delivery

| Technology | Usage | Status |
|---|---|---|
| **Docker** — base images `mcr.microsoft.com/dotnet/{sdk,aspnet,runtime}:10.0` | All services | ✓ |
| **Docker Compose** | `adapter.restful`, `adapter.scheduler`; dev-only `postgres`, `redis`, `seq`, `keycloak` (override file) | ✓ |
| **VS Container Tools** (`Microsoft.VisualStudio.Azure.Containers.Tools.Targets`) | `Adapter.Restful` | ✓ |

The deployment guidelines (`docs/deployment/guidelines/`) own the authoritative detail of images, Compose files, and environments.

## Testing

Deferred — no test projects exist yet. The test framework and tooling are decided separately; until then, introducing a test framework is a decision to raise with the user.
