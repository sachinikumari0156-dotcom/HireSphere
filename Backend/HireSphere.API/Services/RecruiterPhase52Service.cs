using System.Text.Json;
using HireSphere.API.Data;
using HireSphere.API.DTOs.Recruiter;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface IRecruiterPhase52Service
{
    Task<(bool Ok, string? Error, RecruiterRankingDto? Result)> GetRankingAsync(int applicationId);

    Task<(bool Ok, string? Error, RankingReviewDto? Result)> RecordRankingReviewAsync(
        int applicationId,
        CreateRankingReviewDto dto);

    Task<(bool Ok, string? Error, IReadOnlyList<ScreeningQueueItemDto>? Result)> GetScreeningQueueAsync();

    Task<(bool Ok, string? Error)> ApplyScreeningDecisionAsync(int applicationId, ScreeningDecisionDto dto);

    Task<(bool Ok, string? Error, IReadOnlyList<RecruiterAssessmentListItemDto>? Result)> ListAssessmentsAsync();

    Task<(bool Ok, string? Error, RecruiterAssessmentDetailDto? Result)> GetAssessmentAsync(int id);

    Task<(bool Ok, string? Error, RecruiterAssessmentDetailDto? Result)> CreateAssessmentAsync(UpsertAssessmentDto dto);

    Task<(bool Ok, string? Error, RecruiterAssessmentDetailDto? Result)> UpdateAssessmentAsync(
        int id,
        UpsertAssessmentDto dto);

    Task<(bool Ok, string? Error)> ArchiveAssessmentAsync(int id);

    Task<(bool Ok, string? Error, RecruiterAssessmentQuestionDto? Result)> AddQuestionAsync(
        int assessmentId,
        UpsertAssessmentQuestionDto dto);

    Task<(bool Ok, string? Error, RecruiterAssessmentQuestionDto? Result)> UpdateQuestionAsync(
        int assessmentId,
        int questionId,
        UpsertAssessmentQuestionDto dto);

    Task<(bool Ok, string? Error)> DeleteQuestionAsync(int assessmentId, int questionId);

    Task<(bool Ok, string? Error, RecruiterAssignmentDetailDto? Result)> AssignAssessmentAsync(
        int applicationId,
        AssignAssessmentDto dto);

    Task<(bool Ok, string? Error, RecruiterAssignmentDetailDto? Result)> GetAssignmentAsync(int assignmentId);

    Task<(bool Ok, string? Error, IReadOnlyList<RecruiterAttemptSummaryDto>? Result)> GetAssignmentAttemptsAsync(
        int assignmentId);

    Task<(bool Ok, string? Error, ApplicationMessageThreadDto? Result)> GetMessagesAsync(
        int applicationId,
        int page,
        int pageSize);

    Task<(bool Ok, string? Error, ApplicationMessageDto? Result)> SendRecruiterMessageAsync(
        int applicationId,
        string body);

    Task<(bool Ok, string? Error, ApplicationMessageDto? Result)> SendCandidateMessageAsync(
        int applicationId,
        string body);

    Task<(bool Ok, string? Error)> MarkMessagesReadAsync(int applicationId);
}

public sealed class RecruiterPhase52Service : IRecruiterPhase52Service
{
    public const string RankingModelVersion = "recruiter-rank-v1";
    public const string HumanReviewNotice =
        "AI-generated insight. Final recruitment decisions must be reviewed by authorized users.";

    private static readonly ApplicationStatus[] ScreeningDecisionStatuses =
    {
        ApplicationStatus.UnderReview,
        ApplicationStatus.ManualReview,
        ApplicationStatus.Shortlisted,
        ApplicationStatus.Rejected,
        ApplicationStatus.Assessment
    };

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IResourceAuthorizationService _authz;
    private readonly IJobMatchingProvider _matching;
    private readonly IApplicationStatusTransitionService _transitions;
    private readonly INotificationWriter _notifications;

