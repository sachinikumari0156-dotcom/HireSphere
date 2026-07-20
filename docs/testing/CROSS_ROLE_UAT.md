# Cross-role UAT

**Date:** 2026-07-21  
**Evidence:** `cross-role-denial.png`, portal Forbidden API tests, `candidate-authz.spec.js`

| Case | Result |
|------|--------|
| Candidate denied Recruiter/HM/Admin UI | PASS |
| Recruiter denied Admin | PASS |
| HM denied Admin final decisions | PASS |
| Cross-organization access denied | PASS |
| Document / application ownership | PASS |
| Disabled user denied | PASS |
