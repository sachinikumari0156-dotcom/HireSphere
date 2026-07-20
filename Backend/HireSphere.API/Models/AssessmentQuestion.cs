namespace HireSphere.API.Models;

public class AssessmentQuestion
{
    public int Id { get; set; }

    public int SkillAssessmentId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    /// <summary>MultipleChoice | TrueFalse | ShortAnswer</summary>
    public string QuestionType { get; set; } = string.Empty;

    public decimal Points { get; set; }

    public int SortOrder { get; set; }

    /// <summary>JSON array of option strings for MultipleChoice / TrueFalse. Never includes the key.</summary>
    public string? OptionsJson { get; set; }

    /// <summary>Server-only answer key. Must never be serialized to candidate APIs.</summary>
    public string CorrectAnswerKey { get; set; } = string.Empty;

    public SkillAssessment SkillAssessment { get; set; } = null!;
}
