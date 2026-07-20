using HireSphere.API.Data;
using HireSphere.API.DTOs.Candidate;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface ICandidateJobService
{
    Task<(bool Ok, string? Error, PagedResultDto<CandidateJobListItemDto>? Result)> SearchJobsAsync(
        CandidateJobSearchQuery query);

    Task<(bool Ok, string? Error, CandidateJobDetailDto? Result)> GetJobAsync(int jobId);

    Task<(bool Ok, string? Error, JobMatchResultDto? Result)> GetJobMatchAsync(int jobId);

    Task<(bool Ok, string? Error, RecommendationsResultDto? Result)> GetRecommendationsAsync(int? take = null);
}

public sealed class CandidateJobService : ICandidateJobService
{
    public const int MinimumProfileCompletionForRecommendations = 40;

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IJobMatchingProvider _matchingProvider;

    public CandidateJobService(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        IJobMatchingProvider matchingProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _matchingProvider = matchingProvider;
    }

    public async Task<(bool Ok, string? Error, PagedResultDto<CandidateJobListItemDto>? Result)> SearchJobsAsync(
        CandidateJobSearchQuery query)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : Math.Min(query.PageSize, 50);

        var jobsQuery = OpenJobsQuery();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            jobsQuery = jobsQuery.Where(j =>
                j.Title.Contains(keyword)
                || j.Description.Contains(keyword)
                || j.RequiredSkills.Contains(keyword)
                || j.Location.Contains(keyword)
                || j.JobSkills.Any(js => js.Skill.Name.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var location = query.Location.Trim();
            jobsQuery = jobsQuery.Where(j => j.Location.Contains(location));
        }

        if (query.DepartmentId is int departmentId)
        {
            jobsQuery = jobsQuery.Where(j => j.DepartmentId == departmentId);
        }

        if (query.EmploymentType is EmploymentType employmentType)
        {
            jobsQuery = jobsQuery.Where(j => j.EmploymentType == employmentType);
        }

        if (query.WorkArrangement is WorkArrangement workArrangement)
        {
            jobsQuery = jobsQuery.Where(j => j.WorkArrangement == workArrangement);
        }

        if (query.SkillId is int skillId)
        {
            jobsQuery = jobsQuery.Where(j => j.JobSkills.Any(js => js.SkillId == skillId));
        }

        var sortBy = (query.SortBy ?? "postedDate").Trim().ToLowerInvariant();
        var sortDesc = string.Equals(query.SortDir, "asc", StringComparison.OrdinalIgnoreCase) == false;

        jobsQuery = sortBy switch
        {
            "title" => sortDesc
                ? jobsQuery.OrderByDescending(j => j.Title)
                : jobsQuery.OrderBy(j => j.Title),
            "location" => sortDesc
                ? jobsQuery.OrderByDescending(j => j.Location)
                : jobsQuery.OrderBy(j => j.Location),
            _ => sortDesc
                ? jobsQuery.OrderByDescending(j => j.PostedDate)
                : jobsQuery.OrderBy(j => j.PostedDate)
        };

        var totalCount = await jobsQuery.CountAsync();
        var jobs = await jobsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var profileForMatch = await LoadProfileForMatchingAsync(profile.Id);
        var items = jobs.Select(j => MapListItem(j, profileForMatch)).ToList();

        if (string.Equals(sortBy, "matchscore", StringComparison.OrdinalIgnoreCase))
        {
            items = sortDesc
                ? items.OrderByDescending(i => i.MatchScore ?? 0).ToList()
                : items.OrderBy(i => i.MatchScore ?? 0).ToList();
        }

        return (true, null, new PagedResultDto<CandidateJobListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    public async Task<(bool Ok, string? Error, CandidateJobDetailDto? Result)> GetJobAsync(int jobId)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var job = await OpenJobsQuery()
            .Include(j => j.ScreeningQuestions)
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job == null)
        {
            return (false, "Job not found.", null);
        }

        var profileForMatch = await LoadProfileForMatchingAsync(profile.Id);
        var match = _matchingProvider.ComputeMatch(profileForMatch, job);
        await PersistMatchAsync(profile.Id, job.Id, match);

        var alreadyApplied = await _db.Applications
            .AnyAsync(a => a.CandidateId == profile.UserId && a.JobId == jobId);

        return (true, null, MapDetail(job, match, alreadyApplied));
    }

    public async Task<(bool Ok, string? Error, JobMatchResultDto? Result)> GetJobMatchAsync(int jobId)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var job = await OpenJobsQuery().FirstOrDefaultAsync(j => j.Id == jobId);
        if (job == null)
        {
            return (false, "Job not found.", null);
        }

