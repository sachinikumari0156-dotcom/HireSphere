# Candidate UAT / Test Evidence (Phase 4.1)

**Date:** 2026-07-20  
**Backend tests:** `CandidatePortalControllerTests` included in `dotnet test` — suite **43 passed**  
**Frontend tests:** Vitest candidate suite **3** + auth **13** = **16 passed**  
**SQL:** Migration `AddCandidateProfilePortalFields` applied to `HireSphereDev`

## Automated coverage (backend)

- Own-profile access
- Cross-candidate access blocked
- Experience date validation
- Duplicate skills blocked
- Unsafe / unsupported / oversized file rejected
- Resume metadata has no absolute path
- Password hash not serialized
- Recruiter cannot access candidate portal

## Automated coverage (frontend)

- Dashboard empty state from API summary
- Dashboard error state
- Profile page loads desired job title from API

## Manual / live

Full candidate end-to-end (jobs → apply → assessment → interview) is **not claimed** until Phases 4.2–4.3 complete.

## Screenshots

None captured for 4.1. Do not invent screenshot evidence.
