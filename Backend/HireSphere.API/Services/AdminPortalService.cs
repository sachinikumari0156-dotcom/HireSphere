using HireSphere.API.Data;
using HireSphere.API.DTOs.Admin;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface IAdminPortalService
{
    Task<(bool Ok, string? Error, AdminDashboardDto? Result)> GetDashboardAsync();
    Task<(bool Ok, string? Error, AdminPagedResultDto<AdminUserListItemDto>? Result)> ListUsersAsync(AdminUserListQuery query);
    Task<(bool Ok, string? Error, AdminUserDetailDto? Result)> GetUserAsync(int userId);
    Task<(bool Ok, string? Error)> UpdateUserStatusAsync(int userId, string status);
    Task<(bool Ok, string? Error)> AssignOrganizationAsync(int userId, int? organizationId, int? departmentId);
    Task<(bool Ok, string? Error)> AssignDepartmentAsync(int userId, int? departmentId);
    Task<(bool Ok, string? Error, IReadOnlyList<AdminRoleAssignmentDto>? Result)> GetUserRolesAsync(int userId);
    Task<(bool Ok, string? Error)> AddUserRoleAsync(int userId, string roleName);
    Task<(bool Ok, string? Error)> RemoveUserRoleAsync(int userId, int roleId);
    Task<(bool Ok, string? Error, IReadOnlyList<AdminRoleDto>? Result)> ListRolesAsync();
    Task<(bool Ok, string? Error, IReadOnlyList<AdminPermissionDto>? Result)> ListPermissionsAsync();
    Task<(bool Ok, string? Error)> UpdateRolePermissionsAsync(int roleId, IReadOnlyList<int> permissionIds);
    Task<(bool Ok, string? Error, AdminRecruiterRequestDetailDto? Result)> GetRecruiterRequestAsync(int id);
    Task<(bool Ok, string? Error)> ApproveRecruiterRequestAsync(int id, ReviewRecruiterRequestAdminDto dto);
    Task<(bool Ok, string? Error)> RejectRecruiterRequestAsync(int id, string? notes);
    Task<(bool Ok, string? Error, IReadOnlyList<AdminOrganizationDto>? Result)> ListOrganizationsAsync();
    Task<(bool Ok, string? Error, AdminOrganizationDto? Result)> GetOrganizationAsync(int id);
    Task<(bool Ok, string? Error, AdminOrganizationDto? Result)> CreateOrganizationAsync(UpsertOrganizationDto dto);
    Task<(bool Ok, string? Error, AdminOrganizationDto? Result)> UpdateOrganizationAsync(int id, UpsertOrganizationDto dto);
    Task<(bool Ok, string? Error)> UpdateOrganizationStatusAsync(int id, string status);
    Task<(bool Ok, string? Error, IReadOnlyList<AdminDepartmentDto>? Result)> ListDepartmentsAsync(int? organizationId);
    Task<(bool Ok, string? Error, AdminDepartmentDto? Result)> CreateDepartmentAsync(UpsertDepartmentDto dto);
    Task<(bool Ok, string? Error, AdminDepartmentDto? Result)> UpdateDepartmentAsync(int id, UpsertDepartmentDto dto);
    Task<(bool Ok, string? Error)> UpdateDepartmentStatusAsync(int id, string status);
    Task<(bool Ok, string? Error)> AssignHiringManagerAsync(AssignHiringManagerDto dto);
}

