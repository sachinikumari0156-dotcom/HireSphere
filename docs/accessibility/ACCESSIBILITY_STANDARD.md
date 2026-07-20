# Accessibility standard — HireSphere

**Target:** WCAG 2.2 Level AA for normal application content
**Phase:** 9
**Tools:** axe-core / `@axe-core/playwright`, keyboard Playwright checks, semantic HTML review

## Scope

Public, Candidate, Recruiter, Hiring Manager, and Administrator portals delivered through Phase 9.

## Requirements

- Visible focus indicators (`:focus-visible`)
- Skip link to `#main-content`
- One primary `main` landmark (page sections use `div`, not nested `main`)
- Labels associated with form controls
- Status not conveyed by colour alone (`StatusBadge` text + marker)
- Charts include text summary and table alternative where used
- `prefers-reduced-motion` respected in design-system CSS
- Dialogs: accessible name, Escape where appropriate, focus management (Modal component)

## Exceptions

No formal external accessibility certification is claimed. Full assistive-technology certification (NVDA/JAWS/VoiceOver user testing) is out of Phase 9 scope unless separately executed.