    public RecruiterPhase52Service(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        IResourceAuthorizationService authz,
        IJobMatchingProvider matching,
        IApplicationStatusTransitionService transitions,
        INotificationWriter notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _authz = authz;
        _matching = matching;
        _transitions = transitions;
        _notifications = notifications;
    }

    public async Task<(bool Ok, string? Error, RecruiterRankingDto? Result)> GetRankingAsync(int applicationId)
    {
        if (!await _authz.RecruiterCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.", null);
        }

        var application = await _db.Applications
            .AsNoTracking()
            .Include(a => a.Job).ThenInclude(j => j.JobSkills).ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application is null)
        {
            return (false, "Application not found or access denied.", null);
        }

        var profile = await _db.CandidateProfiles
            .AsNoTracking()
            .Include(p => p.CandidateSkills).ThenInclude(cs => cs.Skill)
            .Include(p => p.Educations)
            .Include(p => p.WorkExperiences)
            .FirstOrDefaultAsync(p => p.UserId == application.CandidateId);

        if (profile is null)
        {
            return (false, "Candidate profile not found.", null);
        }

        var match = _matching.ComputeMatch(profile, application.Job);
        var required = application.Job.JobSkills?
            .Where(js => js.IsRequired && js.Skill != null)
            .Select(js => js.Skill.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();
        var preferred = application.Job.JobSkills?
            .Where(js => !js.IsRequired && js.Skill != null)
            .Select(js => js.Skill.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        var candidateSkills = profile.CandidateSkills
            .Where(cs => cs.Skill != null)
            .Select(cs => cs.Skill.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var matchedRequired = required
            .Where(s => candidateSkills.Contains(s))
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var matchedPreferred = preferred
            .Where(s => candidateSkills.Contains(s))
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var missingRequired = required
            .Where(s => !candidateSkills.Contains(s))
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();

        decimal? assessmentFactor = null;
        var latestScore = await (
            from aa in _db.AssessmentAssignments.AsNoTracking()
            where aa.ApplicationId == applicationId
            from at in aa.Attempts
            where at.Status == AssessmentStatus.Completed && at.Result != null && at.Result.MaxScore > 0
            orderby at.CompletedAtUtc descending
            select (decimal?)(at.Result!.Score / at.Result.MaxScore * 100m)).FirstOrDefaultAsync();

        if (latestScore is decimal pct)
        {
            assessmentFactor = Math.Round(pct / 100m * 15m, 2);
        }

        var completeness = ScoreProfileCompleteness(profile);
        var confidence = missingRequired.Count == 0 && profile.YearsOfExperience is > 0
            ? "High"
            : missingRequired.Count > 2 || profile.YearsOfExperience is null
                ? "Low"
                : "Medium";

        var review = await _db.RankingReviews
            .AsNoTracking()
            .Include(r => r.Reviewer)
            .Where(r => r.ApplicationId == applicationId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .FirstOrDefaultAsync();

        return (true, null, new RecruiterRankingDto
        {
            ApplicationId = applicationId,
            JobId = application.JobId,
            TotalScore = match.MatchScore,
            MatchedRequiredSkills = matchedRequired,
            MatchedPreferredSkills = matchedPreferred,
            MissingRequiredSkills = missingRequired,
            ExperienceFactor = match.Experience.FactorScore,
            EducationFactor = match.Education.FactorScore,
            AssessmentFactor = assessmentFactor,
            ProfileCompletenessFactor = completeness,
            Explanation = match.Explanation
                + (assessmentFactor is decimal af
                    ? $" Assessment contribution (completed attempts): {af}."
                    : " No completed assessment score available.")
                + $" Profile completeness factor: {completeness}."
                + " Protected characteristics (gender, race, religion, disability, marital status, age) are not used.",
            Confidence = confidence,
            ProviderName = _matching.ProviderName,
            ModelVersion = RankingModelVersion,
            GeneratedAtUtc = DateTime.UtcNow,
            HumanReviewNotice = HumanReviewNotice,
            LatestHumanReview = review is null
                ? null
                : new RankingReviewDto
                {
                    Id = review.Id,
                    Decision = review.Decision,
                    OverrideScore = review.OverrideScore,
                    Notes = review.Notes,
                    ReviewerUserId = review.ReviewerUserId,
                    ReviewerName = review.Reviewer.FullName,
                    CreatedAtUtc = review.CreatedAtUtc
                }
        });
    }

    public async Task<(bool Ok, string? Error, RankingReviewDto? Result)> RecordRankingReviewAsync(
        int applicationId,
        CreateRankingReviewDto dto)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        if (!await _authz.RecruiterCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.Decision) || string.IsNullOrWhiteSpace(dto.Notes))
        {
            return (false, "Decision and notes are required for human review.", null);
        }

        var review = new RankingReview
        {
            ApplicationId = applicationId,
            ReviewerUserId = userId,
            Decision = dto.Decision.Trim(),
            OverrideScore = dto.OverrideScore,
            Notes = Sanitize(dto.Notes)!,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.RankingReviews.Add(review);
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "RankingHumanReview",
            EntityType = "Application",
            EntityId = applicationId,
            Details = review.Decision,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var reviewer = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
        return (true, null, new RankingReviewDto
        {
            Id = review.Id,
            Decision = review.Decision,
            OverrideScore = review.OverrideScore,
            Notes = review.Notes,
            ReviewerUserId = userId,
            ReviewerName = reviewer.FullName,
            CreatedAtUtc = review.CreatedAtUtc
        });
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<ScreeningQueueItemDto>? Result)> GetScreeningQueueAsync()
    {
        if (_currentUser.OrganizationId is not int orgId || _currentUser.UserId is not int userId)
        {
            return (false, "Organization context is required.", null);
        }

        var apps = await _db.Applications
            .AsNoTracking()
            .Include(a => a.Job).ThenInclude(j => j.ScreeningQuestions)
            .Include(a => a.Candidate)
            .Include(a => a.Answers).ThenInclude(ans => ans.ScreeningQuestion)
            .Where(a => (a.Job.OrganizationId == orgId || a.Job.RecruiterId == userId)
                && (a.Status == ApplicationStatus.Pending
                    || a.Status == ApplicationStatus.UnderReview
                    || a.Status == ApplicationStatus.ManualReview))
            .OrderBy(a => a.AppliedDate)
            .Take(100)
            .ToListAsync();

        var items = new List<ScreeningQueueItemDto>();
        foreach (var app in apps)
        {
            var required = app.Job.ScreeningQuestions.Count(q => q.IsRequired);
            var completed = app.Answers.Count(a =>
                a.ScreeningQuestion?.IsRequired == true
                && !string.IsNullOrWhiteSpace(a.AnswerText));

            items.Add(new ScreeningQueueItemDto
            {
                ApplicationId = app.Id,
                JobId = app.JobId,
                JobTitle = app.Job.Title,
                CandidateName = app.Candidate.FullName,
                Status = app.Status,
                AppliedAtUtc = app.AppliedDate,
                RequiredAnswersTotal = required,
                RequiredAnswersCompleted = completed
            });
        }

        return (true, null, items);
    }

    public async Task<(bool Ok, string? Error)> ApplyScreeningDecisionAsync(
        int applicationId,
        ScreeningDecisionDto dto)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.");
        }

        if (!await _authz.RecruiterCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.");
        }

        if (!ScreeningDecisionStatuses.Contains(dto.Status))
        {
            return (false, "Invalid screening decision status.");
        }

        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            return (false, "A reason is required for screening decisions.");
        }

