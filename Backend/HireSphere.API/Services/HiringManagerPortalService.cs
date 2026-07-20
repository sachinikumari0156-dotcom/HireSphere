using System.Text.RegularExpressions;
using HireSphere.API.Data;
using HireSphere.API.DTOs.Candidate;
using HireSphere.API.DTOs.HiringManager;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface IHiringManagerPortalService
{
    Task<(bool Ok, string? Error, HiringManagerDashboardDto? Result)> GetDashboardAsync();

    Task<(bool Ok, string? Error, PagedResultDto<HiringManagerJobListItemDto>? Result)> ListJobsAsync(
        HiringManagerJobListQuery query);

    Task<(bool Ok, string? Error, HiringManagerJobDetailDto? Result)> GetJobAsync(int jobId);

    Task<(bool Ok, string? Error, PagedResultDto<HiringManagerCandidateListItemDto>? Result)> ListCandidatesAsync(
        int jobId,
        HiringManagerCandidateListQuery query);

    Task<(bool Ok, string? Error, HiringManagerApplicationDetailDto? Result)> GetApplicationAsync(int applicationId);

    Task<(bool Ok, string? Error, HiringManagerComparisonDto? Result)> CompareCandidatesAsync(
        IReadOnlyList<int> applicationIds);

    Task<(bool Ok, string? Error, HiringManagerReviewCommentDto? Result)> AddReviewCommentAsync(
        int jobId,
        string content);
}

public sealed class HiringManagerPortalService : IHiringManagerPortalService
{
    public const int MaxComparisonCount = 5;
    public const string HumanReviewNotice =
        "AI-generated insight. Final recruitment decisions must be reviewed by authorized users.";

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IResourceAuthorizationService _authz;
    private readonly IJobMatchingProvider _matching;

    public HiringManagerPortalService(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        IResourceAuthorizationService authz,
        IJobMatchingProvider matching)
    {
        _db = db;
        _currentUser = currentUser;
        _authz = authz;
        _matching = matching;
    }

    public async Task<(bool Ok, string? Error, HiringManagerDashboardDto? Result)> GetDashboardAsync()
    {
        if (!TryGetManagerUserId(out var userId, out var error))
        {
            return (false, error, null);
        }

        var jobs = AssignedJobsQuery(userId);
        var jobIds = await jobs.Select(j => j.Id).ToListAsync();
        var now = DateTime.UtcNow;

        var applications = _db.Applications.AsNoTracking().Where(a => jobIds.Contains(a.JobId));
        var interviews = _db.Interviews.AsNoTracking()
            .Where(i => i.HiringManagerUserId == userId
                || i.Application.Job.HiringManagerUserId == userId
                || i.Participants.Any(p => p.UserId == userId));

        var upcomingInterviewIds = await interviews
            .Where(i => i.InterviewDate >= now
                && i.Status != InterviewStatus.Cancelled
                && i.Status != InterviewStatus.Completed
                && i.Status != InterviewStatus.NoShow)
            .Select(i => i.Id)
            .ToListAsync();

        var feedbackGiven = await _db.InterviewFeedbacks.AsNoTracking()
            .Where(f => f.InterviewerId == userId && upcomingInterviewIds.Contains(f.InterviewId))
            .Select(f => f.InterviewId)
            .Distinct()
            .ToListAsync();

        var shortlistedAppIds = await applications
            .Where(a => a.Status == ApplicationStatus.Shortlisted
                || a.Status == ApplicationStatus.InterviewScheduled
                || a.Status == ApplicationStatus.Interviewed)
            .Select(a => a.Id)
            .ToListAsync();

        var evaluatedAppIds = await _db.CandidateEvaluations.AsNoTracking()
            .Where(e => shortlistedAppIds.Contains(e.ApplicationId) && e.EvaluatorUserId == userId)
            .Select(e => e.ApplicationId)
            .Distinct()
            .ToListAsync();

        var decidedAppIds = await _db.HiringDecisions.AsNoTracking()
            .Where(d => shortlistedAppIds.Contains(d.ApplicationId)
                && (d.Status == HiringDecisionStatus.Approved || d.Status == HiringDecisionStatus.Rejected))
            .Select(d => d.ApplicationId)
            .Distinct()
            .ToListAsync();

        var activity = await _db.AuditLogs.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(10)
            .Select(a => new HiringManagerActivityItemDto
            {
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Details = a.Details,
                CreatedAtUtc = a.CreatedAtUtc
            })
            .ToListAsync();

        return (true, null, new HiringManagerDashboardDto
        {
            AssignedActiveVacancies = await jobs.CountAsync(j =>
                j.Status == JobStatus.Published || j.Status == JobStatus.Open),
            AssignedPausedVacancies = await jobs.CountAsync(j => j.Status == JobStatus.Paused),
            CandidatesAwaitingReview = await applications.CountAsync(a =>
                a.Status == ApplicationStatus.UnderReview
                || a.Status == ApplicationStatus.ManualReview
                || a.Status == ApplicationStatus.Assessment),
            CandidatesShortlisted = await applications.CountAsync(a => a.Status == ApplicationStatus.Shortlisted),
            UpcomingInterviews = upcomingInterviewIds.Count,
            PendingInterviewFeedback = upcomingInterviewIds.Count(id => !feedbackGiven.Contains(id)),
            PendingEvaluations = shortlistedAppIds.Count(id => !evaluatedAppIds.Contains(id)),
            PendingHiringDecisions = shortlistedAppIds.Count(id => !decidedAppIds.Contains(id)),
            RecentActivity = activity
        });
    }

