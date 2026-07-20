# HireSphere — Implementation Changelog

## Phase 0 — 2026-07-20

**Commit:** `07080b12733b9af5d07b1e5cb90d5b55a588bdd4`  
`chore(audit): document baseline gaps and coursework plan`

- Baseline audit, requirement matrix, implementation plan, DoD, risk register, phase status, secret-rotation doc
- `.gitignore` updated for `local-spec/` and local secrets

## Phase 1 — 2026-07-20

**Commit message:** `fix(security): secure credentials authentication and API configuration`

- Removed hard-coded DB password and JWT key from tracked config (placeholders only)
- Added CORS allowed-origins configuration
- Implemented BCrypt password hashing and verification
- Blocked public registration of privileged roles (Candidate only)
- Added global exception handler with sanitized API errors
- Centralized frontend API base URL via `VITE_API_BASE_URL`
- Replaced Hireflow branding with HireSphere on auth pages
- Aligned CodeGeneration package version; added BCrypt package
- Updated matrices, risk register, phase status, SRS traceability
- Backend build: PASS (0 warnings)
- Backend tests: N/A — no test project
- Frontend lint/build: PASS; test script missing
