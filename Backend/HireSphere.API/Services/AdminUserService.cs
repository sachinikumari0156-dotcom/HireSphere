using HireSphere.API.Data;
using HireSphere.API.DTOs.Auth;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public sealed class AdminUserService : IAdminUserService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordService _passwords;

    public AdminUserService(ApplicationDbContext db, IPasswordService passwords)
    {
        _db = db;
        _passwords = passwords;
    }

    public async Task<IReadOnlyList<RecruiterAccessRequestDto>> ListRecruiterRequestsAsync()
    {
        var items = await _db.RecruiterAccessRequests
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync();

        return items.Select(AuthService.MapRequest).ToList();
    }

    public async Task<(bool Ok, string? Error)> ApproveRecruiterRequestAsync(
        int requestId,
        int adminUserId,
        string? notes)
    {
        var request = await _db.RecruiterAccessRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
        {
            return (false, "Request not found.");
        }

        if (request.Status != RecruiterRequestStatus.Pending)
        {
            return (false, "Only pending requests can be approved.");
        }

        if (await _db.Users.AnyAsync(u => u.NormalizedEmail == request.NormalizedBusinessEmail))
        {
            return (false, "A user with this email already exists.");
        }

        // Temporary password must be changed on first login via change-password.
        // Generate a random password; do not log or return it in this coursework prototype.
        var temporaryPassword = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "Aa1!";
        var user = new User
        {
            FullName = request.FullName,
            Email = request.BusinessEmail,
            NormalizedEmail = request.NormalizedBusinessEmail,
            PasswordHash = _passwords.Hash(temporaryPassword),
            Role = "Recruiter",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Name == request.OrganizationName)
            ?? new Organization
            {
                Name = request.OrganizationName,
                Description = "Created from recruiter access approval",
                CreatedAtUtc = DateTime.UtcNow
            };

        if (org.Id == 0)
        {
            _db.Organizations.Add(org);
            await _db.SaveChangesAsync();
        }

        _db.RecruiterProfiles.Add(new RecruiterProfile
        {
            UserId = user.Id,
            OrganizationId = org.Id,
            Title = "Recruiter",
            CreatedAtUtc = DateTime.UtcNow
        });

        var recruiterRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Recruiter");
        if (recruiterRole != null)
        {
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = recruiterRole.Id });
        }

        request.Status = RecruiterRequestStatus.Approved;
        request.ReviewedByUserId = adminUserId;
        request.ReviewedAtUtc = DateTime.UtcNow;
        request.ReviewNotes = notes;
        request.CreatedUserId = user.Id;

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "admin.recruiter-request.approve",
            EntityType = nameof(RecruiterAccessRequest),
            EntityId = request.Id,
            Details = $"Approved recruiter request; created user {user.Id}",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> RejectRecruiterRequestAsync(
        int requestId,
        int adminUserId,
        string? notes)
    {
        var request = await _db.RecruiterAccessRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
        {
            return (false, "Request not found.");
        }

        if (request.Status != RecruiterRequestStatus.Pending)
        {
            return (false, "Only pending requests can be rejected.");
        }

        request.Status = RecruiterRequestStatus.Rejected;
        request.ReviewedByUserId = adminUserId;
        request.ReviewedAtUtc = DateTime.UtcNow;
        request.ReviewNotes = notes;

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "admin.recruiter-request.reject",
            EntityType = nameof(RecruiterAccessRequest),
            EntityId = request.Id,
            Details = "Rejected recruiter access request",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<IReadOnlyList<CurrentUserDto>> ListUsersAsync()
    {
        var users = await _db.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync();
        var result = new List<CurrentUserDto>();
        foreach (var user in users)
        {
            int? orgId = null;
            int? deptId = null;
            if (user.Role == "Recruiter")
            {
                var profile = await _db.RecruiterProfiles.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);
                orgId = profile?.OrganizationId;
                deptId = profile?.DepartmentId;
            }
            else if (user.Role == "HiringManager")
            {
                var profile = await _db.HiringManagerProfiles.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);
                orgId = profile?.OrganizationId;
                deptId = profile?.DepartmentId;
            }

            result.Add(new CurrentUserDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status.ToString(),
                OrganizationId = orgId,
                DepartmentId = deptId
            });
        }

        return result;
    }

    public async Task<(bool Ok, string? Error)> UpdateUserStatusAsync(
        int targetUserId,
        int adminUserId,
        string status)
    {
        if (!Enum.TryParse<UserStatus>(status, true, out var parsed))
        {
            return (false, "Invalid status value.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
        if (user == null)
        {
            return (false, "User not found.");
        }

        user.Status = parsed;
        user.UpdatedAtUtc = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "admin.user.status",
            EntityType = nameof(User),
            EntityId = targetUserId,
            Details = $"Status set to {parsed}",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> UpdateUserRoleAsync(
        int targetUserId,
        int adminUserId,
        string role)
    {
        var allowed = new[] { "Candidate", "Recruiter", "HiringManager", "Admin" };
        if (!allowed.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            return (false, "Invalid role.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
        if (user == null)
        {
            return (false, "User not found.");
        }

        var normalizedRole = allowed.First(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        user.Role = normalizedRole;
        user.UpdatedAtUtc = DateTime.UtcNow;

        var roleEntity = await _db.Roles.FirstOrDefaultAsync(r => r.Name == normalizedRole);
        if (roleEntity != null)
        {
            var existing = await _db.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync();
            _db.UserRoles.RemoveRange(existing);
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleEntity.Id });
        }

        if (normalizedRole == "HiringManager"
            && !await _db.HiringManagerProfiles.AnyAsync(p => p.UserId == user.Id))
        {
            _db.HiringManagerProfiles.Add(new HiringManagerProfile
            {
                UserId = user.Id,
                Title = "Hiring Manager",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "admin.user.role",
            EntityType = nameof(User),
            EntityId = targetUserId,
            Details = $"Role set to {normalizedRole}",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> UpdateUserOrganizationAsync(
        int targetUserId,
        int adminUserId,
        int? organizationId,
        int? departmentId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
        if (user == null)
        {
            return (false, "User not found.");
        }

        if (user.Role == "Recruiter")
        {
            var profile = await _db.RecruiterProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new RecruiterProfile { UserId = user.Id, CreatedAtUtc = DateTime.UtcNow };
                _db.RecruiterProfiles.Add(profile);
            }

            profile.OrganizationId = organizationId;
            profile.DepartmentId = departmentId;
            profile.UpdatedAtUtc = DateTime.UtcNow;
        }
        else if (user.Role == "HiringManager")
        {
            var profile = await _db.HiringManagerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new HiringManagerProfile { UserId = user.Id, CreatedAtUtc = DateTime.UtcNow };
                _db.HiringManagerProfiles.Add(profile);
            }

            profile.OrganizationId = organizationId;
            profile.DepartmentId = departmentId;
            profile.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            return (false, "Organization assignment applies to Recruiter or HiringManager accounts.");
        }

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "admin.user.organization",
            EntityType = nameof(User),
            EntityId = targetUserId,
            Details = $"OrganizationId={organizationId}; DepartmentId={departmentId}",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return (true, null);
    }
}
