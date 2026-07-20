using HireSphere.API.Models.Enums;

namespace HireSphere.API.DTOs.HiringManager;

public sealed class HiringManagerDashboardDto
{
    public int AssignedActiveVacancies { get; set; }
    public int AssignedPausedVacancies { get; set; }
    public int CandidatesAwaitingReview { get; set; }
    public int CandidatesShortlisted { get; set; }
    public int UpcomingInterviews { get; set; }
    public int PendingInterviewFeedback { get; set; }
    public int PendingEvaluations { get; set; }
    public int PendingHiringDecisions { get; set; }
    public IReadOnlyList<HiringManagerActivityItemDto> RecentActivity { get; set; } =
        Array.Empty<HiringManagerActivityItemDto>();
}

public sealed class HiringManagerActivityItemDto
{
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class HiringManagerJobListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public JobStatus? Status { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
}

public sealed class HiringManagerJobListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public JobStatus Status { get; set; }
    public string? DepartmentName { get; set; }
    public string? RecruiterName { get; set; }
    public int ApplicantCount { get; set; }
    public int ShortlistCount { get; set; }
    public int InterviewCount { get; set; }
    public DateTime? ApplicationDeadlineUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class HiringManagerJobDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Responsibilities { get; set; }
    public string Location { get; set; } = string.Empty;
    public JobStatus Status { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public WorkArrangement WorkArrangement { get; set; }
    public int? MinimumExperienceYears { get; set; }
    public string? EducationRequirement { get; set; }
    public DateTime? ApplicationDeadlineUtc { get; set; }
    public int RecruiterId { get; set; }
    public string? RecruiterName { get; set; }
    public int? OrganizationId { get; set; }
    public int ApplicantCount { get; set; }
    public int ShortlistCount { get; set; }
    public int InterviewCount { get; set; }
    public IReadOnlyList<HiringManagerSkillDto> Skills { get; set; } = Array.Empty<HiringManagerSkillDto>();
    public IReadOnlyList<HiringManagerReviewCommentDto> ReviewComments { get; set; } =
        Array.Empty<HiringManagerReviewCommentDto>();
}

public sealed class HiringManagerSkillDto
{
    public string SkillName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
}

public sealed class HiringManagerReviewCommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int AuthorUserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class CreateJobReviewCommentDto
{
    public string Content { get; set; } = string.Empty;
}

public sealed class HiringManagerCandidateListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public ApplicationStatus? Status { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
}

public sealed class HiringManagerCandidateListItemDto
{
    public int ApplicationId { get; set; }
    public int CandidateUserId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public decimal? MatchScore { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? EducationSummary { get; set; }
    public IReadOnlyList<string> Skills { get; set; } = Array.Empty<string>();
    public DateTime AppliedAtUtc { get; set; }
    public string? AssessmentStatus { get; set; }
    public string? InterviewStatus { get; set; }
}

public sealed class HiringManagerApplicationDetailDto
{
    public int ApplicationId { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public int CandidateUserId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string? ProfessionalSummary { get; set; }
    public ApplicationStatus Status { get; set; }
    public decimal? MatchScore { get; set; }
    public string? MatchExplanation { get; set; }
    public string HumanReviewNotice { get; set; } =
        "AI-generated insight. Final recruitment decisions must be reviewed by authorized users.";
    public IReadOnlyList<string> Skills { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingRequiredSkills { get; set; } = Array.Empty<string>();
    public int? YearsOfExperience { get; set; }
    public IReadOnlyList<HiringManagerEducationDto> Education { get; set; } =
        Array.Empty<HiringManagerEducationDto>();
    public IReadOnlyList<HiringManagerExperienceDto> Experience { get; set; } =
        Array.Empty<HiringManagerExperienceDto>();
    public IReadOnlyList<HiringManagerCertificationDto> Certifications { get; set; } =
        Array.Empty<HiringManagerCertificationDto>();
    public IReadOnlyList<HiringManagerResumeDto> Resumes { get; set; } =
        Array.Empty<HiringManagerResumeDto>();
    public IReadOnlyList<HiringManagerScreeningAnswerDto> ScreeningAnswers { get; set; } =
        Array.Empty<HiringManagerScreeningAnswerDto>();
    public IReadOnlyList<HiringManagerStatusHistoryDto> StatusHistory { get; set; } =
        Array.Empty<HiringManagerStatusHistoryDto>();
    public IReadOnlyList<HiringManagerInterviewSummaryDto> Interviews { get; set; } =
        Array.Empty<HiringManagerInterviewSummaryDto>();
    public IReadOnlyList<HiringManagerAssessmentSummaryDto> Assessments { get; set; } =
        Array.Empty<HiringManagerAssessmentSummaryDto>();
}

public sealed class HiringManagerEducationDto
{
    public string Institution { get; set; } = string.Empty;
    public string? Degree { get; set; }
    public string? FieldOfStudy { get; set; }
}

public sealed class HiringManagerExperienceDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrentRole { get; set; }
}

public sealed class HiringManagerCertificationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Issuer { get; set; }
}

public sealed class HiringManagerResumeDto
{
    public int DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public sealed class HiringManagerScreeningAnswerDto
{
    public string QuestionText { get; set; } = string.Empty;
    public string? AnswerText { get; set; }
}

public sealed class HiringManagerStatusHistoryDto
{
    public ApplicationStatus Status { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public string? Notes { get; set; }
}

public sealed class HiringManagerInterviewSummaryDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime InterviewDateUtc { get; set; }
    public string? TimeZoneId { get; set; }
    public string? InterviewType { get; set; }
}

public sealed class HiringManagerAssessmentSummaryDto
{
    public int AssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? ScorePercent { get; set; }
    public bool? Passed { get; set; }
}

public sealed class HiringManagerCompareRequestDto
{
    public IReadOnlyList<int> ApplicationIds { get; set; } = Array.Empty<int>();
}

public sealed class HiringManagerComparisonDto
{
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string HumanReviewNotice { get; set; } =
        "AI-generated insight. Final recruitment decisions must be reviewed by authorized users.";
    public IReadOnlyList<HiringManagerApplicationDetailDto> Candidates { get; set; } =
        Array.Empty<HiringManagerApplicationDetailDto>();
}
