# SMS provider

## Architecture

`ISmsProvider` with `DevelopmentMockSmsProvider`.

## Status

| Provider | Status |
|----------|--------|
| Development Mock SMS | Healthy (mock only) |
| External SMS (Twilio/etc.) | **NotConfigured** |

Successful mock sends return status label **Development Mock** — never “Delivered by External SMS Provider”.

## Rules

- E.164 validation (`+` and 8–15 digits)
- Consent required (`SmsConsent`)
- Preferences enforced server-side
- Destinations masked in outbox (`***` / last-4 helper) — full numbers not logged

## Verification on this host (Phase 8.2)

- Development SMS Mock: Verified via unit/API tests
- External SMS: **NotConfigured**
