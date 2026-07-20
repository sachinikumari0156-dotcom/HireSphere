using HireSphere.API.Models.Enums;

namespace HireSphere.API.DTOs.Admin;

public sealed class AdminDashboardDto
{
    public int ActiveUsers { get; set; }
    public int DisabledUsers { get; set; }
    public int PendingRecruiterRequests { get; set; }
    public int Candidates { get; set; }
    public int Recruiters { get; set; }
    public int HiringManagers { get; set; }
    public int Administrators { get; set; }
    public int Organizations { get; set; }
    public int Departments { get; set; }
    public int ActiveJobs { get; set; }
    public int Applications { get; set; }
    public int PendingFinalDecisions { get; set; }
    public int UpcomingInterviews { get; set; }
    public IReadOnlyList<AdminAuditListItemDto> RecentAuditEvents { get; set; } = Array.Empty<AdminAuditListItemDto>();
}

public sealed class AdminUserListQuery
{
    public string? Keyword { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
    public int? OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminUserListItemDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class AdminUserDetailDto : AdminUserListItemDto
{
    public IReadOnlyList<AdminRoleAssignmentDto> Roles { get; set; } = Array.Empty<AdminRoleAssignmentDto>();
    public IReadOnlyList<AdminAuditListItemDto> RecentAudit { get; set; } = Array.Empty<AdminAuditListItemDto>();
}

public sealed class AdminRoleAssignmentDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime AssignedAtUtc { get; set; }
}

public sealed class UpdateAdminUserStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public sealed class AssignOrganizationDto
{
    public int? OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
}

public sealed class AssignDepartmentDto
{
    public int? DepartmentId { get; set; }
}

public sealed class AssignUserRoleDto
{
    public string RoleName { get; set; } = string.Empty;
}

public sealed class AdminRoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyList<int> PermissionIds { get; set; } = Array.Empty<int>();
}

public sealed class AdminPermissionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class UpdateRolePermissionsDto
{
    public IReadOnlyList<int> PermissionIds { get; set; } = Array.Empty<int>();
}

public sealed class AdminRecruiterRequestDetailDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string BusinessEmail { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public int? ReviewedByUserId { get; set; }
    public int? CreatedUserId { get; set; }
}

public sealed class ReviewRecruiterRequestAdminDto
{
    public string? Notes { get; set; }
    public int? OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
}

public sealed class AdminOrganizationDto
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
    public OrganizationStatus Status { get; set; }
    public int DepartmentCount { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class UpsertOrganizationDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? ContactEmail { get; set; }
    public string? Address { get; set; }
    public string? TimeZoneId { get; set; }
    public string? DefaultCurrency { get; set; }
}

public sealed class UpdateOrganizationStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public sealed class AdminDepartmentDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public DepartmentStatus Status { get; set; }
    public int? ManagerUserId { get; set; }
    public int UserCount { get; set; }
    public int JobCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class UpsertDepartmentDto
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int? ManagerUserId { get; set; }
}

public sealed class UpdateDepartmentStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public sealed class AdminAuditListItemDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public int? ActorUserId { get; set; }
    public string? ActorRole { get; set; }
    public string? Details { get; set; }
    public bool Success { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class AssignHiringManagerDto
{
    public int UserId { get; set; }
    public int? OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
    public int? JobId { get; set; }
}

public sealed class AdminPagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
