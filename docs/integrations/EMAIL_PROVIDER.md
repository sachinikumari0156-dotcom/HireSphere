# Email provider

## Architecture

`IEmailProvider` with `SmtpEmailProvider` using `System.Net.Mail.SmtpClient`.

## Status labels (truthful)

| Environment | Status | Meaning |
|-------------|--------|---------|
| No `Email:Smtp:Host` | **NotConfigured** | Production Email Not Configured |
| localhost / MailHog host | **Configured** | Development SMTP only — not production verification |
| Non-local host configured | **Configured** | Mark **Healthy** only after a successful send/health check |

## Configuration (never commit secrets)

Use User Secrets or environment variables:

- `Email:Smtp:Host`
- `Email:Smtp:Port` (default 25)
- `Email:Smtp:From`
- `Email:Smtp:Username` / `Email:Smtp:Password` (optional)
- `Email:Smtp:EnableSsl`

Example MailHog development:

```
Email__Smtp__Host=localhost
Email__Smtp__Port=1025
Email__Smtp__From=noreply@hiresphere.local
```

## Templates

`NotificationTemplateRenderer` produces escaped HTML and plain-text bodies. User content is HTML-encoded. Assessment answer keys and private recruiter notes are never included.

## Verification on this host (Phase 8.2)

- Development SMTP: **NotConfigured** in default test/runtime until MailHog host is set locally
- Production Email: **NotConfigured**
