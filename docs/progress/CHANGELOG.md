# HireSphere — Implementation Changelog

## Phase 0 — 2026-07-20

**Commit:** `07080b12733b9af5d07b1e5cb90d5b55a588bdd4`
`chore(audit): document baseline gaps and coursework plan`

- Baseline audit, requirement matrix, implementation plan, DoD, risk register, phase status, secret-rotation doc
- `.gitignore` updated for `local-spec/` and local secrets

## Phase 1 — 2026-07-20

**Commit:** `9c50d56`
`fix(security): secure credentials authentication and API configuration`

- Removed hard-coded DB password and JWT key from tracked config (placeholders only)
- Added CORS allowed-origins configuration
- Implemented BCrypt password hashing and verification
- Blocked public registration of privileged roles (Candidate only)
- Added global exception handler with sanitized API errors
- Centralized frontend API base URL via `VITE_API_BASE_URL`
- Replaced Hireflow branding with HireSphere on auth pages
- Aligned CodeGeneration package version; added BCrypt package
- Updated matrices, risk register, phase status, SRS traceability
- Backend build: PASS (0 warnings)
- Backend tests: N/A — no test project
- Frontend lint/build: PASS; test script missing

## Phase 2 — 2026-07-20

**Commit message:** `refactor(data): migrate persistence to SQL Server and complete core model`
**Status:** TESTED (migration apply BLOCKED without SQL Server)

- Switched EF Core provider from MySQL to SQL Server 8.0.11
- Removed legacy MySQL migrations; added `InitialSqlServerCoreModel` SQL Server baseline
- Expanded `ApplicationDbContext` to full recruitment domain model (35+ entities)
- Fluent configurations for indexes, string lengths, and delete behaviors
- History-preserving deletes: Job→Applications Restrict; Application→Interviews Restrict
- `UsersController`: Admin-only mutations, BCrypt password hashing, safe DTO responses
- `AuthRegistrationRules` helper + registration validation tests
- Created `Backend/HireSphere.API.Tests` (xUnit, SQLite, WebApplicationFactory)
- 14 automated tests: unique constraints, UserDto privacy, privileged registration block
- Documentation: DATA_DICTIONARY, ER_DIAGRAM, SQL_SERVER_SETUP, MYSQL_TO_SQLSERVER_MIGRATION_NOTES, DATABASE_ARCHITECTURE
- Backend build: PASS
- Backend tests: PASS (14/14)
- Frontend lint/build: PASS; frontend test script still missing
- Migration apply: BLOCKED — SQL Server not available locally

## Phase 2 verification closure — 2026-07-20

**Commit message:** `fix(data): verify SQL Server migration and secure development seeding`

- Installed/used SQL Server Express (`localhost\SQLEXPRESS`)
- Applied `InitialSqlServerCoreModel` to `HireSphereDev` (Windows auth)
- Confirmed 38 tables + `__EFMigrationsHistory`
- Removed hardcoded seed password; user seed requires explicit enable + secrets/env
- Added SQL Server verification tests; suite now 17 passing
- API smoke test: Swagger 200 against SQL Server
- Frontend lint/build regression: PASS
- M-B02 promoted to VERIFIED

## Phase 3 — 2026-07-20

**Commit message:** `feat(auth): implement secure role based access and account workflows`
**Status:** TESTED

- Service-based auth: AuthService, TokenService, PasswordService, CurrentUserService, AdminUserService, ResourceAuthorizationService
- Candidate self-registration; recruiter access-request + admin approve/reject; admin role/status/org APIs
- Authorization policies for all four roles + combined recruitment policies
- Ownership/org scoping on candidate profiles, applications, and jobs
- Frontend AuthContext, protected role routes, Access Denied / Session Expired, recruiter request page
- Migration `AddRecruiterAccessRequests` applied to HireSphereDev
- Target framework aligned to net10.0 to match installed ASP.NET runtime
- Backend tests: 33 passed
- Frontend: lint/build PASS
- Not claimed: password reset, email verification, refresh-token rotation, account lockout
