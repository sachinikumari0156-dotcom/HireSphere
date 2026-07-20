using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class AssessmentAttempt
{
    public int Id { get; set; }

    public int SkillAssessmentId { get; set; }

    public int CandidateId { get; set; }

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }

    public AssessmentStatus Status { get; set; } = AssessmentStatus.InProgress;

    public SkillAssessment SkillAssessment { get; set; } = null!;

    public User Candidate { get; set; } = null!;

    public AssessmentResult? Result { get; set; }
}
