# HireSphere — Recruiter Portal UAT

**Date:** 2026-07-20  
**Tester:** Chinthaka Jayaweera  
**Build:** `main` after Phase 5.1–5.3 + browser verification  
**DB:** `(localdb)\MSSQLLocalDB` / `HireSphereDev`  
**API:** `http://127.0.0.1:5167`  
**UI:** `http://127.0.0.1:5173`

| ID | Scenario | Expected | Actual | Result | Evidence |
|----|----------|----------|--------|--------|----------|
| R-UAT-01 | Recruiter access request page | Form available | Observed | PASS | `recruiter-request.png` |
| R-UAT-02 | Admin can open admin area | Admin home | Observed | PASS | `admin-recruiter-approval.png` |
| R-UAT-03 | Recruiter dashboard live data | Metrics from SQL (zeros OK) | Observed | PASS | `recruiter-dashboard.png` |
| R-UAT-04 | Create draft job | Draft saved | Observed | PASS | `recruiter-create-job.png` |
| R-UAT-05 | Skills + screening questions | Persist on job | Observed | PASS | `recruiter-job-skills.png`, `recruiter-screening-questions.png` |
| R-UAT-06 | Publish job | Public/candidate visible | Observed | PASS | `recruiter-published-job.png` |
| R-UAT-07 | Applicant pipeline | Search/filter/actions | Observed | PASS | `recruiter-applicant-pipeline.png` |
| R-UAT-08 | Application detail | Authorized fields only | Observed | PASS | `recruiter-application-detail.png` |
| R-UAT-09 | Ranking explanation | Score + human-review notice | Observed | PASS | `recruiter-ranking-explanation.png` |
| R-UAT-10 | Screening queue | Decision confirmation | Observed | PASS | `recruiter-screening.png` |
| R-UAT-11 | Assessment builder | Answer keys recruiter-only | Observed | PASS | `recruiter-assessment-builder.png` |
| R-UAT-12 | Assign + complete assessment | Candidate UI no keys; score server-side | Observed | PASS | `recruiter-assessment-assignment.png`, `recruiter-assessment-result.png` |
| R-UAT-13 | Messages | In-app thread | Observed | PASS | `recruiter-message-thread.png` |
| R-UAT-14 | Schedule interview | UTC + timezone metadata | Observed | PASS | `recruiter-interview-schedule.png`, `recruiter-interview-status.png` |
| R-UAT-15 | Conflict detection | Warning before force | Observed | PASS | `recruiter-conflict-warning.png` |
| R-UAT-16 | Reports + CSV | Org scoped | Observed | PASS | `recruiter-reports.png`, `recruiter-csv-export.png` |
| R-UAT-17 | Cross-org blocked | 404 | Observed | PASS | E2E API |
| R-UAT-18 | Candidate denied recruiter | Access denied | Observed | PASS | `recruiter-access-denied.png` |
| R-UAT-19 | Mobile dashboard | Usable at 390×844 | Observed | PASS | `recruiter-mobile-dashboard.png` |

## Blocked / deferred (not FAIL)

| Item | Status |
|------|--------|
| Google / Outlook calendar sync | Not Configured — Phase 8 |
| External email / SMS | Pending Phase 8 |
| External LLM ranking | Deterministic local ranking only; see `docs/ai/RANKING_MODEL.md` |