public sealed class AdminPortalService : IAdminPortalService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordService _passwords;
    private readonly INotificationWriter _notifications;

    public AdminPortalService(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        IPasswordService passwords,
        INotificationWriter notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _passwords = passwords;
        _notifications = notifications;
    }

    private bool TryAdmin(out int adminId, out string? error)
    {
        if (_currentUser.UserId is not int id || !_currentUser.IsInRole("Admin"))
        {
            adminId = 0;
            error = "Unauthorized.";
            return false;
        }

        adminId = id;
        error = null;
        return true;
    }

    private async Task AuditAsync(int adminId, string action, string entityType, int? entityId, string? details, bool success = true)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            Success = success,
            ActorRole = "Admin",
            CorrelationId = Guid.NewGuid().ToString("N"),
            CreatedAtUtc = DateTime.UtcNow
        });
        await Task.CompletedTask;
    }

    private static void RotateStamp(User user) => user.SecurityStamp = Guid.NewGuid().ToString("N");

    private async Task<int> CountActiveAdminsAsync() =>
        await _db.Users.CountAsync(u => u.Role == "Admin" && u.Status == UserStatus.Active);

    public async Task<(bool Ok, string? Error, AdminDashboardDto? Result)> GetDashboardAsync()
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);

        var now = DateTime.UtcNow;
        var users = _db.Users.AsNoTracking();
        var recent = await _db.AuditLogs.AsNoTracking()
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(10)
            .Select(a => new AdminAuditListItemDto
            {
                Id = a.Id,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                ActorUserId = a.UserId,
                ActorRole = a.ActorRole,
                Details = a.Details,
                Success = a.Success,
                CorrelationId = a.CorrelationId,
                CreatedAtUtc = a.CreatedAtUtc
            })
            .ToListAsync();

        return (true, null, new AdminDashboardDto
        {
            ActiveUsers = await users.CountAsync(u => u.Status == UserStatus.Active),
            DisabledUsers = await users.CountAsync(u => u.Status == UserStatus.Inactive || u.Status == UserStatus.Suspended),
            PendingRecruiterRequests = await _db.RecruiterAccessRequests.CountAsync(r => r.Status == RecruiterRequestStatus.Pending),
            Candidates = await users.CountAsync(u => u.Role == "Candidate"),
            Recruiters = await users.CountAsync(u => u.Role == "Recruiter"),
            HiringManagers = await users.CountAsync(u => u.Role == "HiringManager"),
            Administrators = await users.CountAsync(u => u.Role == "Admin"),
            Organizations = await _db.Organizations.CountAsync(),
            Departments = await _db.Departments.CountAsync(),
            ActiveJobs = await _db.Jobs.CountAsync(j => j.Status == JobStatus.Published || j.Status == JobStatus.Open),
            Applications = await _db.Applications.CountAsync(),
            PendingFinalDecisions = await _db.HiringDecisions.CountAsync(d =>
                !d.IsFinal && (d.DecisionType == HiringDecisionType.RecommendHire || d.DecisionType == HiringDecisionType.RecommendReject)),
            UpcomingInterviews = await _db.Interviews.CountAsync(i =>
                i.InterviewDate >= now && i.Status != InterviewStatus.Cancelled && i.Status != InterviewStatus.Completed),
            RecentAuditEvents = recent
        });
    }

    public async Task<(bool Ok, string? Error, AdminPagedResultDto<AdminUserListItemDto>? Result)> ListUsersAsync(AdminUserListQuery query)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 20 : query.PageSize, 1, 100);
        var q = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim().ToUpperInvariant();
            q = q.Where(u => u.NormalizedEmail.Contains(kw) || u.FullName.ToUpper().Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
            q = q.Where(u => u.Role == query.Role);
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<UserStatus>(query.Status, true, out var st))
            q = q.Where(u => u.Status == st);

        var users = await q.OrderBy(u => u.Id).ToListAsync();
        var items = new List<AdminUserListItemDto>();
        foreach (var u in users)
        {
            var mapped = await MapUserListItemAsync(u);
            if (query.OrganizationId is int oid && mapped.OrganizationId != oid) continue;
            if (query.DepartmentId is int did && mapped.DepartmentId != did) continue;
            items.Add(mapped);
        }

        var total = items.Count;
        var pageItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return (true, null, new AdminPagedResultDto<AdminUserListItemDto>
        {
            Items = pageItems,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        });
    }

    private async Task<AdminUserListItemDto> MapUserListItemAsync(User u)
    {
        int? orgId = null, deptId = null;
        string? orgName = null, deptName = null;
        if (u.Role == "Recruiter")
        {
            var p = await _db.RecruiterProfiles.AsNoTracking().Include(x => x.Organization).Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.UserId == u.Id);
            orgId = p?.OrganizationId; deptId = p?.DepartmentId;
            orgName = p?.Organization?.Name; deptName = p?.Department?.Name;
        }
        else if (u.Role == "HiringManager")
        {
            var p = await _db.HiringManagerProfiles.AsNoTracking().Include(x => x.Organization).Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.UserId == u.Id);
            orgId = p?.OrganizationId; deptId = p?.DepartmentId;
            orgName = p?.Organization?.Name; deptName = p?.Department?.Name;
        }

        return new AdminUserListItemDto
        {
            UserId = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role,
            Status = u.Status.ToString(),
            OrganizationId = orgId,
            OrganizationName = orgName,
            DepartmentId = deptId,
            DepartmentName = deptName,
            MustChangePassword = u.MustChangePassword,
            CreatedAtUtc = u.CreatedAtUtc
        };
    }

    public async Task<(bool Ok, string? Error, AdminUserDetailDto? Result)> GetUserAsync(int userId)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return (false, "User not found.", null);

        var list = await MapUserListItemAsync(user);
        var roles = await _db.UserRoles.AsNoTracking().Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .Select(ur => new AdminRoleAssignmentDto
            {
                RoleId = ur.RoleId,
                RoleName = ur.Role.Name,
                AssignedAtUtc = ur.AssignedAtUtc
            }).ToListAsync();
        var audit = await _db.AuditLogs.AsNoTracking()
            .Where(a => a.EntityType == nameof(User) && a.EntityId == userId)
            .OrderByDescending(a => a.CreatedAtUtc).Take(20)
            .Select(a => new AdminAuditListItemDto
            {
                Id = a.Id,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                ActorUserId = a.UserId,
                ActorRole = a.ActorRole,
                Details = a.Details,
                Success = a.Success,
                CorrelationId = a.CorrelationId,
                CreatedAtUtc = a.CreatedAtUtc
            }).ToListAsync();

        return (true, null, new AdminUserDetailDto
        {
            UserId = list.UserId,
            FullName = list.FullName,
            Email = list.Email,
            Role = list.Role,
            Status = list.Status,
            OrganizationId = list.OrganizationId,
            OrganizationName = list.OrganizationName,
            DepartmentId = list.DepartmentId,
            DepartmentName = list.DepartmentName,
            MustChangePassword = list.MustChangePassword,
            CreatedAtUtc = list.CreatedAtUtc,
            Roles = roles,
            RecentAudit = audit
        });
    }

    public async Task<(bool Ok, string? Error)> UpdateUserStatusAsync(int userId, string status)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error);
        if (!Enum.TryParse<UserStatus>(status, true, out var parsed)) return (false, "Invalid status value.");
        if (userId == adminId && parsed is UserStatus.Inactive or UserStatus.Suspended)
            return (false, "Administrators cannot disable their own account through this endpoint.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return (false, "User not found.");

        if (user.Role == "Admin" && user.Status == UserStatus.Active
            && parsed is UserStatus.Inactive or UserStatus.Suspended)
        {
            if (await CountActiveAdminsAsync() <= 1)
                return (false, "Cannot disable the last active global Administrator.");
        }

        user.Status = parsed;
        user.UpdatedAtUtc = DateTime.UtcNow;
        if (parsed is UserStatus.Inactive or UserStatus.Suspended) RotateStamp(user);
        await AuditAsync(adminId, "admin.user.status", nameof(User), userId, $"Status set to {parsed}");
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> AssignOrganizationAsync(int userId, int? organizationId, int? departmentId)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return (false, "User not found.");

        if (organizationId is int oid)
        {
            var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == oid);
            if (org is null) return (false, "Organization not found.");
            if (org.Status is OrganizationStatus.Inactive or OrganizationStatus.Suspended or OrganizationStatus.Archived)
                return (false, "Cannot assign users to an inactive, suspended, or archived organization.");
        }

        if (departmentId is int did)
        {
            var dept = await _db.Departments.FirstOrDefaultAsync(d => d.Id == did);
            if (dept is null) return (false, "Department not found.");
            if (organizationId is int o2 && dept.OrganizationId != o2)
                return (false, "Department does not belong to the selected organization.");
            if (dept.Status == DepartmentStatus.Archived)
                return (false, "Archived departments cannot receive new assignments.");
            organizationId ??= dept.OrganizationId;
        }

        var assignError = await ApplyOrgDeptAsync(user, organizationId, departmentId);
        if (assignError is not null) return (false, assignError);

        await AuditAsync(adminId, "admin.user.organization", nameof(User), userId,
            $"OrganizationId={organizationId}; DepartmentId={departmentId}");
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> AssignDepartmentAsync(int userId, int? departmentId)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return (false, "User not found.");

        int? orgId = null;
        if (departmentId is int did)
        {
            var dept = await _db.Departments.FirstOrDefaultAsync(d => d.Id == did);
            if (dept is null) return (false, "Department not found.");
            if (dept.Status == DepartmentStatus.Archived)
                return (false, "Archived departments cannot receive new assignments.");
            orgId = dept.OrganizationId;
        }

        var assignError = await ApplyOrgDeptAsync(user, orgId, departmentId);
        if (assignError is not null) return (false, assignError);
        await AuditAsync(adminId, "admin.user.department", nameof(User), userId, $"DepartmentId={departmentId}");
        await _db.SaveChangesAsync();
        return (true, null);
    }

    private async Task<string?> ApplyOrgDeptAsync(User user, int? organizationId, int? departmentId)
    {
        if (user.Role == "Recruiter")
        {
            var profile = await _db.RecruiterProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id)
                ?? new RecruiterProfile { UserId = user.Id, CreatedAtUtc = DateTime.UtcNow };
            if (profile.Id == 0) _db.RecruiterProfiles.Add(profile);
            profile.OrganizationId = organizationId;
            profile.DepartmentId = departmentId;
            profile.UpdatedAtUtc = DateTime.UtcNow;
            return null;
        }

        if (user.Role == "HiringManager")
        {
            var profile = await _db.HiringManagerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id)
                ?? new HiringManagerProfile { UserId = user.Id, CreatedAtUtc = DateTime.UtcNow };
            if (profile.Id == 0) _db.HiringManagerProfiles.Add(profile);
            profile.OrganizationId = organizationId;
            profile.DepartmentId = departmentId;
            profile.UpdatedAtUtc = DateTime.UtcNow;
            return null;
        }

        return "Organization assignment applies to Recruiter or HiringManager accounts.";
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<AdminRoleAssignmentDto>? Result)> GetUserRolesAsync(int userId)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        if (!await _db.Users.AnyAsync(u => u.Id == userId)) return (false, "User not found.", null);
        var roles = await _db.UserRoles.AsNoTracking().Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .Select(ur => new AdminRoleAssignmentDto
            {
                RoleId = ur.RoleId,
                RoleName = ur.Role.Name,
                AssignedAtUtc = ur.AssignedAtUtc
            }).ToListAsync();
        return (true, null, roles);
    }

    public async Task<(bool Ok, string? Error)> AddUserRoleAsync(int userId, string roleName)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error);
        var allowed = new[] { "Candidate", "Recruiter", "HiringManager", "Admin" };
        if (!allowed.Contains(roleName, StringComparer.OrdinalIgnoreCase))
            return (false, "Invalid role.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return (false, "User not found.");

        var normalized = allowed.First(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == normalized);
        if (role is null) return (false, "Role not found.");

        user.Role = normalized;
        user.UpdatedAtUtc = DateTime.UtcNow;
        RotateStamp(user);

        var existing = await _db.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync();
        _db.UserRoles.RemoveRange(existing);
        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, AssignedAtUtc = DateTime.UtcNow });

        if (normalized == "HiringManager" && !await _db.HiringManagerProfiles.AnyAsync(p => p.UserId == user.Id))
        {
            _db.HiringManagerProfiles.Add(new HiringManagerProfile
            {
                UserId = user.Id,
                Title = "Hiring Manager",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        if (normalized == "Recruiter" && !await _db.RecruiterProfiles.AnyAsync(p => p.UserId == user.Id))
        {
            _db.RecruiterProfiles.Add(new RecruiterProfile
            {
                UserId = user.Id,
                Title = "Recruiter",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await AuditAsync(adminId, "admin.user.role", nameof(User), userId, $"Role set to {normalized}");
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> RemoveUserRoleAsync(int userId, int roleId)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return (false, "User not found.");

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        if (role is null) return (false, "Role not found.");

        if (role.Name == "Admin" && user.Role == "Admin" && user.Status == UserStatus.Active)
        {
            if (await CountActiveAdminsAsync() <= 1)
                return (false, "Cannot remove the last global Administrator role assignment.");
            if (userId == adminId)
                return (false, "Administrators cannot remove their own Administrator role when it would leave fewer than required Administrators.");
        }

        var ur = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId);
        if (ur is null) return (false, "Role assignment not found.");
        _db.UserRoles.Remove(ur);

        if (user.Role == role.Name)
        {
            user.Role = "Candidate";
            user.UpdatedAtUtc = DateTime.UtcNow;
            RotateStamp(user);
            var cand = await _db.Roles.FirstAsync(r => r.Name == "Candidate");
            if (!await _db.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == cand.Id))
                _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = cand.Id, AssignedAtUtc = DateTime.UtcNow });
        }

        await AuditAsync(adminId, "admin.user.role.remove", nameof(User), userId, $"Removed role {role.Name}");
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<AdminRoleDto>? Result)> ListRolesAsync()
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var roles = await _db.Roles.AsNoTracking().Include(r => r.RolePermissions).OrderBy(r => r.Name).ToListAsync();
        return (true, null, roles.Select(r => new AdminRoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            PermissionIds = r.RolePermissions.Select(rp => rp.PermissionId).ToList()
        }).ToList());
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<AdminPermissionDto>? Result)> ListPermissionsAsync()
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var perms = await _db.Permissions.AsNoTracking().OrderBy(p => p.Code)
            .Select(p => new AdminPermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description
            }).ToListAsync();
        return (true, null, perms);
    }

    public async Task<(bool Ok, string? Error)> UpdateRolePermissionsAsync(int roleId, IReadOnlyList<int> permissionIds)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error);
        var role = await _db.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Id == roleId);
        if (role is null) return (false, "Role not found.");

        var ids = permissionIds?.Distinct().ToList() ?? new List<int>();
        var valid = await _db.Permissions.Where(p => ids.Contains(p.Id)).Select(p => p.Id).ToListAsync();
        if (valid.Count != ids.Count) return (false, "One or more permissions are invalid.");

        _db.RolePermissions.RemoveRange(role.RolePermissions);
        foreach (var pid in valid)
            _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pid });

        await AuditAsync(adminId, "admin.role.permissions", nameof(Role), roleId, $"Permissions updated ({valid.Count})");
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error, AdminRecruiterRequestDetailDto? Result)> GetRecruiterRequestAsync(int id)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var r = await _db.RecruiterAccessRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return (false, "Request not found.", null);
        return (true, null, MapRequest(r));
    }

    private static AdminRecruiterRequestDetailDto MapRequest(RecruiterAccessRequest r) => new()
    {
        Id = r.Id,
        FullName = r.FullName,
        BusinessEmail = r.BusinessEmail,
        OrganizationName = r.OrganizationName,
        Message = r.Message,
        Status = r.Status.ToString(),
        ReviewNotes = r.ReviewNotes,
        CreatedAtUtc = r.CreatedAtUtc,
        ReviewedAtUtc = r.ReviewedAtUtc,
        ReviewedByUserId = r.ReviewedByUserId,
        CreatedUserId = r.CreatedUserId
    };

    public async Task<(bool Ok, string? Error)> ApproveRecruiterRequestAsync(int id, ReviewRecruiterRequestAdminDto dto)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error);
        var request = await _db.RecruiterAccessRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request is null) return (false, "Request not found.");
        if (request.Status != RecruiterRequestStatus.Pending) return (false, "Only pending requests can be approved.");
        if (await _db.Users.AnyAsync(u => u.NormalizedEmail == request.NormalizedBusinessEmail))
            return (false, "A user with this email already exists.");

        Organization? org = null;
        if (dto.OrganizationId is int oid)
        {
            org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == oid);
            if (org is null) return (false, "Organization not found.");
            if (org.Status != OrganizationStatus.Active) return (false, "Organization must be Active.");
        }
        else
        {
            org = await _db.Organizations.FirstOrDefaultAsync(o => o.Name == request.OrganizationName);
            if (org is null)
            {
                var code = $"ORG-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
                org = new Organization
                {
                    Name = request.OrganizationName,
                    Code = code,
                    Description = "Created from recruiter access approval",
                    Status = OrganizationStatus.Active,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _db.Organizations.Add(org);
                await _db.SaveChangesAsync();
            }
        }

        if (dto.DepartmentId is int did)
        {
            var dept = await _db.Departments.FirstOrDefaultAsync(d => d.Id == did);
            if (dept is null) return (false, "Department not found.");
            if (dept.OrganizationId != org.Id) return (false, "Department does not belong to the organization.");
            if (dept.Status == DepartmentStatus.Archived) return (false, "Archived departments cannot receive new assignments.");
        }

        var temporaryPassword = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "Aa1!";
        var user = new User
        {
            FullName = request.FullName,
            Email = request.BusinessEmail,
            NormalizedEmail = request.NormalizedBusinessEmail,
            PasswordHash = _passwords.Hash(temporaryPassword),
            Role = "Recruiter",
            Status = UserStatus.Active,
            MustChangePassword = true,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.RecruiterProfiles.Add(new RecruiterProfile
        {
            UserId = user.Id,
            OrganizationId = org.Id,
            DepartmentId = dto.DepartmentId,
            Title = "Recruiter",
            CreatedAtUtc = DateTime.UtcNow
        });

        var recruiterRole = await _db.Roles.FirstAsync(r => r.Name == "Recruiter");
        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = recruiterRole.Id, AssignedAtUtc = DateTime.UtcNow });

        request.Status = RecruiterRequestStatus.Approved;
        request.ReviewedByUserId = adminId;
        request.ReviewedAtUtc = DateTime.UtcNow;
        request.ReviewNotes = dto.Notes;
        request.CreatedUserId = user.Id;

        await AuditAsync(adminId, "admin.recruiter-request.approve", nameof(RecruiterAccessRequest), request.Id,
            $"Approved; created user {user.Id}");
        await _db.SaveChangesAsync();

        await _notifications.CreateAsync(
            user.Id,
            "RecruiterAccessApproved",
            "Recruiter access approved",
            "Your recruiter access request was approved. Sign in and change your temporary password.",
            nameof(RecruiterAccessRequest),
            request.Id,
            "/login");

        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> RejectRecruiterRequestAsync(int id, string? notes)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error);
        var request = await _db.RecruiterAccessRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request is null) return (false, "Request not found.");
        if (request.Status != RecruiterRequestStatus.Pending) return (false, "Only pending requests can be rejected.");
        if (string.IsNullOrWhiteSpace(notes)) return (false, "A rejection reason is required.");

        request.Status = RecruiterRequestStatus.Rejected;
        request.ReviewedByUserId = adminId;
        request.ReviewedAtUtc = DateTime.UtcNow;
        request.ReviewNotes = notes.Trim();
        await AuditAsync(adminId, "admin.recruiter-request.reject", nameof(RecruiterAccessRequest), request.Id, notes.Trim());
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<AdminOrganizationDto>? Result)> ListOrganizationsAsync()
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var orgs = await _db.Organizations.AsNoTracking().OrderBy(o => o.Name).ToListAsync();
        var result = new List<AdminOrganizationDto>();
        foreach (var o in orgs) result.Add(await MapOrgAsync(o));
        return (true, null, result);
    }

    public async Task<(bool Ok, string? Error, AdminOrganizationDto? Result)> GetOrganizationAsync(int id)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var o = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (o is null) return (false, "Organization not found.", null);
        return (true, null, await MapOrgAsync(o));
    }

    private async Task<AdminOrganizationDto> MapOrgAsync(Organization o)
    {
        var deptCount = await _db.Departments.CountAsync(d => d.OrganizationId == o.Id);
        var userCount = await _db.RecruiterProfiles.CountAsync(p => p.OrganizationId == o.Id)
            + await _db.HiringManagerProfiles.CountAsync(p => p.OrganizationId == o.Id);
        return new AdminOrganizationDto
        {
            Id = o.Id,
            Name = o.Name,
            Code = o.Code,
            Description = o.Description,
            Website = o.Website,
            ContactEmail = o.ContactEmail,
            Address = o.Address,
            TimeZoneId = o.TimeZoneId,
            DefaultCurrency = o.DefaultCurrency,
            Status = o.Status,
            DepartmentCount = deptCount,
            UserCount = userCount,
            CreatedAtUtc = o.CreatedAtUtc
        };
    }

    public async Task<(bool Ok, string? Error, AdminOrganizationDto? Result)> CreateOrganizationAsync(UpsertOrganizationDto dto)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error, null);
        var code = (dto.Code ?? string.Empty).Trim().ToUpperInvariant();
        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
            return (false, "Name and code are required.", null);
        if (await _db.Organizations.AnyAsync(o => o.Code == code))
            return (false, "Organization code must be unique.", null);

        var org = new Organization
        {
            Name = name,
            Code = code,
            Description = dto.Description,
            Website = dto.Website,
            ContactEmail = dto.ContactEmail,
            Address = dto.Address,
            TimeZoneId = string.IsNullOrWhiteSpace(dto.TimeZoneId) ? "UTC" : dto.TimeZoneId.Trim(),
            DefaultCurrency = dto.DefaultCurrency,
            Status = OrganizationStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Organizations.Add(org);
        await AuditAsync(adminId, "admin.organization.create", nameof(Organization), null, code);
        await _db.SaveChangesAsync();
        return (true, null, await MapOrgAsync(org));
    }

    public async Task<(bool Ok, string? Error, AdminOrganizationDto? Result)> UpdateOrganizationAsync(int id, UpsertOrganizationDto dto)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error, null);
        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == id);
        if (org is null) return (false, "Organization not found.", null);
        var code = (dto.Code ?? string.Empty).Trim().ToUpperInvariant();
        if (await _db.Organizations.AnyAsync(o => o.Code == code && o.Id != id))
            return (false, "Organization code must be unique.", null);

        org.Name = dto.Name.Trim();
        org.Code = code;
        org.Description = dto.Description;
        org.Website = dto.Website;
        org.ContactEmail = dto.ContactEmail;
        org.Address = dto.Address;
        org.TimeZoneId = string.IsNullOrWhiteSpace(dto.TimeZoneId) ? org.TimeZoneId : dto.TimeZoneId.Trim();
        org.DefaultCurrency = dto.DefaultCurrency;
        org.UpdatedAtUtc = DateTime.UtcNow;
        await AuditAsync(adminId, "admin.organization.update", nameof(Organization), id, code);
        await _db.SaveChangesAsync();
        return (true, null, await MapOrgAsync(org));
    }

    public async Task<(bool Ok, string? Error)> UpdateOrganizationStatusAsync(int id, string status)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error);
        if (!Enum.TryParse<OrganizationStatus>(status, true, out var parsed)) return (false, "Invalid organization status.");
        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == id);
        if (org is null) return (false, "Organization not found.");
        org.Status = parsed;
        org.UpdatedAtUtc = DateTime.UtcNow;
        await AuditAsync(adminId, "admin.organization.status", nameof(Organization), id, parsed.ToString());
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<AdminDepartmentDto>? Result)> ListDepartmentsAsync(int? organizationId)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var q = _db.Departments.AsNoTracking().Include(d => d.Organization).AsQueryable();
        if (organizationId is int oid) q = q.Where(d => d.OrganizationId == oid);
        var list = await q.OrderBy(d => d.Name).ToListAsync();
        var result = new List<AdminDepartmentDto>();
        foreach (var d in list) result.Add(await MapDeptAsync(d));
        return (true, null, result);
    }

    private async Task<AdminDepartmentDto> MapDeptAsync(Department d)
    {
        var userCount = await _db.RecruiterProfiles.CountAsync(p => p.DepartmentId == d.Id)
            + await _db.HiringManagerProfiles.CountAsync(p => p.DepartmentId == d.Id);
        var jobCount = await _db.Jobs.CountAsync(j => j.DepartmentId == d.Id);
        return new AdminDepartmentDto
        {
            Id = d.Id,
            OrganizationId = d.OrganizationId,
            OrganizationName = d.Organization?.Name ?? string.Empty,
            Name = d.Name,
            Code = d.Code,
            Description = d.Description,
            Status = d.Status,
            ManagerUserId = d.ManagerUserId,
            UserCount = userCount,
            JobCount = jobCount,
            CreatedAtUtc = d.CreatedAtUtc
        };
    }

    public async Task<(bool Ok, string? Error, AdminDepartmentDto? Result)> CreateDepartmentAsync(UpsertDepartmentDto dto)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error, null);
        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == dto.OrganizationId);
        if (org is null) return (false, "Organization not found.", null);
        if (org.Status != OrganizationStatus.Active) return (false, "Cannot create departments under a non-active organization.", null);
        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name)) return (false, "Department name is required.", null);
        if (await _db.Departments.AnyAsync(d => d.OrganizationId == dto.OrganizationId && d.Name == name))
            return (false, "Department name must be unique within the organization.", null);

        var dept = new Department
        {
            OrganizationId = dto.OrganizationId,
            Name = name,
            Code = dto.Code,
            Description = dto.Description,
            ManagerUserId = dto.ManagerUserId,
            Status = DepartmentStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Departments.Add(dept);
        await AuditAsync(adminId, "admin.department.create", nameof(Department), null, name);
        await _db.SaveChangesAsync();
        dept.Organization = org;
        return (true, null, await MapDeptAsync(dept));
    }

    public async Task<(bool Ok, string? Error, AdminDepartmentDto? Result)> UpdateDepartmentAsync(int id, UpsertDepartmentDto dto)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error, null);
        var dept = await _db.Departments.Include(d => d.Organization).FirstOrDefaultAsync(d => d.Id == id);
        if (dept is null) return (false, "Department not found.", null);
        if (dto.OrganizationId != dept.OrganizationId)
            return (false, "Cross-organization department reassignment is blocked.", null);

        var name = (dto.Name ?? string.Empty).Trim();
        if (await _db.Departments.AnyAsync(d => d.OrganizationId == dept.OrganizationId && d.Name == name && d.Id != id))
            return (false, "Department name must be unique within the organization.", null);

        dept.Name = name;
        dept.Code = dto.Code;
        dept.Description = dto.Description;
        dept.ManagerUserId = dto.ManagerUserId;
        dept.UpdatedAtUtc = DateTime.UtcNow;
        await AuditAsync(adminId, "admin.department.update", nameof(Department), id, name);
        await _db.SaveChangesAsync();
        return (true, null, await MapDeptAsync(dept));
    }

    public async Task<(bool Ok, string? Error)> UpdateDepartmentStatusAsync(int id, string status)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error);
        if (!Enum.TryParse<DepartmentStatus>(status, true, out var parsed)) return (false, "Invalid department status.");
        var dept = await _db.Departments.FirstOrDefaultAsync(d => d.Id == id);
        if (dept is null) return (false, "Department not found.");
        dept.Status = parsed;
        dept.UpdatedAtUtc = DateTime.UtcNow;
        await AuditAsync(adminId, "admin.department.status", nameof(Department), id, parsed.ToString());
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> AssignHiringManagerAsync(AssignHiringManagerDto dto)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error);
        var (ok, err) = await AddUserRoleAsync(dto.UserId, "HiringManager");
        if (!ok) return (false, err);
        if (dto.OrganizationId is not null || dto.DepartmentId is not null)
        {
            var (ok2, err2) = await AssignOrganizationAsync(dto.UserId, dto.OrganizationId, dto.DepartmentId);
            if (!ok2) return (false, err2);
        }

        if (dto.JobId is int jobId)
        {
            var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job is null) return (false, "Job not found.");
            job.HiringManagerUserId = dto.UserId;
            job.UpdatedAtUtc = DateTime.UtcNow;
            await AuditAsync(adminId, "admin.job.hiring-manager", nameof(Job), jobId, $"HM={dto.UserId}");
            await _db.SaveChangesAsync();
        }

        return (true, null);
    }
}
