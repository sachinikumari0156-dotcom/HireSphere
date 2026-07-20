namespace HireSphere.API.Models;

public class ResumeAnalysis
{
    public int Id { get; set; }

    public int ResumeId { get; set; }

    public string? AnalysisSummary { get; set; }

    public decimal? OverallScore { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Resume Resume { get; set; } = null!;
}
