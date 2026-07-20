namespace HireSphere.API.Models;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// ApplicationSubmitted | ApplicationStatusUpdated | AssessmentAssigned |
    /// InterviewScheduled | InterviewUpdated
    /// </summary>
    public string Category { get; set; } = string.Empty;

    public string? RelatedEntityType { get; set; }

    public int? RelatedEntityId { get; set; }

    public string? LinkPath { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
