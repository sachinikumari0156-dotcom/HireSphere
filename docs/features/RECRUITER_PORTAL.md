# Recruiter Portal

**Phase:** 5 (in progress)  
**Last updated:** 2026-07-20  
**Environment:** `(localdb)\MSSQLLocalDB` / `HireSphereDev`

## Phase 5.1 — Job management and applicant pipeline

### Delivered

- Organization-scoped Recruiter dashboard (`GET /api/recruiter/dashboard`) with live SQL metrics and truthful empty states
- Job CRUD + controlled status transitions (`Draft`, `PendingApproval`, `Published`/`Open`, `Paused`, `Closed`, `Archived`)
- Applicant pipeline with search/filter/pagination, application detail, internal notes, comparison (max 5)
- Application status transitions with history, audit logs, and candidate notifications
- Public/candidate job visibility limited to `Published`/`Open` and non-expired deadlines
- Frontend routes under `/recruiter/*` (dashboard, jobs, create/edit, detail, pipeline, application review, compare)
- Registration color-contrast fix for Candidate/Recruiter eyebrow text (`Register.css`)

### Authorization

- JWT `org_id` / recruiter identity used server-side; client-supplied organization IDs are not trusted
- Cross-organization job/application access returns safe 404
- Candidate role cannot call `/api/recruiter/*`
- Internal notes never returned on Candidate application DTOs

### Migration

- `AddRecruiterPortalPhase51` — Job portal fields + `ApplicationNotes`

### Not in 5.1

- Ranking panel, assessment builder, messaging, interviews, reports (Phases 5.2–5.3)
- External email/SMS and calendar providers (Phase 8)

## Phase 5.2 — Screening, ranking, assessments, communication

### Delivered

- Deterministic ranking API with explanation, confidence, human-review notice, and audited override
- Screening queue + reasoned screening decisions (no silent score-only reject)
- Org-scoped assessment CRUD, questions (answer keys recruiter-only), assignment, attempt review
- Application message threads (recruiter ↔ candidate) with sanitization and notifications
- Migration `AddRecruiterPortalPhase52`
- Frontend: screening, ranking, assessments builder, message thread

### Deferred

- Interview scheduling and reports (Phase 5.3)
- External email/SMS (Phase 8)

## Phase 5.3 — Interview scheduling and reports

### Delivered

- Schedule/reschedule/cancel interviews with conflict detection (Candidate, Recruiter, Hiring Manager, participants)
- UTC start + duration; timezone metadata; calendar sync = NotConfigured
- Reports summary + CSV export (org scoped, no password hashes)
- Frontend interview list/schedule/detail and reports dashboard

### Deferred

- Google Calendar / Outlook sync (Phase 8)
- External email/SMS delivery (Phase 8)
- Full Recruiter browser E2E evidence pack (required before Phase 5 VERIFIED)
