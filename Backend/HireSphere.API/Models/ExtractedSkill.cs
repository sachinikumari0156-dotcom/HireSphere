using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class ExtractedSkill
{
    public int Id { get; set; }

    public int ResumeAnalysisId { get; set; }

    public string RawName { get; set; } = string.Empty;

    public string CanonicalName { get; set; } = string.Empty;

    public decimal Confidence { get; set; }

    public ExtractedSkillStatus Status { get; set; } = ExtractedSkillStatus.Pending;

    public string? SourceEvidence { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ResumeAnalysis ResumeAnalysis { get; set; } = null!;
}
