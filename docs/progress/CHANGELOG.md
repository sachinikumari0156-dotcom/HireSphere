# HireSphere — Implementation Changelog

## Phase 9.1 — 2026-07-21

**Commit message:** `feat(ui): establish accessible HireSphere design system`

- Design tokens, design-system CSS, SkipLink, RoleShell mobile nav
- Core UI primitives (Button, Input, Modal, Alert, StatusBadge, Empty/Error, Tabs, Accordion, Pagination, FileUpload)
- Not Found page; Admin/Recruiter/Hiring Manager layouts use RoleShell
- Vitest design-system coverage; docs under `docs/ui/` and accessibility standard

## Phase 8 verification — 2026-07-21

**Commit message:** `test(platform): verify AI integrations calendar and storage workflows`

- Playwright Phase 8 journey PASS; full suite 10/10
- Evidence: 30 screenshots under `docs/evidence/phase8-platform/`
- Phase 8 status: **IMPLEMENTED — EXTERNAL PROVIDER VERIFICATION PENDING**
- Backend 114/114; Vitest 60/60; Phases 9–11 pending

## Phase 8.3 — 2026-07-21

**Commit message:** `feat(storage): add secure cloud document storage`

- IFileStorageProvider with LocalDevelopment active; Azurite/Azure Blob NotConfigured without credentials
- Magic-byte validation, quarantine/logical-delete foundation, randomized tenant storage keys
- Storage keys removed from normal DTOs; antivirus NotConfigured
- Admin storage status + migration dry-run
- Migration `AddStoragePortalPhase83`

## Phase 8.2 — 2026-07-21

**Commit message:** `feat(integrations): add email SMS and calendar providers`

- Notification outbox + user communication preferences
- SMTP email provider (MailHog/local when configured; production NotConfigured by default)
- Development Mock SMS with consent and E.164 validation
- Internal calendar + ICS generation; Google/Outlook stubs NotConfigured
- Admin integrations status dashboard and failed-delivery retry
- Migration `AddIntegrationsPortalPhase82`

## Phase 8.1 — 2026-07-21

**Commit message:** `feat(ai): add resume parsing matching ranking and trend insights`

- Deterministic resume parsing (PDF/DOCX) with ExtractedSkill review/confirm
- External AI adapter remains NotConfigured without verified credentials
- Candidate consent for external processing; prompt-injection sanitization
- Admin AI status + descriptive skill-trend insights
- Migration `AddAiPortalPhase81`
- Phase 8 IN PROGRESS (integrations/storage pending)

## Phase 7 verification — 2026-07-20

**Commit message:** `test(admin): verify complete administrator portal workflows`

- Playwright Administrator journey PASS against live API + Vite + LocalDB
- Evidence pack: 28 screenshots under `docs/evidence/phase7-admin/`
- Docs: ADMIN_E2E_RESULTS, UAT, TEST_EVIDENCE; governance; matrices updated
- E2E seed `POST /api/e2e/ensure-admin-portal`
- Phase 7 marked **VERIFIED**; Phases 8–11 pending
- Backend 77/77; Vitest 56/56; Playwright 9/9

## Phase 7.2 — 2026-07-20

**Commit message:** `feat(admin): add audit monitoring analytics and final decision controls`

- Audit list/export, monitoring summary, recruitment/workforce/skill analytics
- Admin final-decision APIs (HM cannot FinalHire)
- Safe CSV exports with formula-injection neutralization
- Frontend audit/monitoring/analytics/final-decision pages + Vitest
- Phase 7 remains IN PROGRESS until browser E2E verification

## Phase 7.1 — 2026-07-20

**Commit message:** `feat(admin): add user access organization and department governance`

- Admin portal APIs: dashboard, users, roles, orgs, departments, recruiter requests, HM assign
- Self-disable and last-Administrator protections; SecurityStamp rotation
- Migration `AddAdminPortalPhase71`
- Frontend `/admin` nested portal + Vitest
- Phase 7 remains IN PROGRESS until 7.2 + browser E2E

## Phase 6 verification — 2026-07-20

**Commit message:** `test(manager): verify complete hiring manager portal workflows`

