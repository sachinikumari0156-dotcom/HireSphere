namespace HireSphere.API.Models;

public class CandidateJobMatch
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }

    public int JobId { get; set; }

    public decimal MatchScore { get; set; }

    public string? MatchSummary { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public CandidateProfile CandidateProfile { get; set; } = null!;

    public Job Job { get; set; } = null!;
}
