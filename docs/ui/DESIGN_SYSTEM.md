# HireSphere design system

**Date:** 2026-07-21  
**Target:** WCAG 2.2 AA for normal application content  
**Brand direction:** Deep teal + slate (avoids purple/cream/newspaper AI defaults)

## Foundations

- Tokens: `Frontend/src/styles/tokens.css`
- Components + layout utilities: `Frontend/src/styles/design-system.css`
- Primitives: `Frontend/src/components/ui/primitives.jsx`
- Patterns: `Frontend/src/components/ui/patterns.jsx`, `Modal.jsx`
- Role shell: `Frontend/src/components/layout/RoleShell.jsx`

## Principles

1. Semantic HTML and visible focus
2. Labels for every form control
3. Status not by colour alone (badge text + marker)
4. `prefers-reduced-motion` respected
5. Mobile navigation with Escape close and accessible names
6. Server-side authorization remains authoritative

## Route inventory note

All existing public, Candidate, Recruiter, Hiring Manager and Administrator routes remain available. Phase 9.1 introduces the shared system; Phase 9.2 applies responsive portal polish without removing verified workflows.
