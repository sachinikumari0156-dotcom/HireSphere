# Local to cloud migration

## Dry-run

`POST /api/admin/storage/migrations/dry-run`

Returns counts of legacy keys (not starting with `tenant/`) without moving files.

## Execute

Destructive execute is disabled by default (`POST .../migrations/execute` returns BadRequest).

Manual process when authorized:

1. Dry-run and record counts
2. Copy objects to target provider with checksum verification
3. Update metadata keys idempotently
4. Re-verify downloads
5. Only then delete source objects

Never auto-delete source files after migration.
