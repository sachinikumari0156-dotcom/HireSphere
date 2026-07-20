using HireSphere.API.Data;
using HireSphere.API.DTOs.Ai;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using HireSphere.API.Services.Ai;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface ICandidateAiService
{
    Task<(bool Ok, string? Error, ResumeAnalysisDto? Result)> ParseResumeAsync(int resumeId);
    Task<(bool Ok, string? Error, ResumeAnalysisDto? Result)> GetAnalysisAsync(int resumeId);
    Task<(bool Ok, string? Error, ResumeAnalysisDto? Result)> ConfirmAnalysisAsync(int resumeId, ConfirmResumeAnalysisDto dto);
    Task<(bool Ok, string? Error)> RejectAnalysisAsync(int resumeId);
    Task<(bool Ok, string? Error, CandidateAiStatusDto? Result)> SetConsentAsync(ExternalAiConsentDto dto);
    Task<(bool Ok, string? Error, CandidateAiStatusDto? Result)> GetStatusAsync();
}

public sealed class CandidateAiService : ICandidateAiService
{
    public const string HumanReviewNotice =
        "AI-generated insight. Final recruitment decisions must be reviewed by authorized users.";

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILocalFileStorageService _files;
    private readonly DeterministicResumeParsingProvider _deterministic;
    private readonly ExternalAiResumeParsingProvider _external;

    public CandidateAiService(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        ILocalFileStorageService files,
        DeterministicResumeParsingProvider deterministic,
        ExternalAiResumeParsingProvider external)
    {
        _db = db;
        _currentUser = currentUser;
        _files = files;
        _deterministic = deterministic;
        _external = external;
    }

