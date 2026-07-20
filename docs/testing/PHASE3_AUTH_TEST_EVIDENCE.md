# Phase 3 Auth Test Evidence

**Date:** 2026-07-20
**Backend project:** `Backend/HireSphere.API.Tests`
**Command:** `dotnet test`
**Result:** 33 passed, 0 failed

## Coverage themes

| Theme | Evidence |
|-------|----------|
| Candidate registration | Integration + unit tests |
| Duplicate email | Integration |
| Public role escalation impossible | Integration (role=Admin ignored) |
| Password hashed | Integration (`$2` BCrypt) |
| Login success / sanitized failure | Integration |
| Disabled user blocked | Integration |
| Role endpoint isolation | Candidate/Recruiter/Admin Forbidden cases |
| IDOR candidate profile | Integration |
| Cross-org job delete blocked | Integration |
| `/auth/me` safe DTO | Integration |
| Change-password current password | Integration |
| Admin audit log on status change | Integration |

## Package advisory

- NU1903 (`SQLitePCLRaw.lib.e_sqlite3` / GHSA-2m69-gcr7-jv3q) resolved by adding direct `SQLitePCLRaw.bundle_e_sqlite3` **3.0.3** override in the test project.
- `dotnet build` reports **0 Warning(s)** after the override.
