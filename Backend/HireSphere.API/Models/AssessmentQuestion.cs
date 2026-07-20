namespace HireSphere.API.Models;

public class AssessmentQuestion
{
    public int Id { get; set; }

    public int SkillAssessmentId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string QuestionType { get; set; } = string.Empty;

    public decimal Points { get; set; }

    public int SortOrder { get; set; }

    public SkillAssessment SkillAssessment { get; set; } = null!;
}
