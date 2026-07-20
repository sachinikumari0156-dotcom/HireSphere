# Configuration matrix

| Setting | Development | Testing | Production template |
|---------|-------------|---------|---------------------|
| Database | LocalDB HireSphereDev | SQLite in-memory (factory) / optional LocalDB | SQL Server (ops-managed) |
| JWT | User secrets / env | Test key in factory | Secret store required |
| Seed | Gated | Disabled by default | Disabled |
| E2e seed API | HIRESPHERE_E2E_SEED_ENABLED | Off | Off |
| SMTP | Optional MailHog | Mock/NotConfigured | Not Configured until verified |
| SMS | Dev mock | Mock | Not Configured |
| Calendar | Internal + ICS | Internal | Google/Outlook Not Configured |
| Storage | Local | Local | Azure Blob Not Configured |
| CORS | localhost origins | Test host | Explicit allow-list |
| Swagger | On | On | Restrict in real production |

No production secrets are tracked in git.
