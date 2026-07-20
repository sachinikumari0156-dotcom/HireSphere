# Component library

## Available

| Component | Location | Notes |
|-----------|----------|-------|
| Button | primitives | primary/secondary/danger/ghost, loading |
| Input / FormField | primitives | label, hint, error association |
| Alert | primitives | success/warning/error/info |
| StatusBadge | primitives | text + marker |
| EmptyState / ErrorState | primitives | recovery action |
| Spinner | primitives | polite live status |
| PageHeader / ContentContainer | primitives | page framing |
| SkipLink | primitives | skip to `#main-content` |
| Modal | Modal.jsx | focus trap, Escape, restore focus |
| Tabs / Accordion / Pagination / FileUpload | patterns | keyboard support |
| RoleShell | layout | mobile menu, Escape |
| FilterDrawer | FilterDrawer.jsx | mobile filter toggle; desktop inline |

`friendlyStatus()` in `utils/statusLabels.js` maps API enums to user-facing labels.

Additional domain pages continue to use existing portal CSS classes while adopting tokens/layout shell.
