using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class HiringDecision
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public int DecisionByUserId { get; set; }

    public HiringDecisionType DecisionType { get; set; }

    public HiringDecisionStatus Status { get; set; } = HiringDecisionStatus.Pending;

    public bool IsFinal { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public ApplicationStatus? PriorApplicationStatus { get; set; }

    public ApplicationStatus? ResultingApplicationStatus { get; set; }

    public DateTime DecisionDateUtc { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Application Application { get; set; } = null!;

    public User DecisionByUser { get; set; } = null!;
}
