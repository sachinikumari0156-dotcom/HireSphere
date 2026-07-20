# Audit and monitoring (Phase 7.2)

## Audit logs

- Read-only Admin UI/API: filter by date, actor, action, entity type, success, correlation ID
- Paginated list + CSV export
- Records exclude passwords, hashes, JWTs, tokens, connection strings
- Formula injection prevention: values starting with `=+-@` are prefixed with `'`

## Monitoring

Truthful LocalDB-backed counts:

- API health (process responding)
- Database connectivity (`CanConnect`)
- Pending recruiter requests, disabled accounts, assessments, interviews, final decisions

Phase 8 providers (email, SMS, calendar, cloud storage) report **NotConfigured** until configured and verified. No fake Connected states.

## Analytics

Organization-scoped recruitment, department, and skill metrics from LocalDB. Empty filters return truthful empty series. Predictive AI is out of scope until Phase 8.
