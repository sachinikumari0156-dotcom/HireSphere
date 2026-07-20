# Frontend Test Results — Phase 3

**Date:** 2026-07-20
**Stack:** Vitest 3.2.x, React Testing Library, jsdom, user-event
**Commands:** `npm run test:run`, `npm run lint`, `npm run build`

## Scripts

| Script | Purpose |
|--------|---------|
| `npm test` | Vitest watch |
| `npm run test:run` | CI single run |
| `npm run test:coverage` | Coverage run (Vitest) |

## Results

| Suite | Tests | Result |
|-------|-------|--------|
| `src/test/auth.test.jsx` | 13 | PASS |

## Cases

1. Login form renders — PASS
2. Candidate registration validation — PASS
3. Recruiter request validation — PASS
4. Protected route redirects unauthenticated → `/session-expired` — PASS
5. Candidate blocked from Recruiter route — PASS
6. Recruiter blocked from Admin route — PASS
7. Hiring Manager blocked from Candidate route — PASS
8. Administrator can access Admin route — PASS
9. Access Denied page renders — PASS
10. Session Expired state renders — PASS
11. Logout clears authenticated state — PASS
12–13. Current-user restoration loading/success and error/expired — PASS

## Lint / build

- `npm run lint` — PASS (0 errors)
- `npm run build` — PASS

## Notes

- Authorization guards under test use real `ProtectedRoute` + role lists (not a stubbed allow-all).
- Network for `/auth/me` restoration is mocked at the axios module boundary only.
