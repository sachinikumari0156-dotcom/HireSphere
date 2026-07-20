# Authorization Matrix

**Last updated:** 2026-07-20

| Resource / action | Candidate | Recruiter | HiringManager | Admin |
|-------------------|-----------|-----------|---------------|-------|
| Self-register | Yes | No (request only) | No | No |
| Login | Yes (active) | Yes (active) | Yes (active) | Yes (active) |
| `GET /api/auth/me` | Own | Own | Own | Own |
| Change own password | Yes | Yes | Yes | Yes |
| Own candidate profile | Yes | No | No | Yes (admin) |
| List candidate profiles | No | Yes | Yes | Yes |
| Own applications | Yes | No | No | Yes |
| Job CRUD (own/org scope) | No | Yes | No | Yes (via admin tools) |
| Applicant pipeline / notes / compare | No | Yes (org) | No | Yes (admin policy) |
| Ranking / screening / assessments (recruiter mgmt) | No | Yes (org) | No | Yes (admin policy) |
| Assessment take / submit (assigned) | Yes (own) | No | No | No |
| Application messaging | Own thread | Org applications | No | Yes (admin policy) |
| Interview schedule / conflicts | Respond own | Yes (org) | Participant | Yes (admin policy) |
| Recruiter reports / CSV | No | Yes (org) | No | Yes (admin policy) |
| List all users | No | No | No | Yes |
| Approve/reject recruiter requests | No | No | No | Yes |
| Assign roles / status / org | No | No | No | Yes |
| `/candidate/*` UI | Yes | Denied | Denied | Denied |
| `/recruiter/*` UI | Denied | Yes | Denied | Denied |
| `/hiring-manager/*` UI | Denied | Denied | Yes | Denied |
| `/admin/*` UI | Denied | Denied | Denied | Yes |

## Permission claims

Role-permission catalog is seeded. JWT may include `permission` claims from `RolePermissions`. Endpoint authorization primarily uses role policies; Recruiter APIs additionally enforce organization ownership via resource authorization services.

## Phase 5 notes

- Cross-organization Recruiter access returns safe 404.
- Assessment answer keys never appear in Candidate DTOs/UI.
- Calendar provider sync status is NotConfigured until Phase 8 credentials exist.
