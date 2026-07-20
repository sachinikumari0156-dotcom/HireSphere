using HireSphere.API.Models.Enums;

namespace HireSphere.API.DTOs.Candidate;

public sealed class CandidateJobListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public int? DepartmentId { get; set; }
    public string? OrganizationName { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public WorkArrangement WorkArrangement { get; set; }
    public DateTime PostedDate { get; set; }
    public IReadOnlyList<string> RequiredSkillNames { get; set; } = Array.Empty<string>();
    public decimal? MatchScore { get; set; }
}

public sealed class CandidateJobDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public int? DepartmentId { get; set; }
    public string? OrganizationName { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public WorkArrangement WorkArrangement { get; set; }
    public DateTime PostedDate { get; set; }
    public JobStatus Status { get; set; }
    public IReadOnlyList<CandidateJobSkillDto> Skills { get; set; } = Array.Empty<CandidateJobSkillDto>();
    public IReadOnlyList<ScreeningQuestionDto> ScreeningQuestions { get; set; } = Array.Empty<ScreeningQuestionDto>();
    public JobMatchResultDto? Match { get; set; }
    public bool AlreadyApplied { get; set; }
}

public sealed class CandidateJobSkillDto
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? MinProficiencyLevel { get; set; }
}

public sealed class ScreeningQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}

public sealed class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public sealed class CandidateJobSearchQuery
{
    public string? Keyword { get; set; }
    public string? Location { get; set; }
    public int? DepartmentId { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public WorkArrangement? WorkArrangement { get; set; }
    public int? SkillId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
}

public sealed class JobMatchResultDto
{
    public int JobId { get; set; }
    public decimal MatchScore { get; set; }
    public IReadOnlyList<string> MatchedSkills { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingSkills { get; set; } = Array.Empty<string>();
    public ExperienceComparisonDto Experience { get; set; } = new();
    public EducationComparisonDto Education { get; set; } = new();
    public LocationFactorDto Location { get; set; } = new();
    public WorkArrangementFactorDto WorkArrangement { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
    public string Provider { get; set; } = "Deterministic";
    public DateTime ComputedAtUtc { get; set; }
    public string HumanReviewNotice { get; set; } =
        "This match score is produced by a deterministic rules engine and should be reviewed by a human recruiter. It is not an external AI decision.";
}

public sealed class ExperienceComparisonDto
{
    public int? CandidateYears { get; set; }
    public string Assessment { get; set; } = string.Empty;
    public decimal FactorScore { get; set; }
}

public sealed class EducationComparisonDto
{
    public int CandidateEducationCount { get; set; }
    public string Assessment { get; set; } = string.Empty;
    public decimal FactorScore { get; set; }
}

public sealed class LocationFactorDto
{
    public string? CandidateLocation { get; set; }
    public string JobLocation { get; set; } = string.Empty;
    public bool IsMatch { get; set; }
    public decimal FactorScore { get; set; }
}

public sealed class WorkArrangementFactorDto
{
    public WorkArrangement? CandidatePreference { get; set; }
    public WorkArrangement JobArrangement { get; set; }
    public bool IsMatch { get; set; }
    public decimal FactorScore { get; set; }
}

public sealed class RecommendationsResultDto
{
    public bool ProfileCompleteEnough { get; set; }
    public string? Message { get; set; }
    public int ProfileCompletionPercent { get; set; }
    public IReadOnlyList<CandidateJobListItemDto> Jobs { get; set; } = Array.Empty<CandidateJobListItemDto>();
}
