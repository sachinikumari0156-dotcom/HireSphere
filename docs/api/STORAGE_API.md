# Storage API

- `POST /api/candidate/resumes` / `documents` — authorized Candidate upload
- `GET /api/candidate/documents/{id}/download` — own documents only
- `GET /api/documents/{id}/download` — owner or authorized recruitment staff
- `GET /api/applications/{applicationId}/documents` — authorized application scope
- `GET /api/admin/storage/status`
- `POST /api/admin/storage/health-check`
- `POST /api/admin/storage/migrations/dry-run`
- `POST /api/admin/storage/migrations/execute` — disabled by default

DTOs omit storage keys and absolute paths.
