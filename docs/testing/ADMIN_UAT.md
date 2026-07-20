# Administrator Portal — UAT

**Date:** 2026-07-20  
**Environment:** LocalDB `HireSphereDev` · API `http://127.0.0.1:5167` · UI `http://127.0.0.1:5173`

| # | Scenario | Expected | Actual | Status | Evidence |
|---|----------|----------|--------|--------|----------|
| 1 | Admin login + dashboard | Live metrics | Live counts ≥ expected | PASS | admin-dashboard.png |
| 2 | User list/filter | Search works | PASS | PASS | admin-user-list.png |
| 3 | Self-disable | Blocked | Blocked message | PASS | admin-self-disable-blocked.png |
| 4 | Last Admin protection | Role removal blocked | API 400 | PASS | admin-last-admin-protection.png |
| 5 | Recruiter approve/reject | Audit + notification | PASS | PASS | admin-recruiter-*.png |
| 6 | Org/dept governance | Unique code; archive rules | PASS | PASS | admin-organization-*.png |
| 7 | Audit + monitoring | Truthful; NotConfigured providers | PASS | PASS | admin-monitoring.png |
| 8 | Analytics | Scoped LocalDB data | PASS | PASS | admin-recruitment-analytics.png |
| 9 | FinalHire | Status + audit + notification | PASS | PASS | admin-final-decision-success.png |
| 10 | Duplicate final | Blocked | PASS | PASS | admin-duplicate-decision-blocked.png |
| 11 | Non-admin /admin | Access denied | PASS | PASS | admin-access-denied.png |
| 12 | Responsive mobile | Dashboard usable | PASS | PASS | admin-mobile-dashboard.png |

Phases 8–11 remain pending (AI, external providers, design system polish, submission pack).
