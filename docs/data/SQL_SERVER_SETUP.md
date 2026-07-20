# HireSphere — SQL Server Setup

**Last updated:** 2026-07-20 (Phase 2)

---

## Prerequisites

- .NET 8 SDK (project targets `net8.0`; EF CLI may require `DOTNET_ROLL_FORWARD=Major` if only .NET 10 runtime is installed)
- SQL Server 2022 (local or Docker)
- EF Core tools (`dotnet-ef` 8.0.11 in `Backend/HireSphere.API/dotnet-tools.json`)

---

## Option A — Docker (recommended)

```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" `
  -p 1433:1433 --name hiresphere-sql `
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Wait for SQL Server to accept connections, then create the database (optional — migration can create schema):

```powershell
docker exec -it hiresphere-sql /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "YourStrong!Passw0rd" -C `
  -Q "IF DB_ID('HireSphereDB') IS NULL CREATE DATABASE HireSphereDB;"
```

---

## Option B — Local SQL Server / SSMS

1. Install SQL Server Developer or Express edition.
2. Enable TCP/IP on port **1433**.
3. Create database `HireSphereDB` (or let EF migrations create tables).

---

## Connection string configuration

**Do not commit real passwords.** Set via user-secrets, environment variables, or local untracked overrides.

### User secrets (development)

From `Backend/HireSphere.API`:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=HireSphereDB;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=True"
dotnet user-secrets set "Jwt:Key" "YOUR_JWT_SIGNING_KEY_AT_LEAST_32_CHARS"
```

### Environment variable (CI / containers)

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=HireSphereDB;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=True"
$env:Jwt__Key = "YOUR_JWT_SIGNING_KEY_AT_LEAST_32_CHARS"
```

Placeholder values remain in tracked `appsettings.json` / `appsettings.Development.json`.

---

## Apply migrations

From `Backend/HireSphere.API` (SQL Server must be running):

```powershell
$env:DOTNET_ROLL_FORWARD = "Major"
dotnet tool run dotnet-ef database update --project HireSphere.API.csproj
```

Initial migration name: **`InitialSqlServerCoreModel`** (`20260720103526_InitialSqlServerCoreModel`).

---

## Seed data

On startup, `Program.cs` calls `DbSeeder.SeedAsync` when the database is reachable. Demo user passwords are documented **only** in comments inside `Data/Seed/DbSeeder.cs` — rotate them for any shared environment.

---

## Verify connectivity

```powershell
dotnet run --project Backend/HireSphere.API/HireSphere.API.csproj
```

Swagger: `https://localhost:7xxx/swagger` (port from launch profile).

If seed is skipped, check logs for *Database is not reachable* and confirm connection string + migration status.

---

## Troubleshooting

| Symptom | Action |
|---------|--------|
| Login failed for user `sa` | Verify password, SQL auth enabled, container fully started |
| Certificate / encryption errors | Keep `TrustServerCertificate=True` for local dev |
| `dotnet ef` fails on missing net8.0 runtime | Set `$env:DOTNET_ROLL_FORWARD="Major"` or install .NET 8 runtime |
| Migration apply blocked | SQL Server not running — expected until local instance available |
