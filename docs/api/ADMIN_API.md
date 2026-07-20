# Administrator API

**Base:** `/api/admin`  
**Auth:** JWT + `AdministratorOnly` (`Admin` role)

## Phase 7.1 — Governance

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

## Phase 7.2 — Audit, monitoring, analytics, final decisions

| Method | Path | Notes |
|--------|------|-------|
| GET | `/audit-logs` | Filterable, paginated |
| GET | `/audit-logs/export` | CSV; formula-injection neutralized |
| GET | `/monitoring/summary` | Live DB counts; Phase 8 providers `NotConfigured` |
| GET | `/analytics/users` | Role/status counts |
| GET | `/analytics/recruitment` | Org-scoped jobs/apps metrics |
| GET | `/analytics/departments` | Jobs/users by department |
| GET | `/analytics/skills` | Skill demand + candidate availability |
| GET | `/final-decisions/pending` | HM recommendations awaiting final |
| GET | `/final-decisions/{applicationId}` | Review package |
| POST | `/final-decisions/{applicationId}` | FinalHire/Reject/Hold/RequestAdditional* |
| GET | `/security/users/{userId}` | Safe account activity (no hashes/tokens) |
| GET | `/exports/{type}` | `users`, `organizations`, `departments`, `audit` |

Token staleness: `SecurityStamp` rotates on disable/role changes; login rejects Inactive/Suspended; `/auth/me` reloads DB status.

Final decisions: HM recommendations are not final. Only Admin records FinalHire/FinalReject. Duplicate finals, withdrawn apps, and stale `ExpectedUpdatedAtUtc` are rejected. Candidate in-app notification is created on approved final communication.
