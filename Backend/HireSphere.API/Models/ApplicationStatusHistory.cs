using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class ApplicationStatusHistory
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public ApplicationStatus Status { get; set; }

    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;

    public int? ChangedByUserId { get; set; }

    public string? Notes { get; set; }

    public Application Application { get; set; } = null!;

    public User? ChangedByUser { get; set; }
}
