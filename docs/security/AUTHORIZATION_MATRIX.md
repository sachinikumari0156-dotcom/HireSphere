# Authorization matrix (Phase 8)

| Capability | Candidate | Recruiter | Hiring Manager | Admin |
|------------|-----------|-----------|----------------|-------|
| Candidate portal | Yes | No | No | No |
| Own resume parse / skill confirm | Yes | No | No | No |
| Own document upload/download | Yes | No | No | No |
| Cross-candidate document download | No | Authorized apps only | Assigned vacancies only | Policy |
| Recruiter portal / rankings | No | Yes | No | No |
| Hiring Manager portal | No | No | Yes (assigned) | No |
| Notification preferences (own) | Yes | Yes | Yes | Yes |
| Own notification deliveries | Yes | Yes | Yes | Yes |
| Admin integrations / storage status | No | No | No | Yes |
| Failed delivery retry | No | No | No | Yes |
| Storage migration dry-run | No | No | No | Yes |
| Approve Recruiter requests | No | No | No | Yes |
| FinalHire / FinalReject | No | No | No | Yes |
| Audit logs / exports | No | No | No | Yes |

JWT role string for administrators is **`Admin`**.
Storage keys, absolute paths, OAuth tokens, and provider secrets are never returned in normal DTOs.
