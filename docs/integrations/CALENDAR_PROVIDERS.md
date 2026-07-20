# Calendar providers

## Providers

| Provider | Status without OAuth |
|----------|----------------------|
| Internal Calendar | Healthy (interview records + timezone) |
| ICS generation | Healthy when tests pass |
| Google Calendar | **NotConfigured** |
| Outlook Calendar | **NotConfigured** |

## ICS

`GET /api/interviews/{id}/calendar.ics` returns a standards-oriented ICS with UID, DTSTAMP, DTSTART, DTEND, SUMMARY, DESCRIPTION, LOCATION. Private `InternalNotes` are never included.

## Sync stub

`POST /api/interviews/{id}/calendar/sync` updates internal `CalendarSyncStatus` and returns truthful Google/Outlook **NotConfigured** when OAuth is absent.

## Verification on this host (Phase 8.2)

- Internal + ICS: Verified in automated tests
- Google / Outlook: **NotConfigured**
