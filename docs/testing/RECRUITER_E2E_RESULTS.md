# HireSphere — Recruiter Portal Browser E2E Results

**Date:** 2026-07-20  
**Author of verification:** Chinthaka Jayaweera  
**Branch:** `main`  
**Environment:** Windows 10, SQL Server LocalDB `(localdb)\MSSQLLocalDB`, database `HireSphereDev`  
**Backend URL:** `http://127.0.0.1:5167`  
**Frontend URL:** `http://127.0.0.1:5173`  
**Note:** SQL Server Express was not used; LocalDB Option A.

## Summary

| Suite | Result |
|-------|--------|
| Playwright Recruiter journey | **PASS** (`e2e/recruiter-portal.spec.js`) |
| Full Playwright suite (Candidate + Recruiter) | **7/7 PASS** |
| Backend xUnit | **69/69 PASS** |
| Frontend Vitest | **38/38 PASS** |
| Frontend lint / build | **PASS** |
| Evidence screenshots | **26 files** in `docs/evidence/phase5-recruiter/` |

## Journey coverage (mandatory steps)

| Area | Expected | Actual | Result | Evidence |
|------|----------|--------|--------|----------|
| Recruiter request page | Form visible | Observed | PASS | `recruiter-request.png` |
| Admin login + admin area | Seeded admin active | Observed | PASS | `admin-recruiter-approval.png` |
| Recruiter login + dashboard | Live metrics / empty-safe | Observed | PASS | `recruiter-dashboard.png` |
| Create draft job + skills + questions | Draft persisted | Observed | PASS | `recruiter-create-job.png`, `recruiter-job-skills.png`, `recruiter-screening-questions.png` |
| Publish job | Status Published | Observed | PASS | `recruiter-published-job.png`, `recruiter-job-list.png` |
| Candidate public visibility | Job in candidate search | Observed | PASS | API assert in E2E |
| Apply + pipeline + detail | No absolute paths / password hashes | Observed | PASS | `recruiter-applicant-pipeline.png`, `recruiter-application-detail.png` |
| Ranking + human-review notice | Notice visible | Observed | PASS | `recruiter-ranking-explanation.png` |
| Screening + comparison | Pages load | Observed | PASS | `recruiter-screening.png`, `recruiter-candidate-comparison.png` |
| Assessment assign + candidate complete | Server-side score | Observed | PASS | `recruiter-assessment-*.png` |
| Shortlist + messaging | Thread sanitized | Observed | PASS | `recruiter-message-thread.png` |
| Interview + conflict | Conflict warning shown | Observed | PASS | `recruiter-interview-*.png`, `recruiter-conflict-warning.png` |
| Reports + CSV | Org-scoped export | Observed | PASS | `recruiter-reports.png`, `recruiter-report-filters.png`, `recruiter-csv-export.png` |
| Cross-org job GET | 404 | Observed | PASS | API assert |
| Candidate denied `/recruiter` | Access denied | Observed | PASS | `recruiter-access-denied.png` |
| Mobile dashboard | 390×844 | Observed | PASS | `recruiter-mobile-dashboard.png` |

## Authorization negatives (API / browser)

- Candidate cannot use `/recruiter` UI (Access denied).
- Other-organization Recruiter cannot `GET /api/recruiter/jobs/{id}` (404).
- Assessment answer keys not shown in Candidate assessment UI.
- Application detail does not expose `c:\` paths or `passwordHash`.

## Deferred / honest boundaries

- Google Calendar / Outlook: **Not Configured** (Phase 8).
- External email/SMS delivery: pending Phase 8 (in-app notifications only).
- Ranking is deterministic local provider, not an external AI call.

## How to re-run

```powershell
$env:HIRESPHERE_E2E_FRONTEND_URL="http://127.0.0.1:5173"
$env:HIRESPHERE_E2E_API_URL="http://127.0.0.1:5167"
$env:HIRESPHERE_E2E_SEED_ENABLED="true"
# API with E2e seed enabled on :5167; Vite on :5173
cd Frontend
npx playwright test e2e/recruiter-portal.spec.js
```

Passwords come from environment variables or local defaults used only in ignored/local E2E runs; they are not committed.
