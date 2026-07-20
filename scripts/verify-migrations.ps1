# Verify EF migrations against LocalDB without printing secrets.
# Usage (from repo root):
#   powershell -File scripts/verify-migrations.ps1

$ErrorActionPreference = "Stop"
$env:Path += ";C:\Program Files\dotnet"

$root = Split-Path -Parent $PSScriptRoot
$backend = Join-Path $root "Backend"
Set-Location $backend

Write-Host "Listing migrations..."
dotnet ef migrations list --project HireSphere.API --startup-project HireSphere.API

Write-Host "Updating database (idempotent)..."
dotnet ef database update --project HireSphere.API --startup-project HireSphere.API

$emptyName = "HireSphereMigrationEmpty_" + [Guid]::NewGuid().ToString("N").Substring(0, 8)
$emptyCs = "Server=(localdb)\MSSQLLocalDB;Database=$emptyName;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

Write-Host "Applying migrations to temporary empty database (name only logged)..."
Write-Host "Empty DB name: $emptyName"
$env:ConnectionStrings__DefaultConnection = $emptyCs
try {
  dotnet ef database update --project HireSphere.API --startup-project HireSphere.API
  Write-Host "Empty-database migration: PASS"
}
finally {
  Remove-Item Env:ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
  # Drop temporary DB
  $drop = @"
IF DB_ID(N'$emptyName') IS NOT NULL
BEGIN
  ALTER DATABASE [$emptyName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
  DROP DATABASE [$emptyName];
END
"@
  sqlcmd -S "(localdb)\MSSQLLocalDB" -Q $drop | Out-Null
  Write-Host "Temporary database dropped."
}

Write-Host "Migration verification complete."