- Playwright Hiring Manager journey PASS against live API + Vite + LocalDB
- Evidence pack: 21 screenshots under `docs/evidence/phase6-hiring-manager/`
- Docs: HIRING_MANAGER_E2E_RESULTS, UAT, TEST_EVIDENCE; decision workflow; matrices updated
- E2E seed `POST /api/e2e/ensure-hiring-manager-portal`
- Phase 6 marked **VERIFIED**; Phase 7 not started
- Backend 75/75; Vitest 47/47; Playwright 8/8

## Phase 6.2 — 2026-07-20

**Commit message:** `feat(manager): add interview feedback evaluations and hiring decisions`

- Structured interview feedback, Candidate evaluation Draft/Submitted, recommendations
- FinalHire/FinalReject blocked for Hiring Manager; withdrawn apps reject decisions
- Migration `AddHiringManagerPortalPhase62`
- Frontend interviews / evaluation / recommendation pages

## Phase 6.1 — 2026-07-20

**Commit message:** `feat(manager): add assigned vacancies and candidate review workspace`

- Hiring Manager portal APIs: dashboard, assigned jobs, candidates, compare, review comments
- Assignment-scoped authorization (`HiringManagerCanAccess*`)
- Register/Login submit button contrast fix (dark ink on amber)
- Migration `AddHiringManagerPortalPhase61`
- Frontend `/hiring-manager` pages + Vitest coverage
- Phase 6 remained IN PROGRESS until 6.2 + browser E2E

## Phase 5 verification — 2026-07-20

**Commit message:** `test(recruiter): verify complete recruiter portal browser workflow`

- Playwright Recruiter browser journey PASS against live API + Vite + LocalDB
- Evidence pack: 26 screenshots under `docs/evidence/phase5-recruiter/`
- Docs: RECRUITER_E2E_RESULTS, RECRUITER_UAT, RECRUITER_TEST_EVIDENCE
- E2E seed `POST /api/e2e/ensure-recruiter-portal` supports multi-org names for cross-org checks
- Phase 5 marked **VERIFIED**; Phase 6 not started
- Backend 69/69; Vitest 38/38; Playwright 7/7

## Phase 5.3 — 2026-07-20

**Commit message:** `feat(recruiter): add interview scheduling and recruitment reports`

- Interview scheduling with UTC persistence, timezone metadata, conflict detection
- Candidate confirm/reschedule flows; internal notes hidden from Candidate APIs
- Organization-scoped reports + safe CSV export
- Calendar sync accurately shown as NotConfigured
- Migration `AddRecruiterPortalPhase53`
- Frontend interviews/schedule/detail/reports pages

## Phase 5.2 — 2026-07-20

**Commit message:** `feat(recruiter): add screening ranking assessments and communication`

- Deterministic ranking with explanation, human-review notice, audited override
- Screening queue and reasoned screening decisions
- Org-scoped assessment builder/assignment; answer keys hidden from candidate APIs
- Application messaging threads with sanitization and notifications
- Migration `AddRecruiterPortalPhase52` on LocalDB
- Frontend screening/ranking/assessments/messages pages + Vitest coverage
- Phase 5 remains IN PROGRESS until 5.3 + Recruiter E2E evidence

## Phase 5.1 — 2026-07-20

**Commit message:** `feat(recruiter): add job management and applicant pipeline`

- Recruiter portal APIs: dashboard, jobs lifecycle, applicant pipeline, notes, comparison
- Job/application status transition services with audit + notifications
- EF migration `AddRecruiterPortalPhase51` applied on LocalDB
- Frontend `/recruiter` pages for dashboard, jobs, pipeline, review, compare
- Registration color-contrast fix (`Register.css`)
- Backend tests: 65/65 PASS; Frontend Vitest: 31/31 PASS
- Phase 5 remains IN PROGRESS until 5.2, 5.3, and Recruiter E2E evidence

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

## Phase 3 verification closure — 2026-07-20

**Commit message:** `test(auth): verify four role access and frontend authentication flows`
**Status:** VERIFIED

