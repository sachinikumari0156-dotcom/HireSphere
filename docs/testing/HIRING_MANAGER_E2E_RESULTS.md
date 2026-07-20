# HireSphere — Hiring Manager Portal Browser E2E Results

**Date:** 2026-07-20  
**Author of verification:** Chinthaka Jayaweera  
**Branch:** `main`  
**Environment:** Windows 10, SQL Server LocalDB `(localdb)\MSSQLLocalDB`, database `HireSphereDev`  
**Backend URL:** `http://127.0.0.1:5167`  
**Frontend URL:** `http://127.0.0.1:5173`  
**Note:** LocalDB Option A (SQL Express not required for this run).

## Summary

| Suite | Result |
|-------|--------|
| Playwright Hiring Manager journey | **PASS** (`e2e/hiring-manager-portal.spec.js`) |
| Full Playwright suite | **8/8 PASS** |
| Backend xUnit | **75/75 PASS** |
| Frontend Vitest | **47/47 PASS** |
| Frontend lint / build | **PASS** |
| Evidence screenshots | **21 files** in `docs/evidence/phase6-hiring-manager/` |

## Journey coverage

| Area | Expected | Actual | Result | Evidence |
|------|----------|--------|--------|----------|
| Manager login + dashboard | Live assigned counts | Observed | PASS | `manager-login.png`, `manager-dashboard.png` |
| Assigned vacancies + detail | Assignment-scoped | Observed | PASS | `manager-assigned-vacancies.png`, `manager-vacancy-detail.png` |
| Candidate list / review / resume / ranking | No path/hash leaks; human-review notice | Observed | PASS | `manager-candidate-*.png`, `manager-resume-review.png`, `manager-ranking-explanation.png` |
| Comparison + unauthorized compare reject | Same vacancy only | Observed | PASS | `manager-candidate-comparison.png` |
| Interview + structured feedback | Timezone + Confirmed response | Observed | PASS | `manager-interview-*.png`, `manager-feedback-*.png` |
| Evaluation Draft → Submitted | Draft reload + submit | Observed | PASS | `manager-evaluation-*.png` |
| Recommendation vs final | RecommendHire `isFinal=false`; FinalHire rejected for HM | Observed | PASS | `manager-recommendation.png`, `manager-decision-history.png` |
| Unassigned manager denied | 403/404 | Observed | PASS | `manager-unassigned-access-denied.png` |
| Candidate private comments hidden | No panel-only text | Observed | PASS | `manager-candidate-private-comments-hidden.png` |
| Mobile dashboard | 390×844 | Observed | PASS | `manager-mobile-dashboard.png` |
| Axe critical/serious on HM pages | None | Observed | PASS | Embedded in journey |

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
npx playwright test e2e/hiring-manager-portal.spec.js
```

Passwords come from environment variables or local E2E defaults; they are not committed as plaintext secrets in docs.
