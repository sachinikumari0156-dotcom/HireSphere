# Seeding strategy

## Goals

- Deterministic development and E2E identities without committing passwords.
- Idempotent role/permission bootstrap.
- Optional admin seed via environment variables / user secrets only.

## Gating

Seed runs only when explicitly enabled (for example `Seed__Enabled` / `HIRESPHERE_SEED_ENABLED` and related E2E flags). Production-like environments must keep seed disabled unless operators consciously enable it.

## E2E seed

`E2eSeedController` (`api/e2e`) supports test orchestration when E2E seed is enabled. It must remain disabled outside local/test hosts.

## Security rules

- Do not commit real passwords, JWT signing keys, or connection strings.
- Rotate any historically leaked demo passwords (see `docs/security/SECRET_ROTATION_REQUIRED.md`).
- Screenshots and docs use placeholders only.
