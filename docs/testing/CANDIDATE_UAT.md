# Candidate UAT Checklist

**Phase 4.1 scope only**

| Step | Expected | Status |
|------|----------|--------|
| Login as Candidate | Dashboard loads from `/api/candidate/dashboard` | Automated API + UI wired |
| Open `/candidate/profile` | Profile form loads | UI wired |
| Save profile fields | Persisted via PUT `/api/candidate/profile` | Covered by API tests |
| Add experience/education/skill/cert | Listed on profile | Covered by API tests |
| Upload resume | Metadata without absolute path | Covered by API tests |
| Reject `.exe` / oversized file | 400 | Covered by API tests |

Phases 4.2–4.3 extend this checklist for jobs, applications, assessments, and interviews.
