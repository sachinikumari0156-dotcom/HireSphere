using HireSphere.API.Data;
using HireSphere.API.DTOs.Candidate;
using HireSphere.API.DTOs.Recruiter;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface IRecruiterPortalService
{
    Task<(bool Ok, string? Error, RecruiterDashboardDto? Result)> GetDashboardAsync();

    Task<(bool Ok, string? Error, PagedResultDto<RecruiterJobListItemDto>? Result)> ListJobsAsync(
        RecruiterJobListQuery query);

    Task<(bool Ok, string? Error, RecruiterJobDetailDto? Result)> GetJobAsync(int jobId);

    Task<(bool Ok, string? Error, RecruiterJobDetailDto? Result)> CreateJobAsync(UpsertRecruiterJobDto dto);

    Task<(bool Ok, string? Error, RecruiterJobDetailDto? Result)> UpdateJobAsync(int jobId, UpsertRecruiterJobDto dto);

    Task<(bool Ok, string? Error)> DeleteJobAsync(int jobId);

    Task<(bool Ok, string? Error, RecruiterJobDetailDto? Result)> ChangeJobStatusAsync(int jobId, JobStatus status);

    Task<(bool Ok, string? Error, PagedResultDto<RecruiterApplicantListItemDto>? Result)> ListApplicantsAsync(
        int jobId,
        RecruiterPipelineQuery query);

    Task<(bool Ok, string? Error, RecruiterApplicationDetailDto? Result)> GetApplicationAsync(int applicationId);

    Task<(bool Ok, string? Error)> ChangeApplicationStatusAsync(
        int applicationId,
        ApplicationStatus status,
        string? notes);

    Task<(bool Ok, string? Error, RecruiterNoteDto? Result)> AddNoteAsync(int applicationId, string content);

    Task<(bool Ok, string? Error)> DeleteNoteAsync(int noteId);

    Task<(bool Ok, string? Error, CandidateComparisonDto? Result)> CompareApplicantsAsync(IReadOnlyList<int> applicationIds);
}

public sealed class RecruiterPortalService : IRecruiterPortalService
{
    public const int MaxComparisonCount = 5;

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IResourceAuthorizationService _authz;
    private readonly IJobStatusTransitionService _jobTransitions;
    private readonly IApplicationStatusTransitionService _applicationTransitions;
    private readonly IJobMatchingProvider _matching;

    public RecruiterPortalService(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        IResourceAuthorizationService authz,
        IJobStatusTransitionService jobTransitions,
        IApplicationStatusTransitionService applicationTransitions,
        IJobMatchingProvider matching)
    {
        _db = db;
        _currentUser = currentUser;
        _authz = authz;
        _jobTransitions = jobTransitions;
        _applicationTransitions = applicationTransitions;
        _matching = matching;
    }

    public async Task<(bool Ok, string? Error, RecruiterDashboardDto? Result)> GetDashboardAsync()
    {
        var (ok, error, scope) = RequireOrgScope();
        if (!ok || scope is null)
        {
            return (false, error, null);
        }

        var jobs = OrgJobsQuery(scope);
        var now = DateTime.UtcNow;
        var weekAgo = now.AddDays(-7);

        var jobRows = await jobs
            .Select(j => new { j.Id, j.Status })
            .ToListAsync();

        var jobIds = jobRows.Select(j => j.Id).ToList();
        var applications = await _db.Applications
            .AsNoTracking()
            .Where(a => jobIds.Contains(a.JobId))
            .Select(a => new { a.Id, a.Status, a.AppliedDate, a.CandidateId })
            .ToListAsync();

        var pendingAssessments = jobIds.Count == 0
            ? 0
            : await (
                from aa in _db.AssessmentAssignments.AsNoTracking()
                join app in _db.Applications.AsNoTracking() on aa.ApplicationId equals app.Id
                where jobIds.Contains(app.JobId)
                      && (aa.Status == AssessmentStatus.Pending || aa.Status == AssessmentStatus.InProgress)
                select aa.Id).CountAsync();

        var upcomingInterviews = jobIds.Count == 0
            ? 0
            : await (
                from i in _db.Interviews.AsNoTracking()
                join app in _db.Applications.AsNoTracking() on i.ApplicationId equals app.Id
                where jobIds.Contains(app.JobId)
                      && i.Status == InterviewStatus.Scheduled
                      && i.InterviewDate >= now
                select i.Id).CountAsync();

        var recent = await _db.AuditLogs
            .AsNoTracking()
            .Where(a => a.UserId == scope.UserId
                || (a.EntityType == "Job" && a.EntityId != null && jobIds.Contains(a.EntityId.Value))
                || (a.EntityType == "Application" && a.EntityId != null
                    && applications.Select(x => x.Id).Contains(a.EntityId.Value)))
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(10)
            .Select(a => new RecruiterActivityItemDto
            {
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Details = a.Details,
                CreatedAtUtc = a.CreatedAtUtc
            })
            .ToListAsync();

        return (true, null, new RecruiterDashboardDto
        {
            ActiveJobs = jobRows.Count(j => j.Status is JobStatus.Open or JobStatus.Published),
            DraftJobs = jobRows.Count(j => j.Status == JobStatus.Draft),
            PausedJobs = jobRows.Count(j => j.Status == JobStatus.Paused),
            ClosedJobs = jobRows.Count(j => j.Status is JobStatus.Closed or JobStatus.Archived),
            TotalApplicants = applications.Count,
            NewApplicants = applications.Count(a => a.AppliedDate >= weekAgo),
            CandidatesInScreening = applications.Count(a =>
                a.Status is ApplicationStatus.UnderReview or ApplicationStatus.ManualReview),
            ShortlistedCandidates = applications.Count(a => a.Status == ApplicationStatus.Shortlisted),
            PendingAssessments = pendingAssessments,
            UpcomingInterviews = upcomingInterviews,
            RecentActivity = recent
        });
    }

