# Dependency audit — Phase 10.1

**Date:** 2026-07-21

## Backend

```
dotnet list package --vulnerable --include-transitive
```

Result: **no vulnerable packages** reported for `HireSphere.API` and `HireSphere.API.Tests` against nuget.org (this host).

## Frontend

```
npm audit
```

Result (2026-07-21): **0 vulnerabilities** (info/low/moderate/high/critical all 0).

## Notes

- SQLitePCLRaw previously constrained via patched bundle in test project.
- Do not suppress advisories without analysis.
