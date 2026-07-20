using System.Reflection;
using System.Text.Json;
using HireSphere.API.DTOs;

namespace HireSphere.API.Tests;

public class UserDtoTests
{
    [Fact]
    public void UserDto_DoesNotExposePasswordHash()
    {
        var dto = new UserDto
        {
            Id = 1,
            FullName = "Test User",
            Email = "test@example.com",
            Role = "Candidate"
        };

        var json = JsonSerializer.Serialize(dto);
        Assert.DoesNotContain("PasswordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PasswordHash_NotInUserDtoType()
    {
        var passwordHashProperty = typeof(UserDto)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.Name.Equals("PasswordHash", StringComparison.OrdinalIgnoreCase));

        Assert.Null(passwordHashProperty);
    }
}
