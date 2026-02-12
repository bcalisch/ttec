# Repository Guidelines

## Project Structure & Module Organization

This repository is a monorepo for the TTEC Construction Technology Suite.

- `apps/geoops/backend/`: ASP.NET Core Web API (.NET 8), EF Core, SQL Server with spatial types.
- `apps/geoops/frontend/`: Angular 19 SPA with Leaflet mapping, Chart.js, Tailwind CSS.
- `apps/geoops/tests/`: xUnit integration tests for the GeoOps API.
- `apps/geoops/docker-compose.yml`: Local dev stack (SQL Server + API + Frontend).
- `libs/shared/`: Shared .NET class libraries (future).
- `infra/`: Bicep IaC for Azure resources.
- `docs/`: Architecture notes, deployment guides, and implementation plan.
- `spec.md`: Product specification and requirements.
- `ttec.sln`: Root solution file referencing all .NET projects.

## Build, Test, and Development Commands

- `dotnet build ttec.sln` from repo root to build all .NET projects.
- `dotnet test ttec.sln` from repo root to run all tests.
- `cd apps/geoops/frontend && npm ci && npx ng serve` for local Angular dev server.
- `cd apps/geoops && docker compose up -d` for full local stack.

## Coding Style & Naming Conventions

- Indentation: 2 spaces for TypeScript/HTML/CSS, 4 spaces for C#.
- Naming: `PascalCase` for C# types and public members, `camelCase` for local variables; `kebab-case` for Angular selectors; `PascalCase` for Angular components and services.
- Namespace convention: `GeoOps.Api.*` for GeoOps backend code.

## Testing Guidelines

- Backend: xUnit, test project at `apps/geoops/tests/`.
- Frontend: Karma/Jasmine, tests alongside source as `*.spec.ts`.
- Naming: `*Tests.cs` for C#, `*.spec.ts` for Angular.

## Commit & Pull Request Guidelines

Use Conventional Commits: `feat(geoops): add project endpoints`, `fix(geoops-ui): correct map filter`.

## Security & Configuration Tips

- Store secrets in environment variables or user-secrets for local dev.
- Never commit keys or connection strings.
- Use `.env` files only for local placeholders; add them to `.gitignore`.

## Agent-Specific Instructions

- Keep changes minimal and scoped to the task.
- If instructions conflict with repository requirements, call it out explicitly.