    public async Task<(bool Ok, string? Error, PagedResultDto<RecruiterJobListItemDto>? Result)> ListJobsAsync(
        RecruiterJobListQuery query)
    {
        var (ok, error, scope) = RequireOrgScope();
        if (!ok || scope is null)
        {
            return (false, error, null);
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : Math.Min(query.PageSize, 50);
        var jobsQuery = OrgJobsQuery(scope);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            jobsQuery = jobsQuery.Where(j =>
                j.Title.Contains(keyword)
                || j.Description.Contains(keyword)
                || j.RequiredSkills.Contains(keyword)
                || j.Location.Contains(keyword));
        }

        if (query.Status is JobStatus status)
        {
            jobsQuery = jobsQuery.Where(j => j.Status == status);
        }

        if (query.DepartmentId is int departmentId)
        {
            jobsQuery = jobsQuery.Where(j => j.DepartmentId == departmentId);
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var location = query.Location.Trim();
            jobsQuery = jobsQuery.Where(j => j.Location.Contains(location));
        }

        if (query.EmploymentType is EmploymentType employmentType)
        {
            jobsQuery = jobsQuery.Where(j => j.EmploymentType == employmentType);
        }

        if (query.WorkArrangement is WorkArrangement workArrangement)
        {
            jobsQuery = jobsQuery.Where(j => j.WorkArrangement == workArrangement);
        }

        if (query.PostedFromUtc is DateTime from)
        {
            jobsQuery = jobsQuery.Where(j => j.PostedDate >= from);
        }

        if (query.PostedToUtc is DateTime to)
        {
            jobsQuery = jobsQuery.Where(j => j.PostedDate <= to);
        }

        var sortBy = (query.SortBy ?? "created").Trim().ToLowerInvariant();
        var sortDesc = !string.Equals(query.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
        jobsQuery = sortBy switch
        {
            "title" => sortDesc ? jobsQuery.OrderByDescending(j => j.Title) : jobsQuery.OrderBy(j => j.Title),
            "status" => sortDesc ? jobsQuery.OrderByDescending(j => j.Status) : jobsQuery.OrderBy(j => j.Status),
            "posteddate" => sortDesc
                ? jobsQuery.OrderByDescending(j => j.PostedDate)
                : jobsQuery.OrderBy(j => j.PostedDate),
            _ => sortDesc
                ? jobsQuery.OrderByDescending(j => j.CreatedAtUtc)
                : jobsQuery.OrderBy(j => j.CreatedAtUtc)
        };

        var totalCount = await jobsQuery.CountAsync();
        var items = await jobsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new RecruiterJobListItemDto
            {
                Id = j.Id,
                Title = j.Title,
                Location = j.Location,
                Status = j.Status,
                EmploymentType = j.EmploymentType,
                WorkArrangement = j.WorkArrangement,
                DepartmentName = j.Department != null ? j.Department.Name : null,
                ApplicantCount = j.Applications.Count,
                Vacancies = j.Vacancies,
                PostedDate = j.PostedDate,
                ApplicationDeadlineUtc = j.ApplicationDeadlineUtc,
                CreatedAtUtc = j.CreatedAtUtc
            })
            .ToListAsync();

        return (true, null, new PagedResultDto<RecruiterJobListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<(bool Ok, string? Error, RecruiterJobDetailDto? Result)> GetJobAsync(int jobId)
    {
        if (!await _authz.RecruiterOwnsJobAsync(jobId))
        {
            return (false, "Job not found or access denied.", null);
        }

        var job = await LoadJobGraphAsync(jobId);
        return job is null
            ? (false, "Job not found or access denied.", null)
            : (true, null, MapJobDetail(job));
    }

    public async Task<(bool Ok, string? Error, RecruiterJobDetailDto? Result)> CreateJobAsync(UpsertRecruiterJobDto dto)
    {
        var (ok, error, scope) = RequireOrgScope();
        if (!ok || scope is null)
        {
            return (false, error, null);
        }

        var validation = ValidateJobDto(dto);
        if (validation is not null)
        {
            return (false, validation, null);
        }

        if (dto.HiringManagerUserId is int hmId)
        {
            var hmOk = await HiringManagerInOrgAsync(hmId, scope.OrganizationId);
            if (!hmOk)
            {
                return (false, "Hiring Manager is not authorized for this organization.", null);
            }
        }

        if (dto.DepartmentId is int deptId)
        {
            var deptOk = await _db.Departments.AnyAsync(d =>
                d.Id == deptId && d.OrganizationId == scope.OrganizationId);
            if (!deptOk)
            {
                return (false, "Department is not part of your organization.", null);
            }
        }

        var job = new Job
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Responsibilities = NullIfWhiteSpace(dto.Responsibilities),
            RequiredSkills = string.IsNullOrWhiteSpace(dto.RequiredSkillsText)
                ? string.Join(", ", dto.Skills.Where(s => s.IsRequired).Select(s => s.SkillName).Where(n => !string.IsNullOrWhiteSpace(n)))
                : dto.RequiredSkillsText.Trim(),
            Location = dto.Location.Trim(),
            JobType = string.IsNullOrWhiteSpace(dto.JobType) ? dto.EmploymentType.ToString() : dto.JobType.Trim(),
            EmploymentType = dto.EmploymentType,
            WorkArrangement = dto.WorkArrangement,
            SalaryMin = dto.SalaryMin,
            SalaryMax = dto.SalaryMax,
            SalaryCurrency = NullIfWhiteSpace(dto.SalaryCurrency),
            SalaryVisible = dto.SalaryVisible,
            MinimumExperienceYears = dto.MinimumExperienceYears,
            EducationRequirement = NullIfWhiteSpace(dto.EducationRequirement),
            Vacancies = dto.Vacancies,
            ApplicationDeadlineUtc = dto.ApplicationDeadlineUtc,
            DepartmentId = dto.DepartmentId,
            HiringManagerUserId = dto.HiringManagerUserId,
            RecruiterId = scope.UserId,
            OrganizationId = scope.OrganizationId,
            Status = JobStatus.Draft,
            PostedDate = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();

        await ReplaceSkillsAndQuestionsAsync(job, dto);
        await WriteAuditAsync(scope.UserId, "JobCreated", "Job", job.Id, job.Title);
        await _db.SaveChangesAsync();

        var loaded = await LoadJobGraphAsync(job.Id);
        return (true, null, MapJobDetail(loaded!));
    }

    public async Task<(bool Ok, string? Error, RecruiterJobDetailDto? Result)> UpdateJobAsync(
        int jobId,
        UpsertRecruiterJobDto dto)
    {
        var (ok, error, scope) = RequireOrgScope();
        if (!ok || scope is null)
        {
            return (false, error, null);
        }

        if (!await _authz.RecruiterOwnsJobAsync(jobId))
        {
            return (false, "Job not found or access denied.", null);
        }

        var validation = ValidateJobDto(dto);
        if (validation is not null)
        {
            return (false, validation, null);
        }

        var job = await _db.Jobs
            .Include(j => j.JobSkills)
            .Include(j => j.ScreeningQuestions)
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job is null)
        {
            return (false, "Job not found or access denied.", null);
        }

        if (dto.HiringManagerUserId is int hmId)
        {
            var hmOk = await HiringManagerInOrgAsync(hmId, scope.OrganizationId);
            if (!hmOk)
            {
                return (false, "Hiring Manager is not authorized for this organization.", null);
            }
        }

        if (dto.DepartmentId is int deptId)
        {
            var deptOk = await _db.Departments.AnyAsync(d =>
                d.Id == deptId && d.OrganizationId == scope.OrganizationId);
            if (!deptOk)
            {
                return (false, "Department is not part of your organization.", null);
            }
        }

        job.Title = dto.Title.Trim();
        job.Description = dto.Description.Trim();
        job.Responsibilities = NullIfWhiteSpace(dto.Responsibilities);
        job.RequiredSkills = string.IsNullOrWhiteSpace(dto.RequiredSkillsText)
            ? string.Join(", ", dto.Skills.Where(s => s.IsRequired).Select(s => s.SkillName).Where(n => !string.IsNullOrWhiteSpace(n)))
            : dto.RequiredSkillsText.Trim();
        job.Location = dto.Location.Trim();
        job.JobType = string.IsNullOrWhiteSpace(dto.JobType) ? dto.EmploymentType.ToString() : dto.JobType.Trim();
        job.EmploymentType = dto.EmploymentType;
        job.WorkArrangement = dto.WorkArrangement;
        job.SalaryMin = dto.SalaryMin;
        job.SalaryMax = dto.SalaryMax;
        job.SalaryCurrency = NullIfWhiteSpace(dto.SalaryCurrency);
        job.SalaryVisible = dto.SalaryVisible;
        job.MinimumExperienceYears = dto.MinimumExperienceYears;
        job.EducationRequirement = NullIfWhiteSpace(dto.EducationRequirement);
        job.Vacancies = dto.Vacancies;
        job.ApplicationDeadlineUtc = dto.ApplicationDeadlineUtc;
        job.DepartmentId = dto.DepartmentId;
        job.HiringManagerUserId = dto.HiringManagerUserId;
        job.UpdatedAtUtc = DateTime.UtcNow;

        await ReplaceSkillsAndQuestionsAsync(job, dto);
        await WriteAuditAsync(scope.UserId, "JobUpdated", "Job", job.Id, job.Title);
        await _db.SaveChangesAsync();

        var loaded = await LoadJobGraphAsync(job.Id);
        return (true, null, MapJobDetail(loaded!));
    }

    public async Task<(bool Ok, string? Error)> DeleteJobAsync(int jobId)
    {
        var (ok, error, scope) = RequireOrgScope();
        if (!ok || scope is null)
        {
            return (false, error);
        }

        if (!await _authz.RecruiterOwnsJobAsync(jobId))
        {
            return (false, "Job not found or access denied.");
        }

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job is null)
        {
            return (false, "Job not found or access denied.");
        }

        if (job.Status is not (JobStatus.Draft or JobStatus.Archived))
        {
            return (false, "Only Draft or Archived jobs can be deleted. Close the job instead.");
        }

        var hasApplications = await _db.Applications.AnyAsync(a => a.JobId == jobId);
        if (hasApplications)
        {
            return (false, "Cannot delete a job that has applications.");
        }

        _db.Jobs.Remove(job);
        await WriteAuditAsync(scope.UserId, "JobDeleted", "Job", jobId, job.Title);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error, RecruiterJobDetailDto? Result)> ChangeJobStatusAsync(
        int jobId,
        JobStatus status)
    {
        var (ok, error, scope) = RequireOrgScope();
        if (!ok || scope is null)
        {
            return (false, error, null);
        }

        if (!await _authz.RecruiterOwnsJobAsync(jobId))
        {
            return (false, "Job not found or access denied.", null);
        }

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job is null)
        {
            return (false, "Job not found or access denied.", null);
        }

