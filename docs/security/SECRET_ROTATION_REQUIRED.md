# HireSphere — Secret Rotation Required

**Severity:** CRITICAL  
**Discovered:** 2026-07-20 Phase 0 audit  
**Status:** MITIGATED IN CODE — tracked placeholders only; **local credential rotation still required**

---

## Previously exposed secrets (now removed from tracked config)

Historical values previously existed in git history for:

1. Database connection password in `Backend/HireSphere.API/appsettings.json`
2. JWT signing key in `Backend/HireSphere.API/appsettings.json`

Tracked files now contain placeholders only (for example `__CHANGE_ME__` / `__SET_VIA_USER_SECRETS_OR_ENV__`). Do not reintroduce real secret values into git.

**Mandatory local actions:**
1. Rotate any previously used database password on the local/server instance
2. Generate a new strong JWT signing key (≥ 256 bits)
3. Store connection string and JWT key in User Secrets or environment variables
4. Invalidate existing JWTs by forcing re-login after key rotation
5. Re-create or re-hash any accounts created before Phase 1 password hashing

---

## Recommended local setup (after Phase 1)

```powershell
cd Backend/HireSphere.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<YOUR_LOCAL_CONNECTION>"
dotnet user-secrets set "Jwt:Key" "<YOUR_STRONG_JWT_KEY>"
dotnet user-secrets set "Jwt:Issuer" "HireSphereAPI"
dotnet user-secrets set "Jwt:Audience" "HireSphereUsers"
```

Frontend (`.env.local`, gitignored):

```env
VITE_API_BASE_URL=http://localhost:5167/api
```

---

## Git history note

These values exist in committed history. Rotation limits future risk; for coursework, document in the report that secrets were moved to local configuration in Phase 1. Do **not** commit `.env`, `appsettings.Development.local.json`, or user-secrets files.

---

## Pre-commit checklist (all phases)

- [ ] `git diff` contains no passwords, API keys, or connection strings
- [ ] `appsettings.json` uses placeholders or empty strings only
- [ ] `.gitignore` covers `local-spec/`, `*.env`, user secrets paths
