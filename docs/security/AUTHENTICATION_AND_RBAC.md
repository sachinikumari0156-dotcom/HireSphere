# Authentication and RBAC

**Last updated:** 2026-07-20 (Phase 3)

## Architecture

Service-based authentication (controllers stay thin):

| Service | Responsibility |
|---------|----------------|
| `AuthService` | Candidate registration, login, me, change-password, logout audit, recruiter request submit |
| `TokenService` | JWT access-token creation with role, permission, org, and department claims |
| `PasswordService` | BCrypt hash/verify + password policy |
| `CurrentUserService` | Reads authenticated identity from JWT claims |
| `AdminUserService` | Recruiter request review, user status/role/org assignment |
| `ResourceAuthorizationService` | Ownership and organization scoping helpers |

Administrator role name in data/JWT: **`Admin`** (policy `AdministratorOnly`).

## Registration workflows

| Path | Who | Notes |
|------|-----|-------|
| `POST /api/auth/register/candidate` | Public | Candidate only; no client `role` field |
| `POST /api/auth/recruiter-requests` | Public | Creates pending request; no account yet |
| Admin approve recruiter request | Admin | Creates Recruiter user + profile + role |
| Hiring Manager | Admin only | Via `PATCH /api/admin/users/{id}/roles` |
| Administrator | Seed / existing admin | Never publicly registered |

## Login and tokens

- Normalized email match (`NormalizedEmail`)
- Sanitized failure: `"Invalid email or password."`
- Disabled/suspended accounts rejected
- JWT validates issuer, audience, lifetime, signing key
- Claims: `uid`, `role`, `email`, `org_id`, `dept_id`, `permission`
- Client discards token on logout; server records audit only (no server-side denylist in this phase)

## Authorization policies

| Policy | Roles |
|--------|-------|
| `CandidateOnly` | Candidate |
| `RecruiterOnly` | Recruiter |
| `HiringManagerOnly` | HiringManager |
| `AdministratorOnly` | Admin |
| `RecruiterOrAdministrator` | Recruiter, Admin |
| `HiringManagerOrAdministrator` | HiringManager, Admin |
| `RecruitmentTeam` | Recruiter, HiringManager, Admin |

Frontend route guards are UX only. APIs enforce independently.

## Not implemented in Phase 3

- Password reset / forgot-password email
- Email verification
- Refresh-token rotation
- Account lockout after failed attempts
- Server-side token revocation store

These remain next quality requirements.

## Verification (2026-07-20)

- Live four-role UAT against SQL Express: **26/26 PASS** (`docs/testing/PHASE3_LIVE_UAT.md`)
- Frontend Vitest auth suite: **13/13 PASS**
- Backend tests: **33/33 PASS**; NU1903 resolved with `SQLitePCLRaw.bundle_e_sqlite3` 3.0.3
- CORS restricted to configured frontend origins
