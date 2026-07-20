using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class Job
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string RequiredSkills { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string JobType { get; set; } = string.Empty;

    public DateTime PostedDate { get; set; }

    public int RecruiterId { get; set; }

    public int? OrganizationId { get; set; }

    public int? DepartmentId { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Draft;

    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;

    public WorkArrangement WorkArrangement { get; set; } = WorkArrangement.OnSite;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public User? Recruiter { get; set; }

    public Organization? Organization { get; set; }

    public Department? Department { get; set; }

    public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();

    public ICollection<ScreeningQuestion> ScreeningQuestions { get; set; } = new List<ScreeningQuestion>();

    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
