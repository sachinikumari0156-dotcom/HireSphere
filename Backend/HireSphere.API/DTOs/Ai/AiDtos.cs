namespace HireSphere.API.DTOs.Ai;

public sealed class ResumeAnalysisDto
{
    public int Id { get; set; }
    public int ResumeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
    public string ProviderVersion { get; set; } = string.Empty;
    public string? AnalysisSummary { get; set; }
    public string? FailureReason { get; set; }
    public string? ExtractedName { get; set; }
    public string? ExtractedEmail { get; set; }
    public string? ExtractedPhone { get; set; }
    public string? ExtractedSummary { get; set; }
    public int? EstimatedYearsExperience { get; set; }
    public bool ConsentUsedExternal { get; set; }
    public string? FallbackNote { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public string HumanReviewNotice { get; set; } =
        "AI-generated insight. Final recruitment decisions must be reviewed by authorized users.";
    public IReadOnlyList<ExtractedSkillDto> Skills { get; set; } = Array.Empty<ExtractedSkillDto>();
}

public sealed class ExtractedSkillDto
{
    public int Id { get; set; }
    public string RawName { get; set; } = string.Empty;
    public string CanonicalName { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SourceEvidence { get; set; }
}

public sealed class ConfirmResumeAnalysisDto
{
    public IReadOnlyList<int> AcceptSkillIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> RejectSkillIds { get; set; } = Array.Empty<int>();
}

public sealed class ExternalAiConsentDto
{
    public bool AllowExternalAiProcessing { get; set; }
}

public sealed class CandidateAiStatusDto
{
    public bool AllowExternalAiProcessing { get; set; }
    public DateTime? ExternalAiConsentAtUtc { get; set; }
    public string DeterministicProviderStatus { get; set; } = "Healthy";
    public string ExternalAiProviderStatus { get; set; } = "NotConfigured";
    public string HumanReviewNotice { get; set; } =
        "AI-generated insight. Final recruitment decisions must be reviewed by authorized users.";
}

public sealed class AdminAiStatusDto
{
    public string DeterministicStatus { get; set; } = "Healthy";
    public string ExternalAiStatus { get; set; } = "NotConfigured";
    public string InsightKind { get; set; } = "Descriptive insight";
    public string Notes { get; set; } =
        "External AI is NotConfigured unless credentials are set and a verified call succeeds.";
}

public sealed class SkillTrendDto
{
    public string InsightKind { get; set; } = "Descriptive insight";
    public IReadOnlyList<SkillTrendItemDto> MostRequestedSkills { get; set; } = Array.Empty<SkillTrendItemDto>();
    public IReadOnlyList<SkillTrendItemDto> CommonCandidateSkills { get; set; } = Array.Empty<SkillTrendItemDto>();
    public string? EmptyStateNote { get; set; }
}

public sealed class SkillTrendItemDto
{
    public string SkillName { get; set; } = string.Empty;
    public int Count { get; set; }
}
