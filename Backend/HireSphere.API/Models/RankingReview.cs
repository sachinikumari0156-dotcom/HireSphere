namespace HireSphere.API.Models;

/// <summary>Audited human review / override of a deterministic ranking insight.</summary>
public class RankingReview
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public int ReviewerUserId { get; set; }

    public decimal? OverrideScore { get; set; }

    public string Decision { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Application Application { get; set; } = null!;

    public User Reviewer { get; set; } = null!;
}
