# Phase 3 Live UAT script (PowerShell)
# Invoked by agent; records PASS/FAIL to console. Does not print secrets.

$ErrorActionPreference = 'Stop'
$base = $env:HIRES_API_BASE
if (-not $base) { $base = 'http://127.0.0.1:5167' }

function Invoke-Json {
    param($Method, $Path, $Body = $null, $Token = $null)
    $headers = @{ 'Content-Type' = 'application/json' }
    if ($Token) { $headers.Authorization = "Bearer $Token" }
    $params = @{
        Method = $Method
        Uri = "$base$Path"
        Headers = $headers
        UseBasicParsing = $true
    }
    if ($null -ne $Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 6)
    }
    try {
        $resp = Invoke-WebRequest @params
        return @{ Ok = $true; Status = [int]$resp.StatusCode; Body = ($resp.Content | ConvertFrom-Json) }
    }
    catch {
        $status = $null
        $raw = $null
        if ($_.Exception.Response) {
            $status = [int]$_.Exception.Response.StatusCode
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $raw = $reader.ReadToEnd()
            } catch {}
        }
        $parsed = $null
        if ($raw) { try { $parsed = $raw | ConvertFrom-Json } catch {} }
        return @{ Ok = $false; Status = $status; Body = $parsed; Raw = $raw; Error = $_.Exception.Message }
    }
}

$results = @()
function Record($Id, $Role, $Endpoint, $Expected, $Actual, $Pass) {
    $script:results += [pscustomobject]@{
        Id = $Id; Role = $Role; Endpoint = $Endpoint; Expected = $Expected; Actual = $Actual; Result = $(if ($Pass) { 'PASS' } else { 'FAIL' })
    }
    Write-Output ("[{0}] {1} | {2} | {3}" -f $(if ($Pass) {'PASS'} else {'FAIL'}), $Id, $Endpoint, $Actual)
}

$suffix = Get-Random
$candEmail = "uat-cand-$suffix@example.com"
$candPass = "CandidateUatPass123!"
$recEmail = "uat-rec-$suffix@example.com"
$adminEmail = $env:HIRESPHERE_SEED_ADMIN_EMAIL
$adminPass = $env:HIRESPHERE_SEED_ADMIN_PASSWORD

# C1 register
$r = Invoke-Json Post '/api/auth/register/candidate' @{
    firstName='Uat'; lastName='Candidate'; email=$candEmail; password=$candPass; confirmPassword=$candPass; acceptTerms=$true; role='Admin'
}
Record 'C1' 'Candidate' 'POST /api/auth/register/candidate' '200 Candidate' ("{0} role={1}" -f $r.Status, $r.Body.role) ($r.Ok -and $r.Body.role -eq 'Candidate')
$candToken = $r.Body.token

# C2 login
$r = Invoke-Json Post '/api/auth/login' @{ email=$candEmail; password=$candPass }
Record 'C2' 'Candidate' 'POST /api/auth/login' '200 + token' ("{0}" -f $r.Status) ($r.Ok -and $r.Body.token)
$candToken = $r.Body.token

# C3 me
$r = Invoke-Json Get '/api/auth/me' $null $candToken
Record 'C3' 'Candidate' 'GET /api/auth/me' 'safe DTO Candidate' ("{0} role={1} hasHash={2}" -f $r.Status, $r.Body.role, ($null -ne $r.Body.passwordHash)) ($r.Ok -and $r.Body.role -eq 'Candidate' -and $null -eq $r.Body.passwordHash)

# C4 denied admin
$r = Invoke-Json Get '/api/admin/users' $null $candToken
Record 'C4' 'Candidate' 'GET /api/admin/users' '403' ("{0}" -f $r.Status) (-not $r.Ok -and $r.Status -eq 403)

# C5 denied recruiter jobs
$r = Invoke-Json Get '/api/Jobs/MyJobs' $null $candToken
Record 'C5' 'Candidate' 'GET /api/Jobs/MyJobs' '403' ("{0}" -f $r.Status) (-not $r.Ok -and $r.Status -eq 403)

# C6 change password
$newPass = "CandidateUatPass456!"
$r = Invoke-Json Post '/api/auth/change-password' @{ currentPassword=$candPass; newPassword=$newPass; confirmPassword=$newPass } $candToken
Record 'C6' 'Candidate' 'POST /api/auth/change-password' '200' ("{0}" -f $r.Status) $r.Ok
$candPass = $newPass

