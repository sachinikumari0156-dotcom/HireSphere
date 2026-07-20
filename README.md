# HireSphere

AI-assisted recruitment and talent management platform for Software Architecture coursework (SE205.3).

HireSphere provides four role portals â€” **Candidate**, **Recruiter**, **Hiring Manager**, and **Administrator** â€” backed by an ASP.NET Core API and SQL Server (LocalDB in verified development).

## Overview

- End-to-end hiring lifecycle: profile â†’ apply â†’ screen â†’ assess â†’ interview â†’ evaluate â†’ final decision
- JWT authentication and RBAC with organisation scoping
- Deterministic AI assistance with human-review notices (external AI **Not Configured** unless you configure it)
- Modular email / SMS / calendar / storage providers with truthful status
- Responsive, accessible UI foundation (Phase 9)

## Architecture summary

Modular monolith: React (Vite) SPA â†” ASP.NET Core Web API â†” EF Core / SQL Server + local secure file storage.
See `docs/report/HIRESPHERE_FINAL_REPORT.md` and `docs/architecture/`.

## Technology stack

| Layer | Technology |
|-------|------------|
| Frontend | React 19, Vite, React Router, Axios |
| Backend | ASP.NET Core, C#, EF Core |
| Database | SQL Server / LocalDB |
| Tests | xUnit, Vitest, Playwright, axe-core |

## Repository structure

```
Backend/HireSphere.API/     Web API + EF migrations
Backend/HireSphere.API.Tests/
Frontend/                   React SPA + Playwright e2e
docs/                       Architecture, evidence, testing, report
postman/                    Sanitized API collection
scripts/                    Verification helpers
```

## Prerequisites

- .NET SDK (project targets current ASP.NET Core TFM in repo)
- Node.js LTS + npm
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`)
- Git

## Secure local setup

1. Clone the repository.
2. Configure **ignored** local settings / .NET User Secrets / environment variables for:
   - `ConnectionStrings:DefaultConnection`
   - `Jwt:Key` (long random development key)
   - Optional seed flags (`Seed__Enabled`, admin email/password via env â€” **never commit**)
3. Do **not** commit `.env`, `appsettings.*.local.json`, or user-secret values.

Example LocalDB connection (local only):

`Server=(localdb)\MSSQLLocalDB;Database=HireSphereDev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True`

## Database

```powershell
cd Backend
dotnet tool restore
cd HireSphere.API
dotnet ef database update
```

## Backend

```powershell
cd Backend
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
cd HireSphere.API
dotnet run --urls http://127.0.0.1:5167
```

## Frontend

```powershell
cd Frontend
npm ci
npm run lint
npm run test:run
npm run build
npm run dev -- --host 127.0.0.1 --port 5173
```

Set `VITE_API_BASE_URL=http://127.0.0.1:5167/api` for local API.

## Tests

| Command | Purpose |
|---------|---------|
| `dotnet test` | Backend |
| `npm run test:run` | Frontend unit |
| `npm run e2e` | Playwright (API + Vite must be running) |
| `npm run test:a11y` / `test:responsive` / `test:visual` | UI quality |
| `npm run test:integration` / `test:security` | Role + authz journeys |

## Provider status (truthful)

| Provider | Status |
|----------|--------|
| Deterministic AI | Verified in coursework |
| External AI | Not Configured |
| Development SMTP / MailHog | Verified where exercised |
| Production SMTP | Not Configured |
| SMS mock | Verified |
| External SMS | Not Configured |
| Internal calendar + ICS | Verified |
| Google / Outlook Calendar | Not Configured |
| Local storage | Verified |
| Azure Blob cloud | Not Configured |
| Antivirus | Not Configured |

## Documentation

- Final report: `docs/report/HIRESPHERE_FINAL_REPORT.md`
- ADRs: `docs/architecture/adr/`
- API: `docs/api/`
- Evidence: `docs/evidence/EVIDENCE_MASTER_INDEX.md`
- Demo: `docs/demo/`
- Contribution: `docs/contribution/`
- Known limitations: `docs/release/FINAL_KNOWN_LIMITATIONS.md`

## Screenshots

Indexed under `docs/evidence/phase4-candidate` â€¦ `phase10-quality` and `docs/report/SCREENSHOT_INDEX.md`.

## Known limitations

See `docs/release/FINAL_KNOWN_LIMITATIONS.md`. Phase 10 is **PARTIALLY VERIFIED** (real usability participants pending). **Not production ready.**

## Contribution

Historical authorship (including Kalani Rashmika) is preserved in Git. See `docs/contribution/`.

## Academic use notice

This repository is a coursework artefact. It is not a warranty of production fitness. Do not upload real candidate PII or production secrets to demos.

