# Candidate UAT / Test Evidence

**Date:** 2026-07-20  
**Phases covered:** 4.1 + 4.2 (automated).

## Automated results (this session)

| Suite | Result |
|-------|--------|
| Backend `dotnet test` | **52 passed** / 52 (0 errors, 0 warnings on build) |
| Frontend Vitest `npm run test:run` | **19 passed** / 19 (auth 13 + candidate 6) |
| Frontend `npm run lint` | PASS |
| Frontend `npm run build` | PASS |
| `git diff --check` | PASS (after trailing-whitespace trim) |

## Backend coverage (Phase 4.2) — `CandidateJobsControllerTests`

- Job search/filter/pagination
- Active-only job visibility (closed/draft hidden)
- Deterministic match score (matched/missing skills, provider = Deterministic)
- Recommendation ordering by match score
- Incomplete-profile recommendations empty handling
- Successful application with screening answers + status history
- Duplicate application rejection
- Closed-job application rejection
- Cross-candidate application access blocked
- Unit-level deterministic skill-overlap scoring

## Frontend coverage (Phase 4.2)

- Jobs list from API
- Recommendations incomplete-profile message
- Application wizard submit payload (terms + screening answers)

## Manual / live

- Full browser screenshot pack: **not captured**
- Migration `AddApplicationResumeId`: **applied** to HireSphereDev on 2026-07-20

## Screenshots

None captured for 4.2. Do not invent screenshot evidence.
