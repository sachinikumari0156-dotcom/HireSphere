# Form standards

- Visible `<label htmlFor>` (or wrapping label) for every control
- Required indicated with `*` (aria-hidden) plus `required` attribute
- Errors via `aria-invalid` + `aria-describedby` pointing at `role="alert"`
- Hints via `aria-describedby`
- Primary actions use `Button` with disabled/loading to prevent double submit
- Destructive actions prefer `Modal` confirmation (Phase 9.2 expands coverage)
- File inputs use labelled `FileUpload` with size/type guidance
- Autocomplete and correct `type` attributes on auth forms remain required
