# Final decision workflow

## Authority

| Actor | May recommend | May FinalHire / FinalReject |
|-------|---------------|-----------------------------|
| Hiring Manager | Yes (`RecommendHire` / `RecommendReject`) | No |
| Administrator | Views recommendations | Yes |
| Candidate / Recruiter | No | No |

Internal recommendations never auto-promote to final status.

## Administrator actions

- `FinalHire` → application `Hired` + status history + audit + Candidate notification
- `FinalReject` → application `Rejected` + history + audit + notification
- `Hold`, `RequestAdditionalInterview`, `RequestAdditionalAssessment` → non-final records

## Integrity

- Duplicate active FinalHire/FinalReject blocked
- Withdrawn applications rejected
- Optional optimistic concurrency via `ExpectedUpdatedAtUtc`
- Admin final may override intermediate status stages with an explicit reason recorded in status history notes
- Cross-organization access remains subject to Admin authorization policy
