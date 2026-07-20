# HireSphere — SQL Server Setup

**Last updated:** 2026-07-20 (Phase 2 verification)

---

## Prerequisites

- .NET 8 SDK (project targets `net8.0`; EF CLI may require `DOTNET_ROLL_FORWARD=Major` if only .NET 10 runtime is installed)
- SQL Server LocalDB, Express, Developer, or Docker SQL Server
- EF Core tools (`dotnet-ef` 8.0.11 in `Backend/HireSphere.API/dotnet-tools.json`)

---

## Option A — SQL Server LocalDB (preferred on Windows)

```text
Server=(localdb)\MSSQLLocalDB;Database=HireSphereDev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

Install SQL Server Express LocalDB if `sqllocaldb` is not available, then:

```powershell
sqllocaldb start MSSQLLocalDB
```

---

## Option B — SQL Server Express

```text
Server=localhost\SQLEXPRESS;Database=HireSphereDev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

---

## Option C — Docker SQL Server

Pull and run the official image. Supply `ACCEPT_EULA` and a strong `MSSQL_SA_PASSWORD` from your local environment only — do not commit the password.

```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=$env:MSSQL_SA_PASSWORD" `
  -p 1433:1433 --name hiresphere-sql `
  -d mcr.microsoft.com/mssql/server:2022-latest
```

---

## Connection string configuration

**Do not commit real passwords.** Use user-secrets, environment variables, or ignored local overrides (`appsettings.Development.local.json` is gitignored).

### User secrets (development)

From `Backend/HireSphere.API`:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<YOUR_LOCAL_CONNECTION_STRING>"
dotnet user-secrets set "Jwt:Key" "<YOUR_JWT_SIGNING_KEY_AT_LEAST_32_CHARS>"
```

### Environment variable

```powershell
$env:ConnectionStrings__DefaultConnection = "<YOUR_LOCAL_CONNECTION_STRING>"
$env:Jwt__Key = "<YOUR_JWT_SIGNING_KEY_AT_LEAST_32_CHARS>"
```

Tracked `appsettings*.json` files contain placeholders only.

---

## Apply migrations

From `Backend/HireSphere.API` (SQL Server must be running and the connection string configured):

```powershell
$env:DOTNET_ROLL_FORWARD = "Major"
dotnet tool restore
dotnet restore
dotnet build
dotnet tool run dotnet-ef migrations list --project HireSphere.API.csproj
dotnet tool run dotnet-ef database update --project HireSphere.API.csproj
```

Initial migration name: **`InitialSqlServerCoreModel`** (`20260720103526_InitialSqlServerCoreModel`).

---

## Secure development seeding

Reference/catalog seed (roles, permissions, organization, departments, skills) runs when the database is reachable.

**User account seeding is disabled by default.** Enable only when you explicitly supply configuration:

| Key / env var | Purpose |
|---------------|---------|
| `Seed:Enabled` / `HIRESPHERE_SEED_ENABLED` | Must be `true` to create demo users |
| `Seed:AdminEmail` / `HIRESPHERE_SEED_ADMIN_EMAIL` | Admin email used as the seed identity base |
| `Seed:AdminPassword` / `HIRESPHERE_SEED_ADMIN_PASSWORD` | Password hashed with BCrypt before persistence (never logged) |

Example (local only — do not commit values):

```powershell
dotnet user-secrets set "Seed:Enabled" "true"
dotnet user-secrets set "Seed:AdminEmail" "<YOUR_LOCAL_ADMIN_EMAIL>"
dotnet user-secrets set "Seed:AdminPassword" "<YOUR_LOCAL_ADMIN_PASSWORD>"
```

Rules:

- Seed skips safely when disabled or when credentials are missing.
- Passwords must be at least 12 characters.
- Passwords are hashed before storage; plaintext is never written to the database or logs.
- Seed is idempotent — running twice does not duplicate roles, skills, or users.

---

## Verify connectivity

```powershell
dotnet run --project Backend/HireSphere.API/HireSphere.API.csproj
```

Swagger uses the HTTPS port from `launchSettings.json`.

---

## Troubleshooting

| Symptom | Action |
|---------|--------|
| Cannot connect / login failed | Confirm instance name, Windows auth vs SQL auth, and local secrets |
| Certificate / encryption errors | Keep `TrustServerCertificate=True` for local dev |
| `dotnet ef` fails on missing net8.0 runtime | Set `$env:DOTNET_ROLL_FORWARD="Major"` or install .NET 8 runtime |
| Migration apply blocked | No SQL Server instance available on this machine |
| User seed skipped | Enable `Seed:Enabled` and supply admin email/password via secrets or env |
