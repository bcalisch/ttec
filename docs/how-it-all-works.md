# How GeoOps Works: A Complete Guide

This explains every piece of the system, assuming zero prior knowledge
of .NET, Angular, or maps.

---

## Table of Contents

1. [The Big Picture](#1-the-big-picture)
2. [What Problem Does This Solve?](#2-what-problem-does-this-solve)
3. [The Three Layers](#3-the-three-layers)
4. [How the Backend Works (.NET)](#4-how-the-backend-works-net)
5. [How the Database Works (SQL Server + EF Core)](#5-how-the-database-works)
6. [How the Frontend Works (Angular)](#6-how-the-frontend-works-angular)
7. [How the Map Works (Leaflet)](#7-how-the-map-works-leaflet)
8. [How Data Flows Through the System](#8-how-data-flows-through-the-system)
9. [How Docker Ties It Together](#9-how-docker-ties-it-together)
10. [File-by-File Reference](#10-file-by-file-reference)

---

## 1. The Big Picture

```
┌─────────────────────────────────────────────────────────────────┐
│                        YOUR BROWSER                             │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                   Angular Frontend                        │  │
│  │                                                           │  │
│  │  ┌─────────────┐  ┌──────────────┐  ┌────────────────┐   │  │
│  │  │ Project List │  │   Leaflet    │  │  Data Table /  │   │  │
│  │  │   (cards)    │  │   MAP        │  │  Analytics     │   │  │
│  │  │             │──>│  (markers,   │  │  (charts,      │   │  │
│  │  │  click      │  │   polygons,  │  │   tables)      │   │  │
│  │  │  project    │  │   clusters)  │  │                │   │  │
│  │  └─────────────┘  └──────┬───────┘  └────────────────┘   │  │
│  │                          │                                │  │
│  │              Sends HTTP requests (JSON)                    │  │
│  └──────────────────────────┼────────────────────────────────┘  │
│                             │                                   │
└─────────────────────────────┼───────────────────────────────────┘
                              │ HTTP (port 4200 → proxy → 7043)
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     .NET 8 Backend API                          │
│                                                                 │
│  ┌───────────────┐ ┌──────────────┐ ┌────────────────────────┐ │
│  │  Controllers  │ │  Validators  │ │  Services              │ │
│  │               │ │              │ │                        │ │
│  │ Projects      │ │ FluentValid. │ │ ICurrentUserService    │ │
│  │ TestTypes     │ │ checks all   │ │ (who is logged in)     │ │
│  │ TestResults   │ │ incoming     │ │                        │ │
│  │ Analytics     │ │ data         │ └────────────────────────┘ │
│  │ Export        │ │              │                             │
│  │ Seed          │ └──────────────┘                             │
│  └───────┬───────┘                                             │
│          │  C# ←→ SQL via Entity Framework Core                │
└──────────┼─────────────────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────────────────────┐
│                   SQL Server 2022 (Docker)                      │
│                                                                 │
│  ┌─────────┐ ┌──────────┐ ┌────────────┐ ┌──────────────────┐ │
│  │Projects │ │TestTypes │ │TestResults │ │ ProjectBoundaries│ │
│  │         │ │          │ │(lat/lon)   │ │ (polygons)       │ │
│  └─────────┘ └──────────┘ └────────────┘ └──────────────────┘ │
│  ┌──────────────┐ ┌─────────┐ ┌──────────┐                    │
│  │Observations  │ │Sensors  │ │Users     │  ... and more      │
│  └──────────────┘ └─────────┘ └──────────┘                    │
│                                                                 │
│  Uses "geography" column type for spatial data (lat/lon points  │
│  and polygons stored natively for fast spatial queries)         │
└─────────────────────────────────────────────────────────────────┘
```

**In plain English:** You open a browser. Angular shows you a map with
colored dots. Those dots come from the .NET API, which reads them from
SQL Server. The dots represent physical test results taken at real GPS
coordinates on a construction site.

---

## 2. What Problem Does This Solve?

Construction projects (highways, bridges, buildings) require QA/QC testing:

- **Density tests** — Is the soil compacted enough?
- **Moisture tests** — Is the water content in spec?
- **Concrete strength tests** — Will this bridge hold up?

Each test has a **location** (GPS coordinates) and a **result** (a number).
That number either passes or fails against thresholds.

Without GeoOps, these results live in spreadsheets. Nobody can see
*where* the failures are, or whether they've tested everywhere they
need to. GeoOps puts them on a map so you can instantly see:

- Red dots = failures (something is wrong here)
- Yellow dots = warnings (borderline)
- Green dots = passing
- Gray areas = gaps in testing coverage

---

## 3. The Three Layers

```
┌────────────────────────────────────────────┐
│           FRONTEND  (Angular)              │
│     "What the user sees and clicks"        │
│                                            │
│  Written in: TypeScript, HTML, SCSS        │
│  Runs in: the browser                      │
│  Lives in: /frontend/src/                  │
├────────────────────────────────────────────┤
│           BACKEND  (.NET 8 Web API)        │
│     "The brain — processes requests"       │
│                                            │
│  Written in: C#                            │
│  Runs on: your machine (or Docker)         │
│  Lives in: /backend/                       │
├────────────────────────────────────────────┤
│           DATABASE  (SQL Server)           │
│     "Where all data is permanently stored" │
│                                            │
│  Runs in: Docker container                 │
│  Data stored in: Docker volume             │
└────────────────────────────────────────────┘
```

They communicate exclusively via **HTTP/JSON**. The frontend never
talks directly to the database. Everything goes through the backend.

---

## 4. How the Backend Works (.NET)

### What is .NET?

.NET is Microsoft's framework for building server applications.
Think of it as the Node.js/Express of the Microsoft world, but with
C# instead of JavaScript.

### The entry point: `Program.cs`

This is like `index.js` in a Node app. It:

```
1. Creates a web server
2. Registers services (database connection, validation, CORS)
3. Starts listening for HTTP requests
```

Key things configured here:
- **CORS** — Allows the Angular app (port 4200) to call the API (port 7043).
  Browsers block cross-origin requests by default for security.
- **NetTopologySuite** — A library that teaches SQL Server and .NET
  how to work with geographic shapes (points, polygons)
- **FluentValidation** — Automatically checks incoming data
  (e.g., "project name can't be empty")
- **Swagger** — Auto-generates interactive API docs at /swagger

### Controllers = Endpoints

Each controller is a group of related API endpoints:

```
Controller                 What it does
─────────────────────────────────────────────────────────────
ProjectsController         Create/read/update projects
                           Create/read boundaries (polygons)
                           Query features within a map viewport

TestTypesController        CRUD for test types (Density, etc.)
                           Each has min/max thresholds

TestResultsController      Create individual test results
                           Batch ingest (upload many at once)

AnalyticsController        Out-of-spec analysis
                           Coverage gap detection
                           Trend aggregation (avg/min/max over time)

ExportController           Download CSV or GeoJSON files

SeedController             Fills database with demo data
                           (only works in Development mode)
```

### How a request flows through the backend

```
Browser sends: GET /api/projects/abc123/features?bbox=-105,39,-104,40

  1. ASP.NET routes it to ProjectsController.GetFeatures()
                    │
  2. FluentValidation checks query params
                    │
  3. Controller builds a LINQ query:
     "SELECT * FROM TestResults
      WHERE ProjectId = abc123
      AND Location is inside this bounding box"
                    │
  4. Entity Framework translates LINQ → SQL
                    │
  5. SQL Server executes the spatial query
     (uses geography indexes for speed)
                    │
  6. Results come back as C# objects
                    │
  7. Controller maps them to DTOs (Data Transfer Objects)
     (strips out internal fields, formats for the frontend)
                    │
  8. ASP.NET serializes to JSON and sends HTTP 200 response
```

### What is a "DTO" / "Contract"?

The backend has two kinds of data shapes:

- **Models** (`/backend/Models/`) — The real database entities.
  These have navigation properties, internal IDs, and geography objects.
- **Contracts** (`/backend/Contracts/`) — Simplified shapes for
  requests and responses. The frontend never sees raw database models.

Example:
```
Model (internal):
  TestResult.Location = Point(X=-104.93, Y=39.73, SRID=4326)

DTO (sent to frontend):
  { "longitude": -104.93, "latitude": 39.73 }
```

---

## 5. How the Database Works

### What is Entity Framework Core?

EF Core is an **ORM** (Object-Relational Mapper). It lets you write
C# code instead of raw SQL:

```csharp
// This C#:
var projects = await _dbContext.Projects
    .Where(p => p.Status == ProjectStatus.Active)
    .ToListAsync();

// Becomes this SQL automatically:
// SELECT * FROM Projects WHERE Status = 1
```

### The DbContext (`AppDbContext.cs`)

This file is the "map" between C# classes and database tables:

```
C# Class           →  SQL Table
──────────────────────────────────
Project             →  Projects
TestType            →  TestTypes
TestResult          →  TestResults
ProjectBoundary     →  ProjectBoundaries
Observation         →  Observations
Sensor              →  Sensors
SensorReading       →  SensorReadings
User                →  Users
ProjectMembership   →  ProjectMemberships
AuditLog            →  AuditLogs
IngestBatch         →  IngestBatches
Attachment          →  Attachments
```

### How spatial data works

SQL Server has a special `geography` column type that natively
understands latitude/longitude:

```
Table: TestResults
┌──────┬───────────┬────────────────────────┬───────┐
│  Id  │ ProjectId │ Location (geography)   │ Value │
├──────┼───────────┼────────────────────────┼───────┤
│ abc  │ proj1     │ POINT(-104.93 39.73)   │ 98.5  │
│ def  │ proj1     │ POINT(-104.91 39.74)   │ 87.2  │
└──────┴───────────┴────────────────────────┴───────┘

Table: ProjectBoundaries
┌──────┬───────────┬──────────────────────────────────────┐
│  Id  │ ProjectId │ Polygon (geography)                  │
├──────┼───────────┼──────────────────────────────────────┤
│ ghi  │ proj1     │ POLYGON((-104.95 39.72, -104.90 ... │
└──────┴───────────┴──────────────────────────────────────┘
```

The database can answer spatial questions efficiently:
- "Which test results are inside this polygon?"
- "Which test results are visible in this map viewport?"

### What are Migrations?

Migrations are version-controlled database changes. Instead of
manually creating tables, you:

```bash
dotnet ef migrations add InitialCreate  # Generate SQL from your C# models
dotnet ef database update               # Apply it to the actual database
```

The migration files in `/backend/Migrations/` are auto-generated
C# that creates all the tables, columns, indexes, and constraints.

---

## 6. How the Frontend Works (Angular)

### What is Angular?

Angular is a frontend framework (like React or Vue). You build
**components** — each is a self-contained piece of UI with its own
HTML template, CSS styles, and TypeScript logic.

### How Angular is structured

```
frontend/src/app/
├── app.component.ts          ← The outer shell (sidebar + router)
├── app.routes.ts             ← URL → Component mapping
├── app.config.ts             ← Bootstrap config (HTTP, routing)
│
├── core/                     ← Shared code used everywhere
│   ├── models/index.ts       ← TypeScript interfaces (data shapes)
│   └── services/             ← HTTP client wrappers
│       ├── project.service.ts
│       ├── feature.service.ts
│       ├── test-type.service.ts
│       ├── test-result.service.ts
│       ├── analytics.service.ts
│       └── export.service.ts
│
└── features/                 ← Page-level components
    ├── project-list/         ← Landing page (list of projects)
    ├── project-dashboard/    ← Main view for one project
    ├── map/                  ← Leaflet map component
    ├── filter-panel/         ← Date/type/status filters
    ├── data-table/           ← Tabular test result view
    ├── feature-detail/       ← Slide-out panel for one test result
    ├── csv-upload/           ← CSV import wizard (4 steps)
    └── analytics/            ← Charts and out-of-spec analysis
```

### Routing: How URLs map to pages

```
URL                        Component Shown
─────────────────────────────────────────────
/                          ProjectListComponent
/projects/abc-123          ProjectDashboardComponent
```

When you click a project card, Angular navigates to
`/projects/<that-project-id>`, which loads the dashboard.

### Services: How the frontend talks to the backend

Each service wraps HTTP calls:

```typescript
// project.service.ts
getAll(): Observable<Project[]> {
  return this.http.get<Project[]>('/api/projects');
}
```

The URL `/api/projects` doesn't go to port 4200 (Angular's dev server).
The **proxy** redirects it:

```
Browser → localhost:4200/api/projects
                ↓ (proxy.conf.json)
         localhost:7043/api/projects  (the .NET backend)
```

This avoids CORS issues during development and mimics how production
will work (nginx does the same proxying).

### What is an Observable?

Angular uses RxJS Observables instead of Promises. Think of them as
"a promise that can emit multiple values over time." For HTTP calls,
they behave like promises — you `.subscribe()` instead of `.then()`:

```typescript
// Promise style (not used here):
const projects = await fetch('/api/projects').then(r => r.json());

// Observable style (Angular):
this.projectService.getAll().subscribe(projects => {
  this.projects = projects;
});
```

---

## 7. How the Map Works (Leaflet)

### What is Leaflet?

Leaflet is an open-source JavaScript library for interactive maps.
It's NOT a map data provider — it just renders tiles and layers.

```
Leaflet (the library)    = The rendering engine
OpenStreetMap (the tiles) = The actual map images
```

### The tile layer

When you see a map, you're actually seeing a grid of 256x256 pixel images
loaded from OpenStreetMap's servers:

```
┌─────┬─────┬─────┐
│tile │tile │tile │  Each tile is a PNG image
│0,0  │1,0  │2,0  │  from tile.openstreetmap.org
├─────┼─────┼─────┤
│tile │tile │tile │  As you zoom/pan, new tiles load
│0,1  │1,1  │2,1  │
├─────┼─────┼─────┤
│tile │tile │tile │  This is free — no API key needed
│0,2  │1,2  │2,2  │
└─────┴─────┴─────┘
```

### Layers — what's drawn ON the map

```
Layer Stack (bottom to top):
────────────────────────────────────────────
5. MarkerClusterGroup  ← Test result dots (clusters when dense)
4. observationLayer    ← Blue dots for field observations
3. sensorLayer         ← Purple dots for IoT sensors
2. coverageLayer       ← Semi-transparent grid overlay
1. boundaryLayer       ← Blue polygon (project boundary)
0. Tile Layer          ← The actual map imagery (streets, terrain)
────────────────────────────────────────────
```

### Color-coded markers

Each test result is a circle marker. The color tells you the status:

```
  ●  Green  (#22c55e) = Pass    (within threshold)
  ●  Amber  (#f59e0b) = Warning (borderline)
  ●  Red    (#ef4444) = Fail    (outside threshold)
  ●  Blue   (#3b82f6) = Observation (field note)
  ●  Purple (#a855f7) = Sensor
```

### MarkerClusterGroup

When you have 100+ points close together, showing them all makes the
map unreadable. MarkerCluster automatically groups nearby markers:

```
Zoomed out:                 Zoomed in:
┌──────────────────┐       ┌──────────────────┐
│                  │       │   ●  ●           │
│      (27)        │       │  ●  ●  ●         │
│                  │  ───> │    ●    ●         │
│                  │       │  ●   ●            │
│         (14)     │       │    ●  ●  ●  ●    │
└──────────────────┘       └──────────────────┘

The number in (27) means "there are 27 points clustered here."
Click it or zoom in to see individual points.
```

### Bounding box (bbox) queries

When you pan/zoom the map, the frontend tells the backend
"only give me data visible in this viewport":

```
Map viewport:
┌─────────────────────────────┐
│ NW corner                   │ NE corner
│ (-105.0, 40.0)              │ (-104.0, 40.0)
│                             │
│    Only load points         │
│    inside this box!         │
│                             │
│ SW corner                   │ SE corner
│ (-105.0, 39.5)              │ (-104.0, 39.5)
└─────────────────────────────┘

bbox string sent to API: "-105.0,39.5,-104.0,40.0"
                          minLon,minLat,maxLon,maxLat
```

This keeps the app fast — you don't load the entire database every time.

### Boundary polygon

A project boundary is a polygon drawn on the map that defines
"the area of this construction project":

```
         *────────────*
        /              \
       /    Project     \     The polygon is stored as GeoJSON:
      /    Area          \    { "type": "Polygon",
     /                    \     "coordinates": [
    *──────────────────────*      [[-104.95, 39.72],
                                   [-104.90, 39.72],
                                   [-104.90, 39.76],
                                   [-104.95, 39.76],
                                   [-104.95, 39.72]]
                                ] }
```

### What is GeoJSON?

GeoJSON is the standard format for geographic data on the web.
It's just JSON with a specific structure:

```json
{
  "type": "Polygon",
  "coordinates": [
    [
      [-104.95, 39.72],
      [-104.90, 39.72],
      [-104.90, 39.76],
      [-104.95, 39.76],
      [-104.95, 39.72]
    ]
  ]
}
```

Note: GeoJSON uses `[longitude, latitude]` (opposite of how most
people say "lat/lng"). This trips up everyone at first.

---

## 8. How Data Flows Through the System

### Flow 1: Viewing a project on the map

```
User clicks "Highway 101 Expansion" card
         │
         ▼
Angular Router navigates to /projects/abc-123
         │
         ▼
ProjectDashboardComponent.ngOnInit()
         │
         ├──► projectService.getById("abc-123")
         │         │
         │         ▼
         │    GET /api/projects/abc-123
         │         │
         │         ▼
         │    .NET reads from Projects table
         │         │
         │         ▼
         │    Returns: { name: "Highway 101", client: "CO DOT", ... }
         │
         ├──► projectService.getBoundaries("abc-123")
         │         │
         │         ▼
         │    GET /api/projects/abc-123/boundaries
         │         │
         │         ▼
         │    Returns: [{ geoJson: '{"type":"Polygon",...}' }]
         │
         └──► featureService.getFeatures("abc-123", { bbox: "..." })
                   │
                   ▼
              GET /api/projects/abc-123/features?bbox=-105,39,-104,40
                   │
                   ▼
              .NET runs spatial query:
              "WHERE Location.STIntersects(@bbox) = 1"
                   │
                   ▼
              Returns: {
                tests: [{ lat, lon, value, status, ... }, ...],
                observations: [...],
                sensors: [...]
              }
                   │
                   ▼
         Dashboard passes data to MapComponent
                   │
                   ▼
         Map renders: boundary polygon + colored markers
```

### Flow 2: Uploading a CSV file

```
User clicks "Upload CSV" → drags file onto drop zone
         │
         ▼
CsvUploadComponent reads file with FileReader API
         │
         ▼
PapaParse library parses CSV into rows + headers
         │
         ▼
Step 2: User maps CSV columns to expected fields
        CSV Column "lat" → Latitude
        CSV Column "lng" → Longitude
        CSV Column "result" → Value
         │
         ▼
Step 3: Preview table shows first 10 mapped rows
         │
         ▼
Step 4: User clicks "Upload"
         │
         ▼
Generate idempotency key = SHA-256(filename + file content)
         │
         ▼
POST /api/projects/abc-123/ingest/test-results
{
  "idempotencyKey": "report.csv-a1b2c3...",
  "items": [
    { "testTypeId": "...", "value": 98.5, "lat": 39.73, ... },
    ...
  ]
}
         │
         ▼
Backend checks: has this idempotencyKey been seen before?
  YES → return { duplicate: true } (prevent double-upload)
  NO  → insert all rows into TestResults table
         │
         ▼
Map refreshes with new points
```

The **idempotency key** prevents accidental double-uploads. If you
upload the same file twice, the second upload is a no-op.

### Flow 3: Analytics — finding problems

```
User clicks "Analytics" tab
         │
         ▼
AnalyticsComponent loads three things in parallel:
         │
         ├──► GET /analytics/out-of-spec
         │    Backend: for each TestResult, compare value vs thresholds
         │    If value < minThreshold or value > maxThreshold → out of spec
         │    Calculate severity: how far outside the threshold (%)
         │    Return sorted by severity (worst first)
         │         │
         │         ▼
         │    Render: table with red-highlighted severe rows
         │
         ├──► GET /analytics/coverage
         │    Backend: divide project boundary into 10x10 grid cells
         │    Count test results in each cell
         │    Cells with 0 results = coverage gaps
         │         │
         │         ▼
         │    Render: semi-transparent grid overlay on map
         │    Green cells = tested, Red cells = gap
         │
         └──► GET /analytics/trends?interval=day
              Backend: group results by day + test type
              Calculate avg/min/max per group
                   │
                   ▼
              Render: Chart.js line chart with lines for each test type
```

---

## 9. How Docker Ties It Together

### What is Docker?

Docker runs applications in isolated "containers" — like lightweight
virtual machines. This ensures everyone runs the same software version
regardless of their OS.

### Development setup (what you're running now)

```
┌────────────────────────────────────────────────────────┐
│                    Your Mac                             │
│                                                        │
│  ┌──────────────────┐    ┌───────────────────────────┐ │
│  │ Terminal 1        │    │ Docker Desktop             │ │
│  │ cd backend        │    │ ┌─────────────────────┐   │ │
│  │ dotnet run        │    │ │ SQL Server 2022      │   │ │
│  │ (port 7043)       │◄───┤ │ (port 1433)          │   │ │
│  └──────────────────┘    │ │ Container: geoops-sql │   │ │
│                          │ └─────────────────────┘   │ │
│  ┌──────────────────┐    └───────────────────────────┘ │
│  │ Terminal 2        │                                  │
│  │ cd frontend       │                                  │
│  │ ng serve          │                                  │
│  │ (port 4200)       │◄── you open this in browser     │
│  └──────────────────┘                                  │
└────────────────────────────────────────────────────────┘
```

### Production setup (docker-compose up)

```
┌──────────────────────────────────────────────────────────┐
│                    Docker Network                         │
│                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────┐  │
│  │   frontend   │  │     api      │  │   sqlserver    │  │
│  │   (nginx)    │  │   (.NET)     │  │  (SQL Server)  │  │
│  │              │  │              │  │               │  │
│  │  Port 80     │  │  Port 8080   │  │  Port 1433    │  │
│  │              │  │              │  │               │  │
│  │  /api/* ─────┼─>│              ├─>│               │  │
│  │  /*    → SPA │  │              │  │               │  │
│  └──────────────┘  └──────────────┘  └───────────────┘  │
│        │                   │               │             │
│    depends_on          depends_on      healthcheck       │
│      api               sqlserver     (waits until ready) │
└──────────────────────────────────────────────────────────┘
      │
      ▼
  User opens http://localhost
```

The nginx server in the frontend container does double duty:
1. Serves the Angular static files (HTML/JS/CSS)
2. Proxies `/api/*` requests to the backend container

---

## 10. File-by-File Reference

### Backend

```
backend/
├── Program.cs                          The app's entry point. Configures
│                                       all services, middleware, and starts
│                                       the web server.
│
├── Data/
│   └── AppDbContext.cs                 The database "map" — defines which
│                                       C# classes map to which SQL tables,
│                                       and configures column types, indexes,
│                                       and constraints.
│
├── Models/                             One file per database table:
│   ├── Project.cs                      Construction project (name, client, dates)
│   ├── ProjectBoundary.cs              Geographic polygon defining project area
│   ├── TestType.cs                     Type of test (Density, Moisture, etc.)
│   ├── TestResult.cs                   One test at one location at one time
│   ├── Observation.cs                  Field note pinned to a location
│   ├── Sensor.cs                       IoT sensor at a fixed location
│   ├── SensorReading.cs               One reading from one sensor
│   ├── User.cs                         A person in the system
│   ├── ProjectMembership.cs            Links users to projects with roles
│   ├── Attachment.cs                   File attachment (photo, PDF)
│   ├── AuditLog.cs                     Who did what, when
│   ├── IngestBatch.cs                  Tracks CSV upload batches
│   └── Enums.cs                        Shared enums (ProjectStatus, TestStatus)
│
├── Controllers/                        One file per group of API endpoints:
│   ├── ProjectsController.cs           /api/projects + boundaries + features
│   ├── TestTypesController.cs          /api/test-types CRUD
│   ├── TestResultsController.cs        /api/projects/{id}/test-results + ingest
│   ├── AnalyticsController.cs          /api/projects/{id}/analytics/*
│   ├── ExportController.cs             /api/projects/{id}/export/csv|geojson
│   └── SeedController.cs              /api/seed (dev-only demo data)
│
├── Contracts/                          Request/response shapes (DTOs):
│   ├── Projects/
│   │   ├── CreateProjectRequest.cs
│   │   ├── UpdateProjectRequest.cs
│   │   ├── CreateProjectBoundaryRequest.cs
│   │   └── ProjectBoundaryResponse.cs  Returns GeoJSON string, not raw geometry
│   ├── TestTypes/
│   │   ├── CreateTestTypeRequest.cs
│   │   └── UpdateTestTypeRequest.cs
│   ├── TestResults/
│   │   ├── CreateTestResultRequest.cs
│   │   ├── BatchIngestTestResultsRequest.cs
│   │   └── CsvImportRequest.cs
│   └── Features/
│       └── FeaturesResponse.cs         The combined map data response
│
├── Validators/                         Input validation (auto-runs on requests):
│   ├── Projects/
│   ├── TestTypes/
│   └── TestResults/
│
├── Services/
│   └── ICurrentUserService.cs          "Who am I?" — hardcoded dev user for now
│
├── Migrations/                         Auto-generated database schema changes
│   ├── InitialCreate.cs
│   └── AppDbContextModelSnapshot.cs
│
├── Dockerfile                          How to build this into a Docker image
├── Backend.Api.csproj                  NuGet package references (like package.json)
└── appsettings.json                    Database connection string and config
```

### Frontend

```
frontend/src/app/
├── app.component.ts                    The shell: sidebar + main content area
├── app.routes.ts                       / → ProjectList, /projects/:id → Dashboard
├── app.config.ts                       Bootstraps Angular (routing, HTTP)
│
├── core/
│   ├── models/index.ts                 TypeScript interfaces matching backend DTOs
│   └── services/
│       ├── project.service.ts          GET/POST /api/projects, boundaries
│       ├── feature.service.ts          GET /api/projects/{id}/features with filters
│       ├── test-type.service.ts        GET/POST /api/test-types
│       ├── test-result.service.ts      POST test results, batch ingest
│       ├── analytics.service.ts        GET out-of-spec, coverage, trends
│       └── export.service.ts           Trigger CSV/GeoJSON file downloads
│
└── features/
    ├── project-list/                   Landing page — project cards + create form
    ├── project-dashboard/              Main view — assembles all sub-components
    ├── map/                            Leaflet map (markers, boundary, draw tools)
    ├── filter-panel/                   Date range, test type, status, layer toggles
    ├── data-table/                     Sortable table of test results
    ├── feature-detail/                 Slide-out panel showing one test result
    ├── csv-upload/                     4-step CSV import wizard
    └── analytics/                      Out-of-spec table + trend chart + exports
```

### Infrastructure

```
docker-compose.yml                      Defines all 3 services (sql, api, frontend)
.github/workflows/ci.yml               GitHub Actions: build on push/PR
docs/azure-deployment.md                Guide for deploying to Azure cloud
frontend/nginx.conf                     Nginx config for proxying /api/ in production
frontend/Dockerfile                     Build Angular → serve with nginx
backend/Dockerfile                      Build .NET → run in minimal container
```

---

## Key Concepts Glossary

| Term | What it means |
|------|---------------|
| **API** | A set of URLs that accept/return JSON data. The backend IS an API. |
| **Controller** | A C# class that handles HTTP requests for a group of URLs |
| **DTO** | Data Transfer Object — a simplified data shape for API responses |
| **EF Core** | Entity Framework Core — translates C# code into SQL queries |
| **Migration** | A versioned database schema change (like git for your database) |
| **Observable** | An RxJS stream that emits values (Angular's way of handling async) |
| **Component** | An Angular building block — has template (HTML), style, and logic |
| **Service** | An Angular class that handles business logic or HTTP calls |
| **GeoJSON** | A JSON format for geographic shapes (points, polygons, lines) |
| **bbox** | Bounding box — a rectangle defined by min/max lat/lon coordinates |
| **SRID 4326** | The coordinate system for GPS (WGS84). Means "latitude/longitude." |
| **Leaflet** | Open-source JS library for rendering interactive maps |
| **Tile layer** | Map images loaded from a server (like Google Maps tiles, but free) |
| **MarkerCluster** | Groups nearby markers into numbered circles when zoomed out |
| **CORS** | Cross-Origin Resource Sharing — browser security that blocks API calls from different domains. We allow localhost:4200 → localhost:7043 |
| **Proxy** | A middleman that forwards requests. Our dev proxy sends /api/* to the backend |
| **Idempotency key** | A unique ID for an operation so running it twice has no extra effect |
| **NetTopologySuite** | .NET library for working with geographic shapes (the "PostGIS for .NET") |
| **Swagger** | Auto-generated interactive API documentation at /swagger |
