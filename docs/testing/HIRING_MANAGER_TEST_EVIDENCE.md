# Hiring Manager Portal — Test Evidence

**Date:** 2026-07-20  
**Database:** `(localdb)\MSSQLLocalDB` / `HireSphereDev`  
**Migrations:** through `AddHiringManagerPortalPhase62`

## Automated suites

| Suite | Location | Focus |
|-------|----------|-------|
| Backend Phase 6.1 | `HiringManagerPortalPhase61Tests.cs` | Assignment authz, compare limits, denials, audit |
| Backend Phase 6.2 | `HiringManagerPortalPhase62Tests.cs` | Feedback, evaluation, recommendation, finals blocked |
| Frontend Vitest | `hiring-manager.test.jsx` | Dashboard, list, review, compare limit, role denial, forms |
| Playwright | `e2e/hiring-manager-portal.spec.js` | Full browser journey + negatives + axe + responsive |

## Seed

`POST /api/e2e/ensure-hiring-manager-portal` (requires `HIRESPHERE_E2E_SEED_ENABLED=true` / `E2e:Enabled`)

Creates: Admin, Recruiter, assigned HM, unassigned HM, two Candidates, org/dept, assigned published job, unassigned vacancy, applications, assessment result, scheduled interview with participant.

## Screenshots

See `docs/evidence/phase6-hiring-manager/` and `docs/report/SCREENSHOT_INDEX.md`.

## Residual / honest notes

- Calendar providers remain NotConfigured (Phase 8).
- External email/SMS deferred to Phase 8.
- Minor axe residuals outside critical/serious (if any) are recorded in E2E results after the run.
