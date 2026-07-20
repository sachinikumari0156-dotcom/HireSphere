namespace HireSphere.API.Models;

public class AIInsight
{
    public int Id { get; set; }

    public int? ApplicationId { get; set; }

    public int? CandidateProfileId { get; set; }

    public string InsightType { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public decimal? ConfidenceScore { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Application? Application { get; set; }

    public CandidateProfile? CandidateProfile { get; set; }
}
