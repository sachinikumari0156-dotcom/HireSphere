using HireSphere.API.Data;
using HireSphere.API.DTOs.Auth;
using HireSphere.API.Models;
using HireSphere.API.Services;
using Microsoft.Extensions.Configuration;

namespace HireSphere.API.Tests;

public class AuthControllerUnitTests
{
    [Fact]
    public async Task CandidateRegistration_Succeeds()
    {
        var (context, connection) = TestDbFactory.CreateContext();
        await using (context)
        await using (connection)
        {
            context.Roles.Add(new Role { Name = "Candidate", Description = "Candidate role" });
            await context.SaveChangesAsync();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "IntegrationTestJwtSigningKeyMustBeAtLeast32BytesLong!",
                    ["Jwt:Issuer"] = "HireSphereAPI",
                    ["Jwt:Audience"] = "HireSphereUsers"
                })
                .Build();

            var auth = new AuthService(context, new PasswordService(), new TokenService(configuration));
            var (ok, error, result) = await auth.RegisterCandidateAsync(new CandidateRegisterDto
            {
                FirstName = "New",
                LastName = "Candidate",
                Email = $"candidate-{Guid.NewGuid():N}@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                AcceptTerms = true
            });

            Assert.True(ok, error);
            Assert.NotNull(result);
            Assert.Equal(1, context.Users.Count());
            Assert.Equal("Candidate", result!.Role);
        }
    }

    [Fact]
    public async Task PrivilegedRoleCannotBeSuppliedByClient()
    {
        var (context, connection) = TestDbFactory.CreateContext();
        await using (context)
        await using (connection)
        {
            context.Roles.Add(new Role { Name = "Candidate", Description = "Candidate role" });
            await context.SaveChangesAsync();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "IntegrationTestJwtSigningKeyMustBeAtLeast32BytesLong!",
                    ["Jwt:Issuer"] = "HireSphereAPI",
                    ["Jwt:Audience"] = "HireSphereUsers"
                })
                .Build();

            var auth = new AuthService(context, new PasswordService(), new TokenService(configuration));
            var (_, _, result) = await auth.RegisterCandidateAsync(new CandidateRegisterDto
            {
                FirstName = "No",
                LastName = "Escalation",
                Email = $"safe-{Guid.NewGuid():N}@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                AcceptTerms = true
            });

            Assert.Equal("Candidate", result!.Role);
            Assert.All(context.Users, u => Assert.Equal("Candidate", u.Role));
        }
    }
}