- Cleared NU1903 via `SQLitePCLRaw.bundle_e_sqlite3` 3.0.3 override (test project)
- Added Vitest + RTL + jsdom; scripts `test`, `test:run`, `test:coverage`
- Frontend auth tests: 13/13 PASS
- Live four-role UAT on SQL Express `HireSphereDev`: 26/26 PASS
- Evidence: `docs/testing/PHASE3_LIVE_UAT.md`, `PHASE3_AUTH_TEST_EVIDENCE.md`, `FRONTEND_TEST_RESULTS.md`
- Backend: 33 tests PASS; build 0 errors / 0 advisory warnings
- Frontend: lint PASS, build PASS

## Phase 4.1 — 2026-07-20

**Commit message:** `feat(candidate): complete profile resume and document workflows`

- Candidate portal APIs under `/api/candidate` (dashboard, profile, experience, education, skills, certifications, resumes, documents)
- Local secure file storage with MIME/extension/size validation
- Migration `AddCandidateProfilePortalFields` applied to HireSphereDev
- Frontend `/candidate` dashboard + `/candidate/profile` page
- Backend tests: 43 passed; Frontend Vitest: auth 13 + candidate portal tests
- Cloud storage verification: pending (local provider only)

## Phase 4.2 — 2026-07-20

**Commit message:** `feat(candidate): add job discovery recommendations and applications`

- Candidate job discovery under `/api/candidate/jobs` (filters, pagination, sorting, Open-only)
- Deterministic matching provider (`DeterministicJobMatchingProvider`) with explanation + human-review notice
- Recommendations endpoint with highest-match ordering + incomplete-profile empty handling
- Application wizard APIs: resume selection, cover letter, screening answers, terms, duplicate/closed-job guards, status history, withdraw
- Migration `AddApplicationResumeId`
- Frontend routes: jobs, job detail, recommendations, apply wizard, applications list/detail
- Docs updated for API/portal/UAT/evidence/matrices
- Backend/frontend verification: BE **52** tests PASS; FE Vitest **19** PASS; lint/build PASS
- Migration `AddApplicationResumeId` applied to HireSphereDev (SQL Express)

## Phase 4.3 — 2026-07-20

**Commit message:** `feat(candidate): add assessment interview and tracking experience`

- Assessment assignments: list/start/answer/submit with attempt limits, start/expiry checks, server scoring, no answer-key exposure, audit trail
- Interviews: list/detail with timezone; confirm / reschedule-request / decline; meeting info gated; no calendar credentials
- Application tracking: ordered `ApplicationStatusHistory`, latest update, next action, linked interviews/assessments
- In-app notification foundation (`ApplicationSubmitted`, status updates, assessment/interview categories)
- Enum additions: `ApplicationStatus.Assessment`, `Interviewed`; `InterviewCandidateResponse`
- Migration `AddPhase43AssessmentsInterviewsNotifications` **applied** to HireSphereDev
- Frontend routes: assessments, interviews, notifications + dashboard links + application timeline UI
- Verification: BE **58** PASS; FE Vitest **22** PASS; lint/build PASS
- Phase 4 remains **IN PROGRESS / not VERIFIED** — no full browser E2E or screenshot pack; recruiter assign/schedule UI is Phase 5

## Phase 4 browser E2E verification — 2026-07-20

**Commit message:** `test(candidate): verify complete candidate portal browser workflow`
**Author:** Chinthaka Jayaweera

- Playwright Candidate journey, authorization, responsive, accessibility suites
- Development-only `/api/e2e` catalog/journey seed helpers (disabled unless `HIRESPHERE_E2E_SEED_ENABLED`)
- CORS allowlist includes `127.0.0.1` Vite origins
- Profile resume metadata list; education dates; mobile wrap; form labels
- ProtectedRoute: unauthenticated → `/login`; expired → `/session-expired`
- Evidence: 23 screenshots under `docs/evidence/phase4-candidate/`
- Verification host used LocalDB `(localdb)\MSSQLLocalDB` / `HireSphereDev` (SQL Express unavailable)
- Results: Playwright **6/6**, BE **58/58**, FE Vitest **22/22**, lint/build PASS
- Phase 4 marked **VERIFIED**
