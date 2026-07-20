using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class Interview
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public DateTime InterviewDate { get; set; }

    public string InterviewType { get; set; } = string.Empty;

    public string MeetingLink { get; set; } = string.Empty;

    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Application Application { get; set; } = null!;

    public ICollection<InterviewParticipant> Participants { get; set; } = new List<InterviewParticipant>();

    public ICollection<InterviewFeedback> Feedbacks { get; set; } = new List<InterviewFeedback>();
}
