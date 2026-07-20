using HireSphere.API.Data;
using HireSphere.API.DTOs.Candidate;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface ICandidateProfileService
{
    Task<(bool Ok, string? Error, CandidateDashboardSummaryDto? Result)> GetDashboardAsync();

    Task<(bool Ok, string? Error, CandidateProfileDetailDto? Result)> GetProfileAsync();

    Task<(bool Ok, string? Error, CandidateProfileDetailDto? Result)> UpdateProfileAsync(
        UpdateCandidateProfileDto dto);

    Task<(bool Ok, string? Error, WorkExperienceDto? Result)> AddExperienceAsync(
        CreateWorkExperienceDto dto);

    Task<(bool Ok, string? Error, WorkExperienceDto? Result)> UpdateExperienceAsync(
        int id,
        UpdateWorkExperienceDto dto);

    Task<(bool Ok, string? Error)> DeleteExperienceAsync(int id);

    Task<(bool Ok, string? Error, EducationDto? Result)> AddEducationAsync(
        CreateEducationDto dto);

    Task<(bool Ok, string? Error, EducationDto? Result)> UpdateEducationAsync(
        int id,
        UpdateEducationDto dto);

    Task<(bool Ok, string? Error)> DeleteEducationAsync(int id);

    Task<(bool Ok, string? Error, CandidateSkillDto? Result)> AddSkillAsync(
        CreateCandidateSkillDto dto);

    Task<(bool Ok, string? Error, CandidateSkillDto? Result)> UpdateSkillAsync(
        int id,
        UpdateCandidateSkillDto dto);

    Task<(bool Ok, string? Error)> DeleteSkillAsync(int id);

    Task<IReadOnlyList<SkillCatalogItemDto>> ListSkillCatalogAsync();

    Task<(bool Ok, string? Error, CertificationDto? Result)> AddCertificationAsync(
        CreateCertificationDto dto);

    Task<(bool Ok, string? Error, CertificationDto? Result)> UpdateCertificationAsync(
        int id,
        UpdateCertificationDto dto);

    Task<(bool Ok, string? Error)> DeleteCertificationAsync(int id);

    Task<(bool Ok, string? Error, IReadOnlyList<ResumeMetadataDto>? Result)> ListResumesAsync();

    Task<(bool Ok, string? Error, ResumeMetadataDto? Result)> UploadResumeAsync(IFormFile file);

    Task<(bool Ok, string? Error)> DeleteResumeAsync(int id);

    Task<(bool Ok, string? Error)> SetPrimaryResumeAsync(int id);

    Task<(bool Ok, string? Error, IReadOnlyList<DocumentMetadataDto>? Result)> ListDocumentsAsync();

    Task<(bool Ok, string? Error, DocumentMetadataDto? Result)> UploadDocumentAsync(
        IFormFile file,
        DocumentType documentType);

    Task<(bool Ok, string? Error, FileDownloadDto? Result)> DownloadDocumentAsync(int id);

    Task<(bool Ok, string? Error)> DeleteDocumentAsync(int id);
}

