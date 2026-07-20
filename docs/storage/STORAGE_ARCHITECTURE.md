# Storage architecture

## Providers

| Provider | Status without cloud credentials |
|----------|----------------------------------|
| Local development storage | Healthy / Verified when upload/download exercised |
| Azurite | **NotConfigured** unless connection string set and operation succeeds |
| Azure Blob cloud | **NotConfigured** |
| Antivirus | **NotConfigured** |

## Design

- Private `App_Data/uploads` roots with randomized keys:
  `tenant/{org|personal}/candidate/{candidateId}/{category}/{guid}.ext`
- Original display names sanitized and stored separately
- Storage keys never returned in normal API DTOs
- Absolute paths never returned
- Logical delete with audit; quarantined/deleted files are not downloadable

## Validation

- Max 5 MB
- Extension allow-list (pdf, docx, png, jpg/jpeg)
- Blocked executables / macros / scripts
- MIME allow-list
- Magic-byte signature checks
- Path-traversal neutralization on display names

## Migration

`POST /api/admin/storage/migrations/dry-run` reports counts without changing data.
Execute is disabled by default.
