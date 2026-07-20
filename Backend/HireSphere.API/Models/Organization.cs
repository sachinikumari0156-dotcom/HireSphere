namespace HireSphere.API.Models;

public class Organization
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Website { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<Department> Departments { get; set; } = new List<Department>();

    public ICollection<RecruiterProfile> RecruiterProfiles { get; set; } = new List<RecruiterProfile>();

    public ICollection<HiringManagerProfile> HiringManagerProfiles { get; set; } = new List<HiringManagerProfile>();

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