public sealed class CandidateProfileService : ICandidateProfileService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILocalFileStorageService _fileStorage;

    public CandidateProfileService(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        ILocalFileStorageService fileStorage)
    {
        _db = db;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<(bool Ok, string? Error, CandidateDashboardSummaryDto? Result)> GetDashboardAsync()
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var userId = profile.UserId;

        var applicationsQuery = _db.Applications.Where(a => a.CandidateId == userId);
        var latestApplicationsCount = await applicationsQuery.CountAsync();

        var interviewsCount = await _db.Interviews
            .Where(i => i.Application.CandidateId == userId
                && (i.Status == InterviewStatus.Scheduled || i.Status == InterviewStatus.Rescheduled))
            .CountAsync();

        var assessmentsCount = await _db.AssessmentAssignments
            .Where(a => a.CandidateId == userId
                && (a.Status == AssessmentStatus.Pending || a.Status == AssessmentStatus.InProgress))
            .CountAsync();

        var unreadNotificationsCount = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();

        var recommendationsCount = await _db.CandidateJobMatches
            .Where(m => m.CandidateProfileId == profile.Id)
            .CountAsync();

        var resumeAnalysisStatus = "NotAvailable";
        var primaryResume = await _db.Resumes
            .Where(r => r.CandidateProfileId == profile.Id && r.IsPrimary)
            .Select(r => new { r.Id })
            .FirstOrDefaultAsync();

        if (primaryResume != null)
        {
            var hasAnalysis = await _db.ResumeAnalyses
                .AnyAsync(a => a.ResumeId == primaryResume.Id);

            if (hasAnalysis)
            {
                resumeAnalysisStatus = "Available";
            }
        }

        var completion = await ComputeProfileCompletionAsync(profile.Id);

        return (true, null, new CandidateDashboardSummaryDto
        {
            ProfileCompletionPercent = completion,
            LatestApplicationsCount = latestApplicationsCount,
            InterviewsCount = interviewsCount,
            AssessmentsCount = assessmentsCount,
            RecommendationsCount = recommendationsCount,
            UnreadNotificationsCount = unreadNotificationsCount,
            ResumeAnalysisStatus = resumeAnalysisStatus
        });
    }

    public async Task<(bool Ok, string? Error, CandidateProfileDetailDto? Result)> GetProfileAsync()
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync(includeDetails: true);
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var dto = await MapProfileDetailAsync(profile);
        return (true, null, dto);
    }

    public async Task<(bool Ok, string? Error, CandidateProfileDetailDto? Result)> UpdateProfileAsync(
        UpdateCandidateProfileDto dto)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        profile.FullName = dto.FullName.Trim();
        profile.PhoneNumber = dto.PhoneNumber.Trim();
        profile.Address = dto.Address.Trim();
        profile.Summary = dto.Summary?.Trim();
        profile.Location = dto.Location?.Trim();
        profile.YearsOfExperience = dto.YearsOfExperience;
        profile.DesiredJobTitle = dto.DesiredJobTitle?.Trim();
        profile.PreferredWorkArrangement = dto.PreferredWorkArrangement;
        profile.SalaryExpectation = dto.SalaryExpectation;
        profile.Availability = dto.Availability?.Trim();
        profile.PortfolioUrl = dto.PortfolioUrl?.Trim();
        profile.LinkedInUrl = dto.LinkedInUrl?.Trim();
        profile.GitHubUrl = dto.GitHubUrl?.Trim();
        profile.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await _db.CandidateProfiles
            .AsNoTracking()
            .Include(p => p.WorkExperiences)
            .Include(p => p.Educations)
            .Include(p => p.CandidateSkills).ThenInclude(cs => cs.Skill)
            .Include(p => p.Certifications)
            .FirstAsync(p => p.Id == profile.Id);

        return (true, null, await MapProfileDetailAsync(updated));
    }

    public async Task<(bool Ok, string? Error, WorkExperienceDto? Result)> AddExperienceAsync(
        CreateWorkExperienceDto dto)
    {
        var validationError = ValidateExperienceDates(dto.StartDate, dto.EndDate, dto.IsCurrentRole);
        if (validationError != null)
        {
            return (false, validationError, null);
        }

        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var entity = new WorkExperience
        {
            CandidateProfileId = profile.Id,
            CompanyName = dto.CompanyName.Trim(),
            JobTitle = dto.JobTitle.Trim(),
            StartDate = dto.StartDate,
            EndDate = dto.IsCurrentRole ? null : dto.EndDate,
            Description = dto.Description?.Trim(),
            Location = dto.Location?.Trim(),
            IsCurrentRole = dto.IsCurrentRole,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.WorkExperiences.Add(entity);
        await _db.SaveChangesAsync();

        return (true, null, MapExperience(entity));
    }

    public async Task<(bool Ok, string? Error, WorkExperienceDto? Result)> UpdateExperienceAsync(
        int id,
        UpdateWorkExperienceDto dto)
    {
        var validationError = ValidateExperienceDates(dto.StartDate, dto.EndDate, dto.IsCurrentRole);
        if (validationError != null)
        {
            return (false, validationError, null);
        }

        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var entity = await _db.WorkExperiences
            .FirstOrDefaultAsync(e => e.Id == id && e.CandidateProfileId == profile.Id);

        if (entity == null)
        {
            return (false, "Work experience not found.", null);
        }

        entity.CompanyName = dto.CompanyName.Trim();
        entity.JobTitle = dto.JobTitle.Trim();
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.IsCurrentRole ? null : dto.EndDate;
        entity.Description = dto.Description?.Trim();
        entity.Location = dto.Location?.Trim();
        entity.IsCurrentRole = dto.IsCurrentRole;

        await _db.SaveChangesAsync();
        return (true, null, MapExperience(entity));
    }

    public async Task<(bool Ok, string? Error)> DeleteExperienceAsync(int id)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError);
        }

        var entity = await _db.WorkExperiences
            .FirstOrDefaultAsync(e => e.Id == id && e.CandidateProfileId == profile.Id);

        if (entity == null)
        {
            return (false, "Work experience not found.");
        }

        _db.WorkExperiences.Remove(entity);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error, EducationDto? Result)> AddEducationAsync(
        CreateEducationDto dto)
    {
        var validationError = ValidateEducationDates(dto.StartDate, dto.EndDate, dto.IsCurrentStudy);
        if (validationError != null)
        {
            return (false, validationError, null);
        }

        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var entity = new Education
        {
            CandidateProfileId = profile.Id,
            Institution = dto.Institution.Trim(),
            Degree = dto.Degree.Trim(),
            FieldOfStudy = dto.FieldOfStudy?.Trim(),
            StartDate = dto.StartDate,
            EndDate = dto.IsCurrentStudy ? null : dto.EndDate,
            Grade = dto.Grade?.Trim(),
            IsCurrentStudy = dto.IsCurrentStudy,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Educations.Add(entity);
        await _db.SaveChangesAsync();

        return (true, null, MapEducation(entity));
    }

    public async Task<(bool Ok, string? Error, EducationDto? Result)> UpdateEducationAsync(
        int id,
        UpdateEducationDto dto)
    {
        var validationError = ValidateEducationDates(dto.StartDate, dto.EndDate, dto.IsCurrentStudy);
        if (validationError != null)
        {
            return (false, validationError, null);
        }

        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var entity = await _db.Educations
            .FirstOrDefaultAsync(e => e.Id == id && e.CandidateProfileId == profile.Id);

        if (entity == null)
        {
            return (false, "Education not found.", null);
        }

        entity.Institution = dto.Institution.Trim();
        entity.Degree = dto.Degree.Trim();
        entity.FieldOfStudy = dto.FieldOfStudy?.Trim();
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.IsCurrentStudy ? null : dto.EndDate;
        entity.Grade = dto.Grade?.Trim();
        entity.IsCurrentStudy = dto.IsCurrentStudy;

        await _db.SaveChangesAsync();
        return (true, null, MapEducation(entity));
    }

    public async Task<(bool Ok, string? Error)> DeleteEducationAsync(int id)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError);
        }

        var entity = await _db.Educations
            .FirstOrDefaultAsync(e => e.Id == id && e.CandidateProfileId == profile.Id);

        if (entity == null)
        {
            return (false, "Education not found.");
        }

        _db.Educations.Remove(entity);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error, CandidateSkillDto? Result)> AddSkillAsync(
        CreateCandidateSkillDto dto)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var skillExists = await _db.Skills.AnyAsync(s => s.Id == dto.SkillId);
        if (!skillExists)
        {
            return (false, "Skill not found.", null);
        }

        var duplicate = await _db.CandidateSkills
            .AnyAsync(cs => cs.CandidateProfileId == profile.Id && cs.SkillId == dto.SkillId);

        if (duplicate)
        {
            return (false, "This skill is already on your profile.", null);
        }

        var entity = new CandidateSkill
        {
            CandidateProfileId = profile.Id,
            SkillId = dto.SkillId,
            ProficiencyLevel = dto.ProficiencyLevel?.Trim(),
            YearsOfExperience = dto.YearsOfExperience
        };

        _db.CandidateSkills.Add(entity);
        await _db.SaveChangesAsync();

        var skill = await _db.Skills.AsNoTracking().FirstAsync(s => s.Id == dto.SkillId);
        return (true, null, MapSkill(entity, skill.Name));
    }

    public async Task<(bool Ok, string? Error, CandidateSkillDto? Result)> UpdateSkillAsync(
        int id,
        UpdateCandidateSkillDto dto)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var entity = await _db.CandidateSkills
            .Include(cs => cs.Skill)
            .FirstOrDefaultAsync(cs => cs.Id == id && cs.CandidateProfileId == profile.Id);

        if (entity == null)
        {
            return (false, "Skill not found.", null);
        }

        entity.ProficiencyLevel = dto.ProficiencyLevel?.Trim();
        entity.YearsOfExperience = dto.YearsOfExperience;

        await _db.SaveChangesAsync();
        return (true, null, MapSkill(entity, entity.Skill.Name));
    }

    public async Task<(bool Ok, string? Error)> DeleteSkillAsync(int id)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError);
        }

        var entity = await _db.CandidateSkills
            .FirstOrDefaultAsync(cs => cs.Id == id && cs.CandidateProfileId == profile.Id);

        if (entity == null)
        {
            return (false, "Skill not found.");
        }

        _db.CandidateSkills.Remove(entity);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<IReadOnlyList<SkillCatalogItemDto>> ListSkillCatalogAsync()
    {
        return await _db.Skills
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SkillCatalogItemDto { Id = s.Id, Name = s.Name })
            .ToListAsync();
    }

    public async Task<(bool Ok, string? Error, CertificationDto? Result)> AddCertificationAsync(
        CreateCertificationDto dto)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var entity = new Certification
        {
            CandidateProfileId = profile.Id,
            Name = dto.Name.Trim(),
            IssuingOrganization = dto.IssuingOrganization.Trim(),
            IssueDate = dto.IssueDate,
            ExpiryDate = dto.ExpiryDate,
            CredentialId = dto.CredentialId?.Trim(),
            CredentialUrl = dto.CredentialUrl?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Certifications.Add(entity);
        await _db.SaveChangesAsync();

        return (true, null, MapCertification(entity));
    }

    public async Task<(bool Ok, string? Error, CertificationDto? Result)> UpdateCertificationAsync(
        int id,
        UpdateCertificationDto dto)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var entity = await _db.Certifications
            .FirstOrDefaultAsync(c => c.Id == id && c.CandidateProfileId == profile.Id);

        if (entity == null)
        {
            return (false, "Certification not found.", null);
        }

        entity.Name = dto.Name.Trim();
        entity.IssuingOrganization = dto.IssuingOrganization.Trim();
        entity.IssueDate = dto.IssueDate;
        entity.ExpiryDate = dto.ExpiryDate;
        entity.CredentialId = dto.CredentialId?.Trim();
        entity.CredentialUrl = dto.CredentialUrl?.Trim();

        await _db.SaveChangesAsync();
        return (true, null, MapCertification(entity));
    }

    public async Task<(bool Ok, string? Error)> DeleteCertificationAsync(int id)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError);
        }

        var entity = await _db.Certifications
            .FirstOrDefaultAsync(c => c.Id == id && c.CandidateProfileId == profile.Id);

        if (entity == null)
        {
            return (false, "Certification not found.");
        }

        _db.Certifications.Remove(entity);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<ResumeMetadataDto>? Result)> ListResumesAsync()
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var resumes = await _db.Resumes
            .AsNoTracking()
            .Where(r => r.CandidateProfileId == profile.Id)
            .OrderByDescending(r => r.IsPrimary)
            .ThenByDescending(r => r.UploadedAtUtc)
            .ToListAsync();

        return (true, null, resumes.Select(MapResume).ToList());
    }

    public async Task<(bool Ok, string? Error, ResumeMetadataDto? Result)> UploadResumeAsync(IFormFile file)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var (saveOk, saveError, stored) = await _fileStorage.SaveAsync(file, "resumes");
        if (!saveOk || stored == null)
        {
            return (false, saveError, null);
        }

        var isFirst = !await _db.Resumes.AnyAsync(r => r.CandidateProfileId == profile.Id);

        var entity = new Resume
        {
            CandidateProfileId = profile.Id,
            FilePath = stored.StorageKey,
            FileName = stored.OriginalFileName,
            IsPrimary = isFirst,
            UploadedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Resumes.Add(entity);

        if (isFirst)
        {
            profile.ResumePath = stored.StorageKey;
        }

        profile.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (true, null, MapResume(entity));
    }

    public async Task<(bool Ok, string? Error)> DeleteResumeAsync(int id)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError);
        }

        var entity = await _db.Resumes
            .FirstOrDefaultAsync(r => r.Id == id && r.CandidateProfileId == profile.Id);

        if (entity == null)
        {
            return (false, "Resume not found.");
        }

        var storageKey = entity.FilePath;
        var wasPrimary = entity.IsPrimary;

        _db.Resumes.Remove(entity);
        await _db.SaveChangesAsync();
        await _fileStorage.DeleteAsync(storageKey);

        if (wasPrimary)
        {
            var nextPrimary = await _db.Resumes
                .Where(r => r.CandidateProfileId == profile.Id)
                .OrderByDescending(r => r.UploadedAtUtc)
                .FirstOrDefaultAsync();

            if (nextPrimary != null)
            {
                nextPrimary.IsPrimary = true;
                profile.ResumePath = nextPrimary.FilePath;
            }
            else
            {
                profile.ResumePath = string.Empty;
            }

            profile.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> SetPrimaryResumeAsync(int id)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError);
        }

        var entity = await _db.Resumes
            .FirstOrDefaultAsync(r => r.Id == id && r.CandidateProfileId == profile.Id);

        if (entity == null)
        {
            return (false, "Resume not found.");
        }

        var resumes = await _db.Resumes
            .Where(r => r.CandidateProfileId == profile.Id)
            .ToListAsync();

        foreach (var resume in resumes)
        {
            resume.IsPrimary = resume.Id == entity.Id;
        }

        profile.ResumePath = entity.FilePath;
        profile.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<DocumentMetadataDto>? Result)> ListDocumentsAsync()
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var documents = await _db.CandidateDocuments
            .AsNoTracking()
            .Where(d => d.CandidateProfileId == profile.Id)
            .OrderByDescending(d => d.UploadedAtUtc)
            .ToListAsync();

        return (true, null, documents.Select(MapDocument).ToList());
    }

    public async Task<(bool Ok, string? Error, DocumentMetadataDto? Result)> UploadDocumentAsync(
        IFormFile file,
        DocumentType documentType)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var (saveOk, saveError, stored) = await _fileStorage.SaveAsync(file, "documents");
        if (!saveOk || stored == null)
        {
            return (false, saveError, null);
        }

        var entity = new CandidateDocument
        {
            CandidateProfileId = profile.Id,
            DocumentType = documentType,
            FilePath = stored.StorageKey,
            FileName = stored.OriginalFileName,
            UploadedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.CandidateDocuments.Add(entity);
        profile.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (true, null, MapDocument(entity));
    }

    public async Task<(bool Ok, string? Error, FileDownloadDto? Result)> DownloadDocumentAsync(int id)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var entity = await _db.CandidateDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && d.CandidateProfileId == profile.Id);

        if (entity == null)
        {
            return (false, "Document not found.", null);
        }

        var (openOk, openError, content, contentType, _) =
            await _fileStorage.OpenReadAsync(entity.FilePath);

        if (!openOk || content == null)
        {
            return (false, openError ?? "File not found.", null);
        }

        return (true, null, new FileDownloadDto
        {
            Content = content,
            ContentType = contentType ?? "application/octet-stream",
            FileName = entity.FileName
        });
    }

    public async Task<(bool Ok, string? Error)> DeleteDocumentAsync(int id)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError);
        }

        var entity = await _db.CandidateDocuments
            .FirstOrDefaultAsync(d => d.Id == id && d.CandidateProfileId == profile.Id);

        if (entity == null)
        {
            return (false, "Document not found.");
        }

        var storageKey = entity.FilePath;
        _db.CandidateDocuments.Remove(entity);
        await _db.SaveChangesAsync();
        await _fileStorage.DeleteAsync(storageKey);

        return (true, null);
    }

    private async Task<(bool Ok, string? Error, CandidateProfile? Profile)> RequireProfileAsync(
        bool includeDetails = false)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        IQueryable<CandidateProfile> query = _db.CandidateProfiles;
        if (includeDetails)
        {
            query = query
                .Include(p => p.WorkExperiences)
                .Include(p => p.Educations)
                .Include(p => p.CandidateSkills).ThenInclude(cs => cs.Skill)
                .Include(p => p.Certifications);
        }

        var profile = await query.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            return (false, "Candidate profile not found.", null);
        }

        return (true, null, profile);
    }

    private async Task<CandidateProfileDetailDto> MapProfileDetailAsync(CandidateProfile profile)
    {
        var completion = await ComputeProfileCompletionAsync(profile.Id);

        return new CandidateProfileDetailDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            Address = profile.Address,
            Summary = profile.Summary,
            Location = profile.Location,
            YearsOfExperience = profile.YearsOfExperience,
            DesiredJobTitle = profile.DesiredJobTitle,
            PreferredWorkArrangement = profile.PreferredWorkArrangement,
            SalaryExpectation = profile.SalaryExpectation,
            Availability = profile.Availability,
            PortfolioUrl = profile.PortfolioUrl,
            LinkedInUrl = profile.LinkedInUrl,
            GitHubUrl = profile.GitHubUrl,
            ProfileCompletionPercent = completion,
            WorkExperiences = profile.WorkExperiences
                .OrderByDescending(w => w.StartDate)
                .Select(MapExperience)
                .ToList(),
            Educations = profile.Educations
                .OrderByDescending(e => e.StartDate)
                .Select(MapEducation)
                .ToList(),
            Skills = profile.CandidateSkills
                .Select(cs => MapSkill(cs, cs.Skill.Name))
                .ToList(),
            Certifications = profile.Certifications
                .OrderByDescending(c => c.IssueDate)
                .Select(MapCertification)
                .ToList()
        };
    }

    private async Task<int> ComputeProfileCompletionAsync(int profileId)
    {
        var profile = await _db.CandidateProfiles
            .AsNoTracking()
            .Include(p => p.WorkExperiences)
            .Include(p => p.Educations)
            .Include(p => p.CandidateSkills)
            .Include(p => p.Resumes)
            .FirstAsync(p => p.Id == profileId);

        var checks = new List<bool>
        {
            !string.IsNullOrWhiteSpace(profile.FullName),
            !string.IsNullOrWhiteSpace(profile.PhoneNumber),
            !string.IsNullOrWhiteSpace(profile.Summary),
            !string.IsNullOrWhiteSpace(profile.Location),
            !string.IsNullOrWhiteSpace(profile.DesiredJobTitle),
            profile.PreferredWorkArrangement.HasValue,
            profile.SalaryExpectation.HasValue,
            !string.IsNullOrWhiteSpace(profile.Availability),
            !string.IsNullOrWhiteSpace(profile.PortfolioUrl)
                || !string.IsNullOrWhiteSpace(profile.LinkedInUrl)
                || !string.IsNullOrWhiteSpace(profile.GitHubUrl),
            profile.WorkExperiences.Count > 0,
            profile.Educations.Count > 0,
            profile.CandidateSkills.Count > 0,
            profile.Resumes.Count > 0
        };

        var completed = checks.Count(c => c);
        return (int)Math.Round(completed / (double)checks.Count * 100);
    }

    private static string? ValidateExperienceDates(
        DateTime startDate,
        DateTime? endDate,
        bool isCurrentRole)
    {
        if (!isCurrentRole && endDate.HasValue && endDate.Value < startDate)
        {
            return "End date cannot be before start date.";
        }

        return null;
    }

    private static string? ValidateEducationDates(
        DateTime? startDate,
        DateTime? endDate,
        bool isCurrentStudy)
    {
        if (!isCurrentStudy
            && startDate.HasValue
            && endDate.HasValue
            && endDate.Value < startDate.Value)
        {
            return "End date cannot be before start date.";
        }

        return null;
    }

    private static WorkExperienceDto MapExperience(WorkExperience entity) => new()
    {
        Id = entity.Id,
        CompanyName = entity.CompanyName,
        JobTitle = entity.JobTitle,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Description = entity.Description,
        Location = entity.Location,
        IsCurrentRole = entity.IsCurrentRole
    };

    private static EducationDto MapEducation(Education entity) => new()
    {
        Id = entity.Id,
        Institution = entity.Institution,
        Degree = entity.Degree,
        FieldOfStudy = entity.FieldOfStudy,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Grade = entity.Grade,
        IsCurrentStudy = entity.IsCurrentStudy
    };

    private static CandidateSkillDto MapSkill(CandidateSkill entity, string skillName) => new()
    {
        Id = entity.Id,
        SkillId = entity.SkillId,
        SkillName = skillName,
        ProficiencyLevel = entity.ProficiencyLevel,
        YearsOfExperience = entity.YearsOfExperience
    };

    private static CertificationDto MapCertification(Certification entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        IssuingOrganization = entity.IssuingOrganization,
        IssueDate = entity.IssueDate,
        ExpiryDate = entity.ExpiryDate,
        CredentialId = entity.CredentialId,
        CredentialUrl = entity.CredentialUrl
    };

    private static ResumeMetadataDto MapResume(Resume entity) => new()
    {
        Id = entity.Id,
        StorageKey = entity.FilePath,
        FileName = entity.FileName,
        IsPrimary = entity.IsPrimary,
        UploadedAtUtc = entity.UploadedAtUtc
    };

    private static DocumentMetadataDto MapDocument(CandidateDocument entity) => new()
    {
        Id = entity.Id,
        DocumentType = entity.DocumentType,
        StorageKey = entity.FilePath,
        FileName = entity.FileName,
        UploadedAtUtc = entity.UploadedAtUtc
    };
}
