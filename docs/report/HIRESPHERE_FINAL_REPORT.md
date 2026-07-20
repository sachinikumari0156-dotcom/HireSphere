# HireSphere — Final Architecture and Coursework Report

**Document type:** Final report source (Markdown)  
**Module:** SE205.3 Software Architecture (2026) — *[module details PENDING USER confirmation]*  
**Coursework title:** Software Architecture Coursework — HireSphere  
**Project title:** HireSphere — AI-Assisted Recruitment and Talent Management Platform  
**Report author (this finalisation):** Chinthaka Jayaweera `<chinthakajayaweera1@gmail.com>`  
**Historical contributors (Git):** Kalani Rashmika; sachini kumudu kumari; Chinthaka Jayaweera  
**Student ID(s):** *[PENDING USER — do not invent]*  
**Lecturer / group details:** *[PENDING USER — do not invent]*  
**Submission date:** *[PENDING USER]*  
**Repository:** https://github.com/sachinikumari0156-dotcom/HireSphere.git  
**Branch:** `main`  
**Baseline SHA at Phase 11 start:** `508e389`

---

## Declaration

I declare that this report truthfully describes the HireSphere system as implemented and verified in the repository. Test counts, provider statuses, usability participant counts, and deployment claims match recorded evidence. I have not fabricated participants, external-provider verification, or production deployment.

Where Cursor / AI tooling assisted drafting or coding, this is disclosed in Chapter 10 and in `docs/contribution/AI_ASSISTANCE_DISCLOSURE.md` (Phase 11.2).

---

## Abstract

HireSphere is a four-role recruitment platform comprising a React (Vite) single-page application and an ASP.NET Core Web API backed by SQL Server (LocalDB in verified development). The system supports Candidate self-service, Recruiter pipeline management, Hiring Manager evaluation, and Administrator governance, with JWT authentication, RBAC, organisation scoping, secure local document storage, deterministic AI assistance with mandatory human review, and modular integration providers. Phases 0–9 delivered and verified core product capability. Phase 10 established automated quality, security verification, four-role UAT, and a release candidate, with **Phase 10 remaining PARTIALLY VERIFIED** because genuine usability participants were unavailable (M-F07 PENDING). External production providers (cloud AI, production SMTP, external SMS, Google/Outlook calendars, Azure Blob, antivirus) remain **Not Configured**. This report documents architecture, design, security, testing, limitations, and submission readiness for coursework—not production go-live.

---

## Acknowledgements

Acknowledgements are extended to prior repository contributors whose Git authorship remains intact, and to module staff *[PENDING USER names]*. Tooling acknowledgements: Microsoft .NET, React, Playwright, and Cursor AI assistance (disclosed).

---

## Table of contents

1. Introduction  
2. Requirements analysis  
3. Architecture and design  
4. Detailed system design  
5. Implementation  
6. Security, privacy and ethics  
7. Testing and quality assurance  
8. Usability evaluation  
9. Deployment and operations  
10. Project management and contribution  
11. Evaluation, limitations and future work  
12. Conclusion  
References  
Appendices  

---

## List of figures

| ID | Caption | Source |
|----|---------|--------|
| F1 | System context | `docs/architecture/diagrams/source/01-system-context.mmd` |
| F2 | Container diagram | `02-container.mmd` |
| F3 | Component diagram | `03-component.mmd` |
| F4 | Deployment (local) | `04-deployment.mmd` |
| F5 | Use-case overview | `05-use-case.mmd` |
| F6 | ER diagram | `06-er.mmd` / `docs/data/ER_DIAGRAM.md` |
| F7–F17 | Sequence diagrams | `08`–`17-*.mmd` |

Rendered PNG export: **PENDING** (no Mermaid CLI in build environment). Mermaid sources are authoritative.

---

## List of tables

| ID | Caption |
|----|---------|
| T1 | Role stakeholders |
| T2 | Provider status |
| T3 | Final automated test counts (Phase 11 baseline) |
| T4 | Readiness classification |

---

