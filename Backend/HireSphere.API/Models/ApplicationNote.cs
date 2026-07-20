namespace HireSphere.API.Models;

public class ApplicationNote
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public int AuthorUserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public Application Application { get; set; } = null!;

    public User Author { get; set; } = null!;
}
