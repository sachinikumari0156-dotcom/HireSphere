# Candidate UAT / Test Evidence

**Date:** 2026-07-20
**Phases covered:** 4.1 + 4.2 + 4.3 + browser E2E
**Phase 4 status:** **VERIFIED**

## Automated results

| Suite | Result |
|-------|--------|
| Backend `dotnet test` | **58 passed** / 58 |
| Frontend Vitest `npm run test:run` | **22 passed** / 22 |
| Playwright `npm run e2e` | **6 passed** / 6 |
| Frontend `npm run lint` | PASS |
| Frontend `npm run build` | PASS |
| `git diff --check` | PASS |

## Browser E2E

- Full Candidate journey executed against live API + LocalDB
- Evidence: `docs/testing/CANDIDATE_E2E_RESULTS.md`
- Screenshots: `docs/evidence/phase4-candidate/` (23 files)
- Index: `docs/report/SCREENSHOT_INDEX.md`

## Database

- Provider: SQL Server LocalDB `(localdb)\MSSQLLocalDB`
- Database: `HireSphereDev`
- SQL Express: not installed on verification machine

## Storage

- Local secure upload abstraction under `App_Data/uploads`
- Cloud object storage: Phase 8