## List of abbreviations

| Abbreviation | Meaning |
|--------------|---------|
| ADR | Architecture Decision Record |
| API | Application Programming Interface |
| EF | Entity Framework |
| ICS | iCalendar |
| IDOR | Insecure Direct Object Reference |
| JWT | JSON Web Token |
| LocalDB | SQL Server Express LocalDB |
| RBAC | Role-Based Access Control |
| SPA | Single-Page Application |
| UAT | User Acceptance Testing |
| WCAG | Web Content Accessibility Guidelines |

---

# Chapter 1 — Introduction

## 1.1 Background

Digital recruitment workflows often fragment across email, spreadsheets, and disconnected applicant-tracking tools. HireSphere addresses this domain for academic Software Architecture coursework by implementing an end-to-end hiring lifecycle with clear role separation and auditable decisions.

## 1.2 Problem statement

Organisations need a coherent system in which Candidates manage profiles and applications, Recruiters run pipelines, Hiring Managers evaluate assigned candidates, and Administrators govern access and final decisions—without exposing private data across roles or organisations, and without treating AI outputs as unsupervised final decisions.

## 1.3 Aim and objectives

**Aim:** Design and implement a secure, maintainable recruitment platform architecture with verified four-role workflows.

**Objectives:**

1. Deliver Candidate, Recruiter, Hiring Manager, and Administrator portals.  
2. Persist core hiring entities in SQL Server via EF Core.  
3. Enforce JWT authentication and RBAC with ownership and organisation checks.  
4. Provide deterministic AI assistance with human-review policy.  
5. Abstract email/SMS/calendar/storage providers with truthful status.  
6. Establish automated tests, UAT evidence, and architecture documentation.

## 1.4 Scope

**In scope:** Local development/demo on LocalDB; modular monolith; four portals; assessments; interviews; final admin decisions; local storage; deterministic AI; development integration adapters.

**Out of scope / not verified:** Production cloud deployment; paid external AI; production SMTP; external SMS gateways; Google/Outlook OAuth calendars; Azure Blob cloud; antivirus scanning; formal penetration testing; genuine multi-participant usability lab (pending).

## 1.5 Stakeholders

| Stakeholder | Interest |
|-------------|----------|
| Candidate | Profile, applications, assessments, interviews |
| Recruiter | Jobs, pipeline, ranking review, scheduling |
| Hiring Manager | Assigned review, feedback, recommendations |
| Administrator | Users, orgs, audit, final decisions |
| Academic assessors | Architecture quality, evidence, honesty |

## 1.6 Assumptions and constraints

Assumptions: developers can run .NET SDK, Node.js, and LocalDB; demo uses test accounts.  
Constraints: coursework schedule; no force-push; preserve Kalani historical authorship; secrets never committed; Phase 10 usability participant gap carried forward.

## 1.7 Report structure

Chapters 2–6 cover requirements through security; Chapters 7–8 testing and usability; Chapters 9–12 operations, contribution, evaluation, and conclusion.

---

# Chapter 2 — Requirements analysis

Functional requirements are traced in `docs/audit/COURSEWORK_REQUIREMENT_MATRIX.md` and `docs/report/COURSEWORK_REQUIREMENTS_CHECKLIST.md`.

## 2.1 Functional themes

- **Candidate:** register, profile, documents, job search, apply, track, assess, interview confirm, notifications.  
- **Recruiter:** jobs, pipeline, screening, ranking review, assessments, messaging, interviews, reports/CSV.  
- **Hiring Manager:** assigned vacancies, compare, feedback, evaluation, recommendation (non-final).  
- **Administrator:** users/roles/orgs, audit, monitoring, analytics, final hire/reject.

## 2.2 Non-functional requirements

Security (JWT, hashing, IDOR resistance), privacy (DTO minimisation), accessibility (axe critical/serious zero on covered routes), responsiveness (Phase 9), performance smoke (local p95 target), maintainability (ADRs, providers).

## 2.3 Prioritisation