    public async Task<(bool Ok, string? Error, PagedResultDto<HiringManagerJobListItemDto>? Result)> ListJobsAsync(
        HiringManagerJobListQuery query)
    {
        if (!TryGetManagerUserId(out var userId, out var error))
        {
            return (false, error, null);
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 10 : query.PageSize, 1, 50);
        var q = AssignedJobsQuery(userId);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            q = q.Where(j => j.Title.Contains(kw) || j.Location.Contains(kw));
        }

        if (query.Status is JobStatus status)
        {
            q = q.Where(j => j.Status == status);
        }

        q = (query.SortBy?.ToLowerInvariant(), query.SortDir?.ToLowerInvariant()) switch
        {
            ("title", "desc") => q.OrderByDescending(j => j.Title),
            ("title", _) => q.OrderBy(j => j.Title),
            ("deadline", "desc") => q.OrderByDescending(j => j.ApplicationDeadlineUtc),
            ("deadline", _) => q.OrderBy(j => j.ApplicationDeadlineUtc),
            (_, "asc") => q.OrderBy(j => j.CreatedAtUtc),
            _ => q.OrderByDescending(j => j.CreatedAtUtc)
        };

        var total = await q.CountAsync();
        var jobs = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Include(j => j.Department)
            .Include(j => j.Recruiter)
            .Include(j => j.Applications)
            .ThenInclude(a => a.Interviews)
            .ToListAsync();

        var items = jobs.Select(j => new HiringManagerJobListItemDto
        {
            Id = j.Id,
            Title = j.Title,
            Location = j.Location,
            Status = j.Status,
            DepartmentName = j.Department?.Name,
            RecruiterName = j.Recruiter?.FullName,
            ApplicantCount = j.Applications.Count,
            ShortlistCount = j.Applications.Count(a => a.Status == ApplicationStatus.Shortlisted),
            InterviewCount = j.Applications.SelectMany(a => a.Interviews).Count(),
            ApplicationDeadlineUtc = j.ApplicationDeadlineUtc,
            CreatedAtUtc = j.CreatedAtUtc
        }).ToList();

