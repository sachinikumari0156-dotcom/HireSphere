# Phase 3 Live UAT Evidence

**Date:** 2026-07-20
**Environment:** Windows 10, SQL Server Express `localhost\SQLEXPRESS`, database `HireSphereDev`, API `http://127.0.0.1:5167`, frontend Vite build against `VITE_API_BASE_URL` default `http://localhost:5167/api`
**Method:** Live HTTP calls against running ASP.NET API + SQL Server (script `scripts/phase3-live-uat.ps1`). Frontend role-route guards verified by Vitest (see `FRONTEND_TEST_RESULTS.md`).
**Admin seed:** Enabled for session only via env (`HIRESPHERE_SEED_ENABLED`); credentials not committed.

## Summary

| Suite | Result |
|-------|--------|
| Four-role API UAT | **26 PASS / 0 FAIL** |
| Frontend auth Vitest | **13 PASS / 0 FAIL** |
| Backend xUnit | **33 PASS / 0 FAIL** |

## Candidate

| ID | Role | Endpoint/page | Expected | Actual | Result |
|----|------|---------------|----------|--------|--------|
| C1 | Candidate | POST `/api/auth/register/candidate` (client sent `role=Admin`) | 200, role Candidate | 200 role=Candidate | PASS |
| C2 | Candidate | POST `/api/auth/login` | 200 + token | 200 | PASS |
| C3 | Candidate | GET `/api/auth/me` | Safe DTO, no passwordHash | 200 role=Candidate hasHash=False | PASS |
| C4 | Candidate | GET `/api/admin/users` | 403 | 403 | PASS |
| C5 | Candidate | GET `/api/Jobs/MyJobs` | 403 | 403 | PASS |
| C6 | Candidate | POST `/api/auth/change-password` | 200 | 200 | PASS |
| C7 | Candidate | POST `/api/auth/logout` | 200 | 200 | PASS |
| C8 | Candidate | POST `/api/auth/login` (new password) | 200 | 200 | PASS |

Frontend: Candidate route allowed / Recruiter+Admin denied — Vitest protected-route cases PASS.

## Recruiter

| ID | Role | Endpoint/page | Expected | Actual | Result |
|----|------|---------------|----------|--------|--------|
| R1 | Public | POST `/api/auth/recruiter-requests` | Pending | 200 status=Pending | PASS |
| A3 | Admin | POST approve request | 200 | 200 | PASS |
| A4 | Admin | PATCH user roles → Recruiter | 200 | 200 | PASS |
| A5 | Admin | PATCH user organization | 200 | 200 | PASS |
| R3 | Recruiter | POST `/api/auth/login` | role Recruiter | 200 role=Recruiter | PASS |
| R4 | Recruiter | GET `/api/admin/users` | 403 | 403 | PASS |
| R5 | Recruiter | GET `/api/Jobs/MyJobs` | 200 | 200 | PASS |

## Hiring Manager

| ID | Role | Endpoint/page | Expected | Actual | Result |
|----|------|---------------|----------|--------|--------|
| A6 | Admin | PATCH roles → HiringManager | 200 | 200 | PASS |
| H1 | HiringManager | POST `/api/auth/login` | role HiringManager | 200 role=HiringManager | PASS |
| H2 | HiringManager | GET `/api/admin/users` | 403 | 403 | PASS |
| H3 | HiringManager | GET `/api/Applications/MyApplications` | 403 | 403 | PASS |
| A7 | Admin | PATCH status Inactive | 200 | 200 | PASS |
| H4 | HiringManager | Login while disabled | 401 disabled | 401 This account is disabled. | PASS |

## Administrator

| ID | Role | Endpoint/page | Expected | Actual | Result |
|----|------|---------------|----------|--------|--------|
| A1 | Admin | POST `/api/auth/login` | role Admin | 200 role=Admin | PASS |
| A2 | Admin | GET `/api/admin/recruiter-requests` | 200 list | 200 count≥1 | PASS |
| A3–A7 | Admin | approve / role / org / status | 200 | 200 | PASS |

## Security checks

| ID | Check | Expected | Actual | Result |
|----|-------|----------|--------|--------|
| S1 | Invalid login | Sanitized 401 | Invalid email or password. | PASS |
| S2 | Unauth `/api/auth/me` | 401 | 401 | PASS |
| S3 | Cross-candidate profile | 403 | 403 | PASS |

## SQL evidence (post-UAT)

- `AuditLogs` contains `candidate.register`, `auth.login`, `admin.user.role`, `admin.user.organization`, `admin.user.status`
- Users by role observed: Admin, Candidate, Recruiter, HiringManager present in `HireSphereDev`

## Screenshots

No browser screenshots captured in this run. Evidence is command/API output and automated tests. Do not invent screenshots.
