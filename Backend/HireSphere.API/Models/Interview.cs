using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class Interview
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public DateTime InterviewDate { get; set; }

    /// <summary>IANA timezone id, e.g. Asia/Colombo. Display only — no calendar provider tokens.</summary>
    public string TimeZoneId { get; set; } = "UTC";

    public string InterviewType { get; set; } = string.Empty;

    public string MeetingLink { get; set; } = string.Empty;

    public string? MeetingInstructions { get; set; }

    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public InterviewCandidateResponse CandidateResponse { get; set; } = InterviewCandidateResponse.Pending;

    public string? CandidateResponseReason { get; set; }

    public DateTime? CandidateRespondedAtUtc { get; set; }

    /// <summary>When true, meeting link is only returned after the candidate confirms.</summary>
    public bool RequireConfirmForMeetingInfo { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public Application Application { get; set; } = null!;

    public ICollection<InterviewParticipant> Participants { get; set; } = new List<InterviewParticipant>();

    public ICollection<InterviewFeedback> Feedbacks { get; set; } = new List<InterviewFeedback>();
}