        var (transitionOk, transitionError) = _jobTransitions.Apply(job, status);
        if (!transitionOk)
        {
            return (false, transitionError, null);
        }

        await WriteAuditAsync(scope.UserId, "JobStatusChanged", "Job", job.Id, $"{job.Title}: {status}");
        await _db.SaveChangesAsync();

        var loaded = await LoadJobGraphAsync(job.Id);
        return (true, null, MapJobDetail(loaded!));
    }

    public async Task<(bool Ok, string? Error, PagedResultDto<RecruiterApplicantListItemDto>? Result)> ListApplicantsAsync(
        int jobId,
        RecruiterPipelineQuery query)
    {
        if (!await _authz.RecruiterOwnsJobAsync(jobId))
        {
            return (false, "Job not found or access denied.", null);
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : Math.Min(query.PageSize, 50);

        var appsQuery = _db.Applications
            .AsNoTracking()
            .Include(a => a.Candidate)
            .Include(a => a.Interviews)
            .Where(a => a.JobId == jobId);

        if (query.Status is ApplicationStatus status)
        {
            appsQuery = appsQuery.Where(a => a.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            appsQuery = appsQuery.Where(a =>
                a.Candidate.FullName.Contains(keyword)
                || a.CoverLetter.Contains(keyword));
        }

        if (query.InterviewStatus is InterviewStatus interviewStatus)
        {
            appsQuery = appsQuery.Where(a => a.Interviews.Any(i => i.Status == interviewStatus));
        }

        if (query.AssessmentStatus is AssessmentStatus assessmentStatus)
        {
            appsQuery = appsQuery.Where(a =>
                _db.AssessmentAssignments.Any(aa =>
                    aa.ApplicationId == a.Id && aa.Status == assessmentStatus));
        }

        var sortBy = (query.SortBy ?? "applied").Trim().ToLowerInvariant();
        var sortDesc = !string.Equals(query.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
        appsQuery = sortBy switch
        {
            "name" => sortDesc
                ? appsQuery.OrderByDescending(a => a.Candidate.FullName)
                : appsQuery.OrderBy(a => a.Candidate.FullName),
            "status" => sortDesc
                ? appsQuery.OrderByDescending(a => a.Status)
                : appsQuery.OrderBy(a => a.Status),
            _ => sortDesc
                ? appsQuery.OrderByDescending(a => a.AppliedDate)
                : appsQuery.OrderBy(a => a.AppliedDate)
        };

        var totalCount = await appsQuery.CountAsync();
        var applications = await appsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var candidateIds = applications.Select(a => a.CandidateId).Distinct().ToList();
        var profiles = await _db.CandidateProfiles
            .AsNoTracking()
            .Include(p => p.CandidateSkills).ThenInclude(cs => cs.Skill)
            .Include(p => p.Educations)
            .Where(p => candidateIds.Contains(p.UserId))
            .ToListAsync();

        var profileByUser = profiles.ToDictionary(p => p.UserId);
        var job = await LoadJobGraphAsync(jobId);

        var items = new List<RecruiterApplicantListItemDto>();
        foreach (var app in applications)
        {
            profileByUser.TryGetValue(app.CandidateId, out var profile);
            if (query.MinExperienceYears is int minExp
                && (profile?.YearsOfExperience ?? 0) < minExp)
            {
                continue;
            }

            if (query.MaxExperienceYears is int maxExp
                && (profile?.YearsOfExperience ?? 0) > maxExp)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(query.Skill))
            {
                var skill = query.Skill.Trim();
                var hasSkill = profile?.CandidateSkills.Any(cs =>
                    cs.Skill != null
                    && cs.Skill.Name.Contains(skill, StringComparison.OrdinalIgnoreCase)) == true;
                if (!hasSkill)
                {
                    continue;
                }
            }

            decimal? matchScore = null;
            if (profile != null && job != null)
            {
                matchScore = _matching.ComputeMatch(profile, job).MatchScore;
            }

            var latestAssessment = await _db.AssessmentAssignments
                .AsNoTracking()
                .Where(aa => aa.ApplicationId == app.Id)
                .OrderByDescending(aa => aa.AssignedAtUtc)
                .Select(aa => aa.Status.ToString())
                .FirstOrDefaultAsync();

            var latestInterview = app.Interviews
                .OrderByDescending(i => i.InterviewDate)
                .Select(i => i.Status.ToString())
                .FirstOrDefault();

            items.Add(new RecruiterApplicantListItemDto
            {
                ApplicationId = app.Id,
                CandidateUserId = app.CandidateId,
                CandidateName = profile?.FullName ?? app.Candidate.FullName,
                AppliedAtUtc = app.AppliedDate,
                Status = app.Status,
                MatchScore = matchScore,
                MainSkills = profile?.CandidateSkills
                    .Where(cs => cs.Skill != null)
                    .Select(cs => cs.Skill.Name)
                    .Take(8)
                    .ToList() ?? new List<string>(),
                YearsOfExperience = profile?.YearsOfExperience,
                EducationSummary = profile?.Educations
                    .OrderByDescending(e => e.EndDate ?? e.StartDate)
                    .Select(e => $"{e.Degree} — {e.Institution}")
                    .FirstOrDefault(),
                AssessmentStatus = latestAssessment,
                InterviewStatus = latestInterview,
                HasUnreadCommunication = false
            });
        }

        return (true, null, new PagedResultDto<RecruiterApplicantListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<(bool Ok, string? Error, RecruiterApplicationDetailDto? Result)> GetApplicationAsync(
        int applicationId)
    {
        if (!await _authz.RecruiterCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.", null);
        }

        var application = await _db.Applications
            .AsNoTracking()
            .Include(a => a.Candidate)
            .Include(a => a.Job).ThenInclude(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Include(a => a.Answers).ThenInclude(ans => ans.ScreeningQuestion)
            .Include(a => a.StatusHistory)
            .Include(a => a.InternalNotes).ThenInclude(n => n.Author)
            .Include(a => a.Resume)
            .Include(a => a.Interviews)
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

        var match = profile == null ? null : _matching.ComputeMatch(profile, application.Job);
        var assessmentStatus = await _db.AssessmentAssignments
            .AsNoTracking()
            .Where(aa => aa.ApplicationId == application.Id)
            .OrderByDescending(aa => aa.AssignedAtUtc)
            .Select(aa => aa.Status.ToString())
            .FirstOrDefaultAsync();

        return (true, null, new RecruiterApplicationDetailDto
        {
            ApplicationId = application.Id,
            JobId = application.JobId,
            JobTitle = application.Job.Title,
            CandidateUserId = application.CandidateId,
            CandidateName = profile?.FullName ?? application.Candidate.FullName,
            ProfessionalSummary = profile?.Summary,
            Location = profile?.Location,
            YearsOfExperience = profile?.YearsOfExperience,
            CoverLetter = application.CoverLetter,
            Status = application.Status,
            AppliedAtUtc = application.AppliedDate,
            MatchScore = match?.MatchScore,
            ResumeId = application.ResumeId,
            ResumeFileName = application.Resume?.FileName,
            Skills = profile?.CandidateSkills
                .Where(cs => cs.Skill != null)
                .Select(cs => cs.Skill.Name)
                .ToList() ?? new List<string>(),
            MissingRequiredSkills = match?.MissingSkills ?? new List<string>(),
            Education = profile?.Educations.Select(e => new RecruiterEducationSummaryDto
            {
                Institution = e.Institution,
                Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy
            }).ToList() ?? new List<RecruiterEducationSummaryDto>(),
            Experience = profile?.WorkExperiences.Select(w => new RecruiterExperienceSummaryDto
            {
                CompanyName = w.CompanyName,
                JobTitle = w.JobTitle,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                IsCurrentRole = w.IsCurrentRole
            }).ToList() ?? new List<RecruiterExperienceSummaryDto>(),
            ScreeningAnswers = application.Answers.Select(a => new RecruiterScreeningAnswerDto
            {
                QuestionId = a.ScreeningQuestionId ?? 0,
                QuestionText = a.ScreeningQuestion?.QuestionText ?? a.QuestionText,
                IsRequired = a.ScreeningQuestion?.IsRequired ?? false,
                AnswerText = a.AnswerText
            }).ToList(),
            StatusHistory = application.StatusHistory
                .OrderBy(h => h.ChangedAtUtc)
                .Select(h => new RecruiterStatusHistoryDto
                {
                    Status = h.Status,
                    ChangedAtUtc = h.ChangedAtUtc,
                    ChangedByUserId = h.ChangedByUserId,
                    Notes = h.Notes
                }).ToList(),
            InternalNotes = application.InternalNotes
                .OrderByDescending(n => n.CreatedAtUtc)
                .Select(n => new RecruiterNoteDto
                {
                    Id = n.Id,
                    Content = n.Content,
                    AuthorUserId = n.AuthorUserId,
                    AuthorName = n.Author.FullName,
                    CreatedAtUtc = n.CreatedAtUtc,
                    UpdatedAtUtc = n.UpdatedAtUtc
                }).ToList(),
            AssessmentStatus = assessmentStatus,
            InterviewStatus = application.Interviews
                .OrderByDescending(i => i.InterviewDate)
                .Select(i => i.Status.ToString())
                .FirstOrDefault()
        });
    }

    public async Task<(bool Ok, string? Error)> ChangeApplicationStatusAsync(
        int applicationId,
        ApplicationStatus status,
        string? notes)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.");
        }

        if (!await _authz.RecruiterCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.");
        }

        return await _applicationTransitions.TransitionAsync(applicationId, status, userId, notes);
    }

    public async Task<(bool Ok, string? Error, RecruiterNoteDto? Result)> AddNoteAsync(
        int applicationId,
        string content)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        if (!await _authz.RecruiterCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.", null);
        }

        var sanitized = SanitizeNote(content);
        if (sanitized is null)
        {
            return (false, "Note content is required.", null);
        }

        var note = new ApplicationNote
        {
            ApplicationId = applicationId,
            AuthorUserId = userId,
            Content = sanitized,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.ApplicationNotes.Add(note);
        await WriteAuditAsync(userId, "ApplicationNoteCreated", "ApplicationNote", null, $"Application {applicationId}");
        await _db.SaveChangesAsync();

        var author = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
        return (true, null, new RecruiterNoteDto
        {
            Id = note.Id,
            Content = note.Content,
            AuthorUserId = userId,
            AuthorName = author.FullName,
            CreatedAtUtc = note.CreatedAtUtc
        });
    }

    public async Task<(bool Ok, string? Error)> DeleteNoteAsync(int noteId)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.");
        }

        var note = await _db.ApplicationNotes.FirstOrDefaultAsync(n => n.Id == noteId);
        if (note is null)
        {
            return (false, "Note not found.");
        }

        if (!await _authz.RecruiterCanAccessApplicationAsync(note.ApplicationId))
        {
            return (false, "Note not found or access denied.");
        }

        _db.ApplicationNotes.Remove(note);
        await WriteAuditAsync(userId, "ApplicationNoteDeleted", "ApplicationNote", noteId, null);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error, CandidateComparisonDto? Result)> CompareApplicantsAsync(
        IReadOnlyList<int> applicationIds)
    {
        if (applicationIds.Count is < 2 or > MaxComparisonCount)
        {
            return (false, $"Select between 2 and {MaxComparisonCount} applicants to compare.", null);
        }

        var distinctIds = applicationIds.Distinct().ToList();
        var items = new List<RecruiterComparisonItemDto>();

        foreach (var id in distinctIds)
        {
            if (!await _authz.RecruiterCanAccessApplicationAsync(id))
            {
                return (false, "One or more applications are not accessible.", null);
            }

            var (ok, error, detail) = await GetApplicationAsync(id);
            if (!ok || detail is null)
            {
                return (false, error ?? "Application not found or access denied.", null);
            }

            items.Add(new RecruiterComparisonItemDto
            {
                ApplicationId = detail.ApplicationId,
                CandidateName = detail.CandidateName,
                ProfessionalSummary = detail.ProfessionalSummary,
                Skills = detail.Skills,
                MissingRequiredSkills = detail.MissingRequiredSkills,
                YearsOfExperience = detail.YearsOfExperience,
                EducationSummary = detail.Education.FirstOrDefault() is { } edu
                    ? $"{edu.Degree} — {edu.Institution}"
                    : null,
                MatchScore = detail.MatchScore,
                AssessmentStatus = detail.AssessmentStatus,
                InterviewStatus = detail.InterviewStatus,
                ApplicationStatus = detail.Status
            });
        }

        return (true, null, new CandidateComparisonDto { Items = items });
    }

    private (bool Ok, string? Error, OrgScope? Scope) RequireOrgScope()
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        if (_currentUser.IsInRole("Admin") && _currentUser.OrganizationId is null)
        {
            // Admin without org can still operate when accessing owned resources via authz helpers;
            // for list/create they need an org context from JWT when acting as recruiter tooling.
            return (false, "Organization context is required.", null);
        }

        if (_currentUser.OrganizationId is not int orgId)
        {
            return (false, "Organization context is required.", null);
        }

        return (true, null, new OrgScope(userId, orgId));
    }

    private IQueryable<Job> OrgJobsQuery(OrgScope scope) =>
        _db.Jobs.Where(j => j.OrganizationId == scope.OrganizationId
            || j.RecruiterId == scope.UserId);

    private async Task<Job?> LoadJobGraphAsync(int jobId) =>
        await _db.Jobs
            .AsNoTracking()
            .Include(j => j.Department)
            .Include(j => j.HiringManager)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Include(j => j.ScreeningQuestions)
            .FirstOrDefaultAsync(j => j.Id == jobId);

    private static RecruiterJobDetailDto MapJobDetail(Job job) => new()
    {
        Id = job.Id,
        Title = job.Title,
        Description = job.Description,
        Responsibilities = job.Responsibilities,
        RequiredSkillsText = job.RequiredSkills,
        Location = job.Location,
        JobType = job.JobType,
        Status = job.Status,
        EmploymentType = job.EmploymentType,
        WorkArrangement = job.WorkArrangement,
        SalaryMin = job.SalaryMin,
        SalaryMax = job.SalaryMax,
        SalaryCurrency = job.SalaryCurrency,
        SalaryVisible = job.SalaryVisible,
        MinimumExperienceYears = job.MinimumExperienceYears,
        EducationRequirement = job.EducationRequirement,
        Vacancies = job.Vacancies,
        ApplicationDeadlineUtc = job.ApplicationDeadlineUtc,
        RecruiterId = job.RecruiterId,
        OrganizationId = job.OrganizationId,
        DepartmentId = job.DepartmentId,
        DepartmentName = job.Department?.Name,
        HiringManagerUserId = job.HiringManagerUserId,
        HiringManagerName = job.HiringManager?.FullName,
        PostedDate = job.PostedDate,
        PublishedAtUtc = job.PublishedAtUtc,
        ClosedAtUtc = job.ClosedAtUtc,
        CreatedAtUtc = job.CreatedAtUtc,
        UpdatedAtUtc = job.UpdatedAtUtc,
        Skills = job.JobSkills.Select(js => new RecruiterJobSkillDto
        {
            Id = js.Id,
            SkillId = js.SkillId,
            SkillName = js.Skill?.Name ?? string.Empty,
            IsRequired = js.IsRequired,
            MinProficiencyLevel = js.MinProficiencyLevel
        }).ToList(),
        ScreeningQuestions = job.ScreeningQuestions
            .OrderBy(q => q.SortOrder)
            .Select(q => new RecruiterScreeningQuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                IsRequired = q.IsRequired,
                SortOrder = q.SortOrder
            }).ToList()
    };

    private async Task ReplaceSkillsAndQuestionsAsync(Job job, UpsertRecruiterJobDto dto)
    {
        if (job.JobSkills.Count > 0)
        {
            _db.JobSkills.RemoveRange(job.JobSkills);
        }

        foreach (var skillDto in dto.Skills)
        {
            var skill = await ResolveSkillAsync(skillDto.SkillId, skillDto.SkillName);
            if (skill is null)
            {
                continue;
            }

            _db.JobSkills.Add(new JobSkill
            {
                JobId = job.Id,
                SkillId = skill.Id,
                IsRequired = skillDto.IsRequired,
                MinProficiencyLevel = NullIfWhiteSpace(skillDto.MinProficiencyLevel)
            });
        }

        var keepQuestionIds = dto.ScreeningQuestions
            .Where(q => q.Id is int)
            .Select(q => q.Id!.Value)
            .ToHashSet();

        var removeQuestions = job.ScreeningQuestions.Where(q => !keepQuestionIds.Contains(q.Id)).ToList();
        if (removeQuestions.Count > 0)
        {
            _db.ScreeningQuestions.RemoveRange(removeQuestions);
        }

        foreach (var questionDto in dto.ScreeningQuestions.OrderBy(q => q.SortOrder))
        {
            if (string.IsNullOrWhiteSpace(questionDto.QuestionText))
            {
                continue;
            }

            if (questionDto.Id is int existingId)
            {
                var existing = job.ScreeningQuestions.FirstOrDefault(q => q.Id == existingId);
                if (existing != null)
                {
                    existing.QuestionText = questionDto.QuestionText.Trim();
                    existing.QuestionType = string.IsNullOrWhiteSpace(questionDto.QuestionType)
                        ? "Text"
                        : questionDto.QuestionType.Trim();
                    existing.IsRequired = questionDto.IsRequired;
                    existing.SortOrder = questionDto.SortOrder;
                    continue;
                }
            }

            _db.ScreeningQuestions.Add(new ScreeningQuestion
            {
                JobId = job.Id,
                QuestionText = questionDto.QuestionText.Trim(),
                QuestionType = string.IsNullOrWhiteSpace(questionDto.QuestionType)
                    ? "Text"
                    : questionDto.QuestionType.Trim(),
                IsRequired = questionDto.IsRequired,
                SortOrder = questionDto.SortOrder,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
    }

    private async Task<Skill?> ResolveSkillAsync(int? skillId, string? skillName)
    {
        if (skillId is int id)
        {
            return await _db.Skills.FirstOrDefaultAsync(s => s.Id == id);
        }

        if (string.IsNullOrWhiteSpace(skillName))
        {
            return null;
        }

        var name = skillName.Trim();
        var existing = await _db.Skills.FirstOrDefaultAsync(s => s.Name == name);
        if (existing != null)
        {
            return existing;
        }

        var skill = new Skill { Name = name, CreatedAtUtc = DateTime.UtcNow };
        _db.Skills.Add(skill);
        await _db.SaveChangesAsync();
        return skill;
    }

    private async Task<bool> HiringManagerInOrgAsync(int hiringManagerUserId, int organizationId)
    {
        return await _db.HiringManagerProfiles.AnyAsync(p =>
            p.UserId == hiringManagerUserId && p.OrganizationId == organizationId);
    }

    private async Task WriteAuditAsync(int userId, string action, string entityType, int? entityId, string? details)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            CreatedAtUtc = DateTime.UtcNow
        });
        await Task.CompletedTask;
    }

    private static string? ValidateJobDto(UpsertRecruiterJobDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return "Title is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return "Description is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.Location))
        {
            return "Location is required.";
        }

        if (dto.Vacancies < 1)
        {
            return "Vacancies must be greater than zero.";
        }

        if (dto.SalaryMin is decimal min && dto.SalaryMax is decimal max && min > max)
        {
            return "Minimum salary cannot exceed maximum salary.";
        }

        if (dto.ApplicationDeadlineUtc is DateTime deadline && deadline <= DateTime.UtcNow.AddMinutes(-1))
        {
            return "Application deadline must be in the future.";
        }

        return null;
    }

    private static string? SanitizeNote(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var trimmed = content.Trim();
        if (trimmed.Length > 4000)
        {
            trimmed = trimmed[..4000];
        }

        return trimmed
            .Replace("<", string.Empty, StringComparison.Ordinal)
            .Replace(">", string.Empty, StringComparison.Ordinal);
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record OrgScope(int UserId, int OrganizationId);
}
