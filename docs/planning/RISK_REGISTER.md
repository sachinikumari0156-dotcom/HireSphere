# HireSphere — Risk Register

**Last updated:** 2026-07-20 (Phase 2)

| ID | Risk | Likelihood | Impact | Mitigation | Owner | Status |
|----|------|------------|--------|------------|-------|--------|
| R-01 | GitHub CLI / login mismatch | Low | High | Verified `kalanirashmika` | Kalani | CLOSED |
| R-02 | Git author misconfiguration | Low | High | Local author set to Kalani Rashmika | Kalani | CLOSED |
| R-03 | Working on wrong branch | Low | Medium | On `kalanirashmika/coursework-completion` | Kalani | CLOSED |
| R-04 | Local main divergence | Low | Medium | Feature branch based and pushed | Kalani | CLOSED |
| R-05 | Plain-text password storage | Low | Critical | Phase 1 BCrypt hashing | Agent | CLOSED |
| R-06 | Committed DB/JWT secrets | Medium | Critical | Placeholders in tracked config; rotate local credentials | Kalani/Agent | MITIGATED — rotate local values |
| R-07 | MySQL vs SQL Server mismatch | High | High | Phase 2 provider swap + new migration | Agent | MITIGATED / CLOSED |
| R-08 | Node.js unavailable | Low | Medium | Node installed; use explicit path if PATH stale | Kalani | CLOSED |
| R-09 | Split API URLs | Low | Medium | Centralized `VITE_API_BASE_URL` / `api/config.js` | Agent | CLOSED |
| R-10 | Open privileged self-registration | Low | High | Candidate-only public register + tests | Agent | CLOSED |
| R-11 | Unrestricted CORS | Low | High | Configured allowed origins | Agent | CLOSED |
| R-12 | No automated tests | High | High | Phase 2 test project (14 tests); expand in Phase 10 | Agent | MITIGATED — partial |
| R-13 | Large scope vs deadline | High | High | Mandatory-first tiers | Team | OPEN |
| R-14 | External integration credentials | Medium | Medium | Adapter + honest BLOCKED status | Kalani | OPEN |
| R-15 | EF package/version mismatch | Low | Low | EF Core 8.0.11 aligned across packages | Agent | MITIGATED |
| R-16 | Hireflow branding inconsistency | Low | Low | Auth pages updated; remaining UI in Phase 9 | Agent | MITIGATED |
| R-17 | Placeholder recruiter/manager/admin UIs | High | High | Phases 5–7 | Agent | OPEN |
| R-18 | Coursework/SRS in public repo | Low | Medium | Moved to ignored `local-spec/` | Kalani | CLOSED |
| R-19 | Academic integrity attribution | Low | Critical | Only Kalani SHAs for new work | Agent | MONITORING |
| R-20 | .NET SDK 10 vs project net8.0 | Low | Low | Build/test succeed; EF CLI needs roll-forward | Agent | MONITORING |
| R-21 | Existing users with plaintext passwords | High | High | Re-register or migrate hashes after deploy | Kalani | OPEN |
| R-22 | SQL Server unavailable locally | Low | Medium | Express installed; migration applied to HireSphereDev | Kalani | CLOSED |

---

## Risk response summary

**Closed in Phase 1–2:** R-01–R-05, R-07–R-11, R-18
**Mitigated / monitoring:** R-06, R-12 (partial), R-15, R-16, R-19, R-20
**Next focus:** R-22 (SQL Server apply), R-17 (role UIs), R-21 (password migration), R-13 (scope)
