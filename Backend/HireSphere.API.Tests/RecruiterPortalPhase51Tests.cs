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

public class RecruiterPortalPhase51Tests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RecruiterPortalPhase51Tests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(User Recruiter, int OrgId, string Token)> SeedRecruiterAsync(string orgName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);

        var org = new Organization { Name = orgName, CreatedAtUtc = DateTime.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var email = $"rec-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var user = new User
        {
            FullName = "Recruiter User",
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
            OrganizationId = org.Id,
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var payload = await login.Content.ReadFromJsonAsync<JsonElement>();
        var token = payload.GetProperty("token").GetString()!;
        return (user, org.Id, token);
    }

    private async Task<(User Candidate, int ProfileId, string Token)> SeedCandidateAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);

        var email = $"cand-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var user = new User
        {
            FullName = "Candidate User",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Candidate",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var role = await db.Roles.FirstAsync(r => r.Name == "Candidate");
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        var profile = new CandidateProfile
        {
            UserId = user.Id,
            FullName = user.FullName,
            Summary = "Backend engineer",
            YearsOfExperience = 3,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.CandidateProfiles.Add(profile);
        await db.SaveChangesAsync();

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var payload = await login.Content.ReadFromJsonAsync<JsonElement>();
        return (user, profile.Id, payload.GetProperty("token").GetString()!);
    }

    private HttpClient Client(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Recruiter_CreatesJob_InOwnOrganization()
    {
        var (recruiter, orgId, token) = await SeedRecruiterAsync($"Org-{Guid.NewGuid():N}");
        var client = Client(token);

        var response = await client.PostAsJsonAsync("/api/recruiter/jobs", new
        {
            title = "Software Engineer",
            description = "Build APIs",
            location = "Colombo",
            employmentType = "FullTime",
            workArrangement = "Hybrid",
            vacancies = 2,
            skills = new[] { new { skillName = "C#", isRequired = true } }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("Software Engineer", body.GetProperty("title").GetString());
        Assert.Equal(orgId, body.GetProperty("organizationId").GetInt32());
        Assert.Equal(recruiter.Id, body.GetProperty("recruiterId").GetInt32());
        Assert.Equal("Draft", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Recruiter_CannotAccess_OtherOrganizationJob()
    {
        var (_, _, tokenA) = await SeedRecruiterAsync($"OrgA-{Guid.NewGuid():N}");
        var (_, orgB, tokenB) = await SeedRecruiterAsync($"OrgB-{Guid.NewGuid():N}");

        var create = await Client(tokenB).PostAsJsonAsync("/api/recruiter/jobs", new
        {
            title = "Secret Role",
            description = "Internal only",
            location = "Remote",
            vacancies = 1
        });
        create.EnsureSuccessStatusCode();
        var job = await create.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var jobId = job.GetProperty("id").GetInt32();

        var get = await Client(tokenA).GetAsync($"/api/recruiter/jobs/{jobId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        var update = await Client(tokenA).PutAsJsonAsync($"/api/recruiter/jobs/{jobId}", new
        {
            title = "Hacked",
            description = "Nope",
            location = "Remote",
            vacancies = 1
        });
        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(orgB, job.GetProperty("organizationId").GetInt32());
    }

    [Fact]
    public async Task JobStatus_ValidAndInvalidTransitions()
    {
        var (_, _, token) = await SeedRecruiterAsync($"Org-{Guid.NewGuid():N}");
        var client = Client(token);
        var create = await client.PostAsJsonAsync("/api/recruiter/jobs", new
        {
            title = "Lifecycle Job",
            description = "Transitions",
            location = "Kandy",
            vacancies = 1
        });
        create.EnsureSuccessStatusCode();
        var jobId = (await create.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();

        var publish = await client.PatchAsJsonAsync($"/api/recruiter/jobs/{jobId}/status", new { status = "Published" });
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);
        Assert.Equal("Published", (await publish.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("status").GetString());

        var invalid = await client.PatchAsJsonAsync($"/api/recruiter/jobs/{jobId}/status", new { status = "Draft" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task PublishedJob_IsPublic_DraftIsNot()
    {
        var (_, _, recruiterToken) = await SeedRecruiterAsync($"Org-{Guid.NewGuid():N}");
        var (_, _, candidateToken) = await SeedCandidateAsync();
        var recruiterClient = Client(recruiterToken);
        var candidateClient = Client(candidateToken);

        var draft = await recruiterClient.PostAsJsonAsync("/api/recruiter/jobs", new
        {
            title = "Draft Only",
            description = "Hidden",
            location = "Galle",
            vacancies = 1
        });
        draft.EnsureSuccessStatusCode();
        var draftId = (await draft.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();

        var published = await recruiterClient.PostAsJsonAsync("/api/recruiter/jobs", new
        {
            title = "Visible Role",
            description = "Public",
            location = "Galle",
            vacancies = 1
        });
        published.EnsureSuccessStatusCode();
        var publishedId = (await published.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();
        (await recruiterClient.PatchAsJsonAsync($"/api/recruiter/jobs/{publishedId}/status", new { status = "Published" }))
            .EnsureSuccessStatusCode();

        var search = await candidateClient.GetAsync("/api/candidate/jobs?keyword=Visible");
        search.EnsureSuccessStatusCode();
        var list = await search.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var items = list.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetInt32()).ToList();
        Assert.Contains(publishedId, items);
        Assert.DoesNotContain(draftId, items);

        var draftGet = await candidateClient.GetAsync($"/api/candidate/jobs/{draftId}");
        Assert.True(draftGet.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PausedClosedExpired_RejectApplications()
    {
        var (_, _, recruiterToken) = await SeedRecruiterAsync($"Org-{Guid.NewGuid():N}");
        var (candidate, _, candidateToken) = await SeedCandidateAsync();
        var recruiterClient = Client(recruiterToken);
        var candidateClient = Client(candidateToken);

        async Task<int> CreateAndPublish(string title)
        {
            var create = await recruiterClient.PostAsJsonAsync("/api/recruiter/jobs", new
            {
                title,
                description = "Apply test",
                location = "Colombo",
                vacancies = 1
            });
            create.EnsureSuccessStatusCode();
            var id = (await create.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();
            (await recruiterClient.PatchAsJsonAsync($"/api/recruiter/jobs/{id}/status", new { status = "Published" }))
                .EnsureSuccessStatusCode();
            return id;
        }

        var pausedId = await CreateAndPublish("Paused Job");
        (await recruiterClient.PatchAsJsonAsync($"/api/recruiter/jobs/{pausedId}/status", new { status = "Paused" }))
            .EnsureSuccessStatusCode();
        var pausedApply = await candidateClient.PostAsJsonAsync("/api/candidate/applications", new
        {
            jobId = pausedId,
            coverLetter = "Hello",
            termsAccepted = true,
            answers = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.BadRequest, pausedApply.StatusCode);

        var closedId = await CreateAndPublish("Closed Job");
        (await recruiterClient.PatchAsJsonAsync($"/api/recruiter/jobs/{closedId}/status", new { status = "Closed" }))
            .EnsureSuccessStatusCode();
        var closedApply = await candidateClient.PostAsJsonAsync("/api/candidate/applications", new
        {
            jobId = closedId,
            coverLetter = "Hello",
            termsAccepted = true,
            answers = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.BadRequest, closedApply.StatusCode);

        var expiredId = await CreateAndPublish("Expired Job");
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var job = await db.Jobs.FirstAsync(j => j.Id == expiredId);
            job.ApplicationDeadlineUtc = DateTime.UtcNow.AddDays(-1);
            await db.SaveChangesAsync();
        }

        var expiredApply = await candidateClient.PostAsJsonAsync("/api/candidate/applications", new
        {
            jobId = expiredId,
            coverLetter = "Hello",
            termsAccepted = true,
            answers = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.BadRequest, expiredApply.StatusCode);
        Assert.NotEqual(0, candidate.Id);
    }

    [Fact]
    public async Task Pipeline_ShortlistReject_HistoryNotesCompare_AndAuthz()
    {
        var (_, orgId, recruiterToken) = await SeedRecruiterAsync($"Org-{Guid.NewGuid():N}");
        var (_, otherOrgId, otherToken) = await SeedRecruiterAsync($"Other-{Guid.NewGuid():N}");
        var (candidate, _, candidateToken) = await SeedCandidateAsync();
        var recruiterClient = Client(recruiterToken);
        var candidateClient = Client(candidateToken);

        var create = await recruiterClient.PostAsJsonAsync("/api/recruiter/jobs", new
        {
            title = "Pipeline Job",
            description = "Screening",
            location = "Colombo",
            vacancies = 1,
            skills = new[] { new { skillName = "SQL", isRequired = true } }
        });
        create.EnsureSuccessStatusCode();
        var jobId = (await create.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();
        (await recruiterClient.PatchAsJsonAsync($"/api/recruiter/jobs/{jobId}/status", new { status = "Published" }))
            .EnsureSuccessStatusCode();

        var apply = await candidateClient.PostAsJsonAsync("/api/candidate/applications", new
        {
            jobId,
            coverLetter = "I am interested",
            termsAccepted = true,
            answers = Array.Empty<object>()
        });
        apply.EnsureSuccessStatusCode();
        var applicationId = (await apply.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();

        var pipeline = await recruiterClient.GetAsync($"/api/recruiter/jobs/{jobId}/applications?page=1&pageSize=10");
        pipeline.EnsureSuccessStatusCode();
        var pipelineBody = await pipeline.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(pipelineBody.GetProperty("totalCount").GetInt32() >= 1);

        var otherPipeline = await Client(otherToken).GetAsync($"/api/recruiter/jobs/{jobId}/applications");
        Assert.Equal(HttpStatusCode.NotFound, otherPipeline.StatusCode);

        var shortlist = await recruiterClient.PatchAsJsonAsync(
            $"/api/recruiter/applications/{applicationId}/status",
            new { status = "Shortlisted", notes = "Strong fit" });
        Assert.Equal(HttpStatusCode.NoContent, shortlist.StatusCode);

        var detail = await recruiterClient.GetAsync($"/api/recruiter/applications/{applicationId}");
        detail.EnsureSuccessStatusCode();
        var detailBody = await detail.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("Shortlisted", detailBody.GetProperty("status").GetString());
        Assert.True(detailBody.GetProperty("statusHistory").GetArrayLength() >= 1);

        var note = await recruiterClient.PostAsJsonAsync(
            $"/api/recruiter/applications/{applicationId}/notes",
            new { content = "Internal only note" });
        note.EnsureSuccessStatusCode();

        var candidateDetail = await candidateClient.GetAsync($"/api/candidate/applications/{applicationId}");
        candidateDetail.EnsureSuccessStatusCode();
        var candidateJson = await candidateDetail.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Internal only note", candidateJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("internalNotes", candidateJson, StringComparison.OrdinalIgnoreCase);

        var invalidTransition = await recruiterClient.PatchAsJsonAsync(
            $"/api/recruiter/applications/{applicationId}/status",
            new { status = "Hired" });
        Assert.Equal(HttpStatusCode.BadRequest, invalidTransition.StatusCode);

        // Second application for comparison limit/scope
        var (candidate2, _, candidate2Token) = await SeedCandidateAsync();
        var apply2 = await Client(candidate2Token).PostAsJsonAsync("/api/candidate/applications", new
        {
            jobId,
            coverLetter = "Second",
            termsAccepted = true,
            answers = Array.Empty<object>()
        });
        apply2.EnsureSuccessStatusCode();
        var applicationId2 = (await apply2.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();

        var compare = await recruiterClient.PostAsJsonAsync("/api/recruiter/applications/compare", new
        {
            applicationIds = new[] { applicationId, applicationId2 }
        });
        compare.EnsureSuccessStatusCode();

        var crossCompare = await Client(otherToken).PostAsJsonAsync("/api/recruiter/applications/compare", new
        {
            applicationIds = new[] { applicationId, applicationId2 }
        });
        Assert.Equal(HttpStatusCode.NotFound, crossCompare.StatusCode);

        var deny = await candidateClient.GetAsync("/api/recruiter/dashboard");
        Assert.Equal(HttpStatusCode.Forbidden, deny.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.True(await db.AuditLogs.AnyAsync(a =>
                a.Action == "ApplicationStatusChanged" && a.EntityId == applicationId));
            Assert.True(await db.AuditLogs.AnyAsync(a => a.Action == "JobCreated"));
            Assert.NotEqual(orgId, otherOrgId);
            Assert.NotEqual(0, candidate2.Id);
        }
    }

    [Fact]
    public async Task Recruiter_CanUpdateOwnJob()
    {
        var (_, _, token) = await SeedRecruiterAsync($"Org-{Guid.NewGuid():N}");
        var client = Client(token);
        var create = await client.PostAsJsonAsync("/api/recruiter/jobs", new
        {
            title = "Original",
            description = "Desc",
            location = "Jaffna",
            vacancies = 1
        });
        create.EnsureSuccessStatusCode();
        var jobId = (await create.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();

        var update = await client.PutAsJsonAsync($"/api/recruiter/jobs/{jobId}", new
        {
            title = "Updated Title",
            description = "Updated desc",
            location = "Jaffna",
            vacancies = 3,
            salaryMin = 1000,
            salaryMax = 2000,
            salaryCurrency = "USD"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var body = await update.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("Updated Title", body.GetProperty("title").GetString());
        Assert.Equal(3, body.GetProperty("vacancies").GetInt32());
    }
}
