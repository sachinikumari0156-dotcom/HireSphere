# Candidate API

**Base:** `/api/candidate`  
**Auth:** Bearer JWT, policy `CandidateOnly`

## Dashboard & profile

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/candidate/dashboard` | Profile completion, applications, interviews, assessments, recommendations, notifications, resume analysis status |
| GET | `/api/candidate/profile` | Full profile + nested sections |
| PUT | `/api/candidate/profile` | Update personal/professional fields |

## Sections

| Method | Path | Description |
|--------|------|-------------|
| POST/PUT/DELETE | `/api/candidate/experience[/{id}]` | Work experience CRUD |
| POST/PUT/DELETE | `/api/candidate/education[/{id}]` | Education CRUD |
| GET | `/api/candidate/skills/catalog` | Available skills |
| POST/PUT/DELETE | `/api/candidate/skills[/{id}]` | Candidate skills CRUD (duplicate skill blocked) |
| POST/PUT/DELETE | `/api/candidate/certifications[/{id}]` | Certifications CRUD |

## Resumes & documents

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/candidate/resumes` | Resume metadata (no absolute paths) |
| POST | `/api/candidate/resumes` | Multipart upload |
| DELETE | `/api/candidate/resumes/{id}` | Delete |
| POST | `/api/candidate/resumes/{id}/primary` | Set primary |
| GET | `/api/candidate/documents` | Document metadata |
| POST | `/api/candidate/documents` | Multipart upload + documentType |
| GET | `/api/candidate/documents/{id}/download` | Authorized download |
| DELETE | `/api/candidate/documents/{id}` | Delete |

## Storage

Local development provider stores files under `App_Data/uploads` with randomized keys. MIME/extension/size validation enforced. **Cloud storage verification is pending** for a later phase.
