# AI API (Phase 8.1)

## Candidate

| Method | Path | Notes |
|--------|------|-------|
| POST | `/api/candidate/resumes/{id}/parse` | Ownership required |
| GET | `/api/candidate/resumes/{id}/analysis` | |
| POST | `/api/candidate/resumes/{id}/analysis/confirm` | Accept/reject skill IDs |
| POST | `/api/candidate/resumes/{id}/analysis/reject` | Reject all |
| PUT | `/api/candidate/ai/consent` | External processing consent |
| GET | `/api/candidate/ai/status` | Provider statuses |
| GET | `/api/candidate/jobs/{id}/match` | Existing match + human-review notice |
| GET | `/api/candidate/recommendations` | Existing |

## Recruiter

| Method | Path | Notes |
|--------|------|-------|
| GET | `/api/recruiter/applications/{id}/ranking` | Existing ranking |
| POST | `/api/recruiter/applications/{id}/ranking/review` | Human override |

## Administrator

| Method | Path | Notes |
|--------|------|-------|
| GET | `/api/admin/integrations/ai/status` | Truthful provider status |
| GET | `/api/admin/analytics/skill-trends` | Descriptive insight |

No PasswordHash, API keys, or raw prompts in responses.
