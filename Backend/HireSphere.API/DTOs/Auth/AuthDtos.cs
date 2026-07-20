namespace HireSphere.API.DTOs.Auth;

public class CandidateRegisterDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;

    public bool AcceptTerms { get; set; }
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Message { get; set; } = string.Empty;

    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? Token { get; set; }

    public int? OrganizationId { get; set; }

    public int? DepartmentId { get; set; }
}

public class CurrentUserDto
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int? OrganizationId { get; set; }

    public int? DepartmentId { get; set; }

    public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
}

public class CreateRecruiterAccessRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string BusinessEmail { get; set; } = string.Empty;

    public string OrganizationName { get; set; } = string.Empty;

    public string? Message { get; set; }
}

public class RecruiterAccessRequestDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string BusinessEmail { get; set; } = string.Empty;

    public string OrganizationName { get; set; } = string.Empty;

    public string? Message { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }

    public string? ReviewNotes { get; set; }
}

public class ReviewRecruiterRequestDto
{
    public string? Notes { get; set; }
}

public class UpdateUserStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class UpdateUserRoleDto
{
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserOrganizationDto
{
    public int? OrganizationId { get; set; }

    public int? DepartmentId { get; set; }
}
