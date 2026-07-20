using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class CandidateEvaluation
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public int EvaluatorUserId { get; set; }

    public EvaluationSubmissionStatus SubmissionStatus { get; set; } = EvaluationSubmissionStatus.Draft;

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

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }

    public Application Application { get; set; } = null!;
    public User EvaluatorUser { get; set; } = null!;
}
