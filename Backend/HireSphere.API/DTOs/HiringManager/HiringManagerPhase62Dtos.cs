using HireSphere.API.Models.Enums;

namespace HireSphere.API.DTOs.HiringManager;

public sealed class HiringManagerInterviewListItemDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? RecruiterName { get; set; }
    public string InterviewType { get; set; } = string.Empty;
    public DateTime InterviewDateUtc { get; set; }
    public string TimeZoneId { get; set; } = "UTC";
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CandidateResponse { get; set; } = string.Empty;
}

public sealed class HiringManagerInterviewDetailDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? RecruiterName { get; set; }
    public string InterviewType { get; set; } = string.Empty;
    public DateTime InterviewDateUtc { get; set; }
    public string TimeZoneId { get; set; } = "UTC";
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CandidateResponse { get; set; } = string.Empty;
    public string? CandidateResponseReason { get; set; }
    public string? MeetingLink { get; set; }
    public string? MeetingInstructions { get; set; }
    public string? PhysicalLocation { get; set; }
    public HiringManagerInterviewFeedbackDto? MyFeedback { get; set; }
}

public sealed class HiringManagerInterviewFeedbackDto
{
    public int Id { get; set; }
    public decimal Rating { get; set; }
    public decimal? TechnicalCompetency { get; set; }
    public decimal? Communication { get; set; }
    public decimal? ProblemSolving { get; set; }
    public decimal? RoleKnowledge { get; set; }
    public decimal? Teamwork { get; set; }
    public decimal? Leadership { get; set; }
    public decimal? CulturalContribution { get; set; }
    public string? Strengths { get; set; }
    public string? Concerns { get; set; }
    public string? Recommendation { get; set; }
    public string? Comments { get; set; }
    public string? PrivatePanelComments { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
}

public sealed class UpsertInterviewFeedbackDto
{
    public decimal? TechnicalCompetency { get; set; }
    public decimal? Communication { get; set; }
    public decimal? ProblemSolving { get; set; }
    public decimal? RoleKnowledge { get; set; }
    public decimal? Teamwork { get; set; }
    public decimal? Leadership { get; set; }
    public decimal? CulturalContribution { get; set; }
    public string? Strengths { get; set; }
    public string? Concerns { get; set; }
    public string? Recommendation { get; set; }
    public string? Comments { get; set; }
    public string? PrivatePanelComments { get; set; }
}

public sealed class HiringManagerEvaluationDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string SubmissionStatus { get; set; } = "Draft";
    public decimal OverallScore { get; set; }
    public decimal? RequiredSkillsAlignment { get; set; }
    public decimal? PreferredSkillsAlignment { get; set; }
    public decimal? RelevantExperience { get; set; }
    public decimal? EducationRequirement { get; set; }
    public decimal? AssessmentPerformance { get; set; }
    public decimal? InterviewPerformance { get; set; }
    public decimal? Communication { get; set; }
    public decimal? ProblemSolving { get; set; }
    public decimal? RoleReadiness { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public string? DocumentedRisks { get; set; }
    public string? Justification { get; set; }
    public string? Recommendation { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
}

public sealed class UpsertEvaluationDto
{
    public bool Submit { get; set; }
    public decimal? RequiredSkillsAlignment { get; set; }
    public decimal? PreferredSkillsAlignment { get; set; }
    public decimal? RelevantExperience { get; set; }
    public decimal? EducationRequirement { get; set; }
    public decimal? AssessmentPerformance { get; set; }
    public decimal? InterviewPerformance { get; set; }
    public decimal? Communication { get; set; }
    public decimal? ProblemSolving { get; set; }
    public decimal? RoleReadiness { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public string? DocumentedRisks { get; set; }
    public string? Justification { get; set; }
    public string? Recommendation { get; set; }
}

public sealed class CreateRecommendationDto
{
    public HiringDecisionType DecisionType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public sealed class HiringDecisionHistoryItemDto
{
    public int Id { get; set; }
    public string DecisionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int DecisionByUserId { get; set; }
    public string DecisionByName { get; set; } = string.Empty;
    public DateTime DecisionDateUtc { get; set; }
    public string? PriorApplicationStatus { get; set; }
    public string? ResultingApplicationStatus { get; set; }
}
