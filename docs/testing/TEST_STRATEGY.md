# HireSphere test strategy (Phase 10)

**Date:** 2026-07-21  
**Levels:** unit → integration (WebApplicationFactory + optional LocalDB) → Playwright E2E → UAT/usability  

## Principles

- Prefer deterministic fixtures and unique emails per run.
- Do not treat NotConfigured external providers as PASS.
- SQLite in-memory factory covers API behaviour; LocalDB verifies SQL Server migrations.
- Critical/High defects must be fixed and retested before Phase 10 VERIFIED.
- Formal usability participant testing is separate from automated tests and heuristic review.

## Suites

| Suite | Command | Environment |
|-------|---------|-------------|
| Backend xUnit | `dotnet test` (Backend/) | Testing + SQLite; LocalDB optional |
| Frontend Vitest | `npm run test:run` | jsdom |
| Playwright E2E | `npm run e2e` | live API + Vite |
| A11y / responsive / visual | `npm run test:a11y` / `test:responsive` / `test:visual` | Playwright |
| Migration script | `scripts/verify-migrations.ps1` | LocalDB |
| Security filter | `dotnet test --filter FullyQualifiedName~Phase10QualityTests` | Testing |

## Coverage honesty

Test counts do not equal complete requirement coverage. Traceability lives in `AUTOMATED_TEST_INVENTORY.md` and the coursework/SRS matrices.
