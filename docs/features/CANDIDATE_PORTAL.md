# Candidate Portal

**Phase:** 4.1 (profile) + 4.2 (jobs/applications) + 4.3 (assessments/interviews/tracking/notifications)
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

### Phase 4.3
- Assigned skill assessments: list/detail, start, answer, submit, server-scored results when reveal allowed
- Attempt limits, start/expiry windows, ownership checks, audit logs; answer keys never exposed to candidates
- Interviews: list/detail with timezone, confirm / reschedule-request / decline; meeting info gated until authorized
- Application tracking: ordered status timeline, latest update, next action, linked interviews/assessments
- In-app notifications foundation (application submitted/status, assessment assigned, interview scheduled/updated)
- Dashboard nav wired to assessments, interviews, notifications

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
| `/candidate/applications/:id` | Application detail + tracking timeline |
| `/candidate/assessments` | Assigned assessments |
| `/candidate/assessments/:id` | Take / view assessment |
| `/candidate/interviews` | Scheduled interviews |
| `/candidate/interviews/:id` | Interview detail / respond |
| `/candidate/notifications` | In-app notification inbox |

## Not claimed as complete

- Recruiter-side assign/schedule UIs (Phase 5) — E2E uses Development-only `/api/e2e/*` seed helpers when enabled
- External email/SMS/calendar providers (Phase 8+)
- External/cloud AI matching (deterministic only)
- Cloud object storage (local abstraction in use)

## Phase 4 verification

Phase 4 is **VERIFIED** after Playwright browser E2E (2026-07-20): see `docs/testing/CANDIDATE_E2E_RESULTS.md` and `docs/evidence/phase4-candidate/`.
