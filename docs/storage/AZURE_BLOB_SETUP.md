# Azure Blob setup

Azure Blob cloud verification requires secure credentials via User Secrets or environment variables:

- `Storage:AzureBlob:ConnectionString` or `Storage:AzureBlob:AccountUrl`

Until a real upload/download against Azure succeeds in a verified environment, status remains:

**Azure Blob cloud: NotConfigured**

Do not commit connection strings, account keys, or SAS tokens.
