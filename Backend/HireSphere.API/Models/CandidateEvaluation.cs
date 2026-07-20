namespace HireSphere.API.Models;

public class CandidateEvaluation
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public int EvaluatorUserId { get; set; }

    public decimal OverallScore { get; set; }

    public string? Strengths { get; set; }

    public string? Weaknesses { get; set; }

    public string? Recommendation { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public Application Application { get; set; } = null!;

    public User EvaluatorUser { get; set; } = null!;
}
