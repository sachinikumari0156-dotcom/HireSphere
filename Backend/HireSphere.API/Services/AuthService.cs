using HireSphere.API.Data;
using HireSphere.API.DTOs.Auth;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordService _passwords;
    private readonly ITokenService _tokens;

    public AuthService(
        ApplicationDbContext db,
        IPasswordService passwords,
        ITokenService tokens)
    {
        _db = db;
        _passwords = passwords;
        _tokens = tokens;
    }

    public async Task<(bool Ok, string? Error, AuthResponseDto? Result)> RegisterCandidateAsync(
        CandidateRegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
        {
            return (false, "First name and last name are required.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
        {
            return (false, "A valid email address is required.", null);
        }

        if (!dto.AcceptTerms)
        {
            return (false, "You must accept the privacy terms to register.", null);
        }

        if (!string.Equals(dto.Password, dto.ConfirmPassword, StringComparison.Ordinal))
        {
            return (false, "Password confirmation does not match.", null);
        }

        if (!_passwords.MeetsPolicy(dto.Password, out var policyError))
        {
            return (false, policyError, null);
        }

        var normalizedEmail = dto.Email.Trim().ToUpperInvariant();
        if (await _db.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
        {
            return (false, "Email already exists", null);
        }

        var fullName = $"{dto.FirstName.Trim()} {dto.LastName.Trim()}".Trim();
        var user = new User
        {
            FullName = fullName,
            Email = dto.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PasswordHash = _passwords.Hash(dto.Password),
            Role = "Candidate",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var candidateRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Candidate");
        if (candidateRole != null)
        {
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = candidateRole.Id });
        }

        _db.CandidateProfiles.Add(new CandidateProfile
        {
            UserId = user.Id,
            FullName = fullName,
            CreatedAtUtc = DateTime.UtcNow
        });

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "candidate.register",
            EntityType = nameof(User),
            EntityId = user.Id,
            Details = "Candidate self-registration",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        var permissions = await GetPermissionsAsync(user.Id);
        var token = _tokens.CreateAccessToken(user, permissions);

        return (true, null, new AuthResponseDto
        {
            Message = "Registration successful",
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Token = token
        });
    }

    public async Task<(bool Ok, string? Error, AuthResponseDto? Result)> LoginAsync(LoginRequestDto dto)
    {
        const string invalid = "Invalid email or password.";

        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return (false, invalid, null);
        }

        var normalizedEmail = dto.Email.Trim().ToUpperInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

        if (user == null || !_passwords.Verify(dto.Password, user.PasswordHash))
        {
            return (false, invalid, null);
        }

        if (user.Status is UserStatus.Inactive or UserStatus.Suspended)
        {
            return (false, "This account is disabled.", null);
        }

        if (user.Status == UserStatus.PendingApproval)
        {
            return (false, "This account is pending approval.", null);
        }

        var (organizationId, departmentId) = await ResolveOrgScopeAsync(user);
        var permissions = await GetPermissionsAsync(user.Id);
        var token = _tokens.CreateAccessToken(user, permissions, organizationId, departmentId);

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "auth.login",
            EntityType = nameof(User),
            EntityId = user.Id,
            Details = "Successful login",
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return (true, null, new AuthResponseDto
        {
            Message = "Login successful",
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Token = token,
            OrganizationId = organizationId,
            DepartmentId = departmentId
        });
    }

    public async Task<(bool Ok, string? Error, CurrentUserDto? Result)> GetCurrentUserAsync(int userId)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return (false, "User not found.", null);
        }

        var (organizationId, departmentId) = await ResolveOrgScopeAsync(user);
        var permissions = await GetPermissionsAsync(user.Id);

        return (true, null, new CurrentUserDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Status = user.Status.ToString(),
            OrganizationId = organizationId,
            DepartmentId = departmentId,
            Permissions = permissions
        });
    }

    public async Task<(bool Ok, string? Error)> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        if (!string.Equals(dto.NewPassword, dto.ConfirmPassword, StringComparison.Ordinal))
        {
            return (false, "Password confirmation does not match.");
        }

        if (!_passwords.MeetsPolicy(dto.NewPassword, out var policyError))
        {
            return (false, policyError);
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return (false, "User not found.");
        }

        if (!_passwords.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            return (false, "Current password is incorrect.");
        }

        user.PasswordHash = _passwords.Hash(dto.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "auth.change-password",
            EntityType = nameof(User),
            EntityId = userId,
            Details = "Password changed by account owner",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task LogLogoutAsync(int userId)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "auth.logout",
            EntityType = nameof(User),
            EntityId = userId,
            Details = "Client logout recorded",
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<(bool Ok, string? Error, RecruiterAccessRequestDto? Result)> SubmitRecruiterRequestAsync(
        CreateRecruiterAccessRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName)
            || string.IsNullOrWhiteSpace(dto.BusinessEmail)
            || string.IsNullOrWhiteSpace(dto.OrganizationName))
        {
            return (false, "Full name, business email, and organization name are required.", null);
        }

        var normalized = dto.BusinessEmail.Trim().ToUpperInvariant();
        var pendingExists = await _db.RecruiterAccessRequests.AnyAsync(r =>
            r.NormalizedBusinessEmail == normalized && r.Status == RecruiterRequestStatus.Pending);

        if (pendingExists)
        {
            return (false, "A pending recruiter access request already exists for this email.", null);
        }

        if (await _db.Users.AnyAsync(u => u.NormalizedEmail == normalized))
        {
            return (false, "An account with this email already exists.", null);
        }

        var request = new RecruiterAccessRequest
        {
            FullName = dto.FullName.Trim(),
            BusinessEmail = dto.BusinessEmail.Trim(),
            NormalizedBusinessEmail = normalized,
            OrganizationName = dto.OrganizationName.Trim(),
            Message = dto.Message?.Trim(),
            Status = RecruiterRequestStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.RecruiterAccessRequests.Add(request);
        await _db.SaveChangesAsync();

        return (true, null, MapRequest(request));
    }

    private async Task<IReadOnlyList<string>> GetPermissionsAsync(int userId)
    {
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync();
    }

    private async Task<(int? OrganizationId, int? DepartmentId)> ResolveOrgScopeAsync(User user)
    {
        if (user.Role == "Recruiter")
        {
            var profile = await _db.RecruiterProfiles.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            return (profile?.OrganizationId, profile?.DepartmentId);
        }

        if (user.Role == "HiringManager")
        {
            var profile = await _db.HiringManagerProfiles.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            return (profile?.OrganizationId, profile?.DepartmentId);
        }

        return (null, null);
    }

    internal static RecruiterAccessRequestDto MapRequest(RecruiterAccessRequest request) =>
        new()
        {
            Id = request.Id,
            FullName = request.FullName,
            BusinessEmail = request.BusinessEmail,
            OrganizationName = request.OrganizationName,
            Message = request.Message,
            Status = request.Status.ToString(),
            CreatedAtUtc = request.CreatedAtUtc,
            ReviewedAtUtc = request.ReviewedAtUtc,
            ReviewNotes = request.ReviewNotes
        };
}
