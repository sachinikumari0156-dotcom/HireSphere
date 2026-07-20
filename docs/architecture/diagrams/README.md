# Architecture diagrams

**Last updated:** 2026-07-21  
**Author:** Chinthaka Jayaweera  

## Source

Maintainable Mermaid sources live in `source/`. They describe the **implemented** modular monolith (ASP.NET Core API + React SPA + SQL Server / LocalDB), not an imaginary microservice mesh.

External providers that were **Not Configured** during verification are labelled accordingly in diagrams.

## Rendered images

`rendered/` is reserved for exported PNG/SVG outputs.

**Status:** No automated Mermaid renderer was available in this environment.  
**Do not claim rendered PNG/SVG files exist** until they are generated and committed.

To render locally (example):

```powershell
# Requires mermaid-cli (mmdc) if installed
# npx -y @mermaid-js/mermaid-cli -i source/01-system-context.mmd -o rendered/01-system-context.png
```

GitHub and many Markdown viewers render Mermaid blocks inline from the final report.