        // Never auto-reject solely from ranking — decision requires explicit recruiter action + reason.
        return await _transitions.TransitionAsync(applicationId, dto.Status, userId, dto.Reason);
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<RecruiterAssessmentListItemDto>? Result)> ListAssessmentsAsync()
    {
        if (_currentUser.OrganizationId is not int orgId)
        {
            return (false, "Organization context is required.", null);
        }

        var items = await _db.SkillAssessments
            .AsNoTracking()
            .Where(a => a.OrganizationId == orgId || (a.Job != null && a.Job.OrganizationId == orgId))
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new RecruiterAssessmentListItemDto
            {
                Id = a.Id,
                Title = a.Title,
                JobId = a.JobId,
                QuestionCount = a.Questions.Count,
                PassingScorePercent = a.PassingScorePercent,
                DurationMinutes = a.DurationMinutes,
                MaxAttempts = a.MaxAttempts,
                IsArchived = a.IsArchived,
                RevealResultsToCandidate = a.RevealResultsToCandidate
            })
            .ToListAsync();

        return (true, null, items);
    }

    public async Task<(bool Ok, string? Error, RecruiterAssessmentDetailDto? Result)> GetAssessmentAsync(int id)
    {
        var assessment = await LoadOwnedAssessmentAsync(id);
        if (assessment is null)
        {
            return (false, "Assessment not found or access denied.", null);
        }

        return (true, null, MapAssessment(assessment));
    }

    public async Task<(bool Ok, string? Error, RecruiterAssessmentDetailDto? Result)> CreateAssessmentAsync(
        UpsertAssessmentDto dto)
    {
        if (_currentUser.UserId is not int userId || _currentUser.OrganizationId is not int orgId)
        {
            return (false, "Organization context is required.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return (false, "Title is required.", null);
        }

        if (dto.JobId is int jobId && !await _authz.RecruiterOwnsJobAsync(jobId))
        {
            return (false, "Job not found or access denied.", null);
        }

        if (dto.MaxAttempts < 1)
        {
            return (false, "Max attempts must be at least 1.", null);
        }

        var assessment = new SkillAssessment
        {
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            JobId = dto.JobId,
            OrganizationId = orgId,
            PassingScorePercent = dto.PassingScorePercent,
            DurationMinutes = dto.DurationMinutes,
            MaxAttempts = dto.MaxAttempts,
            RevealResultsToCandidate = dto.RevealResultsToCandidate,
            Status = AssessmentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.SkillAssessments.Add(assessment);
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "AssessmentCreated",
            EntityType = "SkillAssessment",
            EntityId = null,
            Details = assessment.Title,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var loaded = await LoadOwnedAssessmentAsync(assessment.Id);
        return (true, null, MapAssessment(loaded!));
    }

    public async Task<(bool Ok, string? Error, RecruiterAssessmentDetailDto? Result)> UpdateAssessmentAsync(
        int id,
        UpsertAssessmentDto dto)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        var assessment = await LoadOwnedAssessmentAsync(id, tracking: true);
        if (assessment is null)
        {
            return (false, "Assessment not found or access denied.", null);
        }

        if (assessment.IsArchived)
        {
            return (false, "Archived assessments cannot be updated.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return (false, "Title is required.", null);
        }

        if (dto.JobId is int jobId && !await _authz.RecruiterOwnsJobAsync(jobId))
        {
            return (false, "Job not found or access denied.", null);
        }

        assessment.Title = dto.Title.Trim();
        assessment.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        assessment.JobId = dto.JobId;
        assessment.PassingScorePercent = dto.PassingScorePercent;
        assessment.DurationMinutes = dto.DurationMinutes;
        assessment.MaxAttempts = Math.Max(1, dto.MaxAttempts);
        assessment.RevealResultsToCandidate = dto.RevealResultsToCandidate;
        assessment.UpdatedAtUtc = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "AssessmentUpdated",
            EntityType = "SkillAssessment",
            EntityId = assessment.Id,
            Details = assessment.Title,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return (true, null, MapAssessment(assessment));
    }

    public async Task<(bool Ok, string? Error)> ArchiveAssessmentAsync(int id)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.");
        }

        var assessment = await LoadOwnedAssessmentAsync(id, tracking: true);
        if (assessment is null)
        {
            return (false, "Assessment not found or access denied.");
        }

        assessment.IsArchived = true;
        assessment.UpdatedAtUtc = DateTime.UtcNow;
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "AssessmentArchived",
            EntityType = "SkillAssessment",
            EntityId = id,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error, RecruiterAssessmentQuestionDto? Result)> AddQuestionAsync(
        int assessmentId,
        UpsertAssessmentQuestionDto dto)
    {
        var assessment = await LoadOwnedAssessmentAsync(assessmentId, tracking: true);
        if (assessment is null)
        {
            return (false, "Assessment not found or access denied.", null);
        }

        if (assessment.IsArchived)
        {
            return (false, "Cannot edit archived assessment questions.", null);
        }

        var validation = ValidateQuestion(dto);
        if (validation is not null)
        {
            return (false, validation, null);
        }

        var question = new AssessmentQuestion
        {
            SkillAssessmentId = assessmentId,
            QuestionText = dto.QuestionText.Trim(),
            QuestionType = dto.QuestionType.Trim(),
            Points = dto.Points,
            SortOrder = dto.SortOrder,
            OptionsJson = dto.OptionsJson,
            CorrectAnswerKey = dto.CorrectAnswerKey.Trim()
        };
        _db.AssessmentQuestions.Add(question);
        await _db.SaveChangesAsync();
        return (true, null, MapQuestion(question));
    }

    public async Task<(bool Ok, string? Error, RecruiterAssessmentQuestionDto? Result)> UpdateQuestionAsync(
        int assessmentId,
        int questionId,
        UpsertAssessmentQuestionDto dto)
    {
        var assessment = await LoadOwnedAssessmentAsync(assessmentId, tracking: true);
        if (assessment is null)
        {
            return (false, "Assessment not found or access denied.", null);
        }

        if (assessment.IsArchived)
        {
            return (false, "Cannot edit archived assessment questions.", null);
        }

        var question = assessment.Questions.FirstOrDefault(q => q.Id == questionId);
        if (question is null)
        {
            return (false, "Question not found.", null);
        }

        var validation = ValidateQuestion(dto);
        if (validation is not null)
        {
            return (false, validation, null);
        }

        question.QuestionText = dto.QuestionText.Trim();
        question.QuestionType = dto.QuestionType.Trim();
        question.Points = dto.Points;
        question.SortOrder = dto.SortOrder;
        question.OptionsJson = dto.OptionsJson;
        question.CorrectAnswerKey = dto.CorrectAnswerKey.Trim();
        await _db.SaveChangesAsync();
        return (true, null, MapQuestion(question));
    }

    public async Task<(bool Ok, string? Error)> DeleteQuestionAsync(int assessmentId, int questionId)
    {
        var assessment = await LoadOwnedAssessmentAsync(assessmentId, tracking: true);
        if (assessment is null)
        {
            return (false, "Assessment not found or access denied.");
        }

        if (assessment.IsArchived)
        {
            return (false, "Cannot edit archived assessment questions.");
        }

        var question = assessment.Questions.FirstOrDefault(q => q.Id == questionId);
        if (question is null)
        {
            return (false, "Question not found.");
        }

        var hasAttempts = await _db.AssessmentAnswers.AnyAsync(a => a.AssessmentQuestionId == questionId);
        if (hasAttempts)
        {
            return (false, "Cannot delete a question that already has candidate answers. Archive the assessment instead.");
        }

        _db.AssessmentQuestions.Remove(question);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error, RecruiterAssignmentDetailDto? Result)> AssignAssessmentAsync(
        int applicationId,
        AssignAssessmentDto dto)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        if (!await _authz.RecruiterCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.", null);
        }

        var assessment = await LoadOwnedAssessmentAsync(dto.AssessmentId);
        if (assessment is null || assessment.IsArchived)
        {
            return (false, "Assessment not found or access denied.", null);
        }

        var application = await _db.Applications.FirstAsync(a => a.Id == applicationId);
        var assignment = new AssessmentAssignment
        {
            SkillAssessmentId = assessment.Id,
            CandidateId = application.CandidateId,
            ApplicationId = applicationId,
            AssignedAtUtc = DateTime.UtcNow,
            StartsAtUtc = dto.StartsAtUtc,
            ExpiresAtUtc = dto.ExpiresAtUtc,
            MaxAttempts = dto.MaxAttempts ?? assessment.MaxAttempts,
            RevealResultsToCandidate = dto.RevealResultsToCandidate ?? assessment.RevealResultsToCandidate,
            Status = AssessmentStatus.Pending
        };
        _db.AssessmentAssignments.Add(assignment);
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "AssessmentAssigned",
            EntityType = "AssessmentAssignment",
            EntityId = null,
            Details = $"Application {applicationId} → Assessment {assessment.Id}",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _notifications.CreateAsync(
            application.CandidateId,
            NotificationCategories.AssessmentAssigned,
            "Assessment assigned",
            $"You have been assigned: {assessment.Title}",
            "AssessmentAssignment",
            null,
            "/candidate/assessments",
            saveChanges: false);

        if (application.Status is ApplicationStatus.Pending
            or ApplicationStatus.UnderReview
            or ApplicationStatus.ManualReview)
        {
            await _transitions.TransitionAsync(
                applicationId,
                ApplicationStatus.Assessment,
                userId,
                "Assessment assigned",
                notifyCandidate: false);
        }
        else
        {
            await _db.SaveChangesAsync();
        }

        return await GetAssignmentAsync(assignment.Id);
    }

    public async Task<(bool Ok, string? Error, RecruiterAssignmentDetailDto? Result)> GetAssignmentAsync(
        int assignmentId)
    {
        var assignment = await _db.AssessmentAssignments
            .AsNoTracking()
            .Include(a => a.SkillAssessment)
            .Include(a => a.Attempts).ThenInclude(t => t.Result)
            .Include(a => a.Application)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment is null)
        {
            return (false, "Assignment not found or access denied.", null);
        }

        if (assignment.ApplicationId is int appId)
        {
            if (!await _authz.RecruiterCanAccessApplicationAsync(appId))
            {
                return (false, "Assignment not found or access denied.", null);
            }
        }
        else if (!await OwnsAssessmentAsync(assignment.SkillAssessmentId))
        {
            return (false, "Assignment not found or access denied.", null);
        }

        return (true, null, MapAssignment(assignment));
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<RecruiterAttemptSummaryDto>? Result)> GetAssignmentAttemptsAsync(
        int assignmentId)
    {
        var (ok, error, detail) = await GetAssignmentAsync(assignmentId);
        if (!ok || detail is null)
        {
            return (false, error, null);
        }

        return (true, null, detail.Attempts);
    }

    public async Task<(bool Ok, string? Error, ApplicationMessageThreadDto? Result)> GetMessagesAsync(
        int applicationId,
        int page,
        int pageSize)
    {
        var canRecruiter = await _authz.RecruiterCanAccessApplicationAsync(applicationId);
        var canCandidate = false;
        if (!canRecruiter && _currentUser.UserId is int uid)
        {
            canCandidate = await _db.Applications.AnyAsync(a => a.Id == applicationId && a.CandidateId == uid);
        }

        if (!canRecruiter && !canCandidate)
        {
            return (false, "Application not found or access denied.", null);
        }

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 50);

        var query = _db.ApplicationMessages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.ApplicationId == applicationId)
            .OrderBy(m => m.SentAtUtc);

        var total = await query.CountAsync();
        var messages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (true, null, new ApplicationMessageThreadDto
        {
            ApplicationId = applicationId,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Messages = messages.Select(MapMessage).ToList()
        });
    }

    public async Task<(bool Ok, string? Error, ApplicationMessageDto? Result)> SendRecruiterMessageAsync(
        int applicationId,
        string body)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        if (!await _authz.RecruiterCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.", null);
        }

        return await SendMessageAsync(applicationId, userId, "Recruiter", body, notifyCandidate: true);
    }

    public async Task<(bool Ok, string? Error, ApplicationMessageDto? Result)> SendCandidateMessageAsync(
        int applicationId,
        string body)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        var owns = await _db.Applications.AnyAsync(a => a.Id == applicationId && a.CandidateId == userId);
        if (!owns)
        {
            return (false, "Application not found or access denied.", null);
        }

        return await SendMessageAsync(applicationId, userId, "Candidate", body, notifyCandidate: false);
    }

    public async Task<(bool Ok, string? Error)> MarkMessagesReadAsync(int applicationId)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.");
        }

        var canRecruiter = await _authz.RecruiterCanAccessApplicationAsync(applicationId);
        var canCandidate = await _db.Applications.AnyAsync(a => a.Id == applicationId && a.CandidateId == userId);
        if (!canRecruiter && !canCandidate)
        {
            return (false, "Application not found or access denied.");
        }

        var role = canRecruiter && !canCandidate ? "Recruiter" : canCandidate && !canRecruiter ? "Candidate" : null;
        var messages = await _db.ApplicationMessages
            .Where(m => m.ApplicationId == applicationId && !m.IsReadByRecipient)
            .ToListAsync();

        foreach (var message in messages)
        {
            if (role == "Recruiter" && message.SenderRole == "Candidate")
            {
                message.IsReadByRecipient = true;
            }
            else if (role == "Candidate" && message.SenderRole == "Recruiter")
            {
                message.IsReadByRecipient = true;
            }
            else if (role is null)
            {
                if (message.SenderUserId != userId)
                {
                    message.IsReadByRecipient = true;
                }
            }
        }

        await _db.SaveChangesAsync();
        return (true, null);
    }

    private async Task<(bool Ok, string? Error, ApplicationMessageDto? Result)> SendMessageAsync(
        int applicationId,
        int senderUserId,
        string senderRole,
        string body,
        bool notifyCandidate)
    {
        var sanitized = Sanitize(body);
        if (sanitized is null)
        {
            return (false, "Message body is required.", null);
        }

        if (sanitized.Contains("password", StringComparison.OrdinalIgnoreCase)
            || sanitized.Contains("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Message content rejected for security reasons.", null);
        }

        var message = new ApplicationMessage
        {
            ApplicationId = applicationId,
            SenderUserId = senderUserId,
            SenderRole = senderRole,
            Body = sanitized,
            IsReadByRecipient = false,
            SentAtUtc = DateTime.UtcNow
        };
        _db.ApplicationMessages.Add(message);
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = senderUserId,
            Action = "ApplicationMessageSent",
            EntityType = "Application",
            EntityId = applicationId,
            Details = senderRole,
            CreatedAtUtc = DateTime.UtcNow
        });

        if (notifyCandidate)
        {
            var application = await _db.Applications.AsNoTracking().FirstAsync(a => a.Id == applicationId);
            await _notifications.CreateAsync(
                application.CandidateId,
                NotificationCategories.ApplicationStatusUpdated,
                "New message from recruiter",
                "You have a new message on your application.",
                "Application",
                applicationId,
                $"/candidate/applications/{applicationId}",
                saveChanges: false);
        }

        await _db.SaveChangesAsync();
        var sender = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == senderUserId);
        message.Sender = sender;
        return (true, null, MapMessage(message));
    }

    private async Task<SkillAssessment?> LoadOwnedAssessmentAsync(int id, bool tracking = false)
    {
        if (_currentUser.OrganizationId is not int orgId)
        {
            return null;
        }

        var query = tracking ? _db.SkillAssessments.AsQueryable() : _db.SkillAssessments.AsNoTracking();
        query = query.Include(a => a.Questions).Include(a => a.Job);
        var assessment = await query.FirstOrDefaultAsync(a => a.Id == id);
        if (assessment is null)
        {
            return null;
        }

        if (assessment.OrganizationId == orgId)
        {
            return assessment;
        }

        if (assessment.Job?.OrganizationId == orgId)
        {
            return assessment;
        }

        if (_currentUser.IsInRole("Admin"))
        {
            return assessment;
        }

        return null;
    }

    private async Task<bool> OwnsAssessmentAsync(int assessmentId) =>
        await LoadOwnedAssessmentAsync(assessmentId) is not null;

    private static RecruiterAssessmentDetailDto MapAssessment(SkillAssessment a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Description = a.Description,
        JobId = a.JobId,
        OrganizationId = a.OrganizationId,
        PassingScorePercent = a.PassingScorePercent,
        DurationMinutes = a.DurationMinutes,
        MaxAttempts = a.MaxAttempts,
        RevealResultsToCandidate = a.RevealResultsToCandidate,
        IsArchived = a.IsArchived,
        Questions = a.Questions.OrderBy(q => q.SortOrder).Select(MapQuestion).ToList()
    };

    private static RecruiterAssessmentQuestionDto MapQuestion(AssessmentQuestion q) => new()
    {
        Id = q.Id,
        QuestionText = q.QuestionText,
        QuestionType = q.QuestionType,
        Points = q.Points,
        SortOrder = q.SortOrder,
        OptionsJson = q.OptionsJson,
        CorrectAnswerKey = q.CorrectAnswerKey
    };

    private static RecruiterAssignmentDetailDto MapAssignment(AssessmentAssignment a) => new()
    {
        Id = a.Id,
        AssessmentId = a.SkillAssessmentId,
        AssessmentTitle = a.SkillAssessment.Title,
        ApplicationId = a.ApplicationId,
        CandidateId = a.CandidateId,
        Status = a.Status,
        AssignedAtUtc = a.AssignedAtUtc,
        StartsAtUtc = a.StartsAtUtc,
        ExpiresAtUtc = a.ExpiresAtUtc,
        MaxAttempts = a.MaxAttempts,
        Attempts = a.Attempts
            .OrderByDescending(t => t.StartedAtUtc)
            .Select(t => new RecruiterAttemptSummaryDto
            {
                AttemptId = t.Id,
                Status = t.Status,
                StartedAtUtc = t.StartedAtUtc,
                CompletedAtUtc = t.CompletedAtUtc,
                ScorePercent = t.Result is null || t.Result.MaxScore <= 0
                    ? null
                    : Math.Round(t.Result.Score / t.Result.MaxScore * 100m, 2),
                Passed = t.Result?.Passed
            }).ToList()
    };

    private static ApplicationMessageDto MapMessage(ApplicationMessage m) => new()
    {
        Id = m.Id,
        ApplicationId = m.ApplicationId,
        SenderUserId = m.SenderUserId,
        SenderRole = m.SenderRole,
        SenderName = m.Sender?.FullName ?? string.Empty,
        Body = m.Body,
        IsReadByRecipient = m.IsReadByRecipient,
        SentAtUtc = m.SentAtUtc
    };

    private static string? ValidateQuestion(UpsertAssessmentQuestionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.QuestionText))
        {
            return "Question text is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.CorrectAnswerKey))
        {
            return "Correct answer key is required.";
        }

        if (dto.Points <= 0)
        {
            return "Points must be greater than zero.";
        }

        if (dto.QuestionType is "MultipleChoice" or "TrueFalse")
        {
            if (string.IsNullOrWhiteSpace(dto.OptionsJson))
            {
                return "OptionsJson is required for multiple-choice questions.";
            }

            try
            {
                JsonSerializer.Deserialize<string[]>(dto.OptionsJson);
            }
            catch
            {
                return "OptionsJson must be a JSON array of strings.";
            }
        }

        return null;
    }

    private static decimal ScoreProfileCompleteness(CandidateProfile profile)
    {
        var score = 0m;
        if (!string.IsNullOrWhiteSpace(profile.Summary)) score += 3;
        if (profile.YearsOfExperience is > 0) score += 2;
        if (profile.CandidateSkills.Count > 0) score += 3;
        if (profile.Educations.Count > 0) score += 1;
        if (profile.WorkExperiences.Count > 0) score += 1;
        return score;
    }

    private static string? Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > 4000)
        {
            trimmed = trimmed[..4000];
        }

        return trimmed
            .Replace("<", string.Empty, StringComparison.Ordinal)
            .Replace(">", string.Empty, StringComparison.Ordinal)
            .Replace("\0", string.Empty, StringComparison.Ordinal);
    }
}
