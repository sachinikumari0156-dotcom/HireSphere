namespace HireSphere.API.Models;

public class ApplicationAnswer
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public int? ScreeningQuestionId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string AnswerText { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Application Application { get; set; } = null!;

    public ScreeningQuestion? ScreeningQuestion { get; set; }
}
