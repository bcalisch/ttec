# Ticketing — IC/PMTP Support Ticket & Resolution Tracker

Standalone service for tracking support tickets, equipment issues, calibration schedules, and field support requests for IC and PMTP operations. Includes time tracking, billing, SLA enforcement, and a knowledge base.

[Back to TTEC Platform](../../README.md)

## What It Does

- **Ticket management** — create, assign, track, and resolve support tickets across categories (software, hardware, calibration, training, field support)
- **SLA tracking** — automatic deadlines based on priority (Critical=4h, High=8h, Medium=24h, Low=72h) with overdue detection
- **Time & billing** — log technician hours at $250/hr, $200 base charge per ticket, per-ticket and summary billing reports
- **Equipment registry** — track rollers, pavers, sensors with unique serial numbers, link equipment to tickets
- **Knowledge base** — searchable articles with tags and publish workflow
- **External references** — tickets can optionally link to GeoOps projects or any external entity via opaque `SourceApp` / `SourceEntityType` / `SourceEntityId` fields

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Angular 19, Tailwind CSS, Chart.js, Leaflet |
| Backend | .NET 8 Web API, Entity Framework Core, NetTopologySuite |
| Database | SQL Server (`ticketing` schema), geography columns for location |
| Auth | JWT Bearer (shared `ttec-dev` issuer across platform) |
| Validation | FluentValidation |
| Tests | xUnit, FluentAssertions, WebApplicationFactory (46 tests) |
| Infra | Docker, Azure Container Apps |

## API Endpoints

| Area | Routes | Description |
|------|--------|-------------|
| Auth | `POST /api/auth/login`, `GET /api/auth/me` | JWT login, current user |
| Health | `GET /api/health` | Health check (anonymous) |
| Tickets | `GET/POST/PUT/DELETE /api/tickets` | Ticket CRUD with status, priority, SLA |
| | `GET /api/tickets?sourceApp=X&sourceEntityId=Y` | Filter by external reference |
| | `GET /api/tickets/sla-summary` | Overdue / at-risk / on-track counts |
| Comments | `GET/POST/DELETE /api/tickets/{id}/comments` | Ticket comments (HTML-encoded for XSS safety) |
| Time Entries | `GET/POST /api/tickets/{id}/time-entries` | Technician time logging |
| Equipment | `GET/POST/PUT/DELETE /api/equipment` | Equipment registry with unique serial numbers |
| Knowledge | `GET/POST/PUT /api/knowledge-articles` | Articles with tag filtering, publish workflow |
| | `GET /api/knowledge-articles?tag=calibration` | Filter by tag |
| Billing | `GET /api/billing/tickets/{id}` | Per-ticket billing ($200 base + hourly) |
| | `GET /api/billing/summary` | Aggregate billing across all tickets |

## Domain Models

- **Ticket** — title, description, status, priority, category, assignee, GPS location, equipment link, SLA deadline, external source reference
- **TicketComment** — author (auto-set from JWT), body (XSS-safe), internal flag
- **TimeEntry** — technician (auto-set), hours (max 24), rate ($250/hr server-controlled)
- **Equipment** — name, serial number (unique), type, manufacturer, model
- **KnowledgeArticle** — title, content, tags, publish status

### Enums

| Enum | Values |
|------|--------|
| TicketStatus | Open, InProgress, AwaitingCustomer, AwaitingParts, Resolved, Closed |
| TicketPriority | Low, Medium, High, Critical |
| TicketCategory | Software, Hardware, Calibration, Training, FieldSupport, Other |
| EquipmentType | Roller, Paver, MillingMachine, Sensor, Software, Other |
| EquipmentManufacturer | BOMAG, CAT, HAMM, Volvo, Dynapac, Other |

## Frontend Pages

| Page | Description |
|------|-------------|
| `/login` | Shared auth login (same credentials as GeoOps) |
| `/tickets` | Ticket list with status/priority filters, overdue highlighting |
| `/tickets/new` | Create ticket form, accepts `?sourceApp=&sourceType=&sourceId=` query params |
| `/tickets/:id` | Ticket detail with comments, time entries, billing sidebar, location map |
| `/equipment` | Equipment registry with CRUD, type/manufacturer badges |
| `/knowledge-base` | Article list with tag filtering, expand/collapse, publish workflow |
| `/dashboard` | SLA dashboard with Chart.js doughnut/bar charts, billing summary |

## Running Locally

```bash
# Full stack via Docker (SQL Server + API + Frontend)
cd apps/ticketing
docker compose up --build
# Frontend: http://localhost:81
# API: http://localhost:8081

# Backend only
cd apps/ticketing/backend
dotnet run
# https://localhost:7044

# Frontend only (dev server with proxy)
cd apps/ticketing/frontend
npm start
# http://localhost:4200

# Tests (46 integration tests)
dotnet test apps/ticketing/tests/Ticketing.Api.Tests.csproj
```

## Seed Data

On first startup the API seeds:
- **5 equipment items** — BOMAG BW 226, CAT CS56B, HAMM HD+ 120i, Pave-IR scanner, Troxler 3440
- **7 tickets** — varied statuses (Open, InProgress, AwaitingParts, AwaitingCustomer, Resolved, Closed), priorities, and categories
- **5 comments** — threaded conversation on active tickets
- **6 time entries** — billable hours across multiple tickets
- **5 knowledge articles** — troubleshooting guides, safety procedures, integration docs (1 draft)

## Integration with GeoOps

The ticketing service accepts optional external references on tickets. [GeoOps](../geoops/README.md) links to ticketing via URL query params — no compile-time coupling:

```
# GeoOps "Create Ticket" button links to:
/tickets/new?sourceApp=geoops&sourceType=project&sourceId={projectId}

# GeoOps "View Tickets" button links to:
/tickets?sourceApp=geoops&sourceEntityId={projectId}
```

The ticketing app stores `SourceApp`, `SourceEntityType`, and `SourceEntityId` as opaque strings — it has zero knowledge of what GeoOps is. Tickets work perfectly without any source reference.

## Security Notes

- Comment bodies are HTML-encoded to prevent stored XSS
- `TimeEntry.HourlyRate` and `TimeEntry.Technician` are server-controlled (not from request body)
- `TicketComment.Author` is auto-set from the JWT — not user-supplied
- Longitude/latitude validated to [-180,180] and [-90,90]
- Description capped at 4000 chars, title at 200
- Equipment serial numbers enforced unique at DB level
- Source* fields are opaque — no validation against external systems
