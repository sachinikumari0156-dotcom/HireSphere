# Auth API

**Base path:** `/api/auth` and `/api/admin`  
**Auth:** Bearer JWT unless noted anonymous

## Public / auth

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/auth/register/candidate` | Anonymous | Candidate self-registration |
| POST | `/api/auth/recruiter-requests` | Anonymous | Submit recruiter access request |
| POST | `/api/auth/login` | Anonymous | Login; returns JWT + safe user fields |
| GET | `/api/auth/me` | Authenticated | Current user DTO (no password hash) |
| POST | `/api/auth/change-password` | Authenticated | Requires current password |
| POST | `/api/auth/logout` | Authenticated | Client discard + audit log |

### Candidate register body

```json
{
  "firstName": "Ada",
  "lastName": "Lovelace",
  "email": "ada@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "acceptTerms": true
}
```

### Login body

```json
{
  "email": "ada@example.com",
  "password": "SecurePass123!"
}
```

## Admin

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/admin/recruiter-requests` | AdministratorOnly | List requests |
| POST | `/api/admin/recruiter-requests/{id}/approve` | AdministratorOnly | Approve + create Recruiter |
| POST | `/api/admin/recruiter-requests/{id}/reject` | AdministratorOnly | Reject |
| GET | `/api/admin/users` | AdministratorOnly | List users (safe DTO) |
| PATCH | `/api/admin/users/{id}/status` | AdministratorOnly | Enable/disable |
| PATCH | `/api/admin/users/{id}/roles` | AdministratorOnly | Assign role |
| PATCH | `/api/admin/users/{id}/organization` | AdministratorOnly | Assign org/department |

Documented in Swagger when the API is running.
