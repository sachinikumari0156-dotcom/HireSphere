using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class RecruiterAccessRequest
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string BusinessEmail { get; set; } = string.Empty;

    public string NormalizedBusinessEmail { get; set; } = string.Empty;

    public string OrganizationName { get; set; } = string.Empty;

    public string? Message { get; set; }

    public RecruiterRequestStatus Status { get; set; } = RecruiterRequestStatus.Pending;

    public int? ReviewedByUserId { get; set; }

    public User? ReviewedByUser { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }

    public string? ReviewNotes { get; set; }

    public int? CreatedUserId { get; set; }

    public User? CreatedUser { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
