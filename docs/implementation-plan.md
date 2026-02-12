# TTEC Construction Tech Suite — Phased Implementation Plan

## Context

Benjamin is building a portfolio of web applications targeting The Transtec Group (Intelligent Construction / Terracon) to demonstrate he can build .NET + Angular + Azure solutions matching their stack and domain. The company builds pavement engineering software (Veta, ProVAL, etc.) and is working on a ticketing system. This plan reorganizes the existing GeoOps project into a monorepo that can host multiple apps, adds personality-driven auth, sets up Azure IaC on a tight budget, and lays out the full application suite as a TODO.

---

## Phase 1: Monorepo Reorganization

**Goal:** Restructure `ttec/` so GeoOps is one app among many, with room for shared code and infrastructure.

### New folder structure

```
ttec/
├── .github/workflows/
│   └── ci-geoops.yml              # Renamed + updated paths
├── apps/
│   └── geoops/
│       ├── backend/               # git mv from /backend
│       │   └── GeoOps.Api.csproj  # Renamed from Backend.Api.csproj
│       ├── frontend/              # git mv from /frontend
│       ├── tests/                 # git mv from /Backend.Api.Tests
│       │   └── GeoOps.Api.Tests.csproj  # Renamed
│       └── docker-compose.yml     # git mv from root, paths updated
├── libs/shared/                   # Future shared .NET class library
├── infra/                         # Bicep IaC (Phase 3)
│   ├── main.bicep
│   ├── modules/
│   └── environments/
├── docs/
├── ttec.sln                       # New solution file referencing all .csproj
├── global.json
├── AGENTS.md                      # Updated paths
├── spec.md
└── .gitignore                     # Updated to use glob patterns
```

### Steps

1. Create directory scaffold: `mkdir -p apps/geoops libs/shared infra/modules infra/environments`
2. `git mv` backend, frontend, Backend.Api.Tests, docker-compose.yml into `apps/geoops/`
3. Rename .csproj files: `Backend.Api.csproj` → `GeoOps.Api.csproj`, `Backend.Api.Tests.csproj` → `GeoOps.Api.Tests.csproj`
4. **Full namespace rename:** `Backend.Api` → `GeoOps.Api` across all .cs files
   - All `namespace Backend.Api.*` → `namespace GeoOps.Api.*`
   - All `using Backend.Api.*` → `using GeoOps.Api.*`
   - `<RootNamespace>` and `<AssemblyName>` in both .csproj files
   - Test project: namespace + project reference path
   - Dockerfile: `ENTRYPOINT ["dotnet", "GeoOps.Api.dll"]`
5. Update project reference in test csproj: `..\backend\GeoOps.Api.csproj`
6. Create root `ttec.sln` with `dotnet new sln` + `dotnet sln add` both projects
7. Update `docker-compose.yml` build context paths (already relative — just verify)
8. Rename `.github/workflows/ci.yml` → `ci-geoops.yml`, update all paths to `apps/geoops/...`, add `paths:` filter
9. Update `.gitignore` to use glob patterns (`**/bin/`, `**/node_modules/`, etc.)
10. Update `AGENTS.md` with new paths

### Files modified
- **Every .cs file** in backend/ and tests/ — namespace rename (~40+ files)
- `.github/workflows/ci.yml` → renamed + all path refs
- Both .csproj files → renamed + namespace + assembly name
- `backend/Dockerfile` → entrypoint DLL name
- `docker-compose.yml` → moved (paths stay relative)
- `.gitignore` — glob patterns
- `AGENTS.md` — path updates
- New: `ttec.sln`