Tier M mandatory features were implemented and largely verified across Phases 3–10. Optional bonus items (refresh tokens, Docker CI, predictive analytics) remain deferred.

## 2.4 Traceability approach

Requirement ID → implementation path → automated/manual test → evidence folder → commit SHA. VERIFIED requires all four. PARTIALLY VERIFIED / BLOCKED used when external credentials or participants are missing.

---

# Chapter 3 — Architecture and design

## 3.1 Selected style

**Modular monolith** (ADR-001): one API process with layered controllers → services → EF Core / provider adapters; separate React SPA.

## 3.2 Frontend / backend separation

Browser SPA calls JSON REST under `/api/*`. AuthContext stores JWT; server remains authoritative for authorization.

## 3.3 Dependency direction

UI → API contracts → application services → domain models → EF / providers. Providers implement interfaces (`IEmailProvider`, `IFileStorageProvider`, AI parse/match interfaces).

## 3.4 AuthZ architecture

JWT Bearer + role policies + resource ownership + organisation scoping. Disabled users cannot obtain usable sessions.

## 3.5 Data, notifications, AI, storage

SQL Server schema via `ApplicationDbContext`. Notification outbox (ADR-008). Deterministic AI (ADR-007). Secure storage abstraction (ADR-009).

## 3.6 Deployment architecture

Verified: developer workstation → Vite `:5173` → API `:5167` → LocalDB + local disk. Production cloud topology is documented as a template only (Not Verified).

## 3.7 Trade-offs

Monolith simplicity versus independent scaling; deterministic AI versus model quality; direct-main workflow versus PR gates (ADR-011).

Diagrams: Figures F1–F5 in `docs/architecture/diagrams/source/`.

---

# Chapter 4 — Detailed system design

## 4.1 Domain model

Core aggregates: User/RBAC, CandidateProfile (+ resume/documents), Job, Application (+ history), Assessment*, Interview*, CandidateEvaluation, HiringDecision, ResumeAnalysis, NotificationOutbox, AuditLog.

## 4.2 Lifecycles

- **Job:** Draft → Published (and related operational states as implemented).  
- **Application:** Submitted → screening/shortlist/reject paths with history.  
- **Assessment:** Assigned → attempted → scored (keys not exposed to Candidate UI).  
- **Interview:** Scheduled with conflict detection; Candidate confirm/reschedule paths; ICS download.  
- **Decision:** HM recommendation advisory; Admin/Recruiter final decision with duplicate guards.

## 4.3 Audit and outbox

Sensitive actions write `AuditLog`. Notifications enqueue to `NotificationOutbox` for retry-safe dispatch.

## 4.4 File and AI workflows

Upload validation → storage key → metadata. Resume analysis persists extracted skills for human confirmation. Ranking explanations include human-review notices.

Sequence diagrams: `08`–`17-*.mmd`.

---

# Chapter 5 — Implementation

## 5.1 Technology

| Layer | Choice |
|-------|--------|
| Backend | ASP.NET Core, C#, EF Core, SQL Server |
| Frontend | React 19, Vite, React Router, Axios |
| Tests | xUnit, Vitest, Playwright, axe-core |
| Auth | JWT, BCrypt password hashing |

## 5.2 Portals

Role shells and design tokens from Phase 9 (`docs/ui/DESIGN_SYSTEM.md`). Skip link and single main landmark patterns applied.

## 5.3 Configuration

Connection strings and JWT keys via user secrets / environment / ignored local files—not Git. Seed gated.

## 5.4 Error handling

Global exception handling returns safe client messages; Phase 10 asserts no stack traces in representative bodies.

Code excerpts are intentionally short; see repository controllers under `Backend/HireSphere.API/Controllers`.

---

# Chapter 6 — Security, privacy and ethics

## 6.1 Controls

Password hashing (BCrypt), JWT validation, RBAC, organisation isolation, ownership checks, input validation, CSV formula neutralization, upload validation, secret externalisation, audit logging.

## 6.2 Application security verification

