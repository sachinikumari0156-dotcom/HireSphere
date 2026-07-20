using HireSphere.API.Models.Enums;

namespace HireSphere.API.DTOs.Admin;

public sealed class AdminAuditLogQuery
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int? ActorUserId { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public bool? Success { get; set; }
    public string? CorrelationId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class AdminMonitoringSummaryDto
{
    public string ApiHealth { get; set; } = "Operational";
    public string DatabaseConnectivity { get; set; } = "Unknown";
    public int PendingRecruiterRequests { get; set; }
    public int DisabledAccounts { get; set; }
    public int PendingAssessments { get; set; }
    public int UpcomingInterviews { get; set; }
    public int PendingInterviewFeedback { get; set; }
    public int PendingFinalDecisions { get; set; }
    public string StorageProviderStatus { get; set; } = "NotConfigured";
    public string EmailProviderStatus { get; set; } = "NotConfigured";
    public string SmsProviderStatus { get; set; } = "NotConfigured";
    public string CalendarProviderStatus { get; set; } = "NotConfigured";
    public string ProviderNotes { get; set; } = "Email/SMS/calendar/cloud storage deferred to Phase 8.";
}

public sealed class AdminAnalyticsFilter
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int? OrganizationId { get; set; }
}

public sealed class AdminUserAnalyticsDto
{
    public IReadOnlyList<NamedCountDto> UsersByRole { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<NamedCountDto> UsersByStatus { get; set; } = Array.Empty<NamedCountDto>();
}

public sealed class AdminRecruitmentAnalyticsDto
{
    public IReadOnlyList<NamedCountDto> JobsByStatus { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<NamedCountDto> ApplicationsByStatus { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<NamedCountDto> ApplicationsOverTime { get; set; } = Array.Empty<NamedCountDto>();
    public int Shortlisted { get; set; }
    public int Rejected { get; set; }
    public int Hired { get; set; }
    public string UnavailableMetricsNote { get; set; } =
        "Time-to-hire averages require richer status timestamps; partial counts shown from LocalDB.";
}

public sealed class AdminDepartmentAnalyticsDto
{
    public IReadOnlyList<NamedCountDto> JobsByDepartment { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<NamedCountDto> UsersByDepartment { get; set; } = Array.Empty<NamedCountDto>();
}

public sealed class AdminSkillAnalyticsDto
{
    public IReadOnlyList<NamedCountDto> SkillDemandFromJobs { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<NamedCountDto> CandidateSkillAvailability { get; set; } = Array.Empty<NamedCountDto>();
}

public sealed class NamedCountDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class AdminFinalDecisionListItemDto
{
    public int ApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string ApplicationStatus { get; set; } = string.Empty;
    public string? LatestRecommendation { get; set; }
    public DateTime? RecommendationDateUtc { get; set; }
}

public sealed class AdminFinalDecisionDetailDto
{
    public int ApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string ApplicationStatus { get; set; } = string.Empty;
    public string? MatchExplanation { get; set; }
    public string? LatestRecommendation { get; set; }
    public string? RecommendationReason { get; set; }
    public IReadOnlyList<HiringDecisionHistoryItemLiteDto> DecisionHistory { get; set; } =
        Array.Empty<HiringDecisionHistoryItemLiteDto>();
    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();
}

public sealed class HiringDecisionHistoryItemLiteDto
{
    public int Id { get; set; }
    public string DecisionType { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime DecisionDateUtc { get; set; }
}

public sealed class AdminFinalDecisionRequestDto
{
    public string DecisionType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? ExpectedUpdatedAtUtc { get; set; }
}

public sealed class AdminSecurityUserDto
{
    public int UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public bool MustChangePassword { get; set; }
    public IReadOnlyList<AdminAuditListItemDto> RecentSecurityEvents { get; set; } =
        Array.Empty<AdminAuditListItemDto>();
}
