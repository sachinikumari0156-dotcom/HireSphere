# HireSphere — Candidate Portal Browser E2E Results

**Date:** 2026-07-20
**Author of verification:** Chinthaka Jayaweera
**Branch:** `main`
**Environment:** Windows 10, SQL Server LocalDB `(localdb)\MSSQLLocalDB`, database `HireSphereDev`
**Backend URL:** `http://127.0.0.1:5167`
**Frontend URL:** `http://127.0.0.1:5173`
**Note:** SQL Server Express (`localhost\SQLEXPRESS`) was not installed on this machine; LocalDB was used (documented Option A).

## Summary

| Suite | Result |
|-------|--------|
| Playwright Candidate journey | **PASS** |
| Authorization boundary API checks | **PASS** |
| Responsive desktop / tablet / mobile | **PASS** |
| Accessibility (axe critical blocking) | **PASS** (1 residual register color-contrast documented) |
| Backend xUnit | **58/58 PASS** |
| Frontend Vitest | **22/22 PASS** |
| Evidence screenshots | **23 files** in `docs/evidence/phase4-candidate/` |

## Mandatory browser scenarios

| # | Scenario | Expected | Actual | Result | Screenshot |
|---|----------|----------|--------|--------|------------|
| 1–4 | Landing, register validation, unique register | Validation + Candidate home | Observed | PASS | `registration-validation.png` |
| 5–7 | Login, dashboard, session restore | Candidate dashboard persists | Observed | PASS | `candidate-login.png`, `candidate-dashboard.png` |
| 8–19 | Profile, experience, education, skills, certs, resume, invalid upload | Updates + metadata + rejection | Observed | PASS | `candidate-profile.png` … `candidate-invalid-upload.png` |
| 20–30 | Job search/filters, match explanation, recommendations | Filters + score + skills + human review | Observed | PASS | `candidate-job-search.png` … `candidate-recommendations.png` |
| 31–41 | Application wizard, success, duplicate, timeline | Create + block duplicate + history | Observed | PASS | `candidate-application-*.png` |
| 42–52 | Assessment, interview, notifications | Score without answer keys; interview; in-app notices | Observed | PASS | `candidate-assessment*.png`, `candidate-interview.png`, `candidate-notifications.png` |
| 53–62 | Access denied, expired token, logout | Role redirects + logout → login | Observed | PASS | `candidate-access-denied.png`, `candidate-mobile-dashboard.png` |

## Authorization

Cross-candidate application/assessment/interview blocked (403/404). Admin/recruiter APIs return 403 for Candidate. `passwordHash`, absolute paths, and `CorrectAnswerKey` absent from Candidate payloads. CORS rejects `https://evil.example`. Duplicate applications rejected.

## Responsive

| Viewport | Result |
|----------|--------|
| 1440×900 | PASS |
| 768×1024 | PASS |
| 390×844 | PASS (navbar/dashboard wrap; no material horizontal overflow) |

## Accessibility

- Keyboard Tab reaches interactive controls.
- Associated labels added on Login/Register; aria-labels on profile sub-forms.
- Residual **color-contrast** on register marketing chrome (1 node) documented — not treated as Phase 4 blocker after label/contrast remediation pass.
- No other critical/serious axe violations on Candidate critical pages.

## Storage honesty

Candidate documents use the local storage abstraction under `App_Data/uploads`. Cloud object storage remains Phase 8.

## How to re-run

```powershell
# API with LocalDB + E2E seed flag
$env:ASPNETCORE_URLS="http://127.0.0.1:5167"
$env:ConnectionStrings__DefaultConnection="Server=(localdb)\MSSQLLocalDB;Database=HireSphereDev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
$env:Jwt__Key="LOCAL-DEV-ONLY-HIRESPHERE-JWT-KEY-32+"
$env:HIRESPHERE_E2E_SEED_ENABLED="true"
# frontend: VITE_API_BASE_URL=http://localhost:5167/api npm run dev
cd Frontend
$env:HIRESPHERE_E2E_FRONTEND_URL="http://127.0.0.1:5173"
$env:HIRESPHERE_E2E_API_URL="http://127.0.0.1:5167"
npm run e2e
```
