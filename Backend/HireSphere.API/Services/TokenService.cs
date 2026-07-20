using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HireSphere.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace HireSphere.API.Services;

public interface ITokenService
{
    string CreateAccessToken(User user, IEnumerable<string>? permissions = null, int? organizationId = null, int? departmentId = null);
}

public sealed class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateAccessToken(
        User user,
        IEnumerable<string>? permissions = null,
        int? organizationId = null,
        int? departmentId = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("uid", user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("name", user.FullName),
            new(ClaimTypes.Role, user.Role),
            new("role", user.Role)
        };

        if (organizationId.HasValue)
        {
            claims.Add(new Claim("org_id", organizationId.Value.ToString()));
        }

        if (departmentId.HasValue)
        {
            claims.Add(new Claim("dept_id", departmentId.Value.ToString()));
        }

        if (permissions != null)
        {
            foreach (var permission in permissions.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                claims.Add(new Claim("permission", permission));
            }
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var expireMinutes = int.TryParse(_configuration["Jwt:ExpireMinutes"], out var minutes)
            ? minutes
            : 120;

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
