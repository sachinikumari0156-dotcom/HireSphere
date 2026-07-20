namespace HireSphere.API.Models;

public class RecruiterProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? OrganizationId { get; set; }

    public int? DepartmentId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Title { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public User User { get; set; } = null!;

    public Organization? Organization { get; set; }

    public Department? Department { get; set; }
}
