# Accessibility standard (Phase 9)

**Target:** WCAG 2.2 AA  
**Tools:** axe-core / `@axe-core/playwright`, React Testing Library, keyboard Playwright

## Required behaviours

- Visible `:focus-visible`
- Skip link to main content
- One primary page heading (`h1`) per view
- No colour-only status
- Dialogs: name, modal, trap, Escape, restore focus
- Mobile menus: accessible name + Escape
- Charts (Phase 9.2+): title + text/table alternative

## Honest limits

Full NVDA/JAWS/VoiceOver certification is **not** claimed in Phase 9 unless a real screen-reader session is recorded. Semantic review covers markup landmarks/labels.
