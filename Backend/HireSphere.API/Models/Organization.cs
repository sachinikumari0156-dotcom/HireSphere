namespace HireSphere.API.Models;

public class Organization
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Website { get; set; }

    public string? ContactEmail { get; set; }

    public string? Address { get; set; }

    public string TimeZoneId { get; set; } = "UTC";

    public string? DefaultCurrency { get; set; }

    public Models.Enums.OrganizationStatus Status { get; set; } = Models.Enums.OrganizationStatus.Active;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<Department> Departments { get; set; } = new List<Department>();

    public ICollection<RecruiterProfile> RecruiterProfiles { get; set; } = new List<RecruiterProfile>();

    public ICollection<HiringManagerProfile> HiringManagerProfiles { get; set; } = new List<HiringManagerProfile>();

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
