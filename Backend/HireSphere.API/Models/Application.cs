using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class Application
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public int JobId { get; set; }

    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public string CoverLetter { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public User Candidate { get; set; } = null!;

    public Job Job { get; set; } = null!;

    public ICollection<ApplicationAnswer> Answers { get; set; } = new List<ApplicationAnswer>();

    public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = new List<ApplicationStatusHistory>();

    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
}
