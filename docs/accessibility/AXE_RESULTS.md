# Axe results — Phase 9

**Date:** 2026-07-21  
**Tool:** `@axe-core/playwright` with tags `wcag2a`, `wcag2aa`  
**Rule policy:** Critical and serious violations fail the suite. No global rule disables.

## Covered routes (Phase 9 journey)

Public: `/`, `/login`, `/register`, `/access-denied`, `/session-expired`  

Candidate: `/candidate`, profile, jobs, applications, assessments, interviews  

Recruiter: `/recruiter`, jobs, job form, pipeline, reports  

Hiring Manager: `/hiring-manager`, vacancies, interviews  

Administrator: `/admin`, users, roles, organizations, departments, audit, monitoring, analytics, integrations, storage, final-decisions  

Plus existing `e2e/candidate-a11y.spec.js` coverage.

## Result

**Critical / serious:** 0 (empty bag in `phase9-ui` run; `responsive-matrix.json` records `axeCriticalSerious: []`)

## Lower-severity / residual notes

- Moderate/minor axe findings may still exist on less-trafficked pages; not claimed as zero for all severities.
- Contrast of decorative gradients on the marketing hero is secondary to text contrast on content surfaces.
- Formal certification not claimed.
