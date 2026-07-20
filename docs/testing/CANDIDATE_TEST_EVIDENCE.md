# Candidate UAT / Test Evidence

**Date:** 2026-07-20
**Phases covered:** 4.1 + 4.2 + 4.3 (automated). Phase 4 **not VERIFIED** (no full browser E2E / screenshot pack).

## Automated results (this session)

| Suite | Result |
|-------|--------|
| Backend `dotnet test` | **58 passed** / 58 |
| Frontend Vitest `npm run test:run` | **22 passed** / 22 (auth 13 + candidate 9) |
| Frontend `npm run lint` | PASS |
| Frontend `npm run build` | PASS |
| `git diff --check` | PASS |

## Backend coverage (Phase 4.3) — `CandidatePhase43ControllerTests`

- Assigned assessment accessible; unassigned/other candidate → 404
- Start blocked when expired; attempt limit enforced after submit
- Submit calculates score; `correctAnswerKey` not present in payloads
- Interview ownership blocked cross-candidate
- Confirm exposes meeting link; reschedule request with reason
- Application status timeline ordered; `nextAction` present
- In-app notification created on application submit; mark-read path

## Frontend coverage (Phase 4.3)

- Assessments empty state
- Interviews list from API
- Notifications unread count + item

## Manual / live

- Full browser screenshot pack: **not captured**
- Migration `AddPhase43AssessmentsInterviewsNotifications`: **applied** to HireSphereDev on 2026-07-20
- Recruiter assign/schedule UI: **not implemented** (candidate APIs tested via seeded data)

## Screenshots

None captured for 4.3. Do not invent screenshot evidence.
