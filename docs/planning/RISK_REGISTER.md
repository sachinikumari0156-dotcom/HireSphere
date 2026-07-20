# HireSphere — Risk Register

**Last updated:** 2026-07-20 (Phase 3)

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
| R-12 | No automated tests | Medium | High | 33 backend tests covering auth/RBAC | Agent | MITIGATED — expand in Phase 10 |
| R-13 | Large scope vs deadline | High | High | Mandatory-first tiers | Team | OPEN |
| R-14 | External integration credentials | Medium | Medium | Adapter + honest BLOCKED status | Kalani | OPEN |
| R-15 | EF package/version mismatch | Low | Low | Aligned to EF Core 10.0.10 / net10.0 | Agent | CLOSED |
| R-16 | Hireflow branding inconsistency | Low | Low | Auth pages updated; remaining UI in Phase 9 | Agent | MITIGATED |
| R-17 | Placeholder recruiter/manager/admin UIs | Medium | High | Phase 3 shells + policies; features in Phases 5–7 | Agent | MITIGATED — shells only |
| R-18 | Coursework/SRS in public repo | Low | Medium | Moved to ignored `local-spec/` | Kalani | CLOSED |
| R-19 | Academic integrity attribution | Low | Critical | Only Kalani SHAs for new work | Agent | MONITORING |
| R-20 | Runtime/SDK alignment | Low | Low | Retargeted API/tests to net10.0 for installed runtime | Agent | CLOSED |
| R-21 | Existing users with plaintext passwords | High | High | Re-register or migrate hashes after deploy | Kalani | OPEN |
| R-22 | SQL Server unavailable locally | Low | Medium | Express installed; migrations applied to HireSphereDev | Kalani | CLOSED |
| R-23 | Missing password reset / email verify | Medium | Medium | Documented as next quality; not claimed complete | Agent | OPEN |

---

## Risk response summary

**Closed in Phase 1–3:** R-01–R-05, R-07–R-11, R-15, R-18, R-20, R-22
**Mitigated / monitoring:** R-06, R-12, R-16, R-17, R-19
**Next focus:** R-13 (scope), R-17 (full role portals), R-21 (password migration), R-23 (reset/verify)
