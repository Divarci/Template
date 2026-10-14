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

| Technology | Project | Status |
|---|---|---|
| **xUnit v3** (`xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`) — test framework and runner | All `api/test/*` projects | ✓ |
| **AwesomeAssertions** — fluent `.Should()` assertions (the Apache-2.0 continuation of FluentAssertions) | All `api/test/*` projects | ✓ |
| **Moq** — mocking; **4.20.2 or later only** (4.20.0 and 4.20.1 shipped SponsorLink and are forbidden) | Unit test projects whose subject has dependencies | ✓ |
| **ArchUnitNET** (`TngTech.ArchUnitNET` + its xUnit v3 integration package) — architecture rules | `Architecture.Tests` | ✓ |

Rules:

- **Test packages live only in `api/test/`.** No `api/src` project ever references a test package. These packages are the only third-party dependencies exempt from the `Infra.{Function}` rule (`hard-rules.md` §2) — the exemption is for test projects alone.
- **FluentAssertions is not used** (commercial licence from v8); use AwesomeAssertions, whose API is identical. No other assertion or mocking library (Shouldly, NSubstitute, FakeItEasy, …) is introduced alongside these.
- **Integration testing is deferred.** Tests that need a real database, cache, or HTTP host (repositories, `UnitOfWork`, `CacheService`, controllers end to end) are out of scope until the integration tooling is decided; introducing it is a decision to raise with the user.
- Test conventions — layout, naming, what each layer's tests cover, the architecture rules — live in `structure.md` (Tests).
