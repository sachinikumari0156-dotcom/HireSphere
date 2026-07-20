# HireSphere — Coursework Requirement Matrix

**Course:** SE205.3 Software Architecture 2026  
**Last updated:** 2026-07-20 (Phase 1 security)
**Legend:** NOT STARTED | IN PROGRESS | IMPLEMENTED | TESTED | VERIFIED | BLOCKED — EXTERNAL CREDENTIAL | DEFERRED — OPTIONAL BONUS

---

## Tier M — Mandatory coursework scope

### Four role experiences

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-R01 | Candidate Portal | IN PROGRESS | Register, login, partial dashboard; missing resume, AI recs, assessments |
| M-R02 | Recruiter Portal | IN PROGRESS | API for jobs/applications; UI is placeholder |
| M-R03 | Hiring Manager Dashboard | NOT STARTED | No routes, models, or APIs |
| M-R04 | Administrator Dashboard | NOT STARTED | No routes, models, or APIs |

### Candidate features

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-C01 | Registration and secure authentication | IN PROGRESS | Phase 1: BCrypt hashing; Candidate-only public register; full RBAC later |
| M-C02 | Professional profile management | IN PROGRESS | `CandidateProfilesController`; no full UI |
| M-C03 | CV/resume upload and management | NOT STARTED | `ResumePath` field only; no upload API |
| M-C04 | Job search and application submission | IN PROGRESS | API + partial dashboard |
| M-C05 | AI-powered job recommendations | NOT STARTED | No AI services |
| M-C06 | Application tracking dashboard | IN PROGRESS | Partial list in dashboard |

### Recruiter features

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-RC01 | Job posting creation and management | IMPLEMENTED | `JobsController` CRUD |
| M-RC02 | Candidate search and filtering | NOT STARTED | No dedicated search |
| M-RC03 | Application review and shortlisting | IN PROGRESS | Status update endpoints exist |
| M-RC04 | AI-powered candidate ranking and screening | NOT STARTED | — |
| M-RC05 | Interview scheduling and management | NOT STARTED | `Interview` model orphan |
| M-RC06 | Communication with applicants | NOT STARTED | — |

### Hiring Manager features

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-HM01 | Review shortlisted candidates | NOT STARTED | — |
| M-HM02 | Interview feedback | NOT STARTED | — |
| M-HM03 | Candidate evaluation and scoring | NOT STARTED | — |
| M-HM04 | Hiring decision management | NOT STARTED | — |

### Administrator features

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-A01 | User management | IN PROGRESS | Basic `UsersController` list/get |
| M-A02 | Role and permission management | NOT STARTED | String role on User only |
| M-A03 | System monitoring | NOT STARTED | — |
| M-A04 | Recruitment analytics dashboard | NOT STARTED | — |
| M-A05 | Organization and department management | NOT STARTED | — |

### Backend and database

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-B01 | C# ASP.NET Core Web API | IMPLEMENTED | Builds successfully |
| M-B02 | SQL Server database | NOT STARTED | Currently MySQL (Pomelo) |
| M-B03 | Core entities (profiles, jobs, applications, interviews, assessments, analytics, orgs) | IN PROGRESS | 4 of ~25+ entities in DbContext |
| M-B04 | REST APIs (auth, profiles, resumes, jobs, applications, interviews, evaluations, analytics) | IN PROGRESS | ~5 controller groups; many missing |
| M-B05 | Swagger/OpenAPI | IMPLEMENTED | Enabled in `Program.cs` |

### Security

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-S01 | JWT authentication | IN PROGRESS | Token issued; validation configured; key via secrets/env |
| M-S02 | RBAC | IN PROGRESS | Role claims; public privileged registration blocked |
| M-S03 | Secure password hashing | IMPLEMENTED | BCrypt hash on register; verify on login |
| M-S04 | HTTPS-ready configuration | IN PROGRESS | Dev HTTPS profile exists |
| M-S05 | Audit logging | NOT STARTED | — |
| M-S06 | Data privacy measures | NOT STARTED | — |
| M-S07 | Resource ownership checks | IN PROGRESS | Partial in applications/jobs |
| M-S08 | Secure secret and document handling | IMPLEMENTED | Tracked secrets replaced with placeholders; rotation documented |

### AI and analytics

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-AI01 | Resume parsing | NOT STARTED | — |
| M-AI02 | Skill extraction | NOT STARTED | — |
| M-AI03 | Candidate-job matching | NOT STARTED | — |
| M-AI04 | Candidate ranking/scoring | NOT STARTED | — |
| M-AI05 | Job recommendations | NOT STARTED | — |
| M-AI06 | Automated feedback | NOT STARTED | — |
| M-AI07 | Recruitment performance analytics | NOT STARTED | — |
| M-AI08 | Hiring trend analysis | NOT STARTED | — |
| M-AI09 | Explainable AI + human-review notice | NOT STARTED | — |

### External integrations

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-I01 | Email notifications | NOT STARTED | BLOCKED until SMTP/MailHog configured |
| M-I02 | SMS notifications | NOT STARTED | BLOCKED — EXTERNAL CREDENTIAL |
| M-I03 | Interview reminders | NOT STARTED | — |
| M-I04 | Application-status updates | NOT STARTED | — |
| M-I05 | Google Calendar | NOT STARTED | BLOCKED — EXTERNAL CREDENTIAL |
| M-I06 | Microsoft Outlook Calendar | NOT STARTED | BLOCKED — EXTERNAL CREDENTIAL |
| M-I07 | Secure cloud document storage | NOT STARTED | BLOCKED until storage configured |

### Frontend quality

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-F01 | Desktop, tablet, mobile responsive | IN PROGRESS | Partial CSS; not verified |
| M-F02 | Accessibility | NOT STARTED | Not audited |
| M-F03 | Consistent UX / design system | IN PROGRESS | HireSphere branding on auth pages; design system later |
| M-F04 | Client-side validation | IN PROGRESS | Login/register forms |
| M-F05 | Secure authentication workflow | IN PROGRESS | Token stored; centralized API URL; no route guards yet |
| M-F06 | Error handling and user feedback | IN PROGRESS | Basic form errors; API global exception handler added |
| M-F07 | Usability testing evidence | NOT STARTED | — |

### Testing and evidence

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-T01 | Unit / API / auth tests | NOT STARTED | No test projects |
| M-T02 | Integration tests | NOT STARTED | — |
| M-T03 | UAT scenarios (18 mandatory) | NOT STARTED | — |
| M-T04 | Postman and Swagger evidence | NOT STARTED | Swagger only |

### Report and submission

| ID | Requirement | Status | Evidence / notes |
|----|-------------|--------|------------------|
| M-D01 | Use Case / Class / Deployment diagrams | NOT STARTED | — |
| M-D02 | Design patterns and ADR discussion | NOT STARTED | — |
| M-D03 | Screenshots and evidence pack | NOT STARTED | — |
| M-D04 | Individual contribution (Kalani) | NOT STARTED | — |
| M-D05 | Demo video script and submission checklist | NOT STARTED | — |

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
| NOT STARTED | ~55 |
| IN PROGRESS | ~18 |
| IMPLEMENTED | ~3 |
| TESTED / VERIFIED | 0 |

**Submission readiness:** NOT READY — majority of mandatory rows are NOT STARTED or unverified.
