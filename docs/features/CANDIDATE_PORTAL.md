# Candidate Portal

**Phase:** 4.1 (profile, resume, documents) + 4.2 (jobs, recommendations, applications)
**Updated:** 2026-07-20

## Delivered

### Phase 4.1
- Backend-driven candidate dashboard summary (`/candidate`)
- Profile CRUD with professional fields, links, salary/availability
- Work experience, education, skills, certifications CRUD
- Secure local resume/document upload with validation
- Ownership enforced via authenticated candidate identity

### Phase 4.2
- Open-job discovery with keyword/location/department/employment/work-arrangement/skill filters, pagination, sorting
- Deterministic candidate–job matching (skills, experience, education, location, work arrangement) with explanation + human-review notice
- Recommendations ranked by highest match; incomplete profiles return empty guidance (no fake metrics)
- Application wizard: resume selection, cover letter, screening answers, terms confirmation
- Duplicate apply blocked; closed/inactive jobs blocked; status history on submit; withdraw when Pending/UnderReview
- My applications list + detail

## UI routes

| Route | Purpose |
|-------|---------|
| `/candidate` | Dashboard summary |
| `/candidate/profile` | Profile & documents |
| `/candidate/jobs` | Job list / search / filters |
| `/candidate/jobs/:id` | Job detail + match explanation |
| `/candidate/jobs/:id/apply` | Application wizard |
| `/candidate/recommendations` | Recommended jobs |
| `/candidate/applications` | My applications |
| `/candidate/applications/:id` | Application detail |

## Not in 4.2

- Assessments / interviews / richer tracking timeline (Phase 4.3)
- External/cloud AI matching providers (deterministic only; Phase 8)
- Cloud object storage (local provider only; verification pending)
