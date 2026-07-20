namespace HireSphere.API.Models;

public class Department
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    public string? Description { get; set; }

    public Models.Enums.DepartmentStatus Status { get; set; } = Models.Enums.DepartmentStatus.Active;

    public int? ManagerUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public Organization Organization { get; set; } = null!;

    public User? ManagerUser { get; set; }

    public ICollection<RecruiterProfile> RecruiterProfiles { get; set; } = new List<RecruiterProfile>();

    public ICollection<HiringManagerProfile> HiringManagerProfiles { get; set; } = new List<HiringManagerProfile>();

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
