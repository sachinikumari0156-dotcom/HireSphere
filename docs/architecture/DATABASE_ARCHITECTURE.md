# HireSphere — Database Architecture

**Last updated:** 2026-07-20 (Phase 2)

---

## Overview

HireSphere uses a **relational SQL Server database** accessed through **Entity Framework Core 8** with a code-first approach. The `ApplicationDbContext` aggregates 35+ entity sets covering identity, recruitment workflows, assessments, AI artifacts, notifications, and audit logging.

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  ASP.NET Core   │────▶│ ApplicationDb    │────▶│  SQL Server     │
│  Web API        │     │ Context + Fluent │     │  HireSphereDB   │
└─────────────────┘     │ Configurations   │     └─────────────────┘
                        └──────────────────┘
                                 │
                        ┌────────┴────────┐
                        │ Migrations/     │
                        │ DbSeeder        │
                        └─────────────────┘
```

---

## Layering

| Layer | Location | Responsibility |
|-------|----------|----------------|
| Domain models | `Models/` | POCO entities, enums |
| Mapping | `Data/Configurations/` | Indexes, FK delete rules, column constraints |
| Context | `Data/ApplicationDbContext.cs` | DbSet registration, apply configurations |
| Migrations | `Migrations/` | SQL Server schema versions |
| Seed | `Data/Seed/DbSeeder.cs` | Dev roles, org, skills, sample data |
| Tests | `Backend/HireSphere.API.Tests/` | SQLite in-memory constraint + auth tests |

Configurations are discovered via `ApplyConfigurationsFromAssembly` — no inline `OnModelCreating` rules in the context.

---

## Identity model (transitional)

Phase 2 maintains **dual role representation**:

1. **Legacy string** `Users.Role` — used by JWT claims and existing controllers.
2. **Normalized RBAC** — `Roles`, `Permissions`, `UserRoles`, `RolePermissions` seeded for Phase 3 expansion.

Public registration creates Candidate users only; privileged roles require admin workflows.

---

## Recruitment core

```
Users (Candidate) ──▶ Applications ◀── Jobs ◀── Users (Recruiter)
                           │
                           ├── ApplicationStatusHistory
                           ├── ApplicationAnswers
                           └── Interviews ──▶ Participants / Feedback
```

**Integrity rules:**

- One application per candidate per job (unique composite index).
- Deleting a job or application with dependent interviews is **blocked** (Restrict).
- Application status history deletes with application (Cascade).

---

## Profile and skills subgraph

Candidate profiles are **1:1 with users** (unique `UserId`). Skills are normalized in `Skills` with join tables `CandidateSkills` and `JobSkills` enforcing uniqueness per profile/job pair.

---

## Provider configuration

`Program.cs` registers:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

Connection string key: `ConnectionStrings:DefaultConnection` (secrets/env in production).

Startup seed runs only when **not** in `Testing` environment and database is reachable.

---

## Testing strategy

Automated tests use **SQLite in-memory** (`TestDbFactory`) to validate relational constraints without SQL Server. Integration auth tests use `WebApplicationFactory` with SQLite substitution and `Testing` environment to skip production seeding.

This is test-only — production and coursework deployment target SQL Server per SRS M-B02.

---

## Related documents

- [DATA_DICTIONARY.md](../data/DATA_DICTIONARY.md) — columns, keys, privacy fields
- [ER_DIAGRAM.md](../data/ER_DIAGRAM.md) — Mermaid relationship map
- [SQL_SERVER_SETUP.md](../data/SQL_SERVER_SETUP.md) — local/Docker setup
- [MYSQL_TO_SQLSERVER_MIGRATION_NOTES.md](../data/MYSQL_TO_SQLSERVER_MIGRATION_NOTES.md) — migration cutover notes
