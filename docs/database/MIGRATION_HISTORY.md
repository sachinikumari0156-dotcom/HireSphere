# Migration history

**Tool:** `dotnet ef` (local tool via `Backend/dotnet-tools.json`)  
**Verified host DB:** `(localdb)\MSSQLLocalDB` / `HireSphereDev`

## Practice

1. Apply migrations with ignored configuration / user secrets (never commit production connection strings).
2. `dotnet ef migrations list` then `dotnet ef database update`.
3. Phase 10 added `MigrationVerificationTests` and `scripts/verify-migrations.ps1` for LocalDB history + idempotent update checks.

## Notes

- Empty-database migrate and existing-database migrate were exercised in Phase 10 migration verification.
- Seed is gated and must not embed production credentials.
- Exact migration class names evolve with the snapshot; consult `Backend/HireSphere.API/Migrations/`.

## Limitation

Clean-machine migration was verified on this development host; cloud SQL deployment remains **Not Configured / Not Verified**.
