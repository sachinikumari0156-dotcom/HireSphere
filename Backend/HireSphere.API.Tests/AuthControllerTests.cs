using System.Net;
using System.Net.Http.Json;
using HireSphere.API.Data;
using HireSphere.API.DTOs;
using HireSphere.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HireSphere.API.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=hiresphere-tests;Mode=Memory;Cache=Shared");

    public TestWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["Jwt:Key"] = "IntegrationTestJwtSigningKeyMustBeAtLeast32BytesLong!",
                ["Jwt:Issuer"] = "HireSphereAPI",
                ["Jwt:Audience"] = "HireSphereUsers"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            if (!db.Roles.Any(r => r.Name == "Candidate"))
            {
                db.Roles.Add(new Role { Name = "Candidate", Description = "Candidate role" });
                db.SaveChanges();
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}

public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PrivilegedRoleRegistration_IsBlocked()
    {
        var dto = new RegisterDto
        {
            FullName = "Admin Attempt",
            Email = "admin-attempt@example.com",
            Password = "SecurePass123!",
            Role = "Admin"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/Register", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
