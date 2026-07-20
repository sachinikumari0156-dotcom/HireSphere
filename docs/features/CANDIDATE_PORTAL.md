# Candidate Portal

**Phase:** 4.1 (profile, resume, documents)  
**Updated:** 2026-07-20

## Delivered

- Backend-driven candidate dashboard summary (`/candidate`)
- Profile CRUD with professional fields, links, salary/availability
- Work experience, education, skills, certifications CRUD
- Secure local resume/document upload with validation
- Ownership enforced via authenticated candidate identity

## UI routes

| Route | Purpose |
|-------|---------|
| `/candidate` | Dashboard summary |
| `/candidate/profile` | Profile & documents |

## Not in 4.1

- Job discovery / recommendations / applications (Phase 4.2)
- Assessments / interviews / tracking (Phase 4.3)
- Cloud object storage (local provider only; verification pending)
