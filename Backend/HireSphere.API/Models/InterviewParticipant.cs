namespace HireSphere.API.Models;

public class InterviewParticipant
{
    public int Id { get; set; }

    public int InterviewId { get; set; }

    public int UserId { get; set; }

    public string ParticipantRole { get; set; } = string.Empty;

    public Interview Interview { get; set; } = null!;

    public User User { get; set; } = null!;
}
