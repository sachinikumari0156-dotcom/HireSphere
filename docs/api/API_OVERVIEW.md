# API overview

**Base URL (local):** `http://127.0.0.1:5167`  
**Frontend (local):** `http://127.0.0.1:5173`  
**Auth:** JWT Bearer unless anonymous  

## Controller groups

| Area | Route prefix | Docs |
|------|--------------|------|
| Authentication | `/api/auth` | `AUTH_API.md` |
| Candidate | `/api/candidate`, `/api/CandidateProfiles` | `CANDIDATE_API.md` |
| Recruiter | `/api/recruiter` | `RECRUITER_API.md` |
| Hiring Manager | `/api/hiring-manager` | `HIRING_MANAGER_API.md` |
| Administrator | `/api/admin` | `ADMIN_API.md` |
| AI (candidate) | `/api/candidate` AI actions | `AI_API.md` |
| Integrations | `/api` integrations/health | `INTEGRATIONS_API.md` |
| Storage | `/api` storage | `STORAGE_API.md` |
| Jobs / Applications / Users | `/api/Jobs`, `/api/Applications`, `/api/Users` | role docs |
| E2E seed | `/api/e2e` | local/test only |

## Conventions

- Success: 200/201 as documented per action.
- Validation: 400 with safe messages.
- Unauthorized: 401; Forbidden: 403; Not found: 404; Conflict: 409.
- CSV/ICS downloads use appropriate content types and sanitized CSV escaping.
- Swagger UI available in Development.

## Related

- `ERROR_RESPONSE_STANDARD.md`
- `POSTMAN_COLLECTION_GUIDE.md`
- `postman/HireSphere.postman_collection.json`
