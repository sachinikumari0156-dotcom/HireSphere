# Candidate API

**Base:** `/api/candidate`
**Auth:** Bearer JWT, policy `CandidateOnly`
**Identity:** All actions use authenticated candidate identity from JWT (`uid`). Client-supplied candidate user IDs are never trusted.

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

## Jobs (Phase 4.2)

Open jobs only (`JobStatus.Open`). Draft/Closed/Archived are not returned.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/candidate/jobs` | Search/filter/paginate open jobs. Query: `keyword`, `location`, `departmentId`, `employmentType`, `workArrangement`, `skillId`, `page`, `pageSize`, `sortBy` (`postedDate`\|`title`\|`location`\|`matchScore`), `sortDir` (`asc`\|`desc`) |
| GET | `/api/candidate/jobs/{id}` | Job detail + deterministic match + `alreadyApplied` |
| GET | `/api/candidate/jobs/{id}/match` | Deterministic match explanation only |
| GET | `/api/candidate/recommendations` | Highest-match open jobs. Incomplete profile (or no skills) returns empty `jobs` with guidance message — not fake scores |

### Match payload

Returns: `matchScore`, `matchedSkills`, `missingSkills`, experience/education/location/work-arrangement factors, `explanation`, `provider` (`Deterministic`), `computedAtUtc`, `humanReviewNotice`.
This is a **rules engine**, not an external AI call.

## Applications (Phase 4.2)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/candidate/jobs/{id}/apply-options` | Resumes + screening questions + `canApply` / `blockReason` |
| POST | `/api/candidate/applications` | Submit application (resume, cover letter, screening answers, `termsAccepted`). Blocks closed jobs and duplicates. Creates initial status history |
| GET | `/api/candidate/applications` | Own applications list |
| GET | `/api/candidate/applications/{id}` | Own application detail (answers + status history). Other candidates → 404 |
| POST | `/api/candidate/applications/{id}/withdraw` | Withdraw when status is `Pending` or `UnderReview` |

## Storage

Local development provider stores files under `App_Data/uploads` with randomized keys. MIME/extension/size validation enforced. **Cloud storage verification is pending** for a later phase.

## Migration (4.2)

`AddApplicationResumeId` — optional `Applications.ResumeId` FK (SetNull on resume delete).