        return (true, null, new PagedResultDto<HiringManagerJobListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        });
    }

    public async Task<(bool Ok, string? Error, HiringManagerJobDetailDto? Result)> GetJobAsync(int jobId)
    {
        if (!await _authz.HiringManagerCanAccessJobAsync(jobId))
        {
            return (false, "Job not found or access denied.", null);
        }

        var job = await _db.Jobs.AsNoTracking()
            .Include(j => j.Recruiter)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Include(j => j.Applications).ThenInclude(a => a.Interviews)
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job is null)
        {
            return (false, "Job not found or access denied.", null);
        }

        var comments = await _db.JobReviewComments.AsNoTracking()
            .Include(c => c.Author)
            .Where(c => c.JobId == jobId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync();

        return (true, null, new HiringManagerJobDetailDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Responsibilities = job.Responsibilities,
            Location = job.Location,
            Status = job.Status,
            EmploymentType = job.EmploymentType,
            WorkArrangement = job.WorkArrangement,
            MinimumExperienceYears = job.MinimumExperienceYears,
            EducationRequirement = job.EducationRequirement,
            ApplicationDeadlineUtc = job.ApplicationDeadlineUtc,
            RecruiterId = job.RecruiterId,
            RecruiterName = job.Recruiter?.FullName,
            OrganizationId = job.OrganizationId,
            ApplicantCount = job.Applications.Count,
            ShortlistCount = job.Applications.Count(a => a.Status == ApplicationStatus.Shortlisted),
            InterviewCount = job.Applications.SelectMany(a => a.Interviews).Count(),
            Skills = job.JobSkills.Select(s => new HiringManagerSkillDto
            {
                SkillName = s.Skill?.Name ?? string.Empty,
                IsRequired = s.IsRequired
            }).ToList(),
            ReviewComments = comments.Select(c => new HiringManagerReviewCommentDto
            {
                Id = c.Id,
                Content = c.Content,
                AuthorUserId = c.AuthorUserId,
                AuthorName = c.Author.FullName,
                CreatedAtUtc = c.CreatedAtUtc
            }).ToList()
        });
    }

    public async Task<(bool Ok, string? Error, PagedResultDto<HiringManagerCandidateListItemDto>? Result)> ListCandidatesAsync(
        int jobId,
        HiringManagerCandidateListQuery query)
    {
        if (!await _authz.HiringManagerCanAccessJobAsync(jobId))
        {
            return (false, "Job not found or access denied.", null);
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 10 : query.PageSize, 1, 50);

        var q = _db.Applications.AsNoTracking()
            .Include(a => a.Candidate)
            .Include(a => a.Job).ThenInclude(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Include(a => a.Interviews)
            .Where(a => a.JobId == jobId);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            q = q.Where(a => a.Candidate.FullName.Contains(kw) || a.Candidate.Email.Contains(kw));
        }

        if (query.Status is ApplicationStatus status)
        {
            q = q.Where(a => a.Status == status);
        }

        q = query.SortDir?.ToLowerInvariant() == "asc"
            ? q.OrderBy(a => a.AppliedDate)
            : q.OrderByDescending(a => a.AppliedDate);

        var total = await q.CountAsync();
        var apps = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var candidateIds = apps.Select(a => a.CandidateId).Distinct().ToList();
        var appIds = apps.Select(a => a.Id).ToList();
        var profiles = await _db.CandidateProfiles.AsNoTracking()
            .Include(p => p.CandidateSkills).ThenInclude(cs => cs.Skill)
            .Include(p => p.Educations)
            .Where(p => candidateIds.Contains(p.UserId))
            .ToListAsync();
        var assignments = await _db.AssessmentAssignments.AsNoTracking()
            .Where(x => x.ApplicationId != null && appIds.Contains(x.ApplicationId.Value))
            .ToListAsync();

        var items = apps.Select(a =>
        {
            var profile = profiles.FirstOrDefault(p => p.UserId == a.CandidateId);
            var match = profile == null ? null : _matching.ComputeMatch(profile, a.Job);
            return new HiringManagerCandidateListItemDto
            {
                ApplicationId = a.Id,
                CandidateUserId = a.CandidateId,
                CandidateName = a.Candidate.FullName,
                Status = a.Status,
                MatchScore = match?.MatchScore,
                YearsOfExperience = profile?.YearsOfExperience,
                EducationSummary = profile?.Educations
                    .Select(e => $"{e.Degree} {e.Institution}".Trim())
                    .FirstOrDefault(),
                Skills = profile?.CandidateSkills.Select(s => s.Skill.Name).Take(8).ToList()
                    ?? new List<string>(),
                AppliedAtUtc = a.AppliedDate,
                AssessmentStatus = assignments.Where(x => x.ApplicationId == a.Id)
                    .OrderByDescending(x => x.AssignedAtUtc)
                    .Select(x => x.Status.ToString()).FirstOrDefault(),
                InterviewStatus = a.Interviews.OrderByDescending(i => i.InterviewDate)
                    .Select(i => i.Status.ToString()).FirstOrDefault()
            };
        }).ToList();

        return (true, null, new PagedResultDto<HiringManagerCandidateListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        });
    }

    public async Task<(bool Ok, string? Error, HiringManagerApplicationDetailDto? Result)> GetApplicationAsync(
        int applicationId)
    {
        if (!await _authz.HiringManagerCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.", null);
        }

        var application = await _db.Applications.AsNoTracking()
            .Include(a => a.Candidate)
            .Include(a => a.Job).ThenInclude(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Include(a => a.Answers).ThenInclude(ans => ans.ScreeningQuestion)
            .Include(a => a.StatusHistory)
            .Include(a => a.Interviews)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application is null)
        {
            return (false, "Application not found or access denied.", null);
        }

        var profile = await _db.CandidateProfiles.AsNoTracking()
            .Include(p => p.CandidateSkills).ThenInclude(cs => cs.Skill)
            .Include(p => p.Educations)
            .Include(p => p.WorkExperiences)
            .Include(p => p.Certifications)
            .Include(p => p.Resumes)
            .FirstOrDefaultAsync(p => p.UserId == application.CandidateId);

        var assignments = await _db.AssessmentAssignments.AsNoTracking()
            .Include(x => x.SkillAssessment)
            .Include(x => x.Attempts).ThenInclude(t => t.Result)
            .Where(x => x.ApplicationId == applicationId)
            .ToListAsync();

        var match = profile == null ? null : _matching.ComputeMatch(profile, application.Job);

        return (true, null, MapApplicationDetail(application, profile, match, assignments));
    }

    public async Task<(bool Ok, string? Error, HiringManagerComparisonDto? Result)> CompareCandidatesAsync(
        IReadOnlyList<int> applicationIds)
    {
        var ids = applicationIds?.Distinct().ToList() ?? new List<int>();
        if (ids.Count == 0)
        {
            return (false, "Select at least one candidate.", null);
        }

        if (ids.Count > MaxComparisonCount)
        {
            return (false, $"Comparison is limited to {MaxComparisonCount} candidates.", null);
        }

        var details = new List<HiringManagerApplicationDetailDto>();
        foreach (var id in ids)
        {
            var (ok, error, detail) = await GetApplicationAsync(id);
            if (!ok || detail is null)
            {
                return (false, error ?? "Application not found or access denied.", null);
            }

            details.Add(detail);
        }

        var jobIds = details.Select(d => d.JobId).Distinct().ToList();
        if (jobIds.Count != 1)
        {
            return (false, "Candidates must belong to the same assigned vacancy.", null);
        }

        return (true, null, new HiringManagerComparisonDto
        {
            JobId = jobIds[0],
            JobTitle = details[0].JobTitle,
            HumanReviewNotice = HumanReviewNotice,
            Candidates = details
        });
    }

    public async Task<(bool Ok, string? Error, HiringManagerReviewCommentDto? Result)> AddReviewCommentAsync(
        int jobId,
        string content)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        if (!await _authz.HiringManagerCanAccessJobAsync(jobId))
        {
            return (false, "Job not found or access denied.", null);
        }

        var sanitized = Sanitize(content);
        if (sanitized is null)
        {
            return (false, "Comment content is required.", null);
        }

        var comment = new JobReviewComment
        {
            JobId = jobId,
            AuthorUserId = userId,
            Content = sanitized,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.JobReviewComments.Add(comment);
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "JobReviewCommentCreated",
            EntityType = "JobReviewComment",
            EntityId = null,
            Details = $"Job {jobId}",
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var author = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
        return (true, null, new HiringManagerReviewCommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            AuthorUserId = userId,
            AuthorName = author.FullName,
            CreatedAtUtc = comment.CreatedAtUtc
        });
    }

    private static HiringManagerApplicationDetailDto MapApplicationDetail(
        Application application,
        CandidateProfile? profile,
        JobMatchResultDto? match,
        IReadOnlyList<AssessmentAssignment> assignments)
    {
        return new HiringManagerApplicationDetailDto
        {
            ApplicationId = application.Id,
            JobId = application.JobId,
            JobTitle = application.Job.Title,
            CandidateUserId = application.CandidateId,
            CandidateName = application.Candidate.FullName,
            ProfessionalSummary = profile?.Summary,
            Status = application.Status,
            MatchScore = match?.MatchScore,
            MatchExplanation = match?.Explanation,
            HumanReviewNotice = HumanReviewNotice,
            Skills = profile?.CandidateSkills.Select(s => s.Skill.Name).ToList() ?? new List<string>(),
            MissingRequiredSkills = match?.MissingSkills ?? new List<string>(),
            YearsOfExperience = profile?.YearsOfExperience,
            Education = profile?.Educations.Select(e => new HiringManagerEducationDto
            {
                Institution = e.Institution,
                Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy
            }).ToList() ?? new List<HiringManagerEducationDto>(),
            Experience = profile?.WorkExperiences.Select(w => new HiringManagerExperienceDto
            {
                CompanyName = w.CompanyName,
                JobTitle = w.JobTitle,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                IsCurrentRole = w.IsCurrentRole
            }).ToList() ?? new List<HiringManagerExperienceDto>(),
            Certifications = profile?.Certifications.Select(c => new HiringManagerCertificationDto
            {
                Name = c.Name,
                Issuer = c.IssuingOrganization
            }).ToList() ?? new List<HiringManagerCertificationDto>(),
            Resumes = profile?.Resumes.Select(r => new HiringManagerResumeDto
            {
                DocumentId = r.Id,
                FileName = r.FileName,
                IsPrimary = r.IsPrimary
            }).ToList() ?? new List<HiringManagerResumeDto>(),
            ScreeningAnswers = application.Answers.Select(a => new HiringManagerScreeningAnswerDto
            {
                QuestionText = a.ScreeningQuestion?.QuestionText ?? a.QuestionText,
                AnswerText = a.AnswerText
            }).ToList(),
            StatusHistory = application.StatusHistory.OrderBy(h => h.ChangedAtUtc)
                .Select(h => new HiringManagerStatusHistoryDto
                {
                    Status = h.Status,
                    ChangedAtUtc = h.ChangedAtUtc,
                    Notes = h.Notes
                }).ToList(),
            Interviews = application.Interviews.OrderByDescending(i => i.InterviewDate)
                .Select(i => new HiringManagerInterviewSummaryDto
                {
                    Id = i.Id,
                    Status = i.Status.ToString(),
                    InterviewDateUtc = i.InterviewDate,
                    TimeZoneId = i.TimeZoneId,
                    InterviewType = i.InterviewType
                }).ToList(),
            Assessments = assignments.Select(x =>
            {
                var latest = x.Attempts.OrderByDescending(t => t.StartedAtUtc).FirstOrDefault();
                var result = latest?.Result;
                decimal? percent = null;
                if (result is not null && result.MaxScore > 0)
                {
                    percent = Math.Round(result.Score / result.MaxScore * 100m, 2);
                }

                return new HiringManagerAssessmentSummaryDto
                {
                    AssignmentId = x.Id,
                    Title = x.SkillAssessment?.Title ?? "Assessment",
                    Status = x.Status.ToString(),
                    ScorePercent = percent,
                    Passed = result?.Passed
                };
            }).ToList()
        };
    }

    private IQueryable<Job> AssignedJobsQuery(int userId) =>
        _db.Jobs.AsNoTracking().Where(j => j.HiringManagerUserId == userId);

    private bool TryGetManagerUserId(out int userId, out string? error)
    {
        userId = 0;
        error = null;
        if (_currentUser.IsInRole("Admin") && _currentUser.UserId is int adminId)
        {
            // Admin dashboard uses empty assigned set unless impersonating; still allow call.
            userId = adminId;
            return true;
        }

        if (_currentUser.UserId is not int id || !_currentUser.IsInRole("HiringManager"))
        {
            error = "Unauthorized.";
            return false;
        }

        userId = id;
        return true;
    }

    private static string? Sanitize(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var trimmed = content.Trim();
        trimmed = Regex.Replace(trimmed, "<.*?>", string.Empty);
        return trimmed.Length > 4000 ? trimmed[..4000] : trimmed;
    }
}
