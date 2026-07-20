# Error response standard

## Principles

- Clients receive stable HTTP status codes and short, user-safe messages.
- Internal exception details and stack traces are not returned to browsers in production-like configurations.
- Phase 10 API contract tests assert absence of raw stack traces in representative error bodies.

## Typical statuses

| Status | Meaning |
|--------|---------|
| 400 | Validation / bad request |
| 401 | Missing or invalid token |
| 403 | Authenticated but not permitted (or safe denial) |
| 404 | Resource missing or not visible |
| 409 | Conflict (duplicate application, duplicate final decision, etc.) |
| 422 | Validation semantics where used |
| 500 | Unexpected server failure (generic client message) |

## File / export errors

Upload validation failures return 400 with reason codes where implemented (extension, size, MIME).  
Unauthorized document download returns 403/404 without leaking storage keys.
