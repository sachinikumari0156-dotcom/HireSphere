# Administrator Portal

**Phase:** 7 (IN PROGRESS — E2E pending)
**Date:** 2026-07-20

## Delivered in 7.1

- Live Admin dashboard
- User list/detail, status, org/dept, roles
- Self-disable and last-Administrator protections
- Recruiter request approve/reject with audit + in-app notification
- Role/permission matrix
- Organization and department CRUD with status
- Hiring Manager assignment helper
- Migration `AddAdminPortalPhase71`

## Delivered in 7.2

- Audit log viewer + CSV export
- Monitoring dashboard (truthful DB; providers NotConfigured)
- Recruitment / department / skill analytics
- Pending final decisions + Admin FinalHire/FinalReject
- Security/account activity panel (safe fields)
- Safe exports (`/api/admin/exports/{type}`)

## Policy

- Role string: `Admin`
- Password hashes never returned
- Password reset email remains Phase 8
- Historical org/dept data retained on archive
- Final decisions: Administrator only; HM recommendations are not final
