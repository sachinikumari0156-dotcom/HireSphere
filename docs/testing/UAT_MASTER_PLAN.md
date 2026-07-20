# UAT master plan — Phase 10.2

**Date:** 2026-07-21  
**Environment:** React `http://localhost:5173` + API `http://localhost:5167` + LocalDB `HireSphereDev`  
**Commit baseline:** after Phase 10.1 (`fe3c0cc`+)  
**Browser:** Playwright Chromium  
**OS:** Windows 10  

## Approach

Four-role UAT is executed via live browser Playwright journeys (Phases 4–9 + Phase 10 evidence spec) against seeded LocalDB data (`/api/e2e/ensure-*-portal`).  

This is **automated browser UAT**, not a substitute for formal participant usability studies.

## Provider status (truthful)

| Provider | Status |
|----------|--------|
| Deterministic AI | Verified |
| External AI | Not Configured |
| Development SMTP / MailHog | Depends on local config; Production SMTP Not Configured |
| Development SMS Mock | Verified |
| External SMS | Not Configured |
| Internal calendar + ICS | Verified |
| Google / Outlook Calendar | Not Configured |
| Local storage | Verified |
| Azure Blob / Azurite | Not Configured unless credentials present |
| Antivirus | Not Configured |

## Pass gate

Mandatory role workflows must PASS. Critical/High defects block verification.
