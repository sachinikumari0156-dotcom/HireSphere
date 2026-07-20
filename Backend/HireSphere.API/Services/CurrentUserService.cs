using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HireSphere.API.Services;

public interface ICurrentUserService
{
    int? UserId { get; }

    string? Email { get; }

    string? Role { get; }

    int? OrganizationId { get; }

    int? DepartmentId { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);
}

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true;

    public int? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue("uid")
                ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email =>
        Principal?.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? Principal?.FindFirstValue(ClaimTypes.Email)
        ?? Principal?.FindFirstValue("email");

    public string? Role =>
        Principal?.FindFirstValue("role")
        ?? Principal?.FindFirstValue(ClaimTypes.Role);

    public int? OrganizationId =>
        int.TryParse(Principal?.FindFirstValue("org_id"), out var id) ? id : null;

    public int? DepartmentId =>
        int.TryParse(Principal?.FindFirstValue("dept_id"), out var id) ? id : null;

    public bool IsInRole(string role) =>
        Principal?.IsInRole(role) == true
        || string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
}