### Also in Phase 1: Add SQL schema to GeoOps DbContext
In `apps/geoops/backend/Data/AppDbContext.cs`, add schema scoping:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema("geoops");
    // ... rest of existing config
}
```
This creates all GeoOps tables under the `geoops` schema (e.g., `geoops.Projects`). Future apps will use their own schemas. A new EF migration will be needed to move existing tables into the schema.

### Verification
```bash
dotnet build ttec.sln
dotnet test ttec.sln
cd apps/geoops && docker compose build
cd apps/geoops/frontend && npm ci && npx ng build
```

---

## Phase 2: Authentication

**Goal:** Replace 3 dev users with a single cheeky demo user. Keep JWT flow intact.

### Chosen approach: "Bold and Direct"
- **Username:** `Benjamin`
- **Password:** `isHired!`
- **Display name:** `Benjamin Calisch`
- Login page shows: *"Portfolio demo — Username: Benjamin / Password: isHired!"*

### Changes

**Backend — `apps/geoops/backend/Controllers/AuthController.cs`:**
- Replace `DevUsers` dictionary with single user:
  ```csharp
  ["Benjamin"] = ("isHired!", Guid.Parse("..."), "Benjamin Calisch")
  ```
- Change error message from "Invalid email or password" → "Invalid credentials"

**Backend — `apps/geoops/backend/Contracts/Auth/LoginRequest.cs`:**
- Rename `Email` → `Username` (small DTO, clean rename is worth it)

**Frontend — `apps/geoops/frontend/src/app/features/login/login.component.ts`:**
- Change label "Email" → "Username"
- Change `type="email"` → `type="text"`
- Change placeholder to "Enter username"
- Update `[(ngModel)]="email"` → `[(ngModel)]="username"`
- Update hint text: `Portfolio demo — Username: Benjamin / Password: isHired!`

**Frontend — `apps/geoops/frontend/src/app/core/services/auth.service.ts`:**
- Update `login()` method to send `username` instead of `email` in the request body

**Tests — `apps/geoops/tests/AuthControllerTests.cs`:**
- Update all test credentials to `("Benjamin", "isHired!")`
- Update assertions: `DisplayName` → `"Benjamin Calisch"`

### Verification
```bash
dotnet test apps/geoops/tests/GeoOps.Api.Tests.csproj
# Manual: run app, login with new credentials, verify JWT works
```

---

## Phase 3: Azure Infrastructure as Code

**Goal:** Deploy GeoOps to Azure for < $20/month using Bicep IaC.

### AWS → Azure translation (for Benjamin's reference)
| AWS | Azure Equivalent | Notes |
|-----|-----------------|-------|
| Lambda | Azure Functions | NOT needed here — .NET API already handles auth/JWT |
| API Gateway | Container Apps ingress | Built-in, no separate service needed |
| ECS/Fargate | Azure Container Apps | Consumption plan = scale to zero |
| RDS | Azure SQL Database | Basic tier = $4.90/month |
| ECR | Azure Container Registry | Basic tier = $5/month |
| CloudFormation | Bicep | Native Azure IaC, no state file needed |
| S3 | Azure Blob Storage | Minimal cost |

### Architecture
```
Azure Container Apps Environment (Consumption plan, scale-to-zero)
├── geoops-api        (.NET 8, port 8080, 0-1 replicas)
├── geoops-frontend   (Nginx, port 80, 0-1 replicas)
│
Azure SQL Server (logical, free)
├── ttec-dev database  (Basic tier, 5 DTU, ~$4.90/mo)
│   ├── [geoops] schema      ← GeoOps tables
│   ├── [ticketing] schema   ← Ticketing tables (future)
│   ├── [workshop] schema    ← Workshop tables (future)
│   └── ...                  ← one schema per app
│
Azure Container Registry (Basic, ~$5/mo)
Log Analytics Workspace (Free tier, 5 GB/mo)
```

**Estimated monthly cost: ~$10/month** (drops under $5 if you skip ACR and push images via GitHub Actions directly)

### Bicep structure
```
infra/
├── main.bicep                    # Orchestrator
├── modules/
│   ├── container-registry.bicep
│   ├── container-apps-env.bicep  # Env + Log Analytics
│   ├── container-app.bicep       # Reusable per-app module
│   └── sql-server.bicep          # Server + database(s)
├── environments/
│   ├── dev.bicepparam
│   └── prod.bicepparam
└── README.md
```

### Azure account setup walkthrough (no account yet)
1. **Sign up** at https://azure.microsoft.com/free — new accounts get **$200 free credit for 30 days** + 12 months of free-tier services. This alone covers months of this project.
2. **Install Azure CLI:** `brew install azure-cli`
3. **Login:** `az login` (opens browser for Microsoft account auth)
4. **Verify subscription:** `az account show` — confirm you have an active subscription
5. **Create resource group:** `az group create --name rg-ttec-dev --location eastus`
6. **Deploy infrastructure:** `az deployment group create --resource-group rg-ttec-dev --template-file infra/main.bicep --parameters @infra/environments/dev.bicepparam`
7. **Create GitHub Actions service principal** (for CI/CD):
   ```bash
   az ad sp create-for-rbac --name ttec-github-actions \
     --role Contributor \
     --scopes /subscriptions/<SUB_ID>/resourceGroups/rg-ttec-dev \
     --json-auth
   ```
   Add the JSON output as `AZURE_CREDENTIALS` secret in GitHub repo settings.

**Cost guardrails:** We'll set an Azure budget alert at $15/month so you get an email before hitting $20. The $200 free credit means you won't pay anything real for the first month regardless.

### CI/CD additions
- Add `deploy` job to `ci-geoops.yml` that triggers on push to `main`
- Builds Docker images, pushes to ACR, deploys to Container Apps

### Database strategy: Single database, schema-per-app
- **One Azure SQL Database** (Basic tier, $4.90/month) — shared by all apps
- Each app gets its own **SQL schema** for table isolation:
  - GeoOps → `geoops.Projects`, `geoops.TestResults`, etc.
  - Ticketing → `ticketing.Tickets`, `ticketing.Comments`, etc.
  - Workshop → `workshop.Sessions`, `workshop.Registrations`, etc.
- Each app's `DbContext` calls `modelBuilder.HasDefaultSchema("geoops")` — it only sees its own tables
- Apps are fully isolated at the code level; they don't know they share a database
- **To split later:** just change the connection string and re-run migrations against a new DB
- Cross-app references (if any) are by GUID, not foreign keys

### Verification
```bash
az deployment group create --what-if ...  # Preview before deploying
# After deploy: hit the Container Apps FQDN URLs, verify login works
```

---

## Phases 4-8: Application Suite TODO

Each app follows the same pattern: `apps/{name}/backend/` + `apps/{name}/frontend/` + `apps/{name}/tests/` + `apps/{name}/docker-compose.yml` + its own CI workflow + its own Azure SQL database + its own Container App pair.

### Phase 4: IC/PMTP Support Ticket & Resolution Tracker
- **Purpose:** Domain-specific helpdesk replacing Freshdesk for construction tech support
- **Key models:** Ticket, TicketComment, TimeEntry, Equipment, KnowledgeArticle
- **Standout features:** Geospatial ticket context (map pin per ticket), equipment/roller tracking (BOMAG, CAT, HAMM, etc.), billing engine ($200/ticket + $250/hr), SLA dashboards, knowledge base
- **Integration:** Link tickets to GeoOps projects/test results

### Phase 5: Workshop & Training Management Platform
- **Purpose:** Schedule workshops, manage registrations, track certifications
- **Key models:** Workshop, Session, Registration, Instructor, Certification, Curriculum
- **Standout features:** Calendar scheduling, capacity/waitlist management, certification with expiration tracking, curriculum builder

### Phase 6: Equipment Fleet & Calibration Manager
- **Purpose:** Track construction equipment locations, maintenance, calibration compliance
- **Key models:** Equipment, CalibrationRecord, MaintenanceLog, Assignment
- **Standout features:** Map-based fleet view, calibration due-date tracking, QR code generation/scanning, assignment to projects

### Phase 7: DOT Specification Compliance Checker
- **Purpose:** Database of 20+ state DOT specs with automated compliance evaluation
- **Key models:** State, Specification, ComplianceRule, ComplianceCheck
- **Standout features:** Rule engine evaluating test results against specs, compliance dashboard (green/yellow/red), PDF report generation

### Phase 8: Field Inspection & Punchlist App
- **Purpose:** Mobile-friendly field data capture with offline support
- **Key models:** Inspection, InspectionTemplate, PunchlistItem, FieldPhoto
- **Standout features:** PWA with service worker, offline IndexedDB storage, camera/GPS integration, batch sync

---

## Implementation Order

1. Phase 1 (Monorepo) — structural only, no functionality changes
2. Phase 2 (Auth) — small, targeted code changes
3. Phase 3 (Azure IaC) — independent of code, can verify incrementally
4. Phase 4 (Ticketing) — most relevant to what the team is building
5. Phases 5-8 — build in any order after ticketing
