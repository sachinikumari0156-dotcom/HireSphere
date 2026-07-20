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

## Phase 5.2 endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/applications/{id}/ranking` | Deterministic ranking + human-review notice |
| POST | `/applications/{id}/ranking/review` | Audited human review/override |
| GET | `/screening-queue` | Org screening queue |
| POST | `/applications/{id}/screening-decision` | Screening status + required reason |
| GET/POST | `/assessments` | List/create assessments |
| GET/PUT | `/assessments/{id}` | Detail/update |
| POST | `/assessments/{id}/archive` | Archive |
| POST/PUT/DELETE | `/assessments/{id}/questions[/{questionId}]` | Question management (keys included for recruiter) |
| POST | `/applications/{applicationId}/assessments` | Assign assessment |
| GET | `/assessment-assignments/{id}` | Assignment detail |
| GET | `/assessment-assignments/{id}/attempts` | Attempt scores |
| GET/POST | `/applications/{id}/messages` | Thread / send |
| POST | `/applications/{id}/messages/read` | Mark read |

Candidate message APIs: `GET/POST /api/candidate/applications/{id}/messages`

- Missing/unauthorized resources: `404` with generic message
- Invalid transitions / validation: `400`
- Unauthenticated: `401`
- Wrong role: `403`
