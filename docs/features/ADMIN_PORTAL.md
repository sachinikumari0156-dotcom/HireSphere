# Administrator Portal

**Phase:** 7.1 (governance) — IN PROGRESS until 7.2 + E2E  
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

## Policy

- Role string: `Admin`
- Password hashes never returned
- Password reset email remains Phase 8
- Historical org/dept data retained on archive
