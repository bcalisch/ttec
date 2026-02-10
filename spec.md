l), crew/technician, source system

Click/hover details panel for any feature (point/line/polygon)Project spec: Intelligent Construction GeoOps
1) Purpose

Build a web-based geospatial application that allows construction/engineering teams to:

Manage construction QA/QC and instrumentation datasets (tests, inspections, sensors, attachments)

Visualize data on an interactive map with layers, filters, and time controls

Analyze data for out-of-spec conditions, coverage gaps, trends, and anomalies

This is directly applicable to construction services and pavement engineering workflows (e.g., for Terracon and The Transtec Group).

2) Scope
MVP scope (what you will build first)

A. Map-first project dashboard

Map with project boundary polygons

Layer toggles (tests, sensors, observations, attachments)

Filters: date range, test type, status (pass/warn/non-goals (for MVP)

Full BIM model viewer

CAD-grade editing

Complex workflow approvals (can come later)

Multi-tenant billing

3) Users and roles
User roles

Admin: manages org settings, users, data sources

Project Manager: creates projects, boundaries, spec thresholds, views analytics

Field Technician: enters tests/observations, uploads photos

Client (Read-only): views map and reports for assigned projects

Primary user journeys

PM creates a project → draws/imports site boundary → sets test thresholds.

Technician uploads tests (CSV or manual) → map updates → out-of-spec flags appear.

PM filters by date/test type → sees coverage gaps and trend charts.

PM exports GeoJSON for GIS tools or CSV for spreadsheets.

4) Data types (“intelligent construction data”)

MVP supports:

QA/QC tests: e.g., density/moisture, concrete breaks, asphalt density (as point features)

Observations: notes + photos (point features)

Sensors/instrumentation: readings over time (point features + time series)

Site geometry: project boundaries (polygons), optional segments (lines/polygons)

All geospatial coordinates stored in WGS84 (EPSG:4326).

5) Functional requirements
5.1 Project management

Create/edit project

Define site boundary polygon(s) (draw on map or import GeoJSON)

Assign users to project with role-based access

5.2 Layer visualization

Toggle layers on/off

Style rules:

Pass = green, warn = amber, fail = red (or equivalent)

Clustering for dense points (zoom-sensitive)

Bounding-box querying: only fetch features within map viewport

5.3 Filtering + time control

Filters apply to both map and table views

Time slider (MVP can be date range picker; phase 2 adds slider playback)

5.4 Ingestion

CSV upload wizard:

Map columns → fields (lat/lon, timestamp, value, test type, etc.)

Validate required fields

Show preview + errors before committing

REST ingestion:

Authenticated endpoint accepts batches

Idempotency key support (avoid duplicates)

5.5 Analytics

Out-of-spec rules engine (MVP)

Configurable thresholds per test type (min/max)

Flag results and compute “severity”

Coverage gaps (MVP)

Overlay grid within boundary; show cells with 0 results in chosen time window

Trends (MVP)

Aggregations: avg/min/max by day/week + rolling average

6) Data model (logical)
Core entities

Project: id, name, client, status, start/end dates

ProjectBoundary: projectId, polygon (geography)

TestType: name, unit, thresholds (min/max), metadata

TestResult: projectId, testTypeId, location (geography point), timestamp, value(s), status, source, technician

Sensor: projectId, type, location, metadata

SensorReading: sensorId, timestamp, value(s)

Observation: projectId, location, timestamp, note, tags

Attachment: linked entity type/id, blobUri, contentType, uploadedBy, uploadedAt

User / Role / ProjectMembership

AuditLog: who did what, when (important for credibility)

7) API spec (high-level)
Auth

JWT bearer tokens

RBAC enforced in API

Endpoints (MVP)

GET/POST /api/projects

GET/PUT /api/projects/{id}

POST /api/projects/{id}/boundaries (GeoJSON polygon)

GET /api/projects/{id}/features?bbox=…&from=…&to=…&types=tests,sensors,obs

POST /api/projects/{id}/test-results:import (CSV metadata + upload reference)

POST /api/projects/{id}/test-results (single)

POST /api/projects/{id}/ingest/test-results (batch JSON)

GET /api/projects/{id}/analytics/out-of-spec?...

GET /api/projects/{id}/analytics/coverage?...

GET /api/projects/{id}/export/csv?...

