using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HireSphere.API.Tests;

public class HiringManagerPortalPhase61Tests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public HiringManagerPortalPhase61Tests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(User User, int OrgId, string Token)> SeedManagerAsync(string orgName, string? email = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);

        var org = await db.Organizations.FirstOrDefaultAsync(o => o.Name == orgName);
        if (org is null)
        {
            org = new Organization { Name = orgName, CreatedAtUtc = DateTime.UtcNow };
            db.Organizations.Add(org);
            await db.SaveChangesAsync();
        }

        email ??= $"hm-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var user = new User
        {
            FullName = "Hiring Manager",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "HiringManager",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var role = await db.Roles.FirstAsync(r => r.Name == "HiringManager");
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        db.HiringManagerProfiles.Add(new HiringManagerProfile
        {
            UserId = user.Id,
            OrganizationId = org.Id,
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var payload = await login.Content.ReadFromJsonAsync<JsonElement>();
        return (user, org.Id, payload!.GetProperty("token").GetString()!);
    }

    private async Task<(User User, string Token)> SeedRecruiterAsync(int orgId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);
        var email = $"rec-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var user = new User
        {
            FullName = "Recruiter",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Recruiter",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var role = await db.Roles.FirstAsync(r => r.Name == "Recruiter");
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        db.RecruiterProfiles.Add(new RecruiterProfile
        {
            UserId = user.Id,
            OrganizationId = orgId,
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var payload = await login.Content.ReadFromJsonAsync<JsonElement>();
        return (user, payload!.GetProperty("token").GetString()!);
    }

    private async Task<(User Candidate, string Token, int ApplicationId, int JobId)> SeedAssignedApplicationAsync(
        int orgId,
        int recruiterId,
        int hiringManagerId,
        ApplicationStatus status = ApplicationStatus.Shortlisted)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);

        var email = $"cand-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var candidate = new User
        {
            FullName = "Candidate",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Candidate",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(candidate);
        await db.SaveChangesAsync();
        var role = await db.Roles.FirstAsync(r => r.Name == "Candidate");
        db.UserRoles.Add(new UserRole { UserId = candidate.Id, RoleId = role.Id });
        db.CandidateProfiles.Add(new CandidateProfile
        {
            UserId = candidate.Id,
            FullName = candidate.FullName,
            Summary = "Engineer",
            YearsOfExperience = 4,
            CreatedAtUtc = DateTime.UtcNow
        });

        var job = new Job
        {
            Title = "Assigned Role",
            Description = "Desc",
            Location = "Colombo",
            RecruiterId = recruiterId,
            HiringManagerUserId = hiringManagerId,
            OrganizationId = orgId,
            Status = JobStatus.Published,
            PostedDate = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            Vacancies = 1
        };
        db.Jobs.Add(job);
        await db.SaveChangesAsync();

        var application = new Application
        {
            CandidateId = candidate.Id,
            JobId = job.Id,
            Status = status,
            AppliedDate = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Applications.Add(application);
        await db.SaveChangesAsync();

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var payload = await login.Content.ReadFromJsonAsync<JsonElement>();
        return (candidate, payload!.GetProperty("token").GetString()!, application.Id, job.Id);
    }

    private HttpClient Client(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Manager_Sees_Assigned_Vacancies_Only()
    {
        var orgName = $"Org-{Guid.NewGuid():N}";
        var (hm, orgId, token) = await SeedManagerAsync(orgName);
        var (recruiter, _) = await SeedRecruiterAsync(orgId);
        await SeedAssignedApplicationAsync(orgId, recruiter.Id, hm.Id);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Jobs.Add(new Job
            {
                Title = "Unassigned",
                Description = "x",
                Location = "Kandy",
                RecruiterId = recruiter.Id,
                HiringManagerUserId = null,
                OrganizationId = orgId,
                Status = JobStatus.Published,
                PostedDate = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow,
                Vacancies = 1
            });
            await db.SaveChangesAsync();
        }

        var response = await Client(token).GetAsync("/api/hiring-manager/jobs");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal(1, body.GetProperty("totalCount").GetInt32());
        Assert.Equal("Assigned Role", body.GetProperty("items")[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task Manager_Cannot_See_Unassigned_Or_CrossOrg_Vacancy()
    {
        var (hmA, orgA, tokenA) = await SeedManagerAsync($"OrgA-{Guid.NewGuid():N}");
        var (hmB, orgB, _) = await SeedManagerAsync($"OrgB-{Guid.NewGuid():N}");
        var (recB, _) = await SeedRecruiterAsync(orgB);
        var seeded = await SeedAssignedApplicationAsync(orgB, recB.Id, hmB.Id);

        var cross = await Client(tokenA).GetAsync($"/api/hiring-manager/jobs/{seeded.JobId}");
        Assert.Equal(HttpStatusCode.NotFound, cross.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var unassigned = new Job
        {
            Title = "Other",
            Description = "x",
            Location = "Galle",
            RecruiterId = recB.Id,
            HiringManagerUserId = hmB.Id,
            OrganizationId = orgB,
            Status = JobStatus.Published,
            PostedDate = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            Vacancies = 1
        };
        db.Jobs.Add(unassigned);
        await db.SaveChangesAsync();

        var denied = await Client(tokenA).GetAsync($"/api/hiring-manager/jobs/{unassigned.Id}");
        Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
        _ = hmA;
        _ = orgA;
    }

    [Fact]
    public async Task Manager_Sees_Candidates_And_Blocks_Unrelated_Application()
    {
        var (hm, orgId, token) = await SeedManagerAsync($"Org-{Guid.NewGuid():N}");
        var (otherHm, otherOrg, otherToken) = await SeedManagerAsync($"Org2-{Guid.NewGuid():N}");
        var (recruiter, _) = await SeedRecruiterAsync(orgId);
        var (otherRec, _) = await SeedRecruiterAsync(otherOrg);
        var mine = await SeedAssignedApplicationAsync(orgId, recruiter.Id, hm.Id);
        var theirs = await SeedAssignedApplicationAsync(otherOrg, otherRec.Id, otherHm.Id);

        var list = await Client(token).GetAsync($"/api/hiring-manager/jobs/{mine.JobId}/candidates");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        var detail = await Client(token).GetAsync($"/api/hiring-manager/applications/{mine.ApplicationId}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        var body = await detail.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.False(body.TryGetProperty("passwordHash", out _));
        Assert.False(body.TryGetProperty("internalNotes", out _));
        Assert.Contains("AI-generated insight", body.GetProperty("humanReviewNotice").GetString());

        var blocked = await Client(token).GetAsync($"/api/hiring-manager/applications/{theirs.ApplicationId}");
        Assert.Equal(HttpStatusCode.NotFound, blocked.StatusCode);
        _ = otherToken;
    }

    [Fact]
    public async Task Compare_Same_Vacancy_Limit_And_Cross_Vacancy_Rejected()
    {
        var (hm, orgId, token) = await SeedManagerAsync($"Org-{Guid.NewGuid():N}");
        var (recruiter, _) = await SeedRecruiterAsync(orgId);
        var a1 = await SeedAssignedApplicationAsync(orgId, recruiter.Id, hm.Id);
        int jobId;
        var appIds = new List<int> { a1.ApplicationId };
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            jobId = a1.JobId;
            for (var i = 0; i < 5; i++)
            {
                var cEmail = $"c{i}-{Guid.NewGuid():N}@example.com";
                var c = new User
                {
                    FullName = $"C{i}",
                    Email = cEmail,
                    NormalizedEmail = cEmail.ToUpperInvariant(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("SecurePass123!"),
                    Role = "Candidate",
                    Status = UserStatus.Active,
                    CreatedAtUtc = DateTime.UtcNow
                };
                db.Users.Add(c);
                await db.SaveChangesAsync();
                db.CandidateProfiles.Add(new CandidateProfile
                {
                    UserId = c.Id,
                    FullName = c.FullName,
                    CreatedAtUtc = DateTime.UtcNow
                });
                var app = new Application
                {
                    CandidateId = c.Id,
                    JobId = jobId,
                    Status = ApplicationStatus.Shortlisted,
                    AppliedDate = DateTime.UtcNow,
                    CreatedAtUtc = DateTime.UtcNow
                };
                db.Applications.Add(app);
                await db.SaveChangesAsync();
                appIds.Add(app.Id);
            }
        }

        var over = await Client(token).PostAsJsonAsync("/api/hiring-manager/candidates/compare", new
        {
            applicationIds = appIds.Take(6)
        });
        Assert.Equal(HttpStatusCode.BadRequest, over.StatusCode);

        var ok = await Client(token).PostAsJsonAsync("/api/hiring-manager/candidates/compare", new
        {
            applicationIds = appIds.Take(2)
        });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);

        var (hm2, org2, _) = await SeedManagerAsync($"OrgX-{Guid.NewGuid():N}");
        var (rec2, _) = await SeedRecruiterAsync(org2);
        var other = await SeedAssignedApplicationAsync(org2, rec2.Id, hm2.Id);
        var cross = await Client(token).PostAsJsonAsync("/api/hiring-manager/candidates/compare", new
        {
            applicationIds = new[] { a1.ApplicationId, other.ApplicationId }
        });
        Assert.Equal(HttpStatusCode.NotFound, cross.StatusCode);
    }

    [Fact]
    public async Task Candidate_And_Recruiter_Denied_Manager_Apis_And_Comment_Audits()
    {
        var (hm, orgId, token) = await SeedManagerAsync($"Org-{Guid.NewGuid():N}");
        var (recruiter, recToken) = await SeedRecruiterAsync(orgId);
        var seeded = await SeedAssignedApplicationAsync(orgId, recruiter.Id, hm.Id);

        var candDenied = await Client(seeded.Token).GetAsync("/api/hiring-manager/dashboard");
        Assert.Equal(HttpStatusCode.Forbidden, candDenied.StatusCode);

        var recDenied = await Client(recToken).GetAsync("/api/hiring-manager/jobs");
        Assert.Equal(HttpStatusCode.Forbidden, recDenied.StatusCode);

        var comment = await Client(token).PostAsJsonAsync(
            $"/api/hiring-manager/jobs/{seeded.JobId}/review-comments",
            new { content = "Looks solid." });
        Assert.Equal(HttpStatusCode.OK, comment.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(await db.AuditLogs.AnyAsync(a =>
            a.UserId == hm.Id && a.Action == "JobReviewCommentCreated"));
    }
}
