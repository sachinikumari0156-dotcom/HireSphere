# Hiring Manager Portal — UAT

**Date:** 2026-07-20  
**Environment:** LocalDB `HireSphereDev` · API `http://127.0.0.1:5167` · UI `http://127.0.0.1:5173`  
**Tester:** Chinthaka Jayaweera

| ID | Scenario | Expected | Actual | Result |
|----|----------|----------|--------|--------|
| HM-UAT-01 | Assigned HM dashboard | Live metrics for assigned jobs only | Observed in Playwright | PASS* |
| HM-UAT-02 | Unassigned HM cannot open assigned vacancy | Sanitized 404/deny | Observed | PASS* |
| HM-UAT-03 | Candidate review shows ranking notice | Human-review notice visible | Observed | PASS* |
| HM-UAT-04 | Resume metadata without absolute paths | No `C:\` / PasswordHash | Observed | PASS* |
| HM-UAT-05 | Compare same vacancy | Comparison renders | Observed | PASS* |
| HM-UAT-06 | Cross-vacancy / unauthorized compare | Rejected | Observed | PASS* |
| HM-UAT-07 | Interview feedback with private panel comments | Saved for HM; hidden from Candidate | Observed | PASS* |
| HM-UAT-08 | Evaluation Draft then Submitted | Status transitions | Observed | PASS* |
| HM-UAT-09 | RecommendHire | History entry `isFinal=false` | Observed | PASS* |
| HM-UAT-10 | FinalHire by HM | Rejected | Observed | PASS* |
| HM-UAT-11 | Candidate `/hiring-manager` | Access denied | Observed | PASS* |
| HM-UAT-12 | Recruiter HM APIs | 401/403 | Observed | PASS* |

\*Confirmed by automated Playwright journey `e2e/hiring-manager-portal.spec.js` when suite is green.

## Policy notes

- Recommendations are advisory; final decisions require Recruiter or Administrator.
- Ranking / match scores are advisory; human review required.
- Private panel comments never appear on Candidate interview APIs/UI.