GET /api/projects/{id}/export/geojson?...

8) Non-functional requirements

Map interactions remain responsive with 50k+ points via clustering + viewport querying

Typical viewport query response: < 500ms (goal)

Auditability: key actions logged

Secure storage of secrets and tokens

Basic rate limiting on ingest endpoints

9) Stack requirements (Mac-friendly, Azure-ready)
9.1 Frontend

Framework: Angular (latest LTS you choose) + TypeScript

UI: Angular Material or Tailwind (keep it clean)

Mapping: Azure Maps Web SDK (preferred for Azure integration)

Alternative: Mapbox GL JS (if you prefer)

Charts: ECharts or Chart.js (simple, fast)

Build: Vite-based Angular build (if using new tooling) or standard Angular CLI

9.2 Backend

Runtime: .NET 8 (LTS)

API: ASP.NET Core Web API

ORM: EF Core + migrations

Validation: FluentValidation (recommended)

OpenAPI: Swagger / Swashbuckle

Background jobs (Phase 2 / optional in MVP): Azure Functions for scheduled analytics recompute or heavy imports

9.3 Database + storage

Primary DB: Azure SQL Database (SQL Server) with spatial types

Store points/polygons as geography

Use spatial indexes for bbox + within-boundary queries

Attachments: Azure Blob Storage

Optional (Phase 2):

Azure Cache for Redis (if needed)

Azure Data Explorer for high-volume sensor time-series

9.4 Hosting (Azure)

Pick one (both are credible):

Azure Container Apps (great for cost control + modern deployment)

Azure App Service (very common, simple)

9.5 Identity & security

Identity provider: Microsoft Entra ID (or Entra ID B2C if you want external client logins)

Secrets: Azure Key Vault (managed identity access)

Transport: HTTPS only; HSTS on frontend

9.6 Observability

Application Insights (requests, dependencies, exceptions)

Log Analytics workspace

Structured logging with correlation IDs

9.7 CI/CD and IaC

GitHub Actions:

build/test backend

build frontend

build/push container(s) (if Container Apps)

deploy

Infrastructure as Code:

Bicep (fits Azure-native) or Terraform

9.8 Local development requirements (Mac)

Docker Desktop (for local SQL Server container)

dotnet SDK + Node + Angular CLI

Local env runs via docker compose:

SQL Server

API

(optional) Azurite for blob emulation

10) Security and data privacy requirements

Role-based access per project (ProjectMembership)

No public project data without auth

PII avoidance: sample datasets only for public demo

Audit logs for imports and edits

Input validation on ingest endpoints (CSV + JSON)

11) Milestones (practical build plan)
Milestone 1: Skeleton (2–3 days)

Repo structure + CI pipeline

API + DB migrations

Angular app scaffold + auth wiring

Milestone 2: Map + boundaries (3–5 days)

Azure Maps in Angular

Create project + draw/import boundary

Feature endpoint with bbox filter

Milestone 3: Ingestion + viewing (4–7 days)

CSV upload + mapping + validation

Render test results on map + table view

Milestone 4: Analytics + export (4–7 days)

Out-of-spec rules + highlighting

Coverage grid overlay

CSV + GeoJSON export

Milestone 5: Azure deployment (1–2 days)

Deploy API + DB + frontend

App Insights dashboards

Demo script + sample dataset

12) Acceptance criteria (MVP is “done” when…)

You can open a live URL, log in, select a project, and see:

boundary polygon

at least two layers (tests + observations) displayed on the map

filtering by date/test type updates map + table

out-of-spec points highlighted

coverage grid shows missing test cells

exports produce valid CSV and GeoJSON

App Insights shows traces and exceptions for demo

B. Data ingestion (3 ways)

Manual entry (form)

CSV upload (drag/drop + mapping)

REST API ingestion (for automated feeds)

C. Core analytics

Out-of-spec highlighting (rule-based)

Coverage view (simple grid overlay showing where tests exist / missing)

Trend charts (time series per test type or sensor)

D. Export

Export filtered results to CSV

Export geospatial to GeoJSON

Phase 2 (high value add-ons)

Offline-capable field capture (PWA mode)

Advanced spatial analytics (buffers, stationing along alignments, segmented pavement sections)

Automated report package generation (PDF bundle)

High-volume streaming ingestion (Event Hub → Functions)
Explicit 
