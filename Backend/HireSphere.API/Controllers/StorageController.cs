using HireSphere.API.Data;
using HireSphere.API.Services;
using HireSphere.API.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class StorageController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILocalFileStorageService _storage;
    private readonly IStorageAdminService _admin;
    private readonly IFileStorageProvider _provider;

    public StorageController(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        ILocalFileStorageService storage,
        IStorageAdminService admin,
        IFileStorageProvider provider)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
        _admin = admin;
        _provider = provider;
    }

    [HttpGet("documents/{id:int}/download")]
    public async Task<IActionResult> DownloadById(int id)
    {
        if (_currentUser.UserId is not int userId) return Unauthorized();

        var document = await _db.CandidateDocuments
            .Include(d => d.CandidateProfile)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (document is null) return NotFound(new { message = "Document not found." });
        if (document.IsDeleted || document.ValidationStatus is "Quarantined" or "Deleted" or "Rejected")
            return BadRequest(new { message = "Document is unavailable." });

        var allowed = document.CandidateProfile.UserId == userId
            || await CanRecruitmentAccessCandidateAsync(userId, document.CandidateProfile.UserId);
        if (!allowed) return Forbid();

        var (ok, error, content, contentType, _) = await _storage.OpenReadAsync(document.FilePath);
        if (!ok || content is null) return NotFound(new { message = error ?? "File not found." });
        return File(content, contentType ?? "application/octet-stream", document.FileName);
    }

    [HttpGet("applications/{applicationId:int}/documents")]
    public async Task<IActionResult> ApplicationDocuments(int applicationId)
    {
        if (_currentUser.UserId is not int userId) return Unauthorized();
        var application = await _db.Applications
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == applicationId);
        if (application is null) return NotFound(new { message = "Application not found." });

        var allowed = application.CandidateId == userId
            || await CanRecruitmentAccessCandidateAsync(userId, application.CandidateId);
        if (!allowed) return Forbid();

        var profile = await _db.CandidateProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == application.CandidateId);
        if (profile is null) return Ok(Array.Empty<object>());

        var docs = await _db.CandidateDocuments.AsNoTracking()
            .Where(d => d.CandidateProfileId == profile.Id && !d.IsDeleted)
            .OrderByDescending(d => d.UploadedAtUtc)
            .Select(d => new
            {
                d.Id,
                documentType = d.DocumentType.ToString(),
                d.FileName,
                d.ValidationStatus,
                d.ScanStatus,
                d.UploadedAtUtc
            })
            .ToListAsync();
        return Ok(docs);
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpGet("admin/storage/status")]
    public async Task<IActionResult> AdminStatus()
    {
        var statuses = await _admin.GetStatusesAsync();
        var json = System.Text.Json.JsonSerializer.Serialize(statuses);
        if (json.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase)
            || json.Contains("AccountKey", StringComparison.OrdinalIgnoreCase)
            || json.Contains("SAS", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(500, new { message = "Status payload failed secret scrub." });
        }
        return Ok(statuses);
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpPost("admin/storage/health-check")]
    public async Task<IActionResult> HealthCheck()
    {
        var statuses = await _admin.GetStatusesAsync();
        return Ok(new { active = _provider.Name, statuses });
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpPost("admin/storage/migrations/dry-run")]
    public async Task<IActionResult> MigrationDryRun()
    {
        return Ok(await _admin.DryRunMigrationAsync());
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpPost("admin/storage/migrations/execute")]
    public IActionResult MigrationExecute()
    {
        return BadRequest(new
        {
            message = "Destructive migration execute is disabled by default. Use dry-run and documented manual process."
        });
    }

    private async Task<bool> CanRecruitmentAccessCandidateAsync(int actorUserId, int candidateUserId)
    {
        var actor = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == actorUserId);
        if (actor is null) return false;
        if (string.Equals(actor.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            return true;

        var hasSharedApplication = await _db.Applications.AsNoTracking()
            .AnyAsync(a => a.CandidateId == candidateUserId && (
                a.Job.RecruiterId == actorUserId
                || a.Job.HiringManagerUserId == actorUserId));
        return hasSharedApplication;
    }
}