Phase 10 performed **application security verification** (not a formal external penetration test): auth bypass attempts, IDOR matrix, mass-assignment, SQL-like inputs, XSS-oriented inputs at API boundary, path traversal/upload abuse attempts, dependency audit (`docs/security/SECURITY_VERIFICATION.md`, `DEPENDENCY_AUDIT.md`).

## 6.3 AI ethics

Deterministic outputs are advisory; protected-characteristic exclusion intent documented in AI privacy docs; Candidate consent and human review notices required in UX.

## 6.4 Unresolved limitations

No antivirus; no formal pen-test; external IdP not used; refresh tokens not implemented; production secret management beyond coursework templates not verified.

---

# Chapter 7 — Testing and quality assurance

## 7.1 Final baseline counts (Phase 11 start host)

| Suite | Result |
|-------|--------|
| Backend xUnit (Release) | **132 passed** |
| Frontend Vitest | **89 passed** |
| Frontend lint | **PASS** |
| Backend Release build | **PASS** |
| Playwright | **14** (Phase 10 recorded; re-run in Phase 11.3) |
| Axe critical/serious (covered routes) | **0** |
| npm audit / NuGet vulnerable | **0** reported in Phase 10 docs |
| Critical/High defects open | **0** |

## 7.2 Strategies

Unit (domain/auth), integration (LocalDB API), migration verification, API contracts, frontend component/resilience, Playwright role journeys, authorization matrix, performance smoke, four-role UAT documentation.

## 7.3 Defect management

`docs/testing/DEFECT_REGISTER.md` — no open Critical/High after Phase 10.1 CSV escape fix.

---

# Chapter 8 — Usability evaluation

## 8.1 Real participant testing

**Participant count:** 0  
**Status:** PENDING / BLOCKED  

No names, times, quotes, or scores were fabricated. See `docs/usability/USABILITY_RESULTS.md`.

## 8.2 Expert heuristic evaluation

Conducted separately (`docs/usability/HEURISTIC_EVALUATION.md`). **Not** presented as user testing.

## 8.3 Accessibility evaluation

Phase 9 axe + keyboard + semantics review. Not a full WCAG certification audit.

## 8.4 Automated role UAT

Four-role browser UAT PASS with evidence under `docs/evidence/phase10-quality/`.

---

# Chapter 9 — Deployment and operations

## 9.1 Local architecture

LocalDB `HireSphereDev`, API http://127.0.0.1:5167, Vite http://127.0.0.1:5173.

## 9.2 Production template

Documented in `docs/release/*` — **Production Ready = NO**. Health checks endpoint not implemented (`AddHealthChecks` absent). HTTPS expectations documented for production templates only.

## 9.3 Migrations and workers

EF migrations; in-process notification outbox processor. Backup/restore: operator responsibility; not automated in-repo.

## 9.4 Monitoring

Admin monitoring surfaces truthful LocalDB + NotConfigured providers—does not invent uptime SLAs.

---

# Chapter 10 — Project management and contribution

## 10.1 Phased delivery

Phases 0–11 with verification commits on `main`. Test gates before push.

## 10.2 Git workflow

Direct-main for Chinthaka verification track (ADR-011); historical Kalani commits preserved; no amend/rebase of shared history.

## 10.3 Contribution (summary)

Approximate commit authorship from `git shortlog` (counts ≠ effort):

- Chinthaka Jayaweera — later phases (auth completion through Phase 11 documentation)  
- Kalani Rashmika — early candidate and coursework branch work  
- sachini kumudu kumari — early commits  

Detailed matrices: Phase 11.2 `docs/contribution/*`.

## 10.4 AI assistance

Cursor agent assisted implementation and documentation under human direction. Co-authored-by trailers may appear; Cursor is **not** a human group member.

---

# Chapter 11 — Evaluation, limitations and future work

## 11.1 Achievements

Four verified role portals; LocalDB persistence; JWT/RBAC; deterministic AI; provider abstractions; Phase 9 accessibility/responsive baseline; Phase 10 quality suite; architecture ADRs and diagrams.

