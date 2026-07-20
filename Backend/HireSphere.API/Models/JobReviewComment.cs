namespace HireSphere.API.Models;

public class JobReviewComment
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public int AuthorUserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Job Job { get; set; } = null!;

    public User Author { get; set; } = null!;
}
