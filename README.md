# TTEC Platform

Monorepo for TTEC's construction technology services — geospatial QA/QC and field support ticketing for IC (Intelligent Compaction) and PMTP (Paving Materials & Thermal Profiling) operations.

## Services

| Service | Description | Local Port | Docs |
|---------|-------------|------------|------|
| [GeoOps](apps/geoops/) | Geospatial QA/QC platform — map-based test results, analytics, sensor data | API: 8080, UI: 80 | [README](apps/geoops/README.md) |
| [Ticketing](apps/ticketing/) | Support ticket & resolution tracker — equipment issues, calibration, field support | API: 8081, UI: 81 | [README](apps/ticketing/README.md) |

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    TTEC Platform                         │
├──────────────────────┬──────────────────────────────────┤
│       GeoOps         │          Ticketing               │
│  Angular + .NET API  │       .NET API (+ future UI)     │
│  geoops.* schema     │       ticketing.* schema         │
├──────────────────────┴──────────────────────────────────┤
│              Shared SQL Server Database                  │
│         (separate schemas, no cross-references)         │
├─────────────────────────────────────────────────────────┤
│              Shared JWT Authentication                   │
│        (ttec-dev issuer, same signing key)              │
└─────────────────────────────────────────────────────────┘
```

Each service is **fully independent** — no shared code, no shared models, no compile-time dependencies. They share only:
- **Database server** (separate schemas: `geoops.*` and `ticketing.*`)
- **JWT tokens** (unified issuer/audience so one login works everywhere)

Integration between services is via **deep-linking with query params**, not API calls or shared libraries.

## Shared Authentication

Both services use the same JWT configuration. Logging into either service produces a token accepted by both:

| Setting | Value |
|---------|-------|
| Issuer | `ttec-dev` |
| Audience | `ttec-spa` |
| Demo user | `Benjamin` / `isHired!` |

## Quick Start

**Prerequisites:** .NET 8 SDK, Docker

```bash
# Run GeoOps locally
cd apps/geoops && docker compose up --build

# Run Ticketing locally
cd apps/ticketing && docker compose up --build

# Build everything
dotnet build ttec.sln

# Test everything
dotnet test apps/geoops/tests/GeoOps.Api.Tests.csproj
dotnet test apps/ticketing/tests/Ticketing.Api.Tests.csproj
```

## Repository Structure

```
ttec/
├── apps/
│   ├── geoops/                 # Geospatial QA/QC service
│   │   ├── backend/            #   .NET 8 API
│   │   ├── frontend/           #   Angular 19 SPA
│   │   ├── tests/              #   Integration tests
│   │   └── docker-compose.yml
│   └── ticketing/              # Ticketing service
│       ├── backend/            #   .NET 8 API
│       ├── tests/              #   Integration tests
│       └── docker-compose.yml
├── infra/                      # Azure Bicep IaC
│   ├── main.bicep
│   └── modules/
├── docs/
│   ├── how-it-all-works.md
│   ├── implementation-plan.md
│   └── azure-deployment.md
├── .github/workflows/
│   ├── ci-geoops.yml
│   └── ci-ticketing.yml
└── ttec.sln
```

## CI/CD

Each service has its own GitHub Actions workflow with path filtering — changes to `apps/geoops/**` trigger only the GeoOps pipeline, and likewise for ticketing. On merge to `main`, images are built, pushed to Azure Container Registry, and deployed to Azure Container Apps.

## Infrastructure

Azure resources (defined in `infra/main.bicep`):
- Azure Container Registry
- Azure SQL Server + Database
- Container Apps Environment with per-service Container Apps
- Auto-scaling (0-1 replicas for cost efficiency)

See [docs/azure-deployment.md](docs/azure-deployment.md) for deployment details.
