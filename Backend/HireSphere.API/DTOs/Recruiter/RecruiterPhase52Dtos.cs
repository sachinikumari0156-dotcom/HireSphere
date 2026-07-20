using HireSphere.API.Models.Enums;

namespace HireSphere.API.DTOs.Recruiter;

public sealed class RecruiterRankingDto
{
    public int ApplicationId { get; set; }
    public int JobId { get; set; }
    public decimal TotalScore { get; set; }
    public IReadOnlyList<string> MatchedRequiredSkills { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> MatchedPreferredSkills { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingRequiredSkills { get; set; } = Array.Empty<string>();
    public decimal ExperienceFactor { get; set; }
    public decimal EducationFactor { get; set; }
    public decimal? AssessmentFactor { get; set; }
    public decimal ProfileCompletenessFactor { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public string Confidence { get; set; } = "Medium";
    public string ProviderName { get; set; } = "Deterministic";
    public string ModelVersion { get; set; } = "recruiter-rank-v1";
    public DateTime GeneratedAtUtc { get; set; }
    public string HumanReviewNotice { get; set; } =
        "AI-generated insight. Final recruitment decisions must be reviewed by authorized users.";
    public RankingReviewDto? LatestHumanReview { get; set; }
}

public sealed class RankingReviewDto
{
    public int Id { get; set; }
    public string Decision { get; set; } = string.Empty;
    public decimal? OverrideScore { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int ReviewerUserId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class CreateRankingReviewDto
{
    public string Decision { get; set; } = string.Empty;
    public decimal? OverrideScore { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public sealed class ScreeningQueueItemDto
{
    public int ApplicationId { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedAtUtc { get; set; }
    public int RequiredAnswersTotal { get; set; }
    public int RequiredAnswersCompleted { get; set; }
    public decimal? MatchScore { get; set; }
}

public sealed class ScreeningDecisionDto
{
    public ApplicationStatus Status { get; set; }
    public string? Reason { get; set; }
}

public sealed class RecruiterAssessmentListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? JobId { get; set; }
    public int QuestionCount { get; set; }
    public decimal PassingScorePercent { get; set; }
    public int? DurationMinutes { get; set; }
    public int MaxAttempts { get; set; }
    public bool IsArchived { get; set; }
    public bool RevealResultsToCandidate { get; set; }
}

public sealed class RecruiterAssessmentDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? JobId { get; set; }
    public int? OrganizationId { get; set; }
    public decimal PassingScorePercent { get; set; }
    public int? DurationMinutes { get; set; }
    public int MaxAttempts { get; set; }
    public bool RevealResultsToCandidate { get; set; }
    public bool IsArchived { get; set; }
    public IReadOnlyList<RecruiterAssessmentQuestionDto> Questions { get; set; } =
        Array.Empty<RecruiterAssessmentQuestionDto>();
}

public sealed class RecruiterAssessmentQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public decimal Points { get; set; }
    public int SortOrder { get; set; }
    public string? OptionsJson { get; set; }
    /// <summary>Included for recruiter/admin only. Never sent to candidate APIs.</summary>
    public string CorrectAnswerKey { get; set; } = string.Empty;
}

public sealed class UpsertAssessmentDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? JobId { get; set; }
    public decimal PassingScorePercent { get; set; } = 60m;
    public int? DurationMinutes { get; set; }
    public int MaxAttempts { get; set; } = 1;
    public bool RevealResultsToCandidate { get; set; }
}

public sealed class UpsertAssessmentQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "MultipleChoice";
    public decimal Points { get; set; } = 1;
    public int SortOrder { get; set; }
    public string? OptionsJson { get; set; }
    public string CorrectAnswerKey { get; set; } = string.Empty;
}

public sealed class AssignAssessmentDto
{
    public int AssessmentId { get; set; }
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public int? MaxAttempts { get; set; }
    public bool? RevealResultsToCandidate { get; set; }
}

public sealed class RecruiterAssignmentDetailDto
{
    public int Id { get; set; }
    public int AssessmentId { get; set; }
    public string AssessmentTitle { get; set; } = string.Empty;
    public int? ApplicationId { get; set; }
    public int CandidateId { get; set; }
    public AssessmentStatus Status { get; set; }
    public DateTime AssignedAtUtc { get; set; }
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public int MaxAttempts { get; set; }
    public IReadOnlyList<RecruiterAttemptSummaryDto> Attempts { get; set; } =
        Array.Empty<RecruiterAttemptSummaryDto>();
}

public sealed class RecruiterAttemptSummaryDto
{
    public int AttemptId { get; set; }
    public AssessmentStatus Status { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public decimal? ScorePercent { get; set; }
    public bool? Passed { get; set; }
}

public sealed class ApplicationMessageDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public int SenderUserId { get; set; }
    public string SenderRole { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsReadByRecipient { get; set; }
    public DateTime SentAtUtc { get; set; }
}

public sealed class SendApplicationMessageDto
{
    public string Body { get; set; } = string.Empty;
}

public sealed class ApplicationMessageThreadDto
{
    public int ApplicationId { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<ApplicationMessageDto> Messages { get; set; } =
        Array.Empty<ApplicationMessageDto>();
}
