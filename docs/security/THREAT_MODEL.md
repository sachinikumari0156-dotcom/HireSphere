# Threat Model (Phase 3 scope)

**Last updated:** 2026-07-20

| Threat | Mitigation | Status |
|--------|------------|--------|
| Privilege escalation via public register `role` | No public role field; server assigns Candidate | Mitigated |
| Credential stuffing / account enumeration | Sanitized login errors | Partial |
| IDOR on candidate profiles/applications | Ownership checks + policies | Mitigated (core paths) |
| Cross-organization job access | Org/recruiter ownership on mutate | Mitigated (core paths) |
| Password storage | BCrypt hashes only | Mitigated |
| Secrets in source | Placeholders + User Secrets / env / ignored local JSON | Mitigated |
| JWT theft (XSS) | Short-lived access token; logout discards client token | Partial (prototype) |
| Token replay after logout | No server denylist | Residual |
| Brute-force login | No lockout yet | Residual |
| Password reset abuse | Feature not shipped | N/A |

Residual items are documented as next quality work, not claimed complete.
