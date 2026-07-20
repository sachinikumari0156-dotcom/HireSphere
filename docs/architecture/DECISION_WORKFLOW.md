# Hiring Decision Workflow

**Phase:** 6.2  
**Date:** 2026-07-20

## Roles

| Action | Hiring Manager | Recruiter | Admin |
|--------|----------------|-----------|-------|
| RecommendHire / RecommendReject / Hold / RequestAdditional* | Yes | Yes (via admin policy paths later) | Yes |
| FinalHire / FinalReject | No | Yes | Yes |
| View decision history (assigned apps) | Yes | Org policy | Yes |

## Rules

1. Recommendations create `HiringDecision` with `IsFinal=false` and do **not** change application status by themselves.
2. Final decisions require Recruiter or Admin; they create status history via `ApplicationStatusTransitionService` and notify Candidate.
3. Withdrawn applications reject new decisions.
4. Duplicate active final decisions are blocked.
5. Private interview panel comments are never returned on Candidate APIs.
6. Evaluation scores are advisory and never auto-decide hiring.
