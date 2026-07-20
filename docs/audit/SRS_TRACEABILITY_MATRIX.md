# HireSphere — SRS Traceability Matrix

**Source:** HireSphere SRS (local-spec; not committed)  
**Last updated:** 2026-07-20 (Phase 1)  
**Legend:** NOT STARTED | IN PROGRESS | IMPLEMENTED | TESTED | VERIFIED | BLOCKED

| SRS Area | Requirement summary | Matrix ID | Status | Phase evidence |
|----------|---------------------|-----------|--------|----------------|
| Auth | Secure registration / login | M-C01, M-S01, M-S03 | IN PROGRESS | Phase 1: BCrypt hashing; Candidate-only public register |
| Auth | JWT-based API auth | M-S01 | IN PROGRESS | JWT still issued; secrets externalized |
| Auth | Privileged role control | M-S02 | IN PROGRESS | Public Admin/Recruiter/HM registration blocked |
| Security | Secret handling | M-S08 | IMPLEMENTED | Tracked secrets replaced with placeholders |
| Security | CORS restriction | M-S08 / quality | IMPLEMENTED | Configured `Cors:AllowedOrigins` |
| Security | Error sanitization | Q-04 (partial) | IMPLEMENTED | Global exception handler returns safe JSON |
| Candidate | Profile / resume / jobs | M-C02–M-C06 | IN PROGRESS | Pre-existing partial APIs/UI |
| Recruiter | Jobs / applications | M-RC01–M-RC06 | IN PROGRESS | API partial; UI placeholder |
| Hiring Manager | Evaluation / decisions | M-HM01–M-HM04 | NOT STARTED | — |
| Administrator | Users / orgs / analytics | M-A01–M-A05 | NOT STARTED | — |
| Database | SQL Server deliverable | M-B02 | NOT STARTED | Still MySQL (Phase 2) |
| AI | Matching / ranking / recs | M-AI01–M-AI09 | NOT STARTED | — |
| Integrations | Email / SMS / calendar / storage | M-I01–M-I07 | NOT STARTED | Credential-dependent |
| Frontend | Branding / API config | M-F03, M-F05 | IN PROGRESS | HireSphere branding; centralized `VITE_API_BASE_URL` |
| Testing | Automated tests | M-T01–M-T04 | NOT STARTED | No backend test project yet |
| Report | Diagrams / contribution / demo | M-D01–M-D05 | NOT STARTED | — |

**Notes**

- Original SRS PDF remains in ignored `local-spec/` and is not committed.
- Phase 1 addresses security baseline rows only; full SRS coverage continues in later phases.
