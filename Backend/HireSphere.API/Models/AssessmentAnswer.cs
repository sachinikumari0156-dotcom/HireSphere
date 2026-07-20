namespace HireSphere.API.Models;

public class AssessmentAnswer
{
    public int Id { get; set; }

    public int AssessmentAttemptId { get; set; }

    public int AssessmentQuestionId { get; set; }

    public string AnswerValue { get; set; } = string.Empty;

    public decimal? AwardedPoints { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public AssessmentAttempt AssessmentAttempt { get; set; } = null!;

    public AssessmentQuestion AssessmentQuestion { get; set; } = null!;
}
