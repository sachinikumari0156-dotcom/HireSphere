namespace HireSphere.API.Models;

public class ScreeningQuestion
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string QuestionType { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Job Job { get; set; } = null!;

    public ICollection<ApplicationAnswer> Answers { get; set; } = new List<ApplicationAnswer>();
}
