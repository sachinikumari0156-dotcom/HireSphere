namespace HireSphere.API.Models;

public class InterviewFeedback
{
    public int Id { get; set; }

    public int InterviewId { get; set; }

    public int InterviewerId { get; set; }

    public decimal Rating { get; set; }

    public string? Comments { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Interview Interview { get; set; } = null!;

    public User Interviewer { get; set; } = null!;
}