## 11.2 Limitations

| Area | Limitation |
|------|------------|
| Usability | Real participants pending (blocks full Phase 10 VERIFIED) |
| Providers | External AI/SMTP/SMS/calendars/Blob/AV Not Configured |
| Ops | No production deploy; no health checks endpoint |
| Auth | No refresh-token rotation |
| Testing | Performance is smoke-level only |

## 11.3 Future work

External provider configuration, health checks, CI/CD, Docker, refresh tokens, participant usability study, production hardening.

---

# Chapter 12 — Conclusion

HireSphere demonstrates a coherent modular-monolith architecture for multi-role recruitment with honest integration boundaries and substantial automated and browser verification. The platform is **Development Ready** and **Demo Ready**. Coursework submission readiness remains **PARTIAL** until genuine usability participant evidence (if mandatory) and user-controlled report identity/PDF/video/NLEARN steps are completed. The system is **not** Production Ready based solely on LocalDB coursework evidence.

---

# References

Citation style: **IEEE** (consistent). Only sources reviewed for this report:

[1] Microsoft, “ASP.NET Core documentation,” Microsoft Learn. [Online]. Available: https://learn.microsoft.com/aspnet/core/  
[2] Microsoft, “Entity Framework Core,” Microsoft Learn. [Online]. Available: https://learn.microsoft.com/ef/core/  
[3] Microsoft, “SQL Server Express LocalDB,” Microsoft Learn. [Online]. Available: https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb  
[4] Meta Open Source, “React documentation,” React.dev. [Online]. Available: https://react.dev/  
[5] Vite team, “Vite documentation.” [Online]. Available: https://vitejs.dev/guide/  
[6] OWASP, “OWASP Top Ten,” OWASP Foundation. [Online]. Available: https://owasp.org/www-project-top-ten/  
[7] W3C, “Web Content Accessibility Guidelines (WCAG) 2.2,” W3C Recommendation. [Online]. Available: https://www.w3.org/TR/WCAG22/  
[8] IETF, “JSON Web Token (JWT),” RFC 7519. [Online]. Available: https://datatracker.ietf.org/doc/html/rfc7519  
[9] Playwright, “Playwright documentation.” [Online]. Available: https://playwright.dev/docs/intro  
[10] Deque, “axe-core,” GitHub. [Online]. Available: https://github.com/dequelabs/axe-core  

Access dates for online sources: 2026-07-21.

---

# Appendices

## Appendix A — Requirement matrix

See `docs/report/COURSEWORK_REQUIREMENTS_CHECKLIST.md` and `docs/audit/COURSEWORK_REQUIREMENT_MATRIX.md`.

## Appendix B — Test summary

See `docs/testing/PHASE10_FINAL_TEST_REPORT.md` and Phase 11.2 `FINAL_VERIFICATION_SUMMARY.md`.

## Appendix C — API summary

See `docs/api/API_OVERVIEW.md`.

## Appendix D — Setup guide

See root `README.md` (finalised in Phase 11.2).

## Appendix E — Screenshot index

See `docs/report/SCREENSHOT_INDEX.md`.

## Appendix F — ADRs

See `docs/architecture/adr/ADR-001.md` … `ADR-012.md`.

## Appendix G — Known limitations

See `docs/release/KNOWN_LIMITATIONS.md` and Phase 11.2 `FINAL_KNOWN_LIMITATIONS.md`.

## Appendix H — Demo script

See `docs/demo/` (Phase 11.2).

## Appendix I — Submission checklist

See `docs/submission/SUBMISSION_CHECKLIST.md` (Phase 11.2–11.3).

---

## Unresolved placeholders (must be completed by user)

- STUDENT ID  
- LECTURER / MODULE staff names (if required on title page)  
- Official SUBMISSION DATE  
- Group member details if required beyond Git history  
- PDF export (Pandoc/PDF engine not available in this environment at Phase 11.1)  
- Demo video URL  

Search tokens intentionally left visible: `PENDING USER`.
