# HireSphere — Phase Status

**Last updated:** 2026-07-20
**Overall readiness:** NOT READY (Phases 4–12 pending)

| Phase | Name | Status | Commit | Push | Notes |
|-------|------|--------|--------|------|-------|
| 0 | Audit and planning | VERIFIED | `07080b1` | SUCCESS | Docs committed and pushed |
| 1 | Security foundation | VERIFIED | `9c50d56` | SUCCESS | BCrypt, CORS, secrets externalized |
| 2 | SQL Server and data model | VERIFIED | `1e4c688` + `e84eeb5` | SUCCESS | Applied on `localhost\SQLEXPRESS` |
| 3 | Auth and RBAC | TESTED | pending push | pending | Services, policies, portal shells, 33 tests; SQL smoke OK |
| 4 | Candidate workflows | NOT STARTED | — | — | — |
| 5 | Recruiter workflows | NOT STARTED | — | — | — |
| 6 | Hiring Manager | NOT STARTED | — | — | — |
| 7 | Administrator | NOT STARTED | — | — | — |
| 8 | AI and integrations | NOT STARTED | — | — | — |
| 9 | UI design system | NOT STARTED | — | — | — |
| 10 | Quality and evidence | NOT STARTED | — | — | — |
| 11 | Submission pack | NOT STARTED | — | — | — |
| 12 | Pull request | NOT STARTED | — | — | — |

---

## Phase 2 verification (closed)

### Instance

- SQL Server Express: `MSSQL$SQLEXPRESS` (Running)
- Database: `HireSphereDev`
- Auth: Windows Trusted Connection (no SQL password in tracked config)

### Evidence

- Migration `InitialSqlServerCoreModel` applied; recorded in `__EFMigrationsHistory`
- 38 tables created including required coursework tables
- Application started successfully; Swagger HTTP 200
- Catalog seed idempotent; user seed requires explicit enable + secrets/env
- Passwords in DB are BCrypt hashes (`$2...`)
- Backend tests: 17 passed at Phase 2 close (later expanded in Phase 3)
- Frontend lint/build: PASS

### Seed security

- Hardcoded development password removed from source
- User seeding gated by `Seed:Enabled` / `HIRESPHERE_SEED_ENABLED`
- Credentials via `Seed:AdminEmail` / `Seed:AdminPassword` or env equivalents

---

## Phase 3 notes

- Auth/Admin services and policies implemented
- Migration `AddRecruiterAccessRequests` applied to `HireSphereDev`
- Frontend: AuthContext, protected `/candidate|recruiter|hiring-manager|admin` routes
- Backend tests: 33 passed
- Password reset / email verification / refresh rotation / lockout: NOT IMPLEMENTED
- Phase 3 marked **TESTED** (33 automated tests + SQL Express smoke: candidate register/login/me, admin 403 for candidate, recruiter request Pending). Promote to VERIFIED after full four-role live UI walkthrough.
