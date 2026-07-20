# Administrator API (Phase 7.1)

**Base:** `/api/admin`  
**Auth:** JWT + `AdministratorOnly` (`Admin` role)

## Governance endpoints

| Method | Path | Notes |
|--------|------|-------|
| GET | `/dashboard` | Live metrics |
| GET | `/users` | Paginated `{ items, page, pageSize, totalCount }` |
| GET | `/users/{id}` | Safe detail (no PasswordHash) |
| PATCH | `/users/{id}/status` | Self-disable and last-Admin blocked |
| PUT | `/users/{id}/organization` | Org/dept; archived dept rejected |
| PUT | `/users/{id}/department` | |
| GET/POST/DELETE | `/users/{id}/roles` | Last Admin role protected |
| GET | `/roles`, `/permissions` | |
| PUT | `/roles/{id}/permissions` | |
| GET | `/recruiter-requests` | Legacy list |
| GET | `/recruiter-requests/{id}` | |
| POST | `/recruiter-requests/{id}/approve` | Creates Recruiter + org profile + notification |
| POST | `/recruiter-requests/{id}/reject` | Reason required |
| GET/POST/PUT/PATCH | `/organizations` | Unique `code` |
| GET/POST/PUT/PATCH | `/departments` | Unique name per org |
| POST | `/hiring-managers/assign` | Role + org + optional job |

Token staleness: `SecurityStamp` rotates on disable/role changes; login rejects Inactive/Suspended; `/auth/me` reloads DB status.
