using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public CandidateProfile? CandidateProfile { get; set; }

    public RecruiterProfile? RecruiterProfile { get; set; }

    public HiringManagerProfile? HiringManagerProfile { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();

    public ICollection<Application> Applications { get; set; } = new List<Application>();

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
