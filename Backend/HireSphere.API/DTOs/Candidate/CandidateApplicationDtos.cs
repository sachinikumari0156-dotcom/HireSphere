using HireSphere.API.Models.Enums;

namespace HireSphere.API.DTOs.Candidate;

public sealed class ApplicationWizardOptionsDto
{
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public bool CanApply { get; set; }
    public string? BlockReason { get; set; }
    public IReadOnlyList<ResumeMetadataDto> Resumes { get; set; } = Array.Empty<ResumeMetadataDto>();
    public IReadOnlyList<ScreeningQuestionDto> ScreeningQuestions { get; set; } = Array.Empty<ScreeningQuestionDto>();
}

public sealed class SubmitApplicationDto
{
    public int JobId { get; set; }
    public int? ResumeId { get; set; }
    public string? CoverLetter { get; set; }
    public bool TermsAccepted { get; set; }
    public IReadOnlyList<ScreeningAnswerInputDto> ScreeningAnswers { get; set; } = Array.Empty<ScreeningAnswerInputDto>();
}

public sealed class ScreeningAnswerInputDto
{
    public int ScreeningQuestionId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
}

public sealed class CandidateApplicationListItemDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string JobLocation { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedDate { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public bool CanWithdraw { get; set; }
}

public sealed class CandidateApplicationDetailDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string JobLocation { get; set; } = string.Empty;
    public JobStatus JobStatus { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedDate { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public string CoverLetter { get; set; } = string.Empty;
    public int? ResumeId { get; set; }
    public string? ResumeFileName { get; set; }
    public bool CanWithdraw { get; set; }
    public IReadOnlyList<ApplicationAnswerDto> Answers { get; set; } = Array.Empty<ApplicationAnswerDto>();
    public IReadOnlyList<ApplicationStatusHistoryDto> StatusHistory { get; set; } = Array.Empty<ApplicationStatusHistoryDto>();
}

public sealed class ApplicationAnswerDto
{
    public int Id { get; set; }
    public int? ScreeningQuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
}

public sealed class ApplicationStatusHistoryDto
{
    public ApplicationStatus Status { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public string? Notes { get; set; }
}
