# Repository Guidelines

## Project Structure & Module Organization

This repository is currently a greenfield workspace. Planned layout:

- `backend/`: ASP.NET Core Web API (.NET 8), EF Core, and migrations.
- `frontend/`: Angular app with Azure Maps integration.
- `infra/`: IaC (Bicep or Terraform) for Azure resources.
- `docs/`: Architecture notes and runbooks.
- `spec.md`: Product specification and requirements.

## Build, Test, and Development Commands

Commands will be added as the codebase is scaffolded. Expected patterns:

- `dotnet build` in `backend/` for API build.
- `dotnet test` in `backend/` for API/unit tests.
- `npm install` then `npm run dev` or `npm start` in `frontend/` for local UI.
- `npm test` in `frontend/` for UI tests.

## Coding Style & Naming Conventions

- Indentation: 2 spaces for TypeScript/HTML/CSS, 4 spaces for C#.
- Naming: `PascalCase` for C# types and public members, `camelCase` for local variables; `kebab-case` for Angular selectors; `PascalCase` for Angular components and services.
- Formatting: use `dotnet format` for backend; use `npm run format` (e.g., Prettier) for frontend once configured.

## Testing Guidelines

- Backend: xUnit (planned), test projects under `backend/tests/` named `*.Tests`.
- Frontend: Angular TestBed/Jest (planned), tests alongside source or under `frontend/src/**/__tests__/`.
- Naming: `*Tests.cs` for C#, `*.spec.ts` for Angular.

## Commit & Pull Request Guidelines

No commit history exists yet. Until conventions are established, use Conventional Commits:

- Examples: `feat(api): add project endpoints`, `fix(ui): correct map filter`.

PRs should include:

- Summary of changes and test evidence.
- Linked issue or spec section (e.g., `spec.md` section number).
- Screenshots for UI changes.

## Security & Configuration Tips

- Store secrets in environment variables or user-secrets for local dev.
- Never commit keys or connection strings.
- Use `.env` files only for local placeholders; add them to `.gitignore`.

## Agent-Specific Instructions

- Keep changes minimal and scoped to the task.
- Prefer `rg` for search and `apply_patch` for single-file edits.
- If instructions conflict with repository requirements, call it out explicitly.
