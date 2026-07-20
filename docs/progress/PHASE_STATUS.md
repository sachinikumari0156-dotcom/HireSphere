# HireSphere — Phase Status

**Last updated:** 2026-07-20
**Overall readiness:** NOT READY

| Phase | Name | Status | Commit | Push | Notes |
|-------|------|--------|--------|------|-------|
| 0 | Audit and planning | VERIFIED | `07080b1` | SUCCESS | Docs committed and pushed |
| 1 | Security foundation | VERIFIED | `9c50d56` | SUCCESS | BCrypt, CORS, secrets externalized |
| 2 | SQL Server and data model | TESTED | pending | pending | Provider + model + tests; DB apply BLOCKED |
| 3 | Auth and RBAC | NOT STARTED | — | — | — |
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

## Phase 0 detail

### Completed

- Identity gate verified as `kalanirashmika`
- Audit/planning/security docs committed
- Push to `kalanirashmika/coursework-completion` succeeded (`07080b1`)

---

## Phase 1 detail

### Completed

- Secrets removed from tracked config
- BCrypt password hashing/verification
- Privileged public registration blocked
- CORS restricted by configuration
- Global API exception handler
- Frontend API base URL centralized
- Hireflow → HireSphere on auth pages
- Matrices / risk / changelog / SRS traceability updated
- **Commit:** `9c50d56` — push SUCCESS

---

## Phase 2 detail

### Completed

- Removed Pomelo/MySQL provider and obsolete MySQL migrations
- SQL Server EF Core 8.0.11 configured with connection-string placeholders
- Core coursework domain model (identity, org, candidate, recruitment, assessments, interviews, AI stubs)
- Fluent API configurations, unique constraints, indexes, Restrict delete for history
- Idempotent `DbSeeder` for roles, org, skills, and four-role demo users
- Migration `InitialSqlServerCoreModel` generated and reviewed
- `HireSphere.API.Tests` — 14 tests passing (SQLite relational constraints + auth)
- Data dictionary, ER diagram, SQL Server setup, migration notes, DB architecture docs

### Blocked / not verified

- `dotnet ef database update` against an empty SQL Server instance — **BLOCKED** (no local SQL Server/Docker/LocalDB)
- Therefore M-B02 migration apply remains **IMPLEMENTED / TESTED**, not VERIFIED

### Next

- Install/start SQL Server (or Docker), apply migration, re-seed, then promote Phase 2 to VERIFIED
- Phase 3: full role-based account workflows
