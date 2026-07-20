using HireSphere.API.Data;
using HireSphere.API.DTOs.Candidate;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface ICandidateApplicationService
{
    Task<(bool Ok, string? Error, ApplicationWizardOptionsDto? Result)> GetApplyOptionsAsync(int jobId);

    Task<(bool Ok, string? Error, CandidateApplicationDetailDto? Result)> SubmitAsync(SubmitApplicationDto dto);

    Task<(bool Ok, string? Error, IReadOnlyList<CandidateApplicationListItemDto>? Result)> ListAsync();

    Task<(bool Ok, string? Error, CandidateApplicationDetailDto? Result)> GetAsync(int applicationId);

    Task<(bool Ok, string? Error, CandidateApplicationDetailDto? Result)> WithdrawAsync(int applicationId);
}

public sealed class CandidateApplicationService : ICandidateApplicationService
{
    private static readonly ApplicationStatus[] WithdrawableStatuses =
    {
        ApplicationStatus.Pending,
        ApplicationStatus.UnderReview
    };

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CandidateApplicationService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<(bool Ok, string? Error, ApplicationWizardOptionsDto? Result)> GetApplyOptionsAsync(int jobId)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var job = await _db.Jobs
            .AsNoTracking()
            .Include(j => j.ScreeningQuestions)
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job == null)
        {
            return (false, "Job not found.", null);
        }

        var alreadyApplied = await _db.Applications
            .AnyAsync(a => a.CandidateId == profile.UserId && a.JobId == jobId);

        string? blockReason = null;
        if (job.Status != JobStatus.Open)
        {
            blockReason = "This job is closed or inactive and is not accepting applications.";
        }
        else if (alreadyApplied)
        {
            blockReason = "You have already applied to this job.";
        }

        var resumes = await _db.Resumes
            .AsNoTracking()
            .Where(r => r.CandidateProfileId == profile.Id)
            .OrderByDescending(r => r.IsPrimary)
            .ThenByDescending(r => r.UploadedAtUtc)
            .Select(r => new ResumeMetadataDto
            {
                Id = r.Id,
                StorageKey = r.FilePath,
                FileName = r.FileName,
                IsPrimary = r.IsPrimary,
                UploadedAtUtc = r.UploadedAtUtc
            })
            .ToListAsync();

        return (true, null, new ApplicationWizardOptionsDto
        {
            JobId = job.Id,
            JobTitle = job.Title,
            CanApply = blockReason == null,
            BlockReason = blockReason,
            Resumes = resumes,
            ScreeningQuestions = job.ScreeningQuestions
                .OrderBy(q => q.SortOrder)
                .Select(q => new ScreeningQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    IsRequired = q.IsRequired,
                    SortOrder = q.SortOrder
                })
                .ToList()
        });
    }

    public async Task<(bool Ok, string? Error, CandidateApplicationDetailDto? Result)> SubmitAsync(
        SubmitApplicationDto dto)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        if (!dto.TermsAccepted)
        {
            return (false, "You must accept the terms to submit an application.", null);
        }

        var job = await _db.Jobs
            .Include(j => j.ScreeningQuestions)
            .FirstOrDefaultAsync(j => j.Id == dto.JobId);

        if (job == null)
        {
            return (false, "Job not found.", null);
        }

        if (job.Status != JobStatus.Open)
        {
            return (false, "This job is closed or inactive and is not accepting applications.", null);
        }

        var duplicate = await _db.Applications
            .AnyAsync(a => a.CandidateId == profile.UserId && a.JobId == dto.JobId);
        if (duplicate)
        {
            return (false, "You have already applied to this job.", null);
        }

        if (dto.ResumeId is int resumeId)
        {
            var ownsResume = await _db.Resumes
                .AnyAsync(r => r.Id == resumeId && r.CandidateProfileId == profile.Id);
            if (!ownsResume)
            {
                return (false, "Selected resume was not found.", null);
            }
        }

        var requiredQuestions = job.ScreeningQuestions.Where(q => q.IsRequired).ToList();
        var answersByQuestion = (dto.ScreeningAnswers ?? Array.Empty<ScreeningAnswerInputDto>())
            .GroupBy(a => a.ScreeningQuestionId)
            .ToDictionary(g => g.Key, g => g.Last().AnswerText?.Trim() ?? string.Empty);

        foreach (var question in requiredQuestions)
        {
            if (!answersByQuestion.TryGetValue(question.Id, out var answer)
                || string.IsNullOrWhiteSpace(answer))
            {
                return (false, $"A required screening question is unanswered: {question.QuestionText}", null);
            }
        }

        foreach (var answer in answersByQuestion)
        {
            if (!job.ScreeningQuestions.Any(q => q.Id == answer.Key))
            {
                return (false, "One or more screening answers refer to an invalid question.", null);
            }
        }

        var submittedAt = DateTime.UtcNow;
        var application = new Application
        {
            CandidateId = profile.UserId,
            JobId = job.Id,
            AppliedDate = submittedAt,
            CreatedAtUtc = submittedAt,
            Status = ApplicationStatus.Pending,
            CoverLetter = dto.CoverLetter?.Trim() ?? string.Empty,
            ResumeId = dto.ResumeId
        };

        foreach (var question in job.ScreeningQuestions.OrderBy(q => q.SortOrder))
        {
            if (!answersByQuestion.TryGetValue(question.Id, out var answerText)
                || string.IsNullOrWhiteSpace(answerText))
            {
                continue;
            }

            application.Answers.Add(new ApplicationAnswer
            {
                ScreeningQuestionId = question.Id,
                QuestionText = question.QuestionText,
                AnswerText = answerText,
                CreatedAtUtc = submittedAt
            });
        }

        application.StatusHistory.Add(new ApplicationStatusHistory
        {
            Status = ApplicationStatus.Pending,
            ChangedAtUtc = submittedAt,
            ChangedByUserId = profile.UserId,
            Notes = "Application submitted by candidate."
        });

        _db.Applications.Add(application);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return (false, "You have already applied to this job.", null);
        }

        return await GetAsync(application.Id);
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<CandidateApplicationListItemDto>? Result)> ListAsync()
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var applications = await _db.Applications
            .AsNoTracking()
            .Include(a => a.Job)
            .Where(a => a.CandidateId == profile.UserId)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();

        var items = applications.Select(a => new CandidateApplicationListItemDto
        {
            Id = a.Id,
            JobId = a.JobId,
            JobTitle = a.Job.Title,
            JobLocation = a.Job.Location,
            Status = a.Status,
            AppliedDate = a.AppliedDate,
            SubmittedAtUtc = a.CreatedAtUtc,
            CanWithdraw = CanWithdraw(a.Status)
        }).ToList();

        return (true, null, items);
    }

    public async Task<(bool Ok, string? Error, CandidateApplicationDetailDto? Result)> GetAsync(int applicationId)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var application = await _db.Applications
            .AsNoTracking()
            .Include(a => a.Job)
            .Include(a => a.Answers)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.CandidateId == profile.UserId);

        if (application == null)
        {
            return (false, "Application not found.", null);
        }

        string? resumeFileName = null;
        if (application.ResumeId is int resumeId)
        {
            resumeFileName = await _db.Resumes
                .AsNoTracking()
                .Where(r => r.Id == resumeId && r.CandidateProfileId == profile.Id)
                .Select(r => r.FileName)
                .FirstOrDefaultAsync();
        }

        return (true, null, MapDetail(application, resumeFileName));
    }

    public async Task<(bool Ok, string? Error, CandidateApplicationDetailDto? Result)> WithdrawAsync(
        int applicationId)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var application = await _db.Applications
            .Include(a => a.Job)
            .Include(a => a.Answers)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.CandidateId == profile.UserId);

        if (application == null)
        {
            return (false, "Application not found.", null);
        }

        if (!CanWithdraw(application.Status))
        {
            return (false, "This application can no longer be withdrawn.", null);
        }

        var changedAt = DateTime.UtcNow;
        application.Status = ApplicationStatus.Withdrawn;
        application.UpdatedAtUtc = changedAt;
        application.StatusHistory.Add(new ApplicationStatusHistory
        {
            Status = ApplicationStatus.Withdrawn,
            ChangedAtUtc = changedAt,
            ChangedByUserId = profile.UserId,
            Notes = "Application withdrawn by candidate."
        });

        await _db.SaveChangesAsync();

        string? resumeFileName = null;
        if (application.ResumeId is int resumeId)
        {
            resumeFileName = await _db.Resumes
                .AsNoTracking()
                .Where(r => r.Id == resumeId && r.CandidateProfileId == profile.Id)
                .Select(r => r.FileName)
                .FirstOrDefaultAsync();
        }

        return (true, null, MapDetail(application, resumeFileName));
    }

    private static bool CanWithdraw(ApplicationStatus status) =>
        WithdrawableStatuses.Contains(status);

    private async Task<(bool Ok, string? Error, CandidateProfile? Profile)> RequireProfileAsync()
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        var profile = await _db.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            return (false, "Candidate profile not found.", null);
        }

        return (true, null, profile);
    }

    private static CandidateApplicationDetailDto MapDetail(Application application, string? resumeFileName)
    {
        return new CandidateApplicationDetailDto
        {
            Id = application.Id,
            JobId = application.JobId,
            JobTitle = application.Job.Title,
            JobLocation = application.Job.Location,
            JobStatus = application.Job.Status,
            Status = application.Status,
            AppliedDate = application.AppliedDate,
            SubmittedAtUtc = application.CreatedAtUtc,
            CoverLetter = application.CoverLetter,
            ResumeId = application.ResumeId,
            ResumeFileName = resumeFileName,
            CanWithdraw = CanWithdraw(application.Status),
            Answers = application.Answers
                .OrderBy(a => a.Id)
                .Select(a => new ApplicationAnswerDto
                {
                    Id = a.Id,
                    ScreeningQuestionId = a.ScreeningQuestionId,
                    QuestionText = a.QuestionText,
                    AnswerText = a.AnswerText
                })
                .ToList(),
            StatusHistory = application.StatusHistory
                .OrderBy(h => h.ChangedAtUtc)
                .Select(h => new ApplicationStatusHistoryDto
                {
                    Status = h.Status,
                    ChangedAtUtc = h.ChangedAtUtc,
                    Notes = h.Notes
                })
                .ToList()
        };
    }
}
