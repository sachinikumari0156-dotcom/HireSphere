# HireSphere — SRS Traceability Matrix

**Source:** HireSphere SRS (local-spec; not committed)
**Last updated:** 2026-07-21 (Phase 9 UI / accessibility VERIFIED)
**Legend:** NOT STARTED | IN PROGRESS | IMPLEMENTED | TESTED | VERIFIED | BLOCKED

| SRS Area | Requirement summary | Matrix ID | Status | Phase evidence |
|----------|---------------------|-----------|--------|----------------|
| Auth | Secure registration / login | M-C01, M-S01, M-S03 | VERIFIED | Live UAT + automated tests 2026-07-20 |
| Auth | JWT-based API auth | M-S01 | VERIFIED | Issuer/audience/lifetime + live login/me |
| Auth | Privileged role control | M-S02 | VERIFIED | Public role ignored; admin assignment only |
| Security | Secret handling | M-S08 | IMPLEMENTED | Tracked secrets replaced with placeholders |
| Security | CORS restriction | M-S08 / quality | IMPLEMENTED | Configured `Cors:AllowedOrigins` |
| Security | Error sanitization | Q-04 (partial) | IMPLEMENTED | Global exception handler returns safe JSON |
| Security | Password not exposed in API | M-S06 | TESTED | Candidate portal DTOs omit hashes; tests assert |
| Candidate | Profile / resume | M-C02–M-C03 | VERIFIED | Browser E2E + local storage; Azure Blob NotConfigured |
| Candidate | Jobs / applications / tracking | M-C04–M-C06 | VERIFIED | Playwright journey + duplicate/timeline evidence |
| Candidate | Assessments / interviews | M-R01 (partial) | VERIFIED (candidate) | Candidate E2E; recruiter assign/schedule VERIFIED in Phase 5 |
| Recruiter | Jobs / applications | M-RC01–M-RC06 | VERIFIED | Phase 5 Playwright + 26 screenshots; org scoping |
| Hiring Manager | Evaluation / decisions | M-HM01–M-HM04 | VERIFIED | Phase 6 Playwright + 21 screenshots; assignment authz |
| Administrator | Users / orgs / analytics | M-A01–M-A05 | VERIFIED | Phase 7 Admin portal + E2E 2026-07-20 |
| Database | SQL Server deliverable | M-B02 | VERIFIED | Applied on LocalDB `HireSphereDev`; migration history confirmed |
| Database | Core entity model | M-B03 | IMPLEMENTED | 35+ entities, fluent configs, ER diagram; Phase 5 recruiter fields |
| AI | Matching / ranking / recs | M-AI01–M-AI09 | IMPLEMENTED — EXTERNAL PENDING | Deterministic AI verified Phase 8; External AI NotConfigured |
| Integrations | Email / SMS / calendar / storage | M-I01–M-I07 | IMPLEMENTED — EXTERNAL PENDING | Outbox + ICS + local storage verified; production/cloud NotConfigured |
| Frontend | Branding / responsive / a11y | M-F03, M-F05 | VERIFIED | Phase 9 design system + responsive + axe; evidence phase9-ui |
| Testing | Automated tests | M-T01 | IN PROGRESS | BE 114; FE Vitest 84; Playwright 13/13; formal UAT Phase 10 |
| Report | Diagrams / contribution / demo | M-D01–M-D05 | IN PROGRESS | ER diagram + data dictionary + Phase 4–9 evidence |

**Notes**

- Original SRS PDF remains in ignored `local-spec/` and is not committed.
- Phase 4 Candidate Portal is **VERIFIED** with Playwright browser E2E evidence (2026-07-20).
- Phase 8 development adapters are verified; external cloud providers remain NotConfigured.
- Phase 9 UI/accessibility is **VERIFIED**; formal usability study remains Phase 10.
- Full SRS coverage continues in Phases 10–11.
