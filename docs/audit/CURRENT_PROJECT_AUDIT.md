# HireSphere — Current Project Audit

**Audit date:** 2026-07-20  
**Auditor:** Cursor implementation agent (Phase 0 baseline)  
**Repository:** `https://github.com/sachinikumari0156-dotcom/HireSphere.git`  
**Local path:** `c:\Users\Rashm\Pictures\New folder (2)\HireSphere`

## Executive summary

HireSphere is an early-stage recruitment platform with a partial ASP.NET Core 8 Web API (MySQL) and a React (Vite) frontend. Candidate and recruiter foundations exist, but mandatory coursework scope for four roles, SQL Server, security hardening, AI, integrations, admin/manager portals, tests, and submission evidence are largely **not started**.

**Readiness:** NOT READY

---

## 1. Git and environment gate (BLOCKED)

| Check | Expected | Actual | Status |
|-------|----------|--------|--------|
| Git installed | Yes | Yes (`C:\Program Files\Git\bin\git.exe`, not on PATH) | WARN |
| GitHub CLI (`gh`) | Installed, login `kalanirashmika` | **Not installed** | FAIL |
| Local git `user.name` | `Kalani Rashmika` | **Not configured** | FAIL |
| Local git `user.email` | Kalani verified email | **Not configured** | FAIL |
| Working branch | `kalanirashmika/coursework-completion` | `main` | FAIL |
| Remote | `sachinikumari0156-dotcom/HireSphere` | Correct | PASS |
| Branch sync | Up to date or on feature branch | `main` behind `origin/main` by 2 commits | WARN |
| .NET SDK | Available | .NET 10.0.302 SDK (builds net8.0 project) | PASS |
| Node.js / npm | Available | **Not installed** | FAIL |

**Action required before any commit/push:**

```powershell
# Add Git to PATH (or use full path)
$env:Path += ";C:\Program Files\Git\bin"

# Install GitHub CLI: winget install GitHub.cli
gh auth login
gh auth status
gh api user --jq .login   # must output: kalanirashmika

git config --local user.name "Kalani Rashmika"
git config --local user.email "<KALANI_GITHUB_VERIFIED_EMAIL>"

git switch main
git pull --ff-only origin main
git switch -c kalanirashmika/coursework-completion
git push -u origin kalanirashmika/coursework-completion
```

Install Node.js LTS for frontend build/lint/test.

---

## 2. Repository structure

```
HireSphere/
├── Backend/HireSphere.API/     # ASP.NET Core 8 Web API
├── Frontend/                   # React 19 + Vite 8
├── README.md                   # Minimal (title only)
├── docs/                       # Created in Phase 0
└── (untracked) Master prompt, TODO, SRS PDF
```

No test projects. No `Features/` modular structure. No `docs/` prior to this audit.

---

## 3. Backend audit

### 3.1 Stack

| Item | Value |
|------|-------|
| Framework | ASP.NET Core 8 (`net8.0`) |
| ORM | Entity Framework Core 9.0.0 |
| Database provider | **Pomelo MySQL** (coursework requires **SQL Server**) |
| Auth | JWT Bearer (partial) |
| API docs | Swagger enabled |

### 3.2 Controllers (5)

| Controller | Endpoints | Auth | Notes |
|------------|-----------|------|-------|
| `AuthController` | Register, Login | Public | Plain-text passwords; open role selection |
| `UsersController` | List, Get by id | `[Authorize]` | No RBAC granularity |
| `JobsController` | CRUD, search | Mixed | Recruiter-only mutations |
| `ApplicationsController` | Apply, list, status | Role-based | Partial pipeline |
| `CandidateProfilesController` | CRUD profile | Candidate role | No resume upload API |

### 3.3 Models in DbContext (4 entities)

- `User`, `Job`, `CandidateProfile`, `Application`

### 3.4 Orphan models (not in DbContext)

- `Interview` — no migration, no controller
- `SkillAssessment` — no migration, no controller

### 3.5 Missing mandatory entities (sample)

`Role`, `Permission`, `Organization`, `Department`, `Resume`, `InterviewFeedback`, `CandidateEvaluation`, `HiringDecision`, `AuditLog`, `Notification`, `AnalyticsSnapshot`, AI insight entities, and others per coursework spec.

### 3.6 Migrations

6 EF migrations present (MySQL `longtext` column types). No SQL Server migrations.

### 3.7 Build result

```
dotnet restore HireSphere.API.csproj  → SUCCESS (NU1603 package resolution warning)
dotnet build HireSphere.API.csproj    → SUCCESS (16 nullable warnings, 0 errors)
dotnet test                           → N/A (no test project)
```

---

## 4. Security audit (CRITICAL)

