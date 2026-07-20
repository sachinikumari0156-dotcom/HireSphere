# Administrator governance

**Phase 7 — VERIFIED 2026-07-20**

## Authorization

- Policy: `AdministratorOnly` (`Admin` role)
- Client Role/OrganizationId/Permission claims are not trusted for authorization decisions
- Sensitive Admin actions check authenticated user + DB role/status
- `SecurityStamp` rotates on disable and critical role changes; Inactive/Suspended cannot login

## Protections

- Self-disable blocked
- Last active global Administrator cannot be disabled / last Admin role cannot be removed
- PasswordHash, JWTs, reset tokens never serialized
- Recruiter approval cannot assign privileged roles beyond Recruiter
- Cross-organization department assignment blocked
- Archived departments reject new assignments

## Final decisions

Hiring Manager recommendations are not final. Only Administrator records FinalHire/FinalReject (see `FINAL_DECISION_WORKFLOW.md`).

## Providers

Email / SMS / calendar / cloud storage: **NotConfigured** until Phase 8.
