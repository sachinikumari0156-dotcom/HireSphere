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
| List all users | No | No | No | Yes |
| Approve/reject recruiter requests | No | No | No | Yes |
| Assign roles / status / org | No | No | No | Yes |
| `/candidate/*` UI | Yes | Denied | Denied | Denied |
| `/recruiter/*` UI | Denied | Yes | Denied | Denied |
| `/hiring-manager/*` UI | Denied | Denied | Yes | Denied |
| `/admin/*` UI | Denied | Denied | Denied | Yes |

## Permission claims

Role-permission catalog is seeded. JWT may include `permission` claims from `RolePermissions`. Endpoint authorization primarily uses role policies in Phase 3; permissions are available for finer checks later.
