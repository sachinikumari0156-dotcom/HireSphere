# HireSphere responsive strategy

**Phase:** 9.2  
**Target:** usable layouts from 320px through 1920px without required page-level horizontal scroll.

## Breakpoints

| Token | Width | Intent |
|-------|-------|--------|
| `--hs-bp-sm` | 480px | large phones |
| `--hs-bp-md` | 768px | tablet / mobile menu boundary |
| `--hs-bp-lg` | 1024px | laptop |
| `--hs-bp-xl` | 1280px | desktop |

Practical CSS splits used in Phase 9:

- **&lt; 768px** — mobile navigation toggle, filter drawer, table→card transforms
- **768px–1023px** — tablet: side-by-side where space allows, compact padding
- **≥ 1024px** — desktop tables, inline filters, expanded nav

## Viewports to verify (Phase 9.3)

320×568, 360×800, 390×844, 430×932, 768×1024, 1024×768, 1280×720, 1366×768, 1440×900, 1920×1080.

## Patterns

1. **RoleShell / Navbar** — accessible menu button; Escape closes; desktop shows full nav.
2. **FilterDrawer** — single filter tree; hidden behind toggle under 768px.
3. **portal-table-wrap--transform** — desktop table + mobile card list (not tiny scroll-only tables).
4. **portal-table-wrap** alone — intentional overflow with focusable region when a full table must remain.
5. **Charts** — text summary (`portal-chart-summary`) + HTML table alternative.
6. **Provider status** — `portal-provider-chip` for Not Configured (text + tone, not colour alone).

## Terminology

Use **Hiring Manager**, **Administrator**, **Candidate**, **Recruiter** in UI copy. Lifecycle/status enums are mapped through `friendlyStatus()` for display while API values remain unchanged.
