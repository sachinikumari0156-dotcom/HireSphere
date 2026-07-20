# HireSphere — Database design

**Last updated:** 2026-07-21  
**Environment verified:** `(localdb)\MSSQLLocalDB` / `HireSphereDev`  
**ORM:** EF Core (`ApplicationDbContext`)

## Overview

HireSphere uses a relational SQL Server schema modelling users, RBAC, organizations, candidate profiles, jobs, applications, assessments, interviews, evaluations, hiring decisions, AI analysis artefacts, notifications/outbox, documents, and audit logs.

## Keys and constraints

- Primary keys are typically `Guid` / identity as configured per entity.
- Foreign keys enforce ownership and lifecycle (e.g. job→applications Restrict where history must survive).
- Unique constraints protect emails, org codes, and duplicate applications where implemented.
- Decimal fields (scores) use precision configured in fluent API.
- Timestamps are stored and compared as UTC for interviews and audit events.

## Organization scoping

Jobs, staff profiles, analytics, and many admin actions are organization-scoped. Cross-organization access is denied in authorization tests.

## Soft deletion / archive

Organizations and departments support archive rules rather than unchecked hard deletes where Admin portal workflows require it.

## Related docs

- `ENTITY_CATALOGUE.md`
- `MIGRATION_HISTORY.md`
- `SEEDING_STRATEGY.md`
- `docs/data/ER_DIAGRAM.md`
- `docs/architecture/diagrams/source/06-er.mmd`
