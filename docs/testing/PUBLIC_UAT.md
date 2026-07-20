# Public / authentication UAT

**Date:** 2026-07-21  
**Evidence:** `docs/evidence/phase10-quality/public-uat-summary.png` + prior auth E2E

| # | Case | Result |
|---|------|--------|
| 1 | Landing page | PASS |
| 2 | Candidate registration | PASS (Playwright / Vitest) |
| 3 | Registration validation | PASS |
| 4 | Duplicate email | PASS |
| 5 | Login | PASS |
| 6 | Invalid login | PASS |
| 7 | Session restoration | PASS |
| 8 | Expired / invalid token | PASS (API) |
| 9 | Access denied | PASS |
| 10 | Session expired page | PASS |
| 11 | Logout | PASS |
| 12 | Disabled user | PASS (Phase10QualityTests) |
| 13 | Recruiter request submission | PASS (prior admin/recruiter E2E) |
