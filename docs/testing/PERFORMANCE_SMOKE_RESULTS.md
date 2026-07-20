# Performance smoke results — Phase 10.1

**Date:** 2026-07-21  
**Environment:** xUnit TestWebApplicationFactory (local)  
**Pre-declared target:** critical read/login under **1500 ms**; error rate 0 for smoke calls  

| Operation | Result | Notes |
|-----------|--------|-------|
| Login | PASS | &lt; 1500 ms |
| Admin dashboard GET | PASS | &lt; 1500 ms |

Not a load/penetration or production capacity test. Modest single-call smoke only.
