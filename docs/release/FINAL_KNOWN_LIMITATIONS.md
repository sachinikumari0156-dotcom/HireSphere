# Final known limitations

**Date:** 2026-07-21  

## Development versus production providers

| Capability | Development / coursework | Production |
|------------|--------------------------|------------|
| AI | Deterministic verified | External AI Not Configured |
| Email | Dev SMTP / MailHog path where tested | Production SMTP Not Configured |
| SMS | Mock verified | External SMS Not Configured |
| Calendar | Internal + ICS verified | Google / Outlook Not Configured |
| Storage | Local verified | Azure Blob cloud Not Configured |
| Antivirus | Not Configured | Not Configured |

## Deployment

- No verified production deployment.
- No ASP.NET health-check endpoint registered.
- LocalDB is not a production HA database.

## Usability

- Real consented participants: **0** (PENDING/BLOCKED).
- Expert heuristic ≠ user testing.

## Accessibility

- Covered-route axe critical/serious = 0; full WCAG certification not claimed.

## Performance

- Smoke tests only; not capacity planning.

## Security

- Application security verification completed; formal penetration test not conducted.

## Future work

External providers, CI/CD, health checks, refresh tokens, participant usability study, production hardening.
