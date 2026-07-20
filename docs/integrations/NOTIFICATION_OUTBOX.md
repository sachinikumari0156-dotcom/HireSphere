# Notification outbox

## Entities

- `NotificationOutbox` — durable queued deliveries with idempotency key, attempt bounds, masked destination, correlation id, safe failure codes
- `UserNotificationPreference` — email/SMS/channel toggles and SMS consent

## Statuses

Queued → Processing → Sent | Delivered | Failed | Cancelled | Suppressed

## Behaviour

- In-app `NotificationWriter.CreateAsync` enqueues email/SMS outbox rows when preferences allow
- Mandatory security notifications can force email via `forceSecurityEmail`
- `NotificationOutboxProcessor` sends via `IEmailProvider` / `ISmsProvider` with max attempts
- `NotificationReminderWorker` (non-Testing) processes the outbox on a 5-minute cadence (idempotent)

## APIs

See `docs/api/INTEGRATIONS_API.md`.
