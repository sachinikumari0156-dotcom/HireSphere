using HireSphere.API.Models.Enums;

namespace HireSphere.API.DTOs.Candidate;

public sealed class CandidateAssessmentListItemDto
{
    public int AssignmentId { get; set; }
    public int SkillAssessmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public int MaxAttempts { get; set; }
    public int AttemptsUsed { get; set; }
    public int AttemptsRemaining { get; set; }
    public DateTime AssignedAtUtc { get; set; }
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public AssessmentStatus Status { get; set; }
    public bool CanStart { get; set; }
    public string? BlockReason { get; set; }
    public int? ActiveAttemptId { get; set; }
    public int? LatestAttemptId { get; set; }
    public int? ApplicationId { get; set; }
    public string? JobTitle { get; set; }
}

public sealed class CandidateAssessmentDetailDto
{
    public int AssignmentId { get; set; }
    public int SkillAssessmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public int MaxAttempts { get; set; }
    public int AttemptsUsed { get; set; }
    public int AttemptsRemaining { get; set; }
    public DateTime AssignedAtUtc { get; set; }
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public AssessmentStatus Status { get; set; }
    public bool CanStart { get; set; }
    public string? BlockReason { get; set; }
    public bool RevealResultsToCandidate { get; set; }
    public int? ActiveAttemptId { get; set; }
    public int? LatestAttemptId { get; set; }
    public int? ApplicationId { get; set; }
    public string? JobTitle { get; set; }
    public IReadOnlyList<CandidateAssessmentQuestionDto> Questions { get; set; } =
        Array.Empty<CandidateAssessmentQuestionDto>();
}

/// <summary>Candidate-safe question payload — never includes CorrectAnswerKey.</summary>
public sealed class CandidateAssessmentQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public decimal Points { get; set; }
    public int SortOrder { get; set; }
    public IReadOnlyList<string> Options { get; set; } = Array.Empty<string>();
}

public sealed class CandidateAssessmentAttemptDto
{
    public int AttemptId { get; set; }
    public int AssignmentId { get; set; }
    public int SkillAssessmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public AssessmentStatus Status { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? AttemptExpiresAtUtc { get; set; }
    public bool ResultsVisible { get; set; }
    public CandidateAssessmentResultDto? Result { get; set; }
    public IReadOnlyList<CandidateAssessmentQuestionDto> Questions { get; set; } =
        Array.Empty<CandidateAssessmentQuestionDto>();
    public IReadOnlyList<CandidateAssessmentAnswerDto> Answers { get; set; } =
        Array.Empty<CandidateAssessmentAnswerDto>();
}

public sealed class CandidateAssessmentResultDto
{
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public decimal ScorePercent { get; set; }
    public bool Passed { get; set; }
    public string? Feedback { get; set; }
}

public sealed class CandidateAssessmentAnswerDto
{
    public int QuestionId { get; set; }
    public string AnswerValue { get; set; } = string.Empty;
    public decimal? AwardedPoints { get; set; }
}

public sealed class SaveAssessmentAnswersDto
{
    public IReadOnlyList<AssessmentAnswerInputDto> Answers { get; set; } = Array.Empty<AssessmentAnswerInputDto>();
}

public sealed class AssessmentAnswerInputDto
{
    public int QuestionId { get; set; }
    public string AnswerValue { get; set; } = string.Empty;
}
