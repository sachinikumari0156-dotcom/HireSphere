# HireSphere — SRS Traceability Matrix

**Source:** HireSphere SRS (local-spec; not committed)
**Last updated:** 2026-07-20 (Phase 2)
**Legend:** NOT STARTED | IN PROGRESS | IMPLEMENTED | TESTED | VERIFIED | BLOCKED

| SRS Area | Requirement summary | Matrix ID | Status | Phase evidence |
|----------|---------------------|-----------|--------|----------------|
| Auth | Secure registration / login | M-C01, M-S01, M-S03 | TESTED | Candidate register/login; BCrypt; terms; sanitized failures |
| Auth | JWT-based API auth | M-S01 | TESTED | TokenService + issuer/audience/lifetime validation |
| Auth | Privileged role control | M-S02 | TESTED | No public role field; admin-only privileged assignment |
| Security | Secret handling | M-S08 | IMPLEMENTED | Tracked secrets replaced with placeholders |
| Security | CORS restriction | M-S08 / quality | IMPLEMENTED | Configured `Cors:AllowedOrigins` |
| Security | Error sanitization | Q-04 (partial) | IMPLEMENTED | Global exception handler returns safe JSON |
| Security | Password not exposed in API | M-S06 | IN PROGRESS | UserDto + UsersController projections; tests |
| Candidate | Profile / resume / jobs | M-C02–M-C06 | IN PROGRESS | Pre-existing partial APIs/UI |
| Recruiter | Jobs / applications | M-RC01–M-RC06 | IN PROGRESS | API partial; UI placeholder |
| Hiring Manager | Evaluation / decisions | M-HM01–M-HM04 | NOT STARTED | Models in DB; APIs pending |
| Administrator | Users / orgs / analytics | M-A01–M-A05 | IN PROGRESS | UsersController secured; org models seeded |
| Database | SQL Server deliverable | M-B02 | VERIFIED | Applied on SQL Express `HireSphereDev`; migration history confirmed |
| Database | Core entity model | M-B03 | IMPLEMENTED | 35+ entities, fluent configs, ER diagram |
| AI | Matching / ranking / recs | M-AI01–M-AI09 | NOT STARTED | Model stubs only |
| Integrations | Email / SMS / calendar / storage | M-I01–M-I07 | NOT STARTED | Credential-dependent |
| Frontend | Branding / API config | M-F03, M-F05 | IN PROGRESS | HireSphere branding; centralized `VITE_API_BASE_URL` |
| Testing | Automated tests | M-T01 | IN PROGRESS | 14 backend tests passing |
| Report | Diagrams / contribution / demo | M-D01–M-D05 | IN PROGRESS | ER diagram + data dictionary added |

**Notes**

- Original SRS PDF remains in ignored `local-spec/` and is not committed.
- Phase 1 addresses security baseline; Phase 2 addresses database architecture and constraint tests.
- Full SRS coverage continues in Phases 3–10.
