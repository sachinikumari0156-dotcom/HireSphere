# HireSphere — SRS Traceability Matrix

**Source:** HireSphere SRS (local-spec; not committed)
**Last updated:** 2026-07-20 (Phase 4.1)
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
| Candidate | Profile / resume | M-C02–M-C03 | TESTED | `/api/candidate` + local file storage; UI profile page |
| Candidate | Jobs / applications / tracking | M-C04–M-C06 | IN PROGRESS | Pending Phase 4.2–4.3 |
| Recruiter | Jobs / applications | M-RC01–M-RC06 | IN PROGRESS | API partial; UI placeholder |
| Hiring Manager | Evaluation / decisions | M-HM01–M-HM04 | NOT STARTED | Models in DB; APIs pending |
| Administrator | Users / orgs / analytics | M-A01–M-A05 | IN PROGRESS | UsersController secured; org models seeded |
| Database | SQL Server deliverable | M-B02 | VERIFIED | Applied on SQL Express `HireSphereDev`; migration history confirmed |
| Database | Core entity model | M-B03 | IMPLEMENTED | 35+ entities, fluent configs, ER diagram |
| AI | Matching / ranking / recs | M-AI01–M-AI09 | NOT STARTED | Model stubs only |
| Integrations | Email / SMS / calendar / storage | M-I01–M-I07 | NOT STARTED | Local file storage only; cloud pending |
| Frontend | Branding / API config | M-F03, M-F05 | IN PROGRESS | HireSphere branding; centralized `VITE_API_BASE_URL` |
| Testing | Automated tests | M-T01 | IN PROGRESS | Backend 43; Frontend auth+candidate Vitest |
| Report | Diagrams / contribution / demo | M-D01–M-D05 | IN PROGRESS | ER diagram + data dictionary added |

**Notes**

- Original SRS PDF remains in ignored `local-spec/` and is not committed.
- Phase 4.1 delivers candidate profile/resume/documents; cloud object storage verification remains pending.
- Full SRS coverage continues in Phases 4.2–10.
