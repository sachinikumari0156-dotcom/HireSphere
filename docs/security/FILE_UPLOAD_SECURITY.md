# File upload security

- Max size 5 MB
- Extension + MIME + magic-byte validation
- Blocked: exe, scripts, HTML/SVG, macro-enabled Office
- Randomized storage keys; sanitized display names
- Quarantine/Rejected/Deleted statuses block download
- Antivirus provider remains NotConfigured without a real scanner
- No long-lived public URLs; no SAS logging
