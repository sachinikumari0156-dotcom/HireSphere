# HireSphere — Coursework Requirement Matrix

**Course:** SE205.3 Software Architecture 2026
**Last updated:** 2026-07-21 (Phase 9 UI / accessibility VERIFIED)
**Legend:** NOT STARTED | IN PROGRESS | IMPLEMENTED | TESTED | VERIFIED | BLOCKED — EXTERNAL CREDENTIAL | DEFERRED — OPTIONAL BONUS

---

## Tier M — Mandatory coursework scope

### Four role experiences

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-R01 | Candidate Portal | VERIFIED | Playwright E2E 2026-07-20 + API/Vitest; screenshots in `docs/evidence/phase4-candidate/` |
| M-R02 | Recruiter Portal | VERIFIED | Playwright E2E 2026-07-20; screenshots in `docs/evidence/phase5-recruiter/` |
| M-R03 | Hiring Manager Dashboard | VERIFIED | Playwright E2E 2026-07-20; screenshots in `docs/evidence/phase6-hiring-manager/` |
| M-R04 | Administrator Dashboard | VERIFIED | Live `/admin` dashboard + Phase 7 E2E |

### Candidate features

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-C01 | Registration and secure authentication | VERIFIED | Phase 3 live UAT + FE/BE tests |
| M-C02 | Professional profile management | VERIFIED | Browser E2E + API tests |
| M-C03 | CV/resume upload and management | VERIFIED | Local secure storage verified Phase 8.3; Azure Blob NotConfigured |
| M-C04 | Job search and application submission | VERIFIED | Browser E2E apply wizard + duplicate rejection |
| M-C05 | AI-powered job recommendations | VERIFIED | Deterministic provider; E2E recommendations |
| M-C06 | Application tracking dashboard | VERIFIED | Timeline E2E + automated ordering tests |

### Recruiter features

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-RC01 | Job posting creation and management | VERIFIED | Recruiter job CRUD + lifecycle + E2E publish |
| M-RC02 | Candidate search and filtering | VERIFIED | Pipeline search/filter/pagination E2E |
| M-RC03 | Application review and shortlisting | VERIFIED | Status transitions + shortlist E2E |
| M-RC04 | AI-powered candidate ranking and screening | VERIFIED | Deterministic ranking + screening queue; human-review notice (`docs/ai/RANKING_MODEL.md`) |
| M-RC05 | Interview scheduling and management | VERIFIED | Schedule + conflicts + ICS/internal calendar; Google/Outlook NotConfigured |
| M-RC06 | Communication with applicants | VERIFIED | In-app + outbox; Dev SMTP NotConfigured unless MailHog; SMS mock verified |

### Hiring Manager features

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-HM01 | Review shortlisted candidates | VERIFIED | Assignment-scoped review + compare; E2E |
| M-HM02 | Interview feedback | VERIFIED | Structured ratings + private panel comments; E2E |
| M-HM03 | Candidate evaluation and scoring | VERIFIED | Draft/Submitted evaluation; advisory scores |
| M-HM04 | Hiring decision management | VERIFIED | Recommendations; finals require Recruiter/Admin |

### Administrator features

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-A01 | User management | VERIFIED | Admin users + Phase 7 E2E |
| M-A02 | Role and permission management | VERIFIED | Matrix UI + APIs + E2E |
| M-A03 | System monitoring | VERIFIED | `/admin/monitoring` truthful LocalDB + NotConfigured providers |
| M-A04 | Recruitment analytics dashboard | VERIFIED | `/admin/analytics` org-scoped LocalDB |
| M-A05 | Organization and department management | VERIFIED | CRUD + archive rules + E2E |

### Backend and database

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-B01 | C# ASP.NET Core Web API | IMPLEMENTED | Builds successfully |
| M-B02 | SQL Server database | VERIFIED | Applied `InitialSqlServerCoreModel` on `localhost\SQLEXPRESS` / `HireSphereDev` |
| M-B03 | Core entities (profiles, jobs, applications, interviews, assessments, analytics, orgs) | IMPLEMENTED | 35+ entities in DbContext + configurations |
| M-B04 | REST APIs (auth, profiles, resumes, jobs, applications, interviews, evaluations, analytics) | IN PROGRESS | ~5 controller groups; many missing |
| M-B05 | Swagger/OpenAPI | IMPLEMENTED | Enabled in `Program.cs` |

### Security

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-S01 | JWT authentication | VERIFIED | Live login/me + TokenService tests |
| M-S02 | RBAC | VERIFIED | Four-role live UAT + policies |
| M-S03 | Secure password hashing | VERIFIED | BCrypt; change-password UAT |
| M-S04 | HTTPS-ready configuration | IN PROGRESS | Dev HTTPS profile exists |
| M-S05 | Audit logging | VERIFIED | AuditLogs observed for auth/admin actions |
| M-S06 | Data privacy measures | VERIFIED | CurrentUserDto / UserDto exclude PasswordHash |
| M-S07 | Resource ownership checks | VERIFIED | Cross-candidate 403 in live UAT |
| M-S08 | Secure secret and document handling | IMPLEMENTED | Tracked secrets replaced with placeholders; rotation documented |

