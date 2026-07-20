# HireSphere — Secret Rotation Required

**Severity:** CRITICAL  
**Discovered:** 2026-07-20 Phase 0 audit  
**Status:** OPEN — rotate before any public deployment or shared demo

---

## Exposed secrets (tracked in repository)

### 1. Database password — `Backend/HireSphere.API/appsettings.json`

```json
"DefaultConnection": "Server=localhost;Database=HireSphereDB;User=root;Password=sachini@2003;"
```

**Actions:**
1. Change the MySQL/SQL Server password immediately if this instance is or was reachable
2. Remove password from tracked `appsettings.json`
3. Store connection string in User Secrets (`dotnet user-secrets`) or environment variable
4. Use SQL Server auth appropriate for coursework (Windows auth or SQL login via secrets)

### 2. JWT signing key — `Backend/HireSphere.API/appsettings.json`

```json
"Key": "HireSphereSuperSecretKey123456789"
```

**Actions:**
1. Generate a cryptographically strong key (≥ 256 bits, base64 or long random string)
2. Store in User Secrets / environment only
3. Invalidate existing JWTs after rotation (users re-login)

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
