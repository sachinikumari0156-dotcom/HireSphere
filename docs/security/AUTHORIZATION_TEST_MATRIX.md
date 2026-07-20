# Authorization test matrix — Phase 10

Extends `AUTHORIZATION_MATRIX.md` with automated verification references.

| Resource / action | Anonymous | Candidate | Recruiter | HM | Admin | Test reference |
|-------------------|-----------|-----------|-----------|----|-------|----------------|
| `/api/auth/me` | 401 | Own | Own | Own | Own | AuthControllerTests |
| Admin dashboard | 401 | 403 | 403 | 403 | 200 | Phase10 + Admin |
| Recruiter dashboard | 401 | 403 | 200 | 403 | 403 | Portal tests |
| HM dashboard | 401 | 403 | 403 | 200 | 403 | Portal tests |
| Cross-org job | — | — | Denied | — | — | Phase10QualityTests |
| Storage download other user | — | Denied | Scoped | Scoped | Policy | StoragePhase83 |
| Final decision | — | Denied | Denied | Denied | Allowed | Admin Phase 7 |
| Audit export | — | Denied | Denied | Denied | Allowed | Admin Phase 7 |

Disabled / suspended users: login denied (Phase10QualityTests).
