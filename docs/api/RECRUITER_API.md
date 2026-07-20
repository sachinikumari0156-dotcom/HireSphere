# Recruiter API

Base path: `/api/recruiter`  
Policy: `RecruiterOrAdministrator`  
Organization scope: from authenticated JWT claims (never from request body)

## Phase 5.1 endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/dashboard` | Org-scoped dashboard metrics + recent audit activity |
| GET | `/jobs` | Paginated job list (keyword, status, department, location, employment type, work arrangement, dates, sort) |
| POST | `/jobs` | Create draft job in caller organization |
| GET | `/jobs/{id}` | Job detail (skills, screening questions) |
| PUT | `/jobs/{id}` | Update job content |
| DELETE | `/jobs/{id}` | Delete Draft/Archived job without applications |
| PATCH | `/jobs/{id}/status` | Controlled status transition |
| GET | `/jobs/{id}/applications` | Applicant pipeline |
| GET | `/applications/{id}` | Application review (no absolute storage paths) |
| PATCH | `/applications/{id}/status` | Controlled application status transition |
| GET/POST | `/applications/{id}/notes` | Internal recruiter notes |
| DELETE | `/notes/{id}` | Delete internal note |
| POST | `/applications/compare` | Compare 2–5 authorized applicants |

## Error behaviour

- Missing/unauthorized resources: `404` with generic message
- Invalid transitions / validation: `400`
- Unauthenticated: `401`
- Wrong role: `403`
