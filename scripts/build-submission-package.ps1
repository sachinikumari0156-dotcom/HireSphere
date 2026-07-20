# Build HireSphere coursework submission package (local artifacts only)

param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"
Set-Location $RepoRoot

Write-Host "=== HireSphere submission package ==="
Write-Host "Repo: $RepoRoot"

$branch = (git branch --show-current).Trim()
if ($branch -ne "main") { throw "Must run on main (current: $branch)" }

$ErrorActionPreference = "Continue"
git fetch origin main 2>&1 | Out-Null
$ErrorActionPreference = "Stop"
$head = (git rev-parse HEAD).Trim()
$origin = (git rev-parse origin/main).Trim()
if ($head -ne $origin) { throw "HEAD ($head) does not match origin/main ($origin)" }
$status = git status --porcelain
if ($status) { Write-Warning "Working tree not clean - package will still build from current files." }

if (-not $SkipTests) {
    Write-Host "Tip: run full backend/frontend/e2e suites before packaging when possible."
}

$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$stageRoot = Join-Path $env:TEMP "HireSphere-submission-$stamp"
$stage = Join-Path $stageRoot "HireSphere"
New-Item -ItemType Directory -Force -Path $stage | Out-Null

$include = @(
    "Backend/HireSphere.API",
    "Backend/HireSphere.API.Tests",
    "Backend/dotnet-tools.json",
    "Frontend",
    "docs",
    "postman",
    "scripts",
    "README.md",
    ".gitignore"
)

foreach ($rel in $include) {
    $src = Join-Path $RepoRoot $rel
    if (-not (Test-Path $src)) { Write-Warning "Missing $rel"; continue }
    $dest = Join-Path $stage $rel
    $destParent = Split-Path $dest -Parent
    New-Item -ItemType Directory -Force -Path $destParent | Out-Null
    if (Test-Path $src -PathType Container) {
        robocopy $src $dest /E /NFL /NDL /NJH /NJS /nc /ns /np `
            /XD node_modules bin obj .tools App_Data playwright-report test-results dist .vs `
            /XF appsettings.Development.local.json appsettings.local.json *.user *.suo | Out-Null
    } else {
        Copy-Item $src $dest -Force
    }
}

$reportSrc = Join-Path $RepoRoot "artifacts/report"
if (Test-Path $reportSrc) {
    $reportDest = Join-Path $stage "artifacts/report"
    New-Item -ItemType Directory -Force -Path $reportDest | Out-Null
    Copy-Item (Join-Path $reportSrc "*") $reportDest -Force -ErrorAction SilentlyContinue
}

$outDir = Join-Path $RepoRoot "artifacts/submission"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$zipPath = Join-Path $outDir "HireSphere_Coursework_Submission.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Compress-Archive -Path (Join-Path $stageRoot "*") -DestinationPath $zipPath -Force

$hash = Get-FileHash -Algorithm SHA256 -Path $zipPath
$hashPath = Join-Path $outDir "HireSphere_Coursework_Submission.sha256"
"$($hash.Hash)  HireSphere_Coursework_Submission.zip" | Set-Content $hashPath -Encoding ascii

$verifyRoot = Join-Path $env:TEMP "HireSphere-submission-verify-$stamp"
Expand-Archive -Path $zipPath -DestinationPath $verifyRoot -Force
$expected = @(
    "HireSphere/README.md",
    "HireSphere/docs/report/HIRESPHERE_FINAL_REPORT.md",
    "HireSphere/Backend/HireSphere.API/HireSphere.API.csproj",
    "HireSphere/Frontend/package.json"
)
$missing = @()
foreach ($e in $expected) {
    if (-not (Test-Path (Join-Path $verifyRoot $e))) { $missing += $e }
}

$manifest = Join-Path $outDir "SUBMISSION_MANIFEST.txt"
@(
    "HireSphere coursework submission package",
    "Created: $(Get-Date -Format o)",
    "Git HEAD: $head",
    "ZIP: $zipPath",
    "SHA256: $($hash.Hash)",
    "SizeBytes: $((Get-Item $zipPath).Length)",
    "VerifyExtract: $verifyRoot",
    "MissingExpected: $(if ($missing.Count) { ($missing -join ', ') } else { 'none' })",
    "Excludes: .git node_modules bin obj secrets local appsettings overrides Playwright auth state"
) | Set-Content $manifest -Encoding utf8

Write-Host "Package: $zipPath"
Write-Host "SHA256: $($hash.Hash)"
Write-Host "Manifest: $manifest"
if ($missing.Count) { throw ("Verification failed: missing " + ($missing -join ', ')) }
Write-Host "ZIP extraction verification: PASS"
