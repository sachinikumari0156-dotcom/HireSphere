# Administrator Portal — E2E Results

**Date:** 2026-07-20  
**Environment:** `(localdb)\MSSQLLocalDB` / `HireSphereDev`  
**API:** `http://127.0.0.1:5167`  
**Frontend:** `http://127.0.0.1:5173`  
**Seed:** `POST /api/e2e/ensure-admin-portal` (`HIRESPHERE_E2E_SEED_ENABLED=true`)

## Result

| Suite | Result |
|-------|--------|
| Administrator portal journey | **PASS** |
| Full Playwright (Candidate + Recruiter + HM + Admin) | **9/9 PASS** |

## Coverage highlights

- Live Admin dashboard metrics from LocalDB
- User search/filter, org/dept assignment, self-disable blocked, last-Admin role removal blocked
- Recruiter request approve/reject with reason
- Organization/department create; cross-org and archived-department assignment blocked
- Role/permission matrix
- Audit filter + CSV export; monitoring DB Connected; providers NotConfigured
- Recruitment analytics; FinalHire after HM recommendation; duplicate final blocked
- Candidate/Recruiter/HM denied `/admin` and Admin APIs
- Session restore after reload; disabled Admin cannot login
- Responsive 1440 / 768 / 390; axe critical/serious = 0 on sampled Admin pages

## Evidence

`docs/evidence/phase7-admin/` — 28 PNG files (see `SCREENSHOT_INDEX.md`)

Passwords and tokens were not captured in screenshots.
