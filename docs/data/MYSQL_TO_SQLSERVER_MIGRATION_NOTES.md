# MySQL → SQL Server Migration Notes

**Last updated:** 2026-07-20 (Phase 2)

---

## Summary

HireSphere Phase 2 replaces the legacy MySQL (Pomelo) provider with **Microsoft.EntityFrameworkCore.SqlServer 8.0.11**. Previous MySQL-specific EF migrations were **removed** from the repository. A fresh SQL Server baseline migration was generated:

| Item | Value |
|------|-------|
| Migration name | `InitialSqlServerCoreModel` |
| Timestamp | `20260720103526` |
| Provider | SQL Server |

---

## Production data migration

**Status: BLOCKED — no automatic production data migration**

There is no scripted ETL from the old MySQL database to SQL Server. If historical MySQL data must be preserved:

1. Export MySQL tables to CSV or BACPAC-equivalent staging format.
2. Map column types (MySQL `VARCHAR`/`DATETIME` → SQL Server `nvarchar`/`datetime2`).
3. Import in dependency order: Users → Roles → Organizations → Jobs → Applications → child tables.
4. Re-hash any plaintext legacy passwords (Phase 1+ requires BCrypt).
5. Validate unique constraints listed in `DATA_DICTIONARY.md`.

---

## Schema differences addressed in Phase 2

- Full 35+ entity model registered in `ApplicationDbContext`
- Fluent configurations for indexes, string lengths, and delete behaviors
- History-preserving deletes: Job→Applications **Restrict**, Application→Interviews **Restrict**
- Status history cascades with application hard-delete
- `NormalizedEmail` unique index on Users
- Composite uniques: Applications (CandidateId, JobId), CandidateSkills, JobSkills, UserRoles, RolePermissions

---

## Apply status

| Environment | Migrate | Seed | Notes |
|-------------|---------|------|-------|
| Local dev (SQL Server running) | Ready | Ready after `database update` | See `SQL_SERVER_SETUP.md` |
| Local dev (no SQL Server) | **BLOCKED** | Skipped at startup | API builds; seeder logs warning |
| CI / coursework evidence | **BLOCKED** until SQL Server service configured | — | Document honest BLOCKED status |

---

## Developer workflow

```powershell
cd Backend/HireSphere.API
$env:DOTNET_ROLL_FORWARD = "Major"
dotnet tool run dotnet-ef migrations add <Name> --project HireSphere.API.csproj --output-dir Migrations
dotnet tool run dotnet-ef database update --project HireSphere.API.csproj
```

`migrations add` does **not** require SQL Server to be running; `database update` does.

---

## Rollback plan

If SQL Server adoption is blocked externally, revert provider package to Pomelo and regenerate MySQL migrations — **not recommended** for coursework deliverable (M-B02 requires SQL Server).
