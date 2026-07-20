using HireSphere.API.Models.Enums;

namespace HireSphere.API.DTOs.Recruiter;

public sealed class RecruiterDashboardDto
{
    public int ActiveJobs { get; set; }
    public int DraftJobs { get; set; }
    public int PausedJobs { get; set; }
    public int ClosedJobs { get; set; }
    public int TotalApplicants { get; set; }
    public int NewApplicants { get; set; }
    public int CandidatesInScreening { get; set; }
    public int ShortlistedCandidates { get; set; }
    public int PendingAssessments { get; set; }
    public int UpcomingInterviews { get; set; }
    public IReadOnlyList<RecruiterActivityItemDto> RecentActivity { get; set; } = Array.Empty<RecruiterActivityItemDto>();
}

public sealed class RecruiterActivityItemDto
{
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class RecruiterJobListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public JobStatus? Status { get; set; }
    public int? DepartmentId { get; set; }
    public string? Location { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public WorkArrangement? WorkArrangement { get; set; }
    public DateTime? PostedFromUtc { get; set; }
    public DateTime? PostedToUtc { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
}

public sealed class RecruiterJobListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public JobStatus Status { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public WorkArrangement WorkArrangement { get; set; }
    public string? DepartmentName { get; set; }
    public int ApplicantCount { get; set; }
    public int Vacancies { get; set; }
    public DateTime PostedDate { get; set; }
    public DateTime? ApplicationDeadlineUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class RecruiterJobDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Responsibilities { get; set; }
    public string RequiredSkillsText { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public JobStatus Status { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public WorkArrangement WorkArrangement { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? SalaryCurrency { get; set; }
    public bool SalaryVisible { get; set; }
    public int? MinimumExperienceYears { get; set; }
    public string? EducationRequirement { get; set; }
    public int Vacancies { get; set; }
    public DateTime? ApplicationDeadlineUtc { get; set; }
    public int RecruiterId { get; set; }
    public int? OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? HiringManagerUserId { get; set; }
    public string? HiringManagerName { get; set; }
    public DateTime PostedDate { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public IReadOnlyList<RecruiterJobSkillDto> Skills { get; set; } = Array.Empty<RecruiterJobSkillDto>();
    public IReadOnlyList<RecruiterScreeningQuestionDto> ScreeningQuestions { get; set; } =
        Array.Empty<RecruiterScreeningQuestionDto>();
}

public sealed class RecruiterJobSkillDto
{
    public int? Id { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? MinProficiencyLevel { get; set; }
}

public sealed class RecruiterScreeningQuestionDto
{
    public int? Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}

public sealed class UpsertRecruiterJobDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Responsibilities { get; set; }
    public string? RequiredSkillsText { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? JobType { get; set; }
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
    public WorkArrangement WorkArrangement { get; set; } = WorkArrangement.OnSite;
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? SalaryCurrency { get; set; }
    public bool SalaryVisible { get; set; }
    public int? MinimumExperienceYears { get; set; }
    public string? EducationRequirement { get; set; }
    public int Vacancies { get; set; } = 1;
    public DateTime? ApplicationDeadlineUtc { get; set; }
    public int? DepartmentId { get; set; }
    public int? HiringManagerUserId { get; set; }
    public List<UpsertJobSkillDto> Skills { get; set; } = new();
    public List<UpsertScreeningQuestionDto> ScreeningQuestions { get; set; } = new();
}

public sealed class UpsertJobSkillDto
{
    public int? SkillId { get; set; }
    public string? SkillName { get; set; }
    public bool IsRequired { get; set; } = true;
    public string? MinProficiencyLevel { get; set; }
}

public sealed class UpsertScreeningQuestionDto
{
    public int? Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}

public sealed class ChangeJobStatusDto
{
    public JobStatus Status { get; set; }
}

public sealed class RecruiterPipelineQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public ApplicationStatus? Status { get; set; }
    public string? Skill { get; set; }
    public int? MinExperienceYears { get; set; }
    public int? MaxExperienceYears { get; set; }
    public AssessmentStatus? AssessmentStatus { get; set; }
    public InterviewStatus? InterviewStatus { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
}

public sealed class RecruiterApplicantListItemDto
{
    public int ApplicationId { get; set; }
    public int CandidateUserId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public DateTime AppliedAtUtc { get; set; }
    public ApplicationStatus Status { get; set; }
    public decimal? MatchScore { get; set; }
    public IReadOnlyList<string> MainSkills { get; set; } = Array.Empty<string>();
    public int? YearsOfExperience { get; set; }
    public string? EducationSummary { get; set; }
    public string? AssessmentStatus { get; set; }
    public string? InterviewStatus { get; set; }
    public bool HasUnreadCommunication { get; set; }
}

public sealed class RecruiterApplicationDetailDto
{
    public int ApplicationId { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public int CandidateUserId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string? ProfessionalSummary { get; set; }
    public string? Location { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? CoverLetter { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedAtUtc { get; set; }
    public decimal? MatchScore { get; set; }
    public int? ResumeId { get; set; }
    public string? ResumeFileName { get; set; }
    public IReadOnlyList<string> Skills { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingRequiredSkills { get; set; } = Array.Empty<string>();
    public IReadOnlyList<RecruiterEducationSummaryDto> Education { get; set; } =
        Array.Empty<RecruiterEducationSummaryDto>();
    public IReadOnlyList<RecruiterExperienceSummaryDto> Experience { get; set; } =
        Array.Empty<RecruiterExperienceSummaryDto>();
    public IReadOnlyList<RecruiterScreeningAnswerDto> ScreeningAnswers { get; set; } =
        Array.Empty<RecruiterScreeningAnswerDto>();
    public IReadOnlyList<RecruiterStatusHistoryDto> StatusHistory { get; set; } =
        Array.Empty<RecruiterStatusHistoryDto>();
    public IReadOnlyList<RecruiterNoteDto> InternalNotes { get; set; } = Array.Empty<RecruiterNoteDto>();
    public string? AssessmentStatus { get; set; }
    public string? InterviewStatus { get; set; }
}

public sealed class RecruiterEducationSummaryDto
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
}

public sealed class RecruiterExperienceSummaryDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrentRole { get; set; }
}

public sealed class RecruiterScreeningAnswerDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? AnswerText { get; set; }
}

public sealed class RecruiterStatusHistoryDto
{
    public ApplicationStatus Status { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public int? ChangedByUserId { get; set; }
    public string? Notes { get; set; }
}

public sealed class ChangeApplicationStatusDto
{
    public ApplicationStatus Status { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpsertApplicationNoteDto
{
    public string Content { get; set; } = string.Empty;
}

public sealed class RecruiterNoteDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int AuthorUserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class CompareApplicantsRequestDto
{
    public List<int> ApplicationIds { get; set; } = new();
}

public sealed class CandidateComparisonDto
{
    public IReadOnlyList<RecruiterComparisonItemDto> Items { get; set; } =
        Array.Empty<RecruiterComparisonItemDto>();
    public string Notice { get; set; } =
        "Comparison is for review support only and is not an automatic hiring decision.";
}

public sealed class RecruiterComparisonItemDto
{
    public int ApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string? ProfessionalSummary { get; set; }
    public IReadOnlyList<string> Skills { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingRequiredSkills { get; set; } = Array.Empty<string>();
    public int? YearsOfExperience { get; set; }
    public string? EducationSummary { get; set; }
    public decimal? MatchScore { get; set; }
    public string? AssessmentStatus { get; set; }
    public string? InterviewStatus { get; set; }
    public ApplicationStatus ApplicationStatus { get; set; }
}
