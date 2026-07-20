# Hiring Manager Portal

**Phase:** 6 — IN PROGRESS (6.1 delivered)  
**Last updated:** 2026-07-20  
**Environment:** `(localdb)\MSSQLLocalDB` / `HireSphereDev`

## Phase 6.2 — Interview feedback, evaluations and decisions

### Delivered

- Interview list/detail for assigned HM / participants
- Structured interview feedback (1–5 ratings, private panel comments)
- Candidate evaluation Draft/Submitted with justification
- Recommendations (`RecommendHire` etc.) with audit + decision history
- FinalHire/FinalReject blocked for Hiring Manager role
- Withdrawn applications reject new decisions
- Migration `AddHiringManagerPortalPhase62`

### Policy

- Hiring Manager: recommendations only
- Final decisions: Recruiter or Administrator
- Candidate never receives private panel comments

## Phase 6.1 — Assigned vacancies and candidate review

### Delivered

- Organization/assignment-scoped dashboard with live SQL metrics
- Assigned vacancy list/detail (no Recruiter job-definition edit)
- Vacancy review comments (`JobReviewComments`) with audit
- Candidate list, review, and same-vacancy comparison (max 5)
- Deterministic match explanation + human-review notice
- Resume metadata without absolute paths; no password hashes; no Recruiter-private notes
- Frontend `/hiring-manager/*` nested portal
- Migration `AddHiringManagerPortalPhase61`

### Not in 6.1

- Interview feedback, evaluations, hiring decisions (Phase 6.2)
- Full browser E2E evidence pack (Phase 6.3)
