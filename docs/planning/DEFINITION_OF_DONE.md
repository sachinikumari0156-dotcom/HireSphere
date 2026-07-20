# HireSphere — Definition of Done

A mandatory coursework requirement is **complete** only when **all** conditions below are met for that requirement.

---

## Per-requirement checklist

1. **Implementation exists** — backend and/or frontend code merged on feature branch
2. **Connected** — frontend calls real API where applicable; no static-only dashboards
3. **Validation** — server-side validation; client validation where appropriate
4. **Authorization** — RBAC and resource ownership enforced on API; UI guards for UX
5. **Executable workflow** — end-to-end path demonstrable in running app
6. **Tests or repeatable manual script** — automated test or documented UAT step with expected result
7. **Evidence recorded** — matrix row updated; screenshot or test output referenced
8. **Clean build** — `dotnet build` and `npm run build` pass
9. **No known critical/high defect** — security issues resolved or explicitly tracked

---

## Status definitions

| Status | Meaning |
|--------|---------|
| NOT STARTED | No meaningful implementation |
| IN PROGRESS | Partial code; workflow not end-to-end |
| IMPLEMENTED | Code complete; not yet tested |
| TESTED | Automated or manual test executed once |
| VERIFIED | Tested + evidence recorded + reviewed |
| BLOCKED — EXTERNAL CREDENTIAL | Waiting on SMTP, SMS, OAuth, Azure, etc. |
| DEFERRED — OPTIONAL BONUS | Tier B; must not block submission |

Never mark VERIFIED without execution evidence.

---

## Phase completion

A phase is done when:

- All phase scope items reach at least IMPLEMENTED
- Build and lint pass
- No new critical security findings
- `PHASE_STATUS.md` and requirement matrix updated
- Focused commit on branch with correct author
- Push succeeds (when identity gate allows)

---

## Submission readiness (`COURSEWORK SUBMISSION READY`)

All Tier M matrix rows must be **VERIFIED** or honestly **BLOCKED — EXTERNAL CREDENTIAL** with documented fallback and manual verification steps.

Zero Tier M rows may remain NOT STARTED or IN PROGRESS.

Additional gates:

- [ ] Four role workflows demonstrable
- [ ] SQL Server migration from empty database
- [ ] No tracked secrets
- [ ] No plain-text passwords
- [ ] Postman/Swagger evidence for core APIs
- [ ] Report pack and diagrams present
- [ ] Individual contribution lists only verified Kalani work with commit SHAs
- [ ] PR open to `main` (not merged)

---

## Explicit non-goals for "done"

- Full enterprise production hardening
- All Tier Q enhancements
- Any Tier B bonus features
- Fabricated usability feedback or test PASS results
