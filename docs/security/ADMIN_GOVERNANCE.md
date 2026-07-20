# Administrator Governance

**Phase:** 7.1

## Authorization

- Endpoints require authenticated **active** `Admin` role (`AdministratorOnly`).
- Client-supplied role/org/permission fields are ignored for authorization; identity comes from JWT + DB.

## Last-Administrator protection

- An Admin cannot disable their own account via status PATCH.
- The last active global Admin cannot be disabled.
- The last Admin role assignment cannot be removed.

## Token / session approach

1. Login rejects `Inactive` / `Suspended` / `PendingApproval`.
2. Critical status/role changes rotate `User.SecurityStamp`.
3. Sensitive Admin actions always load targets from the database (not client claims).
4. JWT lifetime remains short; `/auth/me` refreshes status from DB.

Password reset delivery is Phase 8 unless a provider is configured.