### AI and analytics

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-AI01 | Resume parsing | NOT STARTED | ResumeAnalysis model only |
| M-AI02 | Skill extraction | NOT STARTED | — |
| M-AI03 | Candidate-job matching | TESTED | DeterministicJobMatchingProvider; match API; CandidateJobMatch persistence |
| M-AI04 | Candidate ranking/scoring | VERIFIED | Deterministic recruiter ranking provider + E2E explanation |
| M-AI05 | Job recommendations | TESTED | `/api/candidate/recommendations` highest-match sort; not external AI |
| M-AI06 | Automated feedback | NOT STARTED | — |
| M-AI07 | Recruitment performance analytics | NOT STARTED | — |
| M-AI08 | Hiring trend analysis | NOT STARTED | — |
| M-AI09 | Explainable AI + human-review notice | VERIFIED | Match + ranking + parse notices; External AI NotConfigured |

### External integrations

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-I01 | Email notifications | NOT STARTED | BLOCKED until SMTP/MailHog configured; in-app records exist for key events |
| M-I02 | SMS notifications | NOT STARTED | BLOCKED — EXTERNAL CREDENTIAL |
| M-I03 | Interview reminders | NOT STARTED | — |
| M-I04 | Application-status updates | IN PROGRESS | In-app notifications on submit/withdraw; email channel pending |
| M-I05 | Google Calendar | NOT STARTED | BLOCKED — EXTERNAL CREDENTIAL; Phase 5 shows NotConfigured |
| M-I06 | Microsoft Outlook Calendar | NOT STARTED | BLOCKED — EXTERNAL CREDENTIAL; Phase 5 shows NotConfigured |
| M-I07 | Secure cloud document storage | NOT STARTED | BLOCKED until storage configured |

### Frontend quality

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-F01 | Desktop, tablet, mobile responsive | VERIFIED | Phase 9 responsive matrix + Playwright; `docs/testing/PHASE9_RESPONSIVE_RESULTS.md` |
| M-F02 | Accessibility | VERIFIED | axe critical/serious 0 on covered routes; keyboard results; semantic review (not SR certification) |
| M-F03 | Consistent UX / design system | VERIFIED | Tokens + RoleShell + components; Phase 9.1–9.2 |
| M-F04 | Client-side validation | VERIFIED | Auth + portal forms; accessible errors |
| M-F05 | Secure authentication workflow | VERIFIED | AuthContext, protected routes, session expiry |
| M-F06 | Error handling and user feedback | VERIFIED | Alerts, empty/error states, provider Not Configured |
| M-F07 | Usability testing evidence | PENDING | Real participants unavailable this cycle; heuristic done; automated UAT PASS |

### Testing and evidence

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-T01 | Unit / API / auth tests | VERIFIED | BE 132 + FE 89 + Playwright 14 |
| M-T02 | Integration tests | VERIFIED | Phase 10 LocalDB API integration + migration tests |
| M-T03 | UAT scenarios (18 mandatory) | VERIFIED | Four-role automated UAT + docs (participant usability separate) |
| M-T04 | Postman and Swagger evidence | VERIFIED | Postman collection + Swagger tests/docs |

### Report and submission

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-D01 | Use Case / Class / Deployment diagrams | IMPLEMENTED | Mermaid sources in `docs/architecture/diagrams/source/` (17); rendered PNG PENDING |
| M-D02 | Design patterns and ADR discussion | VERIFIED | ADR-001..012 + final report Ch.3 |
| M-D03 | Screenshots and evidence pack | VERIFIED | `docs/evidence/EVIDENCE_MASTER_INDEX.md` (185 PNGs) |
| M-D04 | Individual contribution | VERIFIED | `docs/contribution/*` (Kalani authorship preserved) |
| M-D05 | Demo video script and submission checklist | PARTIALLY VERIFIED | Scripts COMPLETE; video/NLEARN PENDING USER |

---

## Tier Q — Quality enhancements (after Tier M stable)

| ID | Requirement | Status |
|----|-------------|--------|
| Q-01 | Refresh-token rotation | DEFERRED — OPTIONAL BONUS |
| Q-02 | Account lockout | NOT STARTED |
| Q-03 | Password reset and email verification | NOT STARTED |
| Q-04 | Health checks, structured logging, global errors | IN PROGRESS | Global exception handler added in Phase 1 |
| Q-05 | Pagination/filtering/sorting | NOT STARTED |
| Q-06 | CI and Docker dev environment | NOT STARTED |
| Q-07 | Playwright E2E | NOT STARTED |

---

## Tier B — Optional bonus (do not block submission)

| ID | Requirement | Status |
|----|-------------|--------|
| B-01 | AI interview question generation | DEFERRED — OPTIONAL BONUS |
| B-02 | Sentiment analysis | DEFERRED — OPTIONAL BONUS |
| B-03 | Candidate chatbot | DEFERRED — OPTIONAL BONUS |
| B-04 | Predictive hiring analytics | DEFERRED — OPTIONAL BONUS |

---

## Summary counts (Tier M only)

| Status | Count (approx.) |
|--------|-----------------|
| NOT STARTED | ~50 |
| IN PROGRESS | ~22 |
| IMPLEMENTED | ~6 |
| TESTED / VERIFIED | 0 |

**Submission readiness:** PARTIAL — Phase 11 packaging complete; real usability participants PENDING; title-page placeholders and NLEARN upload PENDING USER; **not** Production Ready.
