using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class SkillAssessment
{
    public int Id { get; set; }

    public int? JobId { get; set; }

    public int? OrganizationId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? DurationMinutes { get; set; }

    public int MaxAttempts { get; set; } = 1;

    public decimal PassingScorePercent { get; set; } = 60m;

    public bool RevealResultsToCandidate { get; set; }

    public bool IsArchived { get; set; }

    public AssessmentStatus Status { get; set; } = AssessmentStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public Job? Job { get; set; }

    public Organization? Organization { get; set; }

    public ICollection<AssessmentQuestion> Questions { get; set; } = new List<AssessmentQuestion>();

    public ICollection<AssessmentAttempt> Attempts { get; set; } = new List<AssessmentAttempt>();

    public ICollection<AssessmentAssignment> Assignments { get; set; } = new List<AssessmentAssignment>();
}