# C7 logout
$r = Invoke-Json Post '/api/auth/logout' $null $candToken
Record 'C7' 'Candidate' 'POST /api/auth/logout' '200' ("{0}" -f $r.Status) $r.Ok

# C8 re-login after password change
$r = Invoke-Json Post '/api/auth/login' @{ email=$candEmail; password=$candPass }
Record 'C8' 'Candidate' 'POST /api/auth/login (new password)' '200' ("{0}" -f $r.Status) $r.Ok
$candToken = $r.Body.token

# R1 recruiter request
$r = Invoke-Json Post '/api/auth/recruiter-requests' @{
    fullName='Uat Recruiter'; businessEmail=$recEmail; organizationName="Uat Org $suffix"; message='Please approve'
}
Record 'R1' 'Public' 'POST /api/auth/recruiter-requests' '200 Pending' ("{0} status={1}" -f $r.Status, $r.Body.status) ($r.Ok -and $r.Body.status -eq 'Pending')
$reqId = $r.Body.id

# A1 admin login
$r = Invoke-Json Post '/api/auth/login' @{ email=$adminEmail; password=$adminPass }
Record 'A1' 'Admin' 'POST /api/auth/login' '200' ("{0} role={1}" -f $r.Status, $r.Body.role) ($r.Ok -and $r.Body.role -eq 'Admin')
$adminToken = $r.Body.token

# A2 list recruiter requests
$r = Invoke-Json Get '/api/admin/recruiter-requests' $null $adminToken
Record 'A2' 'Admin' 'GET /api/admin/recruiter-requests' '200 list' ("{0} count={1}" -f $r.Status, @($r.Body).Count) $r.Ok

# A3 approve recruiter
$r = Invoke-Json Post "/api/admin/recruiter-requests/$reqId/approve" @{ notes='UAT approve' } $adminToken
Record 'A3' 'Admin' "POST /api/admin/recruiter-requests/$reqId/approve" '200' ("{0}" -f $r.Status) $r.Ok

# R2 recruiter cannot login until password known — approve creates random temp password
# Admin assigns role to a seeded approach: create candidate then promote? Better: login as admin and create HM from candidate clone.
# For recruiter: we cannot know temp password. Use admin role assignment path for a known user instead.
# Create a dedicated recruiter via register candidate then admin role patch.

$recKnownEmail = "uat-rec-known-$suffix@example.com"
$recKnownPass = "RecruiterUatPass123!"
$r = Invoke-Json Post '/api/auth/register/candidate' @{
    firstName='Uat'; lastName='RecKnown'; email=$recKnownEmail; password=$recKnownPass; confirmPassword=$recKnownPass; acceptTerms=$true
}
$recUserId = $r.Body.userId
$r = Invoke-Json Patch "/api/admin/users/$recUserId/roles" @{ role='Recruiter' } $adminToken
Record 'A4' 'Admin' "PATCH /api/admin/users/$recUserId/roles=Recruiter" '200' ("{0}" -f $r.Status) $r.Ok

# Assign organization 1 if exists — list users and patch org
$r = Invoke-Json Patch "/api/admin/users/$recUserId/organization" @{ organizationId=1; departmentId=$null } $adminToken
Record 'A5' 'Admin' "PATCH /api/admin/users/$recUserId/organization" '200 or 400 if org missing' ("{0}" -f $r.Status) ($r.Ok -or $r.Status -eq 400)

# R3 recruiter login
$r = Invoke-Json Post '/api/auth/login' @{ email=$recKnownEmail; password=$recKnownPass }
Record 'R3' 'Recruiter' 'POST /api/auth/login' '200 Recruiter' ("{0} role={1}" -f $r.Status, $r.Body.role) ($r.Ok -and $r.Body.role -eq 'Recruiter')
$recToken = $r.Body.token

# R4 denied admin
$r = Invoke-Json Get '/api/admin/users' $null $recToken
Record 'R4' 'Recruiter' 'GET /api/admin/users' '403' ("{0}" -f $r.Status) (-not $r.Ok -and $r.Status -eq 403)

# R5 my jobs accessible
$r = Invoke-Json Get '/api/Jobs/MyJobs' $null $recToken
Record 'R5' 'Recruiter' 'GET /api/Jobs/MyJobs' '200' ("{0}" -f $r.Status) $r.Ok

