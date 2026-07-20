# Final requirement closure

**Date:** 2026-07-21  
**Report SHA context:** Phase 11 submission pack  

## Verified (representative mandatory set)

Four role portals, auth/RBAC, LocalDB SQL Server model, core REST APIs, Swagger, security controls listed in Phase 10, deterministic AI + human review, local storage, Phase 9 responsive/a11y, automated tests, four-role UAT, Postman collection, ADRs, evidence pack, contribution disclosure.

## Partially verified

- External integrations (AI/SMTP/SMS/calendars/Blob/AV) — adapters present; cloud Not Configured  
- AI analytics depth beyond deterministic match/rank/parse  
- Architecture diagram **rendered** PNGs — sources complete, export PENDING  

## Blocked / pending

| ID | Item | Reason |
|----|------|--------|
| M-F07 | Real usability participants | 0 participants available |
| — | Report PDF | No Pandoc/PDF engine |
| — | Student ID / lecturer placeholders | PENDING USER |
| — | Demo video + NLEARN upload | PENDING USER |

## Not implemented / deferred (honest)

Refresh-token rotation, account lockout, password-reset email verification, Docker/CI (optional), formal pen-test, production deploy.

## Accepted limitations

Documented in `docs/release/FINAL_KNOWN_LIMITATIONS.md`.

## User actions still required

1. Confirm student ID and module title-page fields  
2. Export/verify PDF if required  
3. Record and upload demo video  
4. Build ZIP via `scripts/build-submission-package.ps1` on clean tree  
5. Submit via NLEARN and capture confirmation  
