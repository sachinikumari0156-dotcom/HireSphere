# Candidate UAT Checklist

## Phase 4.1–4.3 automated

See prior API/Vitest coverage. Status remains covered by automated suites.

## Phase 4 browser E2E (2026-07-20)

| Step | Expected | Status |
|------|----------|--------|
| Full Candidate browser journey (Playwright) | Register → profile → jobs → apply → assessment → interview → notifications → authz → logout | **PASS** — `docs/testing/CANDIDATE_E2E_RESULTS.md` |
| Authorization boundaries | Cross-candidate + privileged APIs blocked | **PASS** |
| Responsive 1440 / 768 / 390 | Usable; no material overflow | **PASS** |
| Accessibility critical pages | No blocking critical axe issues (1 residual register contrast documented) | **PASS** |
| Screenshots | 23 genuine captures | **PASS** — `docs/evidence/phase4-candidate/` |

### Environment

- LocalDB `(localdb)\MSSQLLocalDB` / `HireSphereDev`
- SQL Express not available on verification host
- Cloud object storage: deferred Phase 8 (local storage abstraction used)

### Not in this cycle

| Step | Status |
|------|--------|
| Recruiter creates assignment/interview in UI | Phase 5 |
| Email/SMS delivery | Deferred — in-app only |
| Phase 5 Recruiter workflows | Not started |
