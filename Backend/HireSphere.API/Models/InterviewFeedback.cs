namespace HireSphere.API.Models;

public class InterviewFeedback
{
    public int Id { get; set; }

    public int InterviewId { get; set; }

    public int InterviewerId { get; set; }

    /// <summary>Legacy overall rating 1–5; kept for compatibility.</summary>
    public decimal Rating { get; set; }

    public decimal? TechnicalCompetency { get; set; }
    public decimal? Communication { get; set; }
    public decimal? ProblemSolving { get; set; }
    public decimal? RoleKnowledge { get; set; }
    public decimal? Teamwork { get; set; }
    public decimal? Leadership { get; set; }
    /// <summary>Job-relevant behavioral contribution (not protected-characteristic scoring).</summary>
    public decimal? CulturalContribution { get; set; }

    public string? Strengths { get; set; }
    public string? Concerns { get; set; }
    public string? Recommendation { get; set; }
    public string? Comments { get; set; }
    /// <summary>Never exposed on Candidate APIs.</summary>
    public string? PrivatePanelComments { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Interview Interview { get; set; } = null!;
    public User Interviewer { get; set; } = null!;
}
