# Coursework requirements checklist

**Module:** SE205.3 Software Architecture (2026)  
**Project:** HireSphere  
**Last updated:** 2026-07-21  
**Author of this checklist:** Chinthaka Jayaweera  

Statuses: VERIFIED | IMPLEMENTED | PARTIALLY VERIFIED | BLOCKED | NOT IMPLEMENTED | NOT APPLICABLE  

Every VERIFIED row cites implementation, test, evidence, and commit references from the repository.

| ID | Description | Implementation | Architecture | Test | Evidence | Commit | Status | Remaining gap |
|----|-------------|----------------|--------------|------|----------|--------|--------|---------------|
| M-R01 | Candidate Portal | `Frontend/src/pages/candidate/*` | Ch.3–5 report | Playwright candidate | `docs/evidence/phase4-candidate/` | Phase 4 verify | VERIFIED | — |
| M-R02 | Recruiter Portal | `Frontend/src/pages/recruiter/*` | Ch.3–5 | Playwright recruiter | `phase5-recruiter/` | Phase 5 verify | VERIFIED | — |
| M-R03 | Hiring Manager | `Frontend/src/pages/hiring-manager/*` | Ch.3–5 | Playwright HM | `phase6-hiring-manager/` | `0fb9e82` | VERIFIED | — |
| M-R04 | Administrator | `Frontend/src/pages/admin/*` | Ch.3–5 | Playwright admin | `phase7-admin/` | `1727b7c` | VERIFIED | — |
| M-C01–C06 | Candidate features | Candidate API + UI | Ch.4–5 | E2E + API | phase4 + phase8 | Phase 4/8 | VERIFIED | External AI N/C |
| M-RC01–RC06 | Recruiter features | Recruiter API + UI | Ch.4–5 | E2E | phase5 + phase8 | Phase 5/8 | VERIFIED | Google/Outlook N/C |
| M-HM01–HM04 | HM features | HM API + UI | Ch.4–5 | E2E | phase6 | Phase 6 | VERIFIED | — |
| M-A01–A05 | Admin features | Admin API + UI | Ch.4–5 | E2E | phase7 | Phase 7 | VERIFIED | — |
| M-B01 | ASP.NET Core API | `Backend/HireSphere.API` | ADR-003 | Release build | — | continuous | VERIFIED | — |
| M-B02 | SQL Server | LocalDB HireSphereDev | ADR-004/010 | migrations + tests | phase10 migration | Phase 2/10 | VERIFIED | Cloud SQL N/C |
| M-B03 | Core entities | `ApplicationDbContext` | DB docs | snapshot | ER diagram | Phase 2+ | VERIFIED | — |
| M-B04 | REST APIs | Controllers listed in API overview | API docs | contract + E2E | Swagger | Phases 3–8 | VERIFIED | — |
| M-B05 | Swagger | `Program.cs` | API overview | Phase10 Swagger test | — | Phase 10 | VERIFIED | — |
| M-S01–S08 | Security | JWT RBAC BCrypt ownership | Ch.6 + security docs | Phase10 quality | `SECURITY_VERIFICATION.md` | `fe3c0cc` | VERIFIED | Formal pen-test N/A |
| M-AI01–AI09 | AI features | Deterministic providers | ADR-007 + AI docs | Phase8 E2E | `phase8-platform/` | `d94e08e` `ed74aed` | PARTIALLY VERIFIED | External AI Not Configured; some analytics limited |
| M-I01–I07 | Integrations/storage | Provider abstractions | ADR-006/008/009 | Phase8/10 | phase8 providers | `3a5ef00` `472df59` | PARTIALLY VERIFIED | Prod SMTP, external SMS, Google, Outlook, Azure Blob, AV Not Configured |
| M-F01–F06 | Frontend quality | Design system Phase 9 | UI docs | axe/responsive/visual | `phase9-ui/` | `e4f1eb3` | VERIFIED | SR certification not claimed |
| M-F07 | Usability testing | Plan + heuristic | Ch.8 | — | `USABILITY_RESULTS.md` | `6cddb19` | BLOCKED | Real participants = 0 PENDING |
| M-T01 | Automated tests | xUnit + Vitest | Ch.7 | 132 + 89 | phase10 | `fe3c0cc` | VERIFIED | — |
| M-T02 | Integration tests | API LocalDB tests | Ch.7 | Phase10 | `INTEGRATION_TEST_RESULTS.md` | `fe3c0cc` | VERIFIED | — |
| M-T03 | UAT scenarios | Four-role UAT docs | Ch.7–8 | Playwright UAT | `phase10-quality/` | `6cddb19` | VERIFIED | Participant usability separate |
| M-T04 | Postman/Swagger | collection + Swagger | API docs | Phase10 | postman/ | `fe3c0cc` | VERIFIED | — |
| M-D01 | Architecture diagrams | Mermaid sources | diagrams/ | — | source/*.mmd | Phase 11.1 | IMPLEMENTED | Rendered PNG PENDING (no mmdc) |
| M-D02 | ADRs / patterns | `docs/architecture/adr/` | ADR-001–012 | — | adr/*.md | Phase 11.1 | VERIFIED | — |
| M-D03 | Evidence pack | `docs/evidence/*` | index | — | phases 4–10 | continuous | VERIFIED | Master index in 11.2 |
| M-D04 | Contribution | Git history | Ch.10 | — | contribution docs | Phase 11.2 | IMPLEMENTED | Packaged in 11.2 |
| M-D05 | Demo + checklist | demo + submission | Ch.9–app | — | demo docs | Phase 11.2–11.3 | IMPLEMENTED | Video/NLEARN PENDING USER |

## Provider truth table (do not invert)

| Provider | Status |
|----------|--------|
| Deterministic AI | Verified |
| External AI | Not Configured |
| Development SMTP / MailHog path | Verified where exercised |
| Production SMTP | Not Configured |
| Development SMS mock | Verified |
| External SMS | Not Configured |
| Internal calendar + ICS | Verified |
| Google Calendar | Not Configured |
| Outlook Calendar | Not Configured |
| Local / Azurite-style storage | Verified (local) |
| Azure Blob cloud | Not Configured |
| Antivirus | Not Configured |
