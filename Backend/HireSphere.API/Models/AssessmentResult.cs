namespace HireSphere.API.Models;

public class AssessmentResult
{
    public int Id { get; set; }

    public int AssessmentAttemptId { get; set; }

    public decimal Score { get; set; }

    public decimal MaxScore { get; set; }

    public bool Passed { get; set; }

    public string? Feedback { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public AssessmentAttempt AssessmentAttempt { get; set; } = null!;
}
