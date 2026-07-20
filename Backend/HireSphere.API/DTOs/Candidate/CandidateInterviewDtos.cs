using HireSphere.API.Models.Enums;

namespace HireSphere.API.DTOs.Candidate;

public sealed class CandidateInterviewListItemDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public DateTime InterviewDateUtc { get; set; }
    public string TimeZoneId { get; set; } = string.Empty;
    public string InterviewType { get; set; } = string.Empty;
    public InterviewStatus Status { get; set; }
    public InterviewCandidateResponse CandidateResponse { get; set; }
    public bool MeetingInfoAvailable { get; set; }
}

public sealed class CandidateInterviewDetailDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public DateTime InterviewDateUtc { get; set; }
    public string TimeZoneId { get; set; } = string.Empty;
    public string InterviewType { get; set; } = string.Empty;
    public InterviewStatus Status { get; set; }
    public InterviewCandidateResponse CandidateResponse { get; set; }
    public string? CandidateResponseReason { get; set; }
    public DateTime? CandidateRespondedAtUtc { get; set; }
    public bool MeetingInfoAvailable { get; set; }
    public string? MeetingLink { get; set; }
    public string? MeetingInstructions { get; set; }
    public bool CanConfirm { get; set; }
    public bool CanRequestReschedule { get; set; }
    public bool CanDecline { get; set; }
}

public sealed class InterviewRescheduleRequestDto
{
    public string Reason { get; set; } = string.Empty;
    public string? PreferredTimesNote { get; set; }
}

public sealed class InterviewDeclineDto
{
    public string Reason { get; set; } = string.Empty;
}
