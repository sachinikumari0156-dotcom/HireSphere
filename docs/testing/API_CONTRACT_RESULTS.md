# API contract results — Phase 10.1

**Date:** 2026-07-21

| Check | Result |
|-------|--------|
| Unauthorized without stack trace | PASS |
| Swagger JSON loads | PASS |
| Login path documented in OpenAPI | PASS |
| Mass-assignment role on register ignored | PASS |
| Malformed/missing JWT → 401 | PASS |

OpenAPI snapshot process: GET `/swagger/v1/swagger.json` in tests. No live tokens in evidence.
