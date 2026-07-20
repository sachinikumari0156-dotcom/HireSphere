namespace HireSphere.API.DTOs.Candidate;

public sealed class CandidateNotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? LinkPath { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class CandidateNotificationListDto
{
    public int UnreadCount { get; set; }
    public IReadOnlyList<CandidateNotificationDto> Items { get; set; } = Array.Empty<CandidateNotificationDto>();
}
