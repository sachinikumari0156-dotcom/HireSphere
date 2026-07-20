# Deployment prerequisites

1. .NET 10 SDK, Node.js LTS, Git, LocalDB or SQL Server.
2. Clone repository; `dotnet restore` / `npm ci`.
3. Configure ignored User Secrets / env for JWT and connection string.
4. `dotnet ef database update` against target database.
5. Do not enable production seed with plaintext passwords in tracked files.
6. Start API then Vite; run `dotnet test` and `npm run e2e` before demo.
7. External providers require separate credential provisioning and verification.
