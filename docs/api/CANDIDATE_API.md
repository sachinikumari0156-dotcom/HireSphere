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

## Applications (Phase 4.2 + 4.3 tracking)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/candidate/jobs/{id}/apply-options` | Resumes + screening questions + `canApply` / `blockReason` |
| POST | `/api/candidate/applications` | Submit application. Creates status history + in-app `ApplicationSubmitted` notification |
| GET | `/api/candidate/applications` | Own applications list (`latestUpdateAtUtc`, `nextAction`) |
| GET | `/api/candidate/applications/{id}` | Detail: ordered status timeline, next action, linked interviews/assessments. Other candidates → 404 |
| POST | `/api/candidate/applications/{id}/withdraw` | Withdraw when `Pending` or `UnderReview`; creates status notification |

### Application status values

`Pending`, `UnderReview`, `Assessment`, `Shortlisted`, `InterviewScheduled`, `Interviewed`, `Offered`, `Hired`, `Rejected`, `Withdrawn`
(Display aliases sometimes used in docs: Submitted≈Pending, Screening≈UnderReview, Offer≈Offered.)

## Assessments (Phase 4.3)

Assignments only — unassigned assessments are not listed. Answer keys are **never** returned to candidates.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/candidate/assessments` | Assigned assessments + attempt/status summary |
| GET | `/api/candidate/assessments/{assignmentId}` | Detail + questions (options only; no correct key) |
| POST | `/api/candidate/assessments/{assignmentId}/start` | Start attempt (enforces start window, expiry, max attempts) |
| GET | `/api/candidate/assessments/attempts/{attemptId}` | Attempt + answers; score/feedback only when `RevealResultsToCandidate` |
| PUT | `/api/candidate/assessments/attempts/{attemptId}/answers` | Save answers while InProgress |
| POST | `/api/candidate/assessments/attempts/{attemptId}/submit` | Server-side score; audit log entry |

## Interviews (Phase 4.3)

No calendar provider tokens or credentials are exposed.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/candidate/interviews` | Own interviews (via application ownership) |
| GET | `/api/candidate/interviews/{id}` | Detail + timezone; meeting link only when authorized (typically after confirm) |
| POST | `/api/candidate/interviews/{id}/confirm` | Confirm attendance |
| POST | `/api/candidate/interviews/{id}/reschedule-request` | Body: `reason`, optional `preferredTimesNote` |
| POST | `/api/candidate/interviews/{id}/decline` | Body: `reason` (required) |

## Notifications (Phase 4.3 — in-app foundation)

External email/SMS remain deferred. Categories in use: `ApplicationSubmitted`, `ApplicationStatusUpdated`, `AssessmentAssigned`, `InterviewScheduled`, `InterviewUpdated`.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/candidate/notifications` | Inbox + `unreadCount` |
| POST | `/api/candidate/notifications/{id}/read` | Mark one read |
| POST | `/api/candidate/notifications/read-all` | Mark all read |

## Storage

Local development provider stores files under `App_Data/uploads` with randomized keys. MIME/extension/size validation enforced. **Cloud storage verification is pending** for a later phase.

## Migrations

| Migration | Phase | Notes |
|-----------|-------|-------|
| `AddApplicationResumeId` | 4.2 | Optional `Applications.ResumeId` |
| `AddPhase43AssessmentsInterviewsNotifications` | 4.3 | Assignments, answers, question keys/options, interview response fields, notification category/link fields, `ApplicationStatus` Assessment/Interviewed |