# HM create
$hmEmail = "uat-hm-$suffix@example.com"
$hmPass = "HiringManagerUatPass123!"
$r = Invoke-Json Post '/api/auth/register/candidate' @{
    firstName='Uat'; lastName='HM'; email=$hmEmail; password=$hmPass; confirmPassword=$hmPass; acceptTerms=$true
}
$hmUserId = $r.Body.userId
$r = Invoke-Json Patch "/api/admin/users/$hmUserId/roles" @{ role='HiringManager' } $adminToken
Record 'A6' 'Admin' "PATCH /api/admin/users/$hmUserId/roles=HiringManager" '200' ("{0}" -f $r.Status) $r.Ok

$r = Invoke-Json Post '/api/auth/login' @{ email=$hmEmail; password=$hmPass }
Record 'H1' 'HiringManager' 'POST /api/auth/login' '200 HiringManager' ("{0} role={1}" -f $r.Status, $r.Body.role) ($r.Ok -and $r.Body.role -eq 'HiringManager')
$hmToken = $r.Body.token

$r = Invoke-Json Get '/api/admin/users' $null $hmToken
Record 'H2' 'HiringManager' 'GET /api/admin/users' '403' ("{0}" -f $r.Status) (-not $r.Ok -and $r.Status -eq 403)

$r = Invoke-Json Get '/api/Applications/MyApplications' $null $hmToken
Record 'H3' 'HiringManager' 'GET /api/Applications/MyApplications' '403' ("{0}" -f $r.Status) (-not $r.Ok -and $r.Status -eq 403)

# A7 disable user
$r = Invoke-Json Patch "/api/admin/users/$hmUserId/status" @{ status='Inactive' } $adminToken
Record 'A7' 'Admin' "PATCH /api/admin/users/$hmUserId/status=Inactive" '200' ("{0}" -f $r.Status) $r.Ok

$r = Invoke-Json Post '/api/auth/login' @{ email=$hmEmail; password=$hmPass }
Record 'H4' 'HiringManager' 'POST /api/auth/login (disabled)' '401 disabled' ("{0} msg={1}" -f $r.Status, $r.Body.message) (-not $r.Ok -and $r.Status -eq 401)

# S1 sanitized invalid login
$r = Invoke-Json Post '/api/auth/login' @{ email='missing-$suffix@example.com'; password='wrong' }
$msg = [string]$r.Body.message
Record 'S1' 'Public' 'POST /api/auth/login invalid' 'sanitized 401' ("{0} msg={1}" -f $r.Status, $msg) (-not $r.Ok -and $r.Status -eq 401 -and $msg -match 'Invalid email or password' -and $msg -notmatch 'does not exist')

# S2 CORS preflight check omitted (manual note); check config presence via me without auth
$r = Invoke-Json Get '/api/auth/me'
Record 'S2' 'Public' 'GET /api/auth/me unauth' '401' ("{0}" -f $r.Status) (-not $r.Ok -and $r.Status -eq 401)

# Cross-candidate IDOR: create second candidate and try first to read second profile
$cand2Email = "uat-cand2-$suffix@example.com"
$r = Invoke-Json Post '/api/auth/register/candidate' @{
    firstName='Other'; lastName='Cand'; email=$cand2Email; password=$candPass; confirmPassword=$candPass; acceptTerms=$true
}
$cand2Token = $r.Body.token
$r = Invoke-Json Get '/api/CandidateProfiles/me' $null $cand2Token
$profile2Id = $r.Body.id
$r = Invoke-Json Get "/api/CandidateProfiles/$profile2Id" $null $candToken
Record 'S3' 'Candidate' "GET /api/CandidateProfiles/$profile2Id (other)" '403' ("{0}" -f $r.Status) (-not $r.Ok -and $r.Status -eq 403)

$fail = @($results | Where-Object { $_.Result -eq 'FAIL' }).Count
$pass = @($results | Where-Object { $_.Result -eq 'PASS' }).Count
Write-Output "SUMMARY pass=$pass fail=$fail total=$($results.Count)"

$outDir = Join-Path $PSScriptRoot '..\docs\testing'
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }
$mdPath = Join-Path $outDir 'PHASE3_LIVE_UAT.md'
# Caller writes final markdown; emit JSON for agent
$results | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $outDir 'phase3_uat_results.json')
if ($fail -gt 0) { exit 1 } else { exit 0 }