        var profileForMatch = await LoadProfileForMatchingAsync(profile.Id);
        var match = _matchingProvider.ComputeMatch(profileForMatch, job);
        await PersistMatchAsync(profile.Id, job.Id, match);
        return (true, null, match);
    }

    public async Task<(bool Ok, string? Error, RecommendationsResultDto? Result)> GetRecommendationsAsync(
        int? take = null)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var completion = await ComputeProfileCompletionAsync(profile.Id);
        var skillCount = await _db.CandidateSkills.CountAsync(cs => cs.CandidateProfileId == profile.Id);
        var profileReady = completion >= MinimumProfileCompletionForRecommendations && skillCount > 0;

        if (!profileReady)
        {
            return (true, null, new RecommendationsResultDto
            {
                ProfileCompleteEnough = false,
                ProfileCompletionPercent = completion,
                Message = skillCount == 0
                    ? "Add at least one skill and complete more of your profile to receive job recommendations."
                    : $"Complete at least {MinimumProfileCompletionForRecommendations}% of your profile to receive job recommendations. Current completion: {completion}%.",
                Jobs = Array.Empty<CandidateJobListItemDto>()
            });
        }

        var limit = take is null or < 1 ? 20 : Math.Min(take.Value, 50);
        var jobs = await OpenJobsQuery().ToListAsync();
        var profileForMatch = await LoadProfileForMatchingAsync(profile.Id);

        var scored = new List<(CandidateJobListItemDto Item, decimal Score)>();
        foreach (var job in jobs)
        {
            var match = _matchingProvider.ComputeMatch(profileForMatch, job);
            await PersistMatchAsync(profile.Id, job.Id, match);
            var item = MapListItem(job, profileForMatch, match.MatchScore);
            scored.Add((item, match.MatchScore));
        }

        var ranked = scored
            .OrderByDescending(s => s.Score)
            .ThenByDescending(s => s.Item.PostedDate)
            .Take(limit)
            .Select(s => s.Item)
            .ToList();

        return (true, null, new RecommendationsResultDto
        {
            ProfileCompleteEnough = true,
            ProfileCompletionPercent = completion,
            Message = ranked.Count == 0
                ? "No open jobs are available to recommend right now."
                : null,
            Jobs = ranked
        });
    }

    private IQueryable<Job> OpenJobsQuery()
    {
        var now = DateTime.UtcNow;
        return _db.Jobs
            .AsNoTracking()
            .Include(j => j.Department)
            .Include(j => j.Organization)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Where(j => (j.Status == JobStatus.Open || j.Status == JobStatus.Published)
                && (j.ApplicationDeadlineUtc == null || j.ApplicationDeadlineUtc > now));
    }

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

    private async Task<CandidateProfile> LoadProfileForMatchingAsync(int profileId)
    {
        return await _db.CandidateProfiles
            .AsNoTracking()
            .Include(p => p.CandidateSkills).ThenInclude(cs => cs.Skill)
            .Include(p => p.WorkExperiences)
            .Include(p => p.Educations)
            .FirstAsync(p => p.Id == profileId);
    }

    private async Task PersistMatchAsync(int profileId, int jobId, JobMatchResultDto match)
    {
        var existing = await _db.CandidateJobMatches
            .FirstOrDefaultAsync(m => m.CandidateProfileId == profileId && m.JobId == jobId);

        if (existing == null)
        {
            _db.CandidateJobMatches.Add(new CandidateJobMatch
            {
                CandidateProfileId = profileId,
                JobId = jobId,
                MatchScore = match.MatchScore,
                MatchSummary = match.Explanation,
                CreatedAtUtc = match.ComputedAtUtc
            });
        }
        else
        {
            existing.MatchScore = match.MatchScore;
            existing.MatchSummary = match.Explanation;
            existing.CreatedAtUtc = match.ComputedAtUtc;
        }

        await _db.SaveChangesAsync();
    }

    private CandidateJobListItemDto MapListItem(
        Job job,
        CandidateProfile profile,
        decimal? precomputedScore = null)
    {
        var score = precomputedScore ?? _matchingProvider.ComputeMatch(profile, job).MatchScore;
        return new CandidateJobListItemDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = Truncate(job.Description, 280),
            Location = job.Location,
            DepartmentName = job.Department?.Name,
            DepartmentId = job.DepartmentId,
            OrganizationName = job.Organization?.Name,
            EmploymentType = job.EmploymentType,
            WorkArrangement = job.WorkArrangement,
            PostedDate = job.PostedDate,
            RequiredSkillNames = job.JobSkills?
                .Where(js => js.IsRequired && js.Skill != null)
                .Select(js => js.Skill.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .ToList()
                ?? ParseRequiredSkills(job.RequiredSkills),
            MatchScore = score
        };
    }

    private static CandidateJobDetailDto MapDetail(Job job, JobMatchResultDto match, bool alreadyApplied)
    {
        return new CandidateJobDetailDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            JobType = job.JobType,
            DepartmentName = job.Department?.Name,
            DepartmentId = job.DepartmentId,
            OrganizationName = job.Organization?.Name,
            EmploymentType = job.EmploymentType,
            WorkArrangement = job.WorkArrangement,
            PostedDate = job.PostedDate,
            Status = job.Status,
            Skills = job.JobSkills?
                .Where(js => js.Skill != null)
                .Select(js => new CandidateJobSkillDto
                {
                    SkillId = js.SkillId,
                    SkillName = js.Skill.Name,
                    IsRequired = js.IsRequired,
                    MinProficiencyLevel = js.MinProficiencyLevel
                })
                .ToList()
                ?? new List<CandidateJobSkillDto>(),
            ScreeningQuestions = job.ScreeningQuestions?
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
                ?? new List<ScreeningQuestionDto>(),
            Match = match,
            AlreadyApplied = alreadyApplied
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

    private static IReadOnlyList<string> ParseRequiredSkills(string? requiredSkills)
    {
        if (string.IsNullOrWhiteSpace(requiredSkills))
        {
            return Array.Empty<string>();
        }

        return requiredSkills
            .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s)
            .ToList();
    }

    private static string Truncate(string value, int max)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= max)
        {
            return value;
        }

        return value[..(max - 1)] + "…";
    }
}
