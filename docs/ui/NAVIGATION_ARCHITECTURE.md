# Navigation architecture

- Public: Home, Sign In, Get Started via `Navbar`
- Authenticated: Dashboard (role home), Preferences, Logout
- Candidate extra: My Profile
- Role portals: `RoleShell` with role-specific links (Recruiter / Hiring Manager / Administrator)
- Mobile: menu toggle with `aria-expanded`, `aria-controls`, Escape closes
- Unauthorized links are omitted from rendered nav arrays; APIs remain authoritative

Active route indication uses React Router `NavLink` + `.active` styles.
