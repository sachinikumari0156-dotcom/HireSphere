using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class CandidateProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Skills { get; set; } = string.Empty;

    public string ResumePath { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? Location { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? DesiredJobTitle { get; set; }

    public WorkArrangement? PreferredWorkArrangement { get; set; }

    public int? SalaryExpectation { get; set; }

    public string? Availability { get; set; }

    public string? PortfolioUrl { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? GitHubUrl { get; set; }

    /// <summary>When false, resume content must not be sent to an external AI provider.</summary>
    public bool AllowExternalAiProcessing { get; set; }

    public DateTime? ExternalAiConsentAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public User User { get; set; } = null!;

    public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();

    public ICollection<Education> Educations { get; set; } = new List<Education>();

    public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();

    public ICollection<Certification> Certifications { get; set; } = new List<Certification>();

    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();

    public ICollection<CandidateDocument> Documents { get; set; } = new List<CandidateDocument>();
}
