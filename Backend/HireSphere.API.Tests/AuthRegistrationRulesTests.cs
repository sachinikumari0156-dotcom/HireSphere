using HireSphere.API.Services;

namespace HireSphere.API.Tests;

public class AuthRegistrationRulesTests
{
    [Theory]
    [InlineData("Admin", false)]
    [InlineData("Recruiter", false)]
    [InlineData("HiringManager", false)]
    [InlineData("Candidate", true)]
    [InlineData("candidate", true)]
    public void PrivilegedRoleRegistration_IsBlocked(string role, bool allowed)
    {
        Assert.Equal(allowed, AuthRegistrationRules.IsPublicRegistrationAllowed(role));
    }
}
