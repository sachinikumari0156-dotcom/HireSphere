# Hiring Manager API

**Phase:** 6.1  
**Base path:** `/api/hiring-manager`  
**Auth policy:** `HiringManagerOrAdministrator`

| Method | Path | Description |
|--------|------|-------------|
| GET | `/dashboard` | Live assigned-vacancy metrics |
| GET | `/jobs` | Paginated assigned vacancies |
| GET | `/jobs/{id}` | Vacancy detail (read-only job definition) |
| GET | `/jobs/{id}/candidates` | Candidates for assigned vacancy |
| GET | `/applications/{id}` | Candidate review DTO |
| POST | `/candidates/compare` | Compare up to 5 apps on same vacancy |
| POST | `/jobs/{id}/review-comments` | Vacancy review comment + audit |
| GET | `/interviews` | Assigned interviews |
| GET | `/interviews/{id}` | Interview detail |
| POST/PUT | `/interviews/{id}/feedback` | Structured feedback |
| GET/POST/PUT | `/applications/{id}/evaluation` | Draft/submit evaluation |
| POST | `/applications/{id}/recommendation` | Recommendation (not auto-final) |
| GET | `/applications/{id}/decision-history` | Decision history |

Authorization is derived from `Job.HiringManagerUserId` / interview participation (and Admin). Cross-scope access returns sanitized 404. Recruiter-private notes are not included. Ranking includes human-review notice. Private panel comments never appear on Candidate APIs.
