using HireSphere.API.DTOs.Auth;
using HireSphere.API.Models;

namespace HireSphere.API.Services;

public interface IAuthService
{
    Task<(bool Ok, string? Error, AuthResponseDto? Result)> RegisterCandidateAsync(CandidateRegisterDto dto);

    Task<(bool Ok, string? Error, AuthResponseDto? Result)> LoginAsync(LoginRequestDto dto);

    Task<(bool Ok, string? Error, CurrentUserDto? Result)> GetCurrentUserAsync(int userId);

    Task<(bool Ok, string? Error)> ChangePasswordAsync(int userId, ChangePasswordDto dto);

    Task LogLogoutAsync(int userId);

    Task<(bool Ok, string? Error, RecruiterAccessRequestDto? Result)> SubmitRecruiterRequestAsync(CreateRecruiterAccessRequestDto dto);
}

public interface IAdminUserService
{
    Task<IReadOnlyList<RecruiterAccessRequestDto>> ListRecruiterRequestsAsync();

    Task<(bool Ok, string? Error)> ApproveRecruiterRequestAsync(int requestId, int adminUserId, string? notes);

    Task<(bool Ok, string? Error)> RejectRecruiterRequestAsync(int requestId, int adminUserId, string? notes);

    Task<IReadOnlyList<CurrentUserDto>> ListUsersAsync();

    Task<(bool Ok, string? Error)> UpdateUserStatusAsync(int targetUserId, int adminUserId, string status);

    Task<(bool Ok, string? Error)> UpdateUserRoleAsync(int targetUserId, int adminUserId, string role);

    Task<(bool Ok, string? Error)> UpdateUserOrganizationAsync(int targetUserId, int adminUserId, int? organizationId, int? departmentId);
}

public interface IResourceAuthorizationService
{
    bool RequireSelf(int resourceOwnerUserId);

    Task<bool> CandidateOwnsProfileAsync(int profileId);

    Task<bool> RecruiterOwnsJobAsync(int jobId);

    Task EnsureCandidateOwnsApplicationAsync(int applicationId);
}
