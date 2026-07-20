# HireSphere — Risk Register

**Last updated:** 2026-07-20 (Phase 0)

| ID | Risk | Likelihood | Impact | Mitigation | Owner | Status |
|----|------|------------|--------|------------|-------|--------|
| R-01 | GitHub CLI not installed; cannot verify `kalanirashmika` login | High | High | Install `gh`, run `gh auth login`; block commits until verified | Kalani | OPEN |
| R-02 | Git author not configured; commits would have wrong attribution | High | High | `git config --local user.name/email` before first commit | Kalani | OPEN |
| R-03 | Working on `main` instead of feature branch | High | Medium | Create `kalanirashmika/coursework-completion` after pull | Agent/Kalani | OPEN |
| R-04 | Local `main` behind `origin/main` by 2 commits | Medium | Medium | `git pull --ff-only origin main` before branching | Kalani | OPEN |
| R-05 | Plain-text password storage | High | Critical | Phase 1: bcrypt/Identity hasher | Agent | OPEN |
| R-06 | Committed DB password and JWT key in appsettings | High | Critical | Rotate credentials; move to secrets; see SECRET_ROTATION_REQUIRED | Kalani/Agent | OPEN |
| R-07 | MySQL vs SQL Server coursework mismatch | High | High | Phase 2 migration to EF SqlServer | Agent | OPEN |
| R-08 | Node.js not installed; frontend unverified | High | Medium | Install Node LTS; run lint/build | Kalani | OPEN |
| R-09 | Split API URLs (5167 vs 7000) cause runtime failures | High | Medium | Single env-based base URL in Phase 3 | Agent | OPEN |
| R-10 | Open role self-registration (Admin escalation) | High | High | Phase 1/3: restrict privileged roles | Agent | OPEN |
| R-11 | Unrestricted CORS | Medium | High | Phase 1: configured origins | Agent | OPEN |
| R-12 | No automated tests | High | High | Phase 10 test project | Agent | OPEN |
| R-13 | Large scope vs deadline | High | High | Mandatory-first tiers; phase commits | Team | OPEN |
| R-14 | External integration credentials unavailable | Medium | Medium | Adapter + mock; mark BLOCKED honestly | Kalani | OPEN |
| R-15 | EF Core 9 packages on net8.0 | Low | Low | Align package versions in Phase 2 if issues arise | Agent | MONITORING |
| R-16 | Hireflow branding inconsistency | Medium | Low | Phase 9 UI pass | Agent | OPEN |
| R-17 | Placeholder recruiter/manager/admin UIs | High | High | Phases 5–7 | Agent | OPEN |
| R-18 | Coursework doc not in `local-spec/` | Low | Low | Kalani copies doc locally | Kalani | OPEN |
| R-19 | Academic integrity — attributing others' commits | Low | Critical | Never amend others' history; list only verified Kalani SHAs | Agent | MONITORING |
| R-20 | .NET SDK 10 vs project net8.0 | Low | Low | Build succeeded; pin SDK if CI issues | Agent | MONITORING |

---

## Risk response summary

**Immediate (before Phase 1 code changes):** R-01, R-02, R-03, R-04, R-06 rotation awareness  
**Phase 1:** R-05, R-06, R-10, R-11  
**Phase 2:** R-07  
**Phase 3:** R-09  
**Phases 5–7:** R-17  
**Phase 10:** R-12
