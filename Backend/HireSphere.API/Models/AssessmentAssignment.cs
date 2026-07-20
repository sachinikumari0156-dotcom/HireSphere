using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

/// <summary>Links a skill assessment to a candidate with attempt limits and window.</summary>
public class AssessmentAssignment
{
    public int Id { get; set; }

    public int SkillAssessmentId { get; set; }

    public int CandidateId { get; set; }

    public int? ApplicationId { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? StartsAtUtc { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public int MaxAttempts { get; set; } = 1;

    public bool RevealResultsToCandidate { get; set; }

    public AssessmentStatus Status { get; set; } = AssessmentStatus.Pending;

    public SkillAssessment SkillAssessment { get; set; } = null!;

    public User Candidate { get; set; } = null!;

    public Application? Application { get; set; }

    public ICollection<AssessmentAttempt> Attempts { get; set; } = new List<AssessmentAttempt>();
}