    private async Task<(bool Ok, string? Error, CandidateProfile? Profile, int UserId)> RequireCandidateAsync()
    {
        if (_currentUser.UserId is not int userId || !_currentUser.IsInRole("Candidate"))
            return (false, "Unauthorized.", null, 0);
        var profile = await _db.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile is null) return (false, "Candidate profile not found.", null, userId);
        return (true, null, profile, userId);
    }

    public async Task<(bool Ok, string? Error, CandidateAiStatusDto? Result)> GetStatusAsync()
    {
        var (ok, error, profile, _) = await RequireCandidateAsync();
        if (!ok || profile is null) return (false, error, null);
        return (true, null, new CandidateAiStatusDto
        {
            AllowExternalAiProcessing = profile.AllowExternalAiProcessing,
            ExternalAiConsentAtUtc = profile.ExternalAiConsentAtUtc,
            DeterministicProviderStatus = _deterministic.GetStatus().Status,
            ExternalAiProviderStatus = _external.GetStatus().Status
        });
    }

    public async Task<(bool Ok, string? Error, CandidateAiStatusDto? Result)> SetConsentAsync(ExternalAiConsentDto dto)
    {
        var (ok, error, profile, userId) = await RequireCandidateAsync();
        if (!ok || profile is null) return (false, error, null);
        profile.AllowExternalAiProcessing = dto.AllowExternalAiProcessing;
        profile.ExternalAiConsentAtUtc = dto.AllowExternalAiProcessing ? DateTime.UtcNow : null;
        profile.UpdatedAtUtc = DateTime.UtcNow;
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "ai.consent",
            EntityType = nameof(CandidateProfile),
            EntityId = profile.Id,
            Details = $"AllowExternalAiProcessing={dto.AllowExternalAiProcessing}",
            Success = true,
            ActorRole = "Candidate",
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return await GetStatusAsync();
    }

    public async Task<(bool Ok, string? Error, ResumeAnalysisDto? Result)> ParseResumeAsync(int resumeId)
    {
        var (ok, error, profile, userId) = await RequireCandidateAsync();
        if (!ok || profile is null) return (false, error, null);

        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId && r.CandidateProfileId == profile.Id);
        if (resume is null) return (false, "Resume not found.", null);

        var ext = Path.GetExtension(resume.FileName)?.ToLowerInvariant() ?? string.Empty;
        if (ext is not (".pdf" or ".docx"))
            return (false, "Unsupported resume type. Only PDF and DOCX are supported.", null);

        var analysis = await _db.ResumeAnalyses.Include(a => a.ExtractedSkills)
            .FirstOrDefaultAsync(a => a.ResumeId == resumeId);
        if (analysis is null)
        {
            analysis = new ResumeAnalysis { ResumeId = resumeId, Status = ResumeAnalysisStatus.Processing };
            _db.ResumeAnalyses.Add(analysis);
        }
        else
        {
            analysis.Status = ResumeAnalysisStatus.Processing;
            analysis.FailureReason = null;
            _db.ExtractedSkills.RemoveRange(analysis.ExtractedSkills);
            analysis.ExtractedSkills.Clear();
        }

        await _db.SaveChangesAsync();

        var open = await _files.OpenReadAsync(resume.FilePath);
        if (!open.Ok || open.Content is null)
        {
            analysis.Status = ResumeAnalysisStatus.Failed;
            analysis.FailureReason = open.Error ?? "Could not open resume file.";
            analysis.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (false, analysis.FailureReason, Map(analysis));
        }

        await using (open.Content)
        {
            ParsedResumeResult parsed;
            var usedExternal = false;
            string? fallbackNote = null;

            if (profile.AllowExternalAiProcessing && _external.IsEnabled)
            {
                parsed = await _external.ParseAsync(open.Content, resume.FileName, open.ContentType ?? "application/octet-stream");
                if (parsed.FailureReason is not null)
                {
                    if (open.Content.CanSeek) open.Content.Position = 0;
                    parsed = await _deterministic.ParseAsync(open.Content, resume.FileName, open.ContentType ?? "application/octet-stream");
                    fallbackNote = "External AI unavailable; deterministic fallback active.";
                }
                else
                {
                    usedExternal = true;
                }
            }
            else
            {
                if (profile.AllowExternalAiProcessing && !_external.IsEnabled)
                    fallbackNote = "External AI NotConfigured; deterministic provider used.";
                parsed = await _deterministic.ParseAsync(open.Content, resume.FileName, open.ContentType ?? "application/octet-stream");
            }

            if (parsed.FailureReason is not null)
            {
                analysis.Status = ResumeAnalysisStatus.Failed;
                analysis.FailureReason = parsed.FailureReason;
                analysis.Provider = parsed.Provider;
                analysis.ProviderType = parsed.ProviderType;
                analysis.ProviderVersion = parsed.ProviderVersion;
                analysis.FallbackNote = fallbackNote;
                analysis.GeneratedAtUtc = DateTime.UtcNow;
                analysis.UpdatedAtUtc = DateTime.UtcNow;
                await AuditAsync(userId, "ai.resume.parse.failed", resumeId, parsed.FailureReason);
                await _db.SaveChangesAsync();
                return (false, parsed.FailureReason, Map(analysis));
            }

            analysis.Status = ResumeAnalysisStatus.ReviewRequired;
            analysis.Provider = usedExternal ? "ExternalAI" : "Deterministic";
            analysis.ProviderType = usedExternal ? "ExternalAI" : "Deterministic";
            analysis.ProviderVersion = parsed.ProviderVersion;
            analysis.AnalysisSummary = parsed.AnalysisSummary;
            analysis.ExtractedName = Truncate(parsed.Name, 200);
            analysis.ExtractedEmail = Truncate(parsed.Email, 256);
            analysis.ExtractedPhone = Truncate(parsed.Phone, 50);
            analysis.ExtractedSummary = Truncate(parsed.Summary, 2000);
            analysis.EstimatedYearsExperience = parsed.EstimatedYearsExperience;
            analysis.ConsentUsedExternal = usedExternal;
            analysis.FallbackNote = fallbackNote ?? parsed.FallbackNote;
            analysis.GeneratedAtUtc = DateTime.UtcNow;
            analysis.UpdatedAtUtc = DateTime.UtcNow;

            foreach (var skill in parsed.Skills)
            {
                analysis.ExtractedSkills.Add(new ExtractedSkill
                {
                    RawName = Truncate(skill.RawName, 200) ?? string.Empty,
                    CanonicalName = Truncate(skill.CanonicalName, 200) ?? string.Empty,
                    Confidence = skill.Confidence,
                    Status = ExtractedSkillStatus.Pending,
                    SourceEvidence = Truncate(skill.SourceEvidence, 200),
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            await AuditAsync(userId, "ai.resume.parse", resumeId, $"{analysis.Provider}; skills={analysis.ExtractedSkills.Count}");
            await _db.SaveChangesAsync();
            return (true, null, Map(analysis));
        }
    }

    public async Task<(bool Ok, string? Error, ResumeAnalysisDto? Result)> GetAnalysisAsync(int resumeId)
    {
        var (ok, error, profile, _) = await RequireCandidateAsync();
        if (!ok || profile is null) return (false, error, null);
        var resume = await _db.Resumes.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.CandidateProfileId == profile.Id);
        if (resume is null) return (false, "Resume not found.", null);
        var analysis = await _db.ResumeAnalyses.AsNoTracking().Include(a => a.ExtractedSkills)
            .FirstOrDefaultAsync(a => a.ResumeId == resumeId);
        if (analysis is null) return (false, "Analysis not found.", null);
        return (true, null, Map(analysis));
    }

    public async Task<(bool Ok, string? Error, ResumeAnalysisDto? Result)> ConfirmAnalysisAsync(
        int resumeId,
        ConfirmResumeAnalysisDto dto)
    {
        var (ok, error, profile, userId) = await RequireCandidateAsync();
        if (!ok || profile is null) return (false, error, null);
        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId && r.CandidateProfileId == profile.Id);
        if (resume is null) return (false, "Resume not found.", null);
        var analysis = await _db.ResumeAnalyses.Include(a => a.ExtractedSkills)
            .FirstOrDefaultAsync(a => a.ResumeId == resumeId);
        if (analysis is null) return (false, "Analysis not found.", null);
        if (analysis.Status is ResumeAnalysisStatus.Failed or ResumeAnalysisStatus.Processing)
            return (false, "Analysis is not ready for confirmation.", null);

        var accept = new HashSet<int>(dto.AcceptSkillIds ?? Array.Empty<int>());
        var reject = new HashSet<int>(dto.RejectSkillIds ?? Array.Empty<int>());
        var catalogue = await _db.Skills.ToListAsync();

        foreach (var skill in analysis.ExtractedSkills)
        {
            if (reject.Contains(skill.Id))
            {
                skill.Status = ExtractedSkillStatus.Rejected;
                continue;
            }

            if (!accept.Contains(skill.Id)) continue;
            skill.Status = ExtractedSkillStatus.Accepted;

            var catalogSkill = catalogue.FirstOrDefault(s =>
                string.Equals(s.Name, skill.CanonicalName, StringComparison.OrdinalIgnoreCase));
            if (catalogSkill is null)
            {
                catalogSkill = new Skill { Name = skill.CanonicalName, CreatedAtUtc = DateTime.UtcNow };
                _db.Skills.Add(catalogSkill);
                await _db.SaveChangesAsync();
                catalogue.Add(catalogSkill);
            }

            var exists = await _db.CandidateSkills.AnyAsync(cs =>
                cs.CandidateProfileId == profile.Id && cs.SkillId == catalogSkill.Id);
            if (!exists)
            {
                _db.CandidateSkills.Add(new CandidateSkill
                {
                    CandidateProfileId = profile.Id,
                    SkillId = catalogSkill.Id,
                    ProficiencyLevel = "Intermediate",
                    YearsOfExperience = 1
                });
            }
        }

        analysis.Status = ResumeAnalysisStatus.Completed;
        analysis.UpdatedAtUtc = DateTime.UtcNow;
        await AuditAsync(userId, "ai.resume.confirm", resumeId, $"accepted={accept.Count};rejected={reject.Count}");
        await _db.SaveChangesAsync();
        return (true, null, Map(analysis));
    }

    public async Task<(bool Ok, string? Error)> RejectAnalysisAsync(int resumeId)
    {
        var (ok, error, profile, userId) = await RequireCandidateAsync();
        if (!ok || profile is null) return (false, error);
        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId && r.CandidateProfileId == profile.Id);
        if (resume is null) return (false, "Resume not found.");
        var analysis = await _db.ResumeAnalyses.Include(a => a.ExtractedSkills)
            .FirstOrDefaultAsync(a => a.ResumeId == resumeId);
        if (analysis is null) return (false, "Analysis not found.");
        foreach (var skill in analysis.ExtractedSkills)
            skill.Status = ExtractedSkillStatus.Rejected;
        analysis.Status = ResumeAnalysisStatus.Completed;
        analysis.UpdatedAtUtc = DateTime.UtcNow;
        await AuditAsync(userId, "ai.resume.reject", resumeId, "All extracted skills rejected; profile unchanged.");
        await _db.SaveChangesAsync();
        return (true, null);
    }

    private async Task AuditAsync(int userId, string action, int entityId, string details)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = nameof(Resume),
            EntityId = entityId,
            Details = details,
            Success = true,
            ActorRole = "Candidate",
            CreatedAtUtc = DateTime.UtcNow
        });
        await Task.CompletedTask;
    }

    private static string? Truncate(string? value, int max) =>
        string.IsNullOrWhiteSpace(value) ? null : (value.Length <= max ? value.Trim() : value.Trim()[..max]);

    private static ResumeAnalysisDto Map(ResumeAnalysis a) => new()
    {
        Id = a.Id,
        ResumeId = a.ResumeId,
        Status = a.Status.ToString(),
        Provider = a.Provider,
        ProviderType = a.ProviderType,
        ProviderVersion = a.ProviderVersion,
        AnalysisSummary = a.AnalysisSummary,
        FailureReason = a.FailureReason,
        ExtractedName = a.ExtractedName,
        ExtractedEmail = a.ExtractedEmail,
        ExtractedPhone = a.ExtractedPhone,
        ExtractedSummary = a.ExtractedSummary,
        EstimatedYearsExperience = a.EstimatedYearsExperience,
        ConsentUsedExternal = a.ConsentUsedExternal,
        FallbackNote = a.FallbackNote,
        GeneratedAtUtc = a.GeneratedAtUtc,
        Skills = a.ExtractedSkills.Select(s => new ExtractedSkillDto
        {
            Id = s.Id,
            RawName = s.RawName,
            CanonicalName = s.CanonicalName,
            Confidence = s.Confidence,
            Status = s.Status.ToString(),
            SourceEvidence = s.SourceEvidence
        }).ToList()
    };
}
