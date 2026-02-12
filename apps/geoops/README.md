# GeoOps — Geospatial QA/QC Platform

Map-based construction quality assurance platform for IC (Intelligent Compaction) and PMTP (Paving Materials & Thermal Profiling) operations. Visualizes test results, sensor data, and field observations on interactive maps with pass/fail/warn status indicators.

[Back to TTEC Platform](../../README.md)

## What It Does

- **Project management** — create and track construction projects with geographic boundaries
- **Test results on a map** — GPS-pinned density, compaction, temperature, roughness, and dielectric test data with color-coded pass/fail markers
- **Analytics** — out-of-spec analysis, coverage gap detection, trend charts
- **Sensor monitoring** — IoT sensor readings (temperature probes, weather stations, strain gauges)
- **Data ingest** — single test entry, batch JSON ingest, CSV import
- **Export** — CSV and GeoJSON downloads
- **Field observations** — timestamped, geotagged notes with tags

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Angular 19, Leaflet maps, Chart.js |
| Backend | .NET 8 Web API, Entity Framework Core, NetTopologySuite |
| Database | SQL Server (`geoops` schema), geography columns for spatial data |
| Auth | JWT Bearer (shared `ttec-dev` issuer across platform) |
| Validation | FluentValidation |
| Infra | Docker, Azure Container Apps |

## API Endpoints

| Area | Routes | Description |
|------|--------|-------------|
| Auth | `POST /api/auth/login`, `GET /api/auth/me` | JWT login, current user |
| Projects | `GET/POST/PUT/DELETE /api/projects` | Project CRUD |
| Boundaries | `GET/POST/DELETE /api/projects/{id}/boundaries` | GeoJSON polygon boundaries |
| Features | `GET /api/projects/{id}/features` | Map data with bbox/time/type filters, pagination |
| Test Results | `POST /api/projects/{id}/test-results` | Single test result |
| Batch Ingest | `POST /api/projects/{id}/ingest/test-results` | Idempotent batch ingest |
| CSV Import | `POST /api/projects/{id}/test-results:import` | Queued CSV import |
| Test Types | `GET/POST/PUT/DELETE /api/test-types` | Test type definitions with thresholds |
| Observations | `GET/POST/DELETE /api/projects/{id}/observations` | Field notes |
| Sensors | `GET/POST/DELETE /api/projects/{id}/sensors` | Sensor management |
| Analytics | `GET /api/projects/{id}/analytics/*` | Out-of-spec, coverage, trends |
| Export | `GET /api/projects/{id}/export/*` | CSV and GeoJSON export |
| Attachments | `GET/POST/DELETE /api/attachments` | File attachments |

## Domain Models

- **Project** — name, client, status (Draft/Active/Closed), date range
- **ProjectBoundary** — GeoJSON polygon (geography column)
- **TestType** — name, unit, min/max thresholds for auto-pass/fail
- **TestResult** — value at a GPS point + timestamp, linked to project and test type
- **Observation** — geotagged field note with tags
- **Sensor / SensorReading** — IoT device data
- **Attachment** — blob storage reference
- **User / ProjectMembership** — users and per-project roles
- **AuditLog** — action tracking
- **IngestBatch** — idempotency for batch imports

## Running Locally

```bash
# Full stack via Docker (SQL Server + API + Angular frontend)
cd apps/geoops
docker compose up --build
# API: http://localhost:8080  |  UI: http://localhost:80

# Backend only
cd apps/geoops/backend
dotnet run

# Frontend only
cd apps/geoops/frontend
npm install && ng serve
# http://localhost:4200

# Tests (43 integration tests)
dotnet test apps/geoops/tests/GeoOps.Api.Tests.csproj
```

## Seed Data

On first startup the API seeds demo data including:
- 4 projects (I-35 OKC, SH-121 Dallas, US-75 Tulsa, I-10 Houston)
- 5 test types (IC compaction, mat temperature, nuclear density, dielectric, IRI)
- 85+ test results with realistic pass/fail distributions
- Field observations, sensors with week-long reading histories
- Audit log entries

## Integration with Ticketing

GeoOps can link to the [Ticketing service](../ticketing/README.md) via deep-links — no compile-time coupling. A "Create Support Ticket" button constructs a URL like:

```
https://ticketing.../tickets/new?sourceApp=geoops&sourceType=project&sourceId={projectId}
```

The ticketing URL is configurable via `TICKETING_APP_URL` environment variable.
