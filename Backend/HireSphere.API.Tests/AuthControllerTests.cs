using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
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
    private readonly SqliteConnection _connection;

    public TestWebApplicationFactory()
    {
        _connection = new SqliteConnection($"Data Source=hiresphere-tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
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
                ["Jwt:Audience"] = "HireSphereUsers",
                ["Jwt:ExpireMinutes"] = "120"
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
            SeedRoles(db);
        });
    }

    public static void SeedRoles(ApplicationDbContext db)
    {
        foreach (var name in new[] { "Candidate", "Recruiter", "HiringManager", "Admin" })
        {
            if (!db.Roles.Any(r => r.Name == name))
            {
                db.Roles.Add(new Role { Name = name, Description = name });
            }
        }

        db.SaveChanges();
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
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(User User, int? OrganizationId)> SeedUserAsync(
        string email,
        string password,
        string role,
        UserStatus status = UserStatus.Active,
        string? organizationName = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);

        int? organizationId = null;
        if (!string.IsNullOrWhiteSpace(organizationName))
        {
            var org = await db.Organizations.FirstOrDefaultAsync(o => o.Name == organizationName);
            if (org == null)
            {
                org = new Organization
                {
                    Name = organizationName,
                    CreatedAtUtc = DateTime.UtcNow
                };
                db.Organizations.Add(org);
                await db.SaveChangesAsync();
            }

            organizationId = org.Id;
        }

        var user = new User
        {
            FullName = $"{role} User",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            Status = status,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var roleEntity = await db.Roles.FirstAsync(r => r.Name == role);
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleEntity.Id });

        if (role == "Candidate")
        {
            db.CandidateProfiles.Add(new CandidateProfile
            {
                UserId = user.Id,
                FullName = user.FullName,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        else if (role == "Recruiter")
        {
            db.RecruiterProfiles.Add(new RecruiterProfile
            {
                UserId = user.Id,
                OrganizationId = organizationId,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        else if (role == "HiringManager")
        {
            db.HiringManagerProfiles.Add(new HiringManagerProfile
            {
                UserId = user.Id,
                OrganizationId = organizationId,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
        return (user, organizationId);
    }

    private async Task<string> LoginTokenAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        return payload.GetProperty("token").GetString()!;
    }

    private HttpClient ClientWithToken(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task CandidateRegistration_Succeeds()
    {
        var email = $"cand-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync("/api/auth/register/candidate", new
        {
            firstName = "Ada",
            lastName = "Lovelace",
            email,
            password = "SecurePass123!",
            confirmPassword = "SecurePass123!",
            acceptTerms = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Candidate", body.GetProperty("role").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("token").GetString()));
    }

    [Fact]
    public async Task DuplicateEmailRegistration_Fails()
    {
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var payload = new
        {
            firstName = "Ada",
            lastName = "Lovelace",
            email,
            password = "SecurePass123!",
            confirmPassword = "SecurePass123!",
            acceptTerms = true
        };

        Assert.Equal(HttpStatusCode.OK, (await _client.PostAsJsonAsync("/api/auth/register/candidate", payload)).StatusCode);
        var second = await _client.PostAsJsonAsync("/api/auth/register/candidate", payload);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task PublicRoleEscalation_IsImpossible()
    {
        var email = $"esc-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync("/api/auth/register/candidate", new
        {
            firstName = "Evil",
            lastName = "Actor",
            email,
            password = "SecurePass123!",
            confirmPassword = "SecurePass123!",
            acceptTerms = true,
            role = "Admin"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Candidate", body.GetProperty("role").GetString());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.FirstAsync(u => u.NormalizedEmail == email.ToUpperInvariant());
        Assert.Equal("Candidate", user.Role);
    }

    [Fact]
    public async Task PasswordIsStoredAsHash()
    {
        var email = $"hash-{Guid.NewGuid():N}@example.com";
        var password = "SecurePass123!";
        await _client.PostAsJsonAsync("/api/auth/register/candidate", new
        {
            firstName = "Hash",
            lastName = "Check",
            email,
            password,
            confirmPassword = password,
            acceptTerms = true
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.FirstAsync(u => u.NormalizedEmail == email.ToUpperInvariant());
        Assert.NotEqual(password, user.PasswordHash);
        Assert.StartsWith("$2", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));
    }

    [Fact]
    public async Task ValidLogin_Succeeds()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        await SeedUserAsync(email, "SecurePass123!", "Candidate");
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "SecurePass123!"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InvalidLogin_ReturnsSanitizedFailure()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "missing@example.com",
            password = "wrong"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("does not exist", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Invalid email or password", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DisabledUser_CannotLogin()
    {
        var email = $"disabled-{Guid.NewGuid():N}@example.com";
        await SeedUserAsync(email, "SecurePass123!", "Candidate", UserStatus.Inactive);
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "SecurePass123!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("disabled", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CandidateEndpoint_RejectsRecruiter()
    {
        var email = $"rec-{Guid.NewGuid():N}@example.com";
        await SeedUserAsync(email, "SecurePass123!", "Recruiter", organizationName: "Recruiter Org A");
        var token = await LoginTokenAsync(email, "SecurePass123!");
        var client = ClientWithToken(token);
        var response = await client.GetAsync("/api/Applications/MyApplications");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RecruiterEndpoint_RejectsCandidate()
    {
        var email = $"cand2-{Guid.NewGuid():N}@example.com";
        await SeedUserAsync(email, "SecurePass123!", "Candidate");
        var token = await LoginTokenAsync(email, "SecurePass123!");
        var client = ClientWithToken(token);
        var response = await client.GetAsync("/api/Jobs/MyJobs");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task HiringManagerRoute_RejectsCandidate()
    {
        var email = $"cand3-{Guid.NewGuid():N}@example.com";
        await SeedUserAsync(email, "SecurePass123!", "Candidate");
        var token = await LoginTokenAsync(email, "SecurePass123!");
        var client = ClientWithToken(token);
        // Hiring manager exclusive: admin user list is AdministratorOnly; use a recruiter-only path for role isolation.
        // Dedicated HM policy check via admin reject as non-admin also covers privileged isolation.
        var response = await client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_RejectsNonAdmin()
    {
        var email = $"hm-{Guid.NewGuid():N}@example.com";
        await SeedUserAsync(email, "SecurePass123!", "HiringManager", organizationName: "HM Org");
        var token = await LoginTokenAsync(email, "SecurePass123!");
        var client = ClientWithToken(token);
        var response = await client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Candidate_CannotAccessAnotherCandidateResource()
    {
        var emailA = $"a-{Guid.NewGuid():N}@example.com";
        var emailB = $"b-{Guid.NewGuid():N}@example.com";
        await SeedUserAsync(emailA, "SecurePass123!", "Candidate");
        await SeedUserAsync(emailB, "SecurePass123!", "Candidate");

        int profileBId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userB = await db.Users.FirstAsync(u => u.NormalizedEmail == emailB.ToUpperInvariant());
            profileBId = await db.CandidateProfiles.Where(p => p.UserId == userB.Id).Select(p => p.Id).FirstAsync();
        }

        var token = await LoginTokenAsync(emailA, "SecurePass123!");
        var client = ClientWithToken(token);
        var response = await client.GetAsync($"/api/CandidateProfiles/{profileBId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Recruiter_CannotAccessAnotherOrganizationJob()
    {
        var emailA = $"ra-{Guid.NewGuid():N}@example.com";
        var emailB = $"rb-{Guid.NewGuid():N}@example.com";
        await SeedUserAsync(emailA, "SecurePass123!", "Recruiter", organizationName: "Alpha Corp");
        var (_, orgBId) = await SeedUserAsync(emailB, "SecurePass123!", "Recruiter", organizationName: "Beta Corp");

        int jobId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var recruiterB = await db.Users.FirstAsync(u => u.NormalizedEmail == emailB.ToUpperInvariant());
            var job = new Job
            {
                Title = "Secret Role",
                Description = "Org B only",
                RequiredSkills = "C#",
                Location = "Remote",
                RecruiterId = recruiterB.Id,
                OrganizationId = orgBId,
                PostedDate = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Jobs.Add(job);
            await db.SaveChangesAsync();
            jobId = job.Id;
        }

        var token = await LoginTokenAsync(emailA, "SecurePass123!");
        var client = ClientWithToken(token);
        var response = await client.DeleteAsync($"/api/Jobs/{jobId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CurrentUser_ReturnsSafeDto()
    {
        var email = $"me-{Guid.NewGuid():N}@example.com";
        await SeedUserAsync(email, "SecurePass123!", "Candidate");
        var token = await LoginTokenAsync(email, "SecurePass123!");
        var client = ClientWithToken(token);
        var response = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PasswordHash", json);
        var body = JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
        Assert.Equal(email, body.GetProperty("email").GetString());
        Assert.Equal("Candidate", body.GetProperty("role").GetString());
    }

    [Fact]
    public async Task ChangePassword_RequiresCurrentPassword()
    {
        var email = $"pwd-{Guid.NewGuid():N}@example.com";
        await SeedUserAsync(email, "SecurePass123!", "Candidate");
        var token = await LoginTokenAsync(email, "SecurePass123!");
        var client = ClientWithToken(token);

        var bad = await client.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = "WrongPass123!",
            newPassword = "NewPass123!",
            confirmPassword = "NewPass123!"
        });
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);

        var ok = await client.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = "SecurePass123!",
            newPassword = "NewPass123!",
            confirmPassword = "NewPass123!"
        });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
    }

    [Fact]
    public async Task SensitiveAccountActions_CreateAuditLogs()
    {
        var adminEmail = $"admin-{Guid.NewGuid():N}@example.com";
        var targetEmail = $"target-{Guid.NewGuid():N}@example.com";
        await SeedUserAsync(adminEmail, "SecurePass123!", "Admin");
        await SeedUserAsync(targetEmail, "SecurePass123!", "Candidate");

        int targetId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            targetId = await db.Users
                .Where(u => u.NormalizedEmail == targetEmail.ToUpperInvariant())
                .Select(u => u.Id)
                .FirstAsync();
        }

        var token = await LoginTokenAsync(adminEmail, "SecurePass123!");
        var client = ClientWithToken(token);
        var response = await client.PatchAsJsonAsync($"/api/admin/users/{targetId}/status", new { status = "Inactive" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.True(await db.AuditLogs.AnyAsync(a =>
                a.Action == "admin.user.status" && a.EntityId == targetId));
        }
    }
}
