using HireSphere.API.Controllers;
using HireSphere.API.DTOs;
using HireSphere.API.Models;
using Microsoft.AspNetCore.Mvc;
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

            var controller = new AuthController(context, configuration);
            var dto = new RegisterDto
            {
                FullName = "New Candidate",
                Email = $"candidate-{Guid.NewGuid():N}@example.com",
                Password = "SecurePass123!",
                Role = "Candidate"
            };

            var result = await controller.Register(dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, context.Users.Count());
        }
    }
}
