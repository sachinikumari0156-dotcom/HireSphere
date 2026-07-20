# Clean-clone verification

**Date:** 2026-07-21  
**Author:** Chinthaka Jayaweera  
**Method:** Documented clean-environment simulation against repository instructions (not a second physical machine).

## Verified on primary host (Phase 11 baseline)

| Step | Result |
|------|--------|
| `git clone` / pull `main` | PASS (this workspace) |
| Backend `dotnet restore` / Release build | PASS |
| Backend `dotnet test` Release | **132 PASS** |
| Frontend `npm run lint` | PASS |
| Frontend `npm run test:run` | **89 PASS** |
| Frontend `npm run build` | PASS |
| LocalDB migrations (when API not locking binaries) | PASS historically Phase 10; EF list may fail if `HireSphere.API.exe` locks build |
| Secure config via env / user secrets / ignored local files | Documented in README |
| Startup API `:5167` + Vite `:5173` | PASS (HTTP 200 during Phase 11) |

## Undocumented dependencies

None required beyond README prerequisites. Untracked secrets must be recreated by the developer; they are intentionally not in Git.

## Residual risk

A brand-new machine still needs LocalDB + .NET + Node installed and local JWT/connection configuration. That is expected, not a missing repo file.
