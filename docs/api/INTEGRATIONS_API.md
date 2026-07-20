# Integrations API

## User

- `GET /api/users/notification-preferences`
- `PUT /api/users/notification-preferences`
- `GET /api/notifications/deliveries` (own deliveries only)

## Interviews / calendar

- `GET /api/interviews/{id}/calendar.ics`
- `POST /api/interviews/{id}/calendar/sync`

## Administrator

- `GET /api/admin/integrations/status`
- `POST /api/admin/integrations/{provider}/health-check`
- `GET /api/admin/notifications/failed`
- `POST /api/admin/notifications/{id}/retry`

## Safety

Responses never include SMTP passwords, OAuth tokens, API keys, or full phone numbers.
