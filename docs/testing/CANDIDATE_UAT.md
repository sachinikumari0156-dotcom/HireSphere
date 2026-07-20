# Candidate UAT Checklist

## Phase 4.1

| Step | Expected | Status |
|------|----------|--------|
| Login as Candidate | Dashboard loads from `/api/candidate/dashboard` | Automated API + UI wired |
| Open `/candidate/profile` | Profile form loads | UI wired |
| Save profile fields | Persisted via PUT `/api/candidate/profile` | Covered by API tests |
| Add experience/education/skill/cert | Listed on profile | Covered by API tests |
| Upload resume | Metadata without absolute path | Covered by API tests |
| Reject `.exe` / oversized file | 400 | Covered by API tests |

## Phase 4.2

| Step | Expected | Status |
|------|----------|--------|
| Browse `/candidate/jobs` | Open jobs only; filters/pagination work | Automated API + UI wired |
| Open job detail | Match score + explanation shown; provider = Deterministic | Automated API + UI wired |
| Recommendations (incomplete profile) | Empty list + guidance message | Automated API + UI wired |
| Recommendations (complete + skills) | Jobs sorted by highest match | Automated API covered |
| Apply wizard | Resume / cover letter / screening / terms | Automated API + FE Vitest |
| Duplicate apply | Rejected | Automated API |
| Apply to closed job | Rejected | Automated API |
| View another candidate’s application | 404 | Automated API |
| Withdraw Pending application | Status → Withdrawn + history entry | API implemented; manual optional |

Phase 4.3 will extend this checklist for assessments and interviews.