| Risk | Severity | Location | Detail |
|------|----------|----------|--------|
| Plain-text passwords | **CRITICAL** | `AuthController.cs` | `PasswordHash = dto.Password`; login compares plaintext |
| Hard-coded DB password | **CRITICAL** | `appsettings.json` | `Password=__REDACTED__` committed |
| Weak JWT secret | **HIGH** | `appsettings.json` | Short static key in repo |
| Unrestricted CORS | **HIGH** | `Program.cs` | `AllowAnyOrigin()` |
| Open role registration | **HIGH** | `AuthController.Register` | Any role including Admin can self-register |
| Secrets in tracked config | **HIGH** | `appsettings.json` | Must move to user secrets / env |
| No audit logging | MEDIUM | — | Not implemented |
| No IDOR checks on all resources | MEDIUM | Partial in some controllers | Needs systematic review |
| No HTTPS enforcement config | LOW | Dev profile uses HTTP 5167 | Acceptable for dev after secrets fix |

---

## 5. Frontend audit

### 5.1 Stack

| Item | Value |
|------|-------|
| React | 19.2.7 |
| Router | react-router-dom 7.18.1 |
| HTTP | axios (partial use) |
| Build | Vite 8 |

### 5.2 Routes

| Route | Page | Status |
|-------|------|--------|
| `/` | Home | Static hero; fake stats; dead "Explore jobs" button |
| `/login` | Login | Works against API; **Hireflow** branding in footer |
| `/register` | Register | Works; **Hireflow** branding; role selectable |
| `/candidate-dashboard` | CandidateDashboard | Partial; fetches jobs/applications |
| `/recruiter-dashboard` | RecruiterDashboard | **Placeholder only** (welcome text) |

### 5.3 Missing routes

Hiring Manager dashboard, Administrator dashboard, forgot/reset password, access denied, 404, public job details, assessments, interviews, analytics, admin panels.

### 5.4 API configuration issues

| Issue | Detail |
|-------|--------|
| Split base URLs | `axios.js` → `http://localhost:5167/api`; `CandidateDashboard` → `https://localhost:7000/api` |
| No route guards | Dashboards accessible without login |
| No protected route wrapper | JWT not consistently used |
| No test script | `package.json` has no `test` script |
| Node not installed | **Cannot run lint/build on this machine** |

### 5.5 Branding

`Hireflow` appears in `Login.jsx` and `Register.jsx` footers — must be replaced with `HireSphere`.

### 5.6 Design system

Partial custom CSS on auth pages. No shared design tokens, no responsive system per coursework spec (`#4F46E5` primary, etc.).

---

## 6. Testing and evidence

| Area | Status |
|------|--------|
| xUnit / API tests | NOT STARTED |
| Vitest / RTL | NOT STARTED |
| Postman collection | NOT STARTED |
| UAT scripts | NOT STARTED |
| Usability evidence | NOT STARTED |
| Report diagrams | NOT STARTED |

---

## 7. Integration readiness

| Integration | Status |
|-------------|--------|
| Email (SMTP/MailHog) | NOT STARTED |
| SMS | NOT STARTED |
| Google Calendar | NOT STARTED |
| Outlook Calendar | NOT STARTED |
| Cloud storage (Azure Blob/Azurite) | NOT STARTED |
| External AI provider | NOT STARTED |

---

## 8. Positive findings

- JWT infrastructure scaffolded (issuer, audience, Swagger bearer)
- Basic RBAC attributes on some endpoints (`[Authorize(Roles = "...")]`)
- Core domain relationships (User ↔ Profile, Job ↔ Application) modeled
- Candidate dashboard attempts real API integration
- Backend builds cleanly on .NET SDK 10 targeting net8.0
- Git remote points to correct upstream repository

---

## 9. Recommended phase order

1. **Phase 0** (this audit) — documentation only  
2. **Phase 1** — secrets, password hashing, CORS, env-based config  
3. **Phase 2** — SQL Server migration + expanded data model  
4. **Phase 3** — auth/RBAC/account workflows  
5. **Phases 4–11** — per `IMPLEMENTATION_PLAN.md`

---

## 10. Files inspected

- `Backend/HireSphere.API/Program.cs`
- `Backend/HireSphere.API/appsettings.json`
- `Backend/HireSphere.API/Controllers/*` (5 files)
- `Backend/HireSphere.API/Models/*` (6 files)
- `Backend/HireSphere.API/Data/ApplicationDbContext.cs`
- `Backend/HireSphere.API/HireSphere.API.csproj`
- `Frontend/src/App.jsx`
- `Frontend/src/api/axios.js`
- `Frontend/src/pages/*` (6 pages)
- `Frontend/package.json`
- `.gitignore`
