using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HireSphere.API.Data;
using HireSphere.API.DTOs.Recruiter;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using HireSphere.API.Services.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HireSphere.API.Tests;

/// <summary>
/// Phase 10.1 — security, authorization, CSV safety, performance smoke, worker/outbox checks.
/// </summary>
public class Phase10QualityTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Phase10QualityTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(User User, string Password)> SeedAsync(
        string role,
        UserStatus status = UserStatus.Active,
        string? orgName = null)
    {
        var password = "Phase10TestPass123!";
        var email = $"{role.ToLowerInvariant()}-{Guid.NewGuid():N}@example.test";
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);

        int? organizationId = null;
        if (!string.IsNullOrWhiteSpace(orgName))
        {
            var org = await db.Organizations.FirstOrDefaultAsync(o => o.Name == orgName)
                      ?? db.Organizations.Add(new Organization { Name = orgName, CreatedAtUtc = DateTime.UtcNow }).Entity;
            await db.SaveChangesAsync();
            organizationId = org.Id;
        }

        var user = new User
        {
            FullName = $"{role} Phase10",
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
        return (user, password);
    }

    private async Task<HttpClient> AuthedClientAsync(string email, string password)
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var payload = await login.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = payload.GetProperty("token").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Theory]
    [InlineData("=1+1")]
    [InlineData("+cmd|' /C calc'!A0")]
    [InlineData("-2+3")]
    [InlineData("@SUM(A1)")]
    public void CsvEscaper_Neutralizes_Formula_Injection(string payload)
    {
        var escaped = CsvEscaper.Escape(payload);
        Assert.StartsWith("'", escaped);
        Assert.False(escaped.StartsWith('='));
        Assert.False(escaped.StartsWith('+'));
        Assert.False(escaped.StartsWith('-'));
        Assert.False(escaped.StartsWith('@'));
    }

    [Fact]
    public void CsvEscaper_Quotes_Commas_And_Quotes()
    {
        var escaped = CsvEscaper.Escape("hello,\"world\"");
        Assert.Equal("\"hello,\"\"world\"\"\"", escaped);
    }

    [Fact]
    public async Task Anonymous_Cannot_Access_Protected_Admin_Endpoints()
    {
        var client = _factory.CreateClient();
        var routes = new[]
        {
            "/api/admin/dashboard",
            "/api/admin/users",
            "/api/admin/audit-logs",
            "/api/admin/integrations/ai/status",
            "/api/admin/storage/status"
        };

        foreach (var route in routes)
        {
            var response = await client.GetAsync(route);
            Assert.True(
                response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.NotFound,
                $"{route} => {response.StatusCode}");
        }
    }

    [Fact]
    public async Task Candidate_Is_Forbidden_From_Admin_And_Recruiter_Apis()
    {
        var (user, password) = await SeedAsync("Candidate");
        var client = await AuthedClientAsync(user.Email, password);

        var forbidden = new[]
        {
            "/api/admin/dashboard",
            "/api/recruiter/dashboard",
            "/api/hiring-manager/dashboard"
        };

        foreach (var route in forbidden)
        {
            var response = await client.GetAsync(route);
            Assert.True(
                response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound,
                $"{route} => {response.StatusCode}");
        }
    }

    [Fact]
    public async Task Disabled_User_Cannot_Login()
    {
        var (user, password) = await SeedAsync("Candidate", UserStatus.Inactive);
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = user.Email, password });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Malformed_And_Missing_Tokens_Are_Rejected()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-jwt");
        var bad = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);

        var anon = _factory.CreateClient();
        var missing = await anon.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, missing.StatusCode);
    }

    [Fact]
    public async Task Mass_Assignment_Role_On_Registration_Is_Ignored()
    {
        var client = _factory.CreateClient();
        var email = $"mass-{Guid.NewGuid():N}@example.test";
        var response = await client.PostAsJsonAsync("/api/auth/register/candidate", new
        {
            firstName = "Mass",
            lastName = "Assign",
            email,
            password = "CandidateE2ePass123!",
            confirmPassword = "CandidateE2ePass123!",
            acceptTerms = true,
            role = "Admin"
        });
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.FirstAsync(u => u.Email == email);
        Assert.Equal("Candidate", user.Role);
    }

    [Fact]
    public async Task Sql_Injection_Like_Login_Email_Does_Not_Bypass_Auth()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "' OR 1=1 --",
            password = "anything"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Cross_Organization_Recruiter_Cannot_See_Other_Org_Jobs()
    {
        var (recA, passA) = await SeedAsync("Recruiter", orgName: $"OrgA-{Guid.NewGuid():N}");
        var (recB, passB) = await SeedAsync("Recruiter", orgName: $"OrgB-{Guid.NewGuid():N}");

        int jobId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var profileA = await db.RecruiterProfiles.FirstAsync(p => p.UserId == recA.Id);
            var job = new Job
            {
                Title = "Secret Role",
                Description = "Internal only",
                RequiredSkills = "C#",
                Location = "Remote",
                JobType = "FullTime",
                Status = JobStatus.Published,
                RecruiterId = recA.Id,
                OrganizationId = profileA.OrganizationId,
                PostedDate = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow,
                PublishedAtUtc = DateTime.UtcNow
            };
            db.Jobs.Add(job);
            await db.SaveChangesAsync();
            jobId = job.Id;
        }

        var clientB = await AuthedClientAsync(recB.Email, passB);
        var response = await clientB.GetAsync($"/api/recruiter/jobs/{jobId}");
        Assert.True(
            response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound,
            response.StatusCode.ToString());
    }

    [Fact]
    public async Task Notification_Outbox_Processor_Is_Idempotent_For_Empty_Queue()
    {
        using var scope = _factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<INotificationOutboxProcessor>();
        await processor.ProcessPendingAsync(5, CancellationToken.None);
        await processor.ProcessPendingAsync(5, CancellationToken.None);
    }

    [Fact]
    public async Task Performance_Smoke_Login_And_Dashboard_Under_Threshold()
    {
        // Pre-declared coursework smoke target: p95-style single-call under 1500ms locally.
        const int thresholdMs = 1500;
        var (admin, password) = await SeedAsync("Admin");
        var client = _factory.CreateClient();

        var sw = Stopwatch.StartNew();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = admin.Email, password });
        sw.Stop();
        Assert.True(login.IsSuccessStatusCode);
        Assert.True(sw.ElapsedMilliseconds < thresholdMs, $"Login took {sw.ElapsedMilliseconds}ms");

        var payload = await login.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = payload.GetProperty("token").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        sw.Restart();
        var dash = await client.GetAsync("/api/admin/dashboard");
        sw.Stop();
        Assert.True(dash.IsSuccessStatusCode, await dash.Content.ReadAsStringAsync());
        Assert.True(sw.ElapsedMilliseconds < thresholdMs, $"Dashboard took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Api_Contract_Unauthorized_Uses_Json_Without_Stack()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/admin/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("at HireSphere", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Swagger_Document_Is_Available()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/auth/login", json);
        Assert.DoesNotContain("AdminE2ePass", json);
    }
}
