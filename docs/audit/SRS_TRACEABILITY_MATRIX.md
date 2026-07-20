# HireSphere — SRS Traceability Matrix

**Source:** HireSphere SRS (local-spec; not committed)
**Last updated:** 2026-07-20 (Phase 4 browser E2E VERIFIED)
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
| Candidate | Profile / resume | M-C02–M-C03 | VERIFIED | Browser E2E + local storage; cloud Phase 8 |
| Candidate | Jobs / applications / tracking | M-C04–M-C06 | VERIFIED | Playwright journey + duplicate/timeline evidence |
| Candidate | Assessments / interviews | M-R01 (partial) | VERIFIED (candidate) | Candidate E2E; recruiter schedule/assign still Phase 5 |
| Recruiter | Jobs / applications | M-RC01–M-RC06 | IN PROGRESS | API partial; UI placeholder; candidate interview respond done |
| Hiring Manager | Evaluation / decisions | M-HM01–M-HM04 | NOT STARTED | Models in DB; APIs pending |
| Administrator | Users / orgs / analytics | M-A01–M-A05 | IN PROGRESS | UsersController secured; org models seeded |
| Database | SQL Server deliverable | M-B02 | VERIFIED | Applied on SQL Express `HireSphereDev`; migration history confirmed |
| Database | Core entity model | M-B03 | IMPLEMENTED | 35+ entities, fluent configs, ER diagram; 4.3 assignment/answer fields |
| AI | Matching / ranking / recs | M-AI01–M-AI09 | IN PROGRESS | Deterministic matching + recommendations in 4.2; external AI / ranking pending Phase 8 |
| Integrations | Email / SMS / calendar / storage | M-I01–M-I07 | IN PROGRESS | In-app notifications foundation; email/SMS/calendar/cloud still pending |
| Frontend | Branding / API config | M-F03, M-F05 | IN PROGRESS | HireSphere branding; centralized `VITE_API_BASE_URL` |
| Testing | Automated tests | M-T01 | IN PROGRESS | BE 58; FE Vitest 22; live E2E screenshot pack pending |
| Report | Diagrams / contribution / demo | M-D01–M-D05 | IN PROGRESS | ER diagram + data dictionary added |

**Notes**

- Original SRS PDF remains in ignored `local-spec/` and is not committed.
- Phase 4 Candidate Portal is **VERIFIED** with Playwright browser E2E evidence (2026-07-20).
- External AI, email/SMS, and calendar credentials remain deferred.
- Full SRS coverage continues in Phases 5–10.
