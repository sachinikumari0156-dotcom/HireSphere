namespace HireSphere.API.Models;

public class Department
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public Organization Organization { get; set; } = null!;

    public ICollection<RecruiterProfile> RecruiterProfiles { get; set; } = new List<RecruiterProfile>();

    public ICollection<HiringManagerProfile> HiringManagerProfiles { get; set; } = new List<HiringManagerProfile>();

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
