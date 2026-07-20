using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class ResumeAnalysis
{
    public int Id { get; set; }

    public int ResumeId { get; set; }

    public ResumeAnalysisStatus Status { get; set; } = ResumeAnalysisStatus.Pending;

    public string Provider { get; set; } = "Deterministic";

    public string ProviderVersion { get; set; } = "deterministic-parse-v1";

    public string ProviderType { get; set; } = "Deterministic";

    public string? AnalysisSummary { get; set; }

    public decimal? OverallScore { get; set; }

    public string? FailureReason { get; set; }

    public string? ExtractedName { get; set; }

    public string? ExtractedEmail { get; set; }

    public string? ExtractedPhone { get; set; }

    public string? ExtractedSummary { get; set; }

    public int? EstimatedYearsExperience { get; set; }

    public bool ConsentUsedExternal { get; set; }

    public string? FallbackNote { get; set; }

    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public Resume Resume { get; set; } = null!;

    public ICollection<ExtractedSkill> ExtractedSkills { get; set; } = new List<ExtractedSkill>();
}
