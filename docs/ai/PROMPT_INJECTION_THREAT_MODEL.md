# Prompt-injection threat model

Resume and job text are untrusted.

Mitigations:

1. Fixed server-side extraction logic (deterministic) — document text cannot change code paths
2. Sanitize display fields (null bytes, fenced code, “ignore previous instructions” patterns)
3. Max extracted text size
4. Strict DTO schema for API responses
5. External AI (when configured) must use delimited user content and JSON schema validation — currently **NotConfigured**

Residual risk: sophisticated adversarial PDFs; mitigated by not executing document content and requiring human confirm for profile updates.
