# HireSphere — Risk Register

**Last updated:** 2026-07-21 (Phase 11)

| ID | Risk | Likelihood | Impact | Mitigation | Owner | Status |
|----|------|------------|--------|------------|-------|--------|
| R-01 | GitHub CLI / login mismatch | Low | High | Verified push account `jkchinthaka` | Team | CLOSED |
| R-02 | Git author misconfiguration | Low | High | Local author Chinthaka for new commits; Kalani history preserved | Team | CLOSED |
| R-13 | Large scope vs deadline | Medium | Medium | Phases 0–11 delivered; residual optional items deferred | Team | MITIGATED |
| R-14 | External integration credentials | Medium | Medium | Adapters + truthful NotConfigured | Team | OPEN — external verification pending |
| R-19 | Academic integrity attribution | Low | Critical | Kalani history preserved; AI disclosure present | Team | MONITORING |
| R-21 | Existing users with plaintext passwords | Medium | High | BCrypt for new hashes; migrate legacy if any | Team | OPEN |
| R-23 | Missing password reset / email verify | Medium | Medium | Documented deferred quality | Team | OPEN |
| R-25 | Cloud document storage not verified | Medium | Medium | Local verified; Azure Blob NotConfigured | Team | OPEN |
| R-36 | Real usability participants unavailable | High | Medium | Heuristic + automated UAT; schedule participants | Chinthaka | OPEN — blocks full coursework VERIFIED |
| R-37 | NLEARN / video / title-page placeholders | High | Medium | Checklist marks PENDING USER | Chinthaka | OPEN — manual submission steps |
| R-38 | Production readiness mistaken for LocalDB demo | Medium | High | Explicit NOT PRODUCTION READY in release docs | Chinthaka | MITIGATED |

Additional historical risks from earlier phases remain in git history; closed verification risks R-28–R-34 stay CLOSED.

## Risk response summary

**Closed / mitigated through Phase 11:** core auth, portals, LocalDB, Phase 8–10 verification honesty, architecture packaging.  
**Remain open:** external providers (R-14/R-25), usability participants (R-36), portal submission steps (R-37), legacy password migration (R-21), password reset (R-23).
