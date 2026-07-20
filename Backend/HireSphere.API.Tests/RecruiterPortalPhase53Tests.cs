using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HireSphere.API.Tests;

public class RecruiterPortalPhase53Tests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RecruiterPortalPhase53Tests(TestWebApplicationFactory factory) => _factory = factory;

    private async Task<(string Token, int OrgId, User Recruiter)> SeedRecruiterAsync(string orgName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);
        var org = new Organization { Name = orgName, CreatedAtUtc = DateTime.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();
        var email = $"rec53-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var user = new User
        {
            FullName = "Recruiter 53",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Recruiter",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = (await db.Roles.FirstAsync(r => r.Name == "Recruiter")).Id });
        db.RecruiterProfiles.Add(new RecruiterProfile { UserId = user.Id, OrganizationId = org.Id, CreatedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;
        return (token, org.Id, user);
    }

    private async Task<(string Token, User Candidate)> SeedCandidateAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);
        var email = $"cand53-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var user = new User
        {
            FullName = "Candidate 53",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Candidate",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = (await db.Roles.FirstAsync(r => r.Name == "Candidate")).Id });
        db.CandidateProfiles.Add(new CandidateProfile { UserId = user.Id, FullName = user.FullName, CreatedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        return ((await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!, user);
    }

    private HttpClient Client(string token)
    {
        var c = _factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    private async Task<(int JobId, int ApplicationId)> SeedAppAsync(string recruiterToken, string candidateToken)
    {
        var recruiter = Client(recruiterToken);
        var create = await recruiter.PostAsJsonAsync("/api/recruiter/jobs", new
        {
            title = "Interview Role",
            description = "Desc",
            location = "Colombo",
            vacancies = 1
        });
        create.EnsureSuccessStatusCode();
        var jobId = (await create.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();
        (await recruiter.PatchAsJsonAsync($"/api/recruiter/jobs/{jobId}/status", new { status = "Published" })).EnsureSuccessStatusCode();
        var apply = await Client(candidateToken).PostAsJsonAsync("/api/candidate/applications", new
        {
            jobId,
            coverLetter = "Hi",
            termsAccepted = true,
            screeningAnswers = Array.Empty<object>()
        });
        apply.EnsureSuccessStatusCode();
        var applicationId = (await apply.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();
        (await recruiter.PatchAsJsonAsync($"/api/recruiter/applications/{applicationId}/status", new { status = "Shortlisted", notes = "ok" }))
            .EnsureSuccessStatusCode();
        return (jobId, applicationId);
    }

    [Fact]
    public async Task Interview_Scheduling_Conflicts_Reports_And_Authz()
    {
        var (tokenA, orgA, recruiterA) = await SeedRecruiterAsync($"A53-{Guid.NewGuid():N}");
        var (tokenB, _, _) = await SeedRecruiterAsync($"B53-{Guid.NewGuid():N}");
        var (candToken, candidate) = await SeedCandidateAsync();
        var (_, applicationId) = await SeedAppAsync(tokenA, candToken);
        var recruiter = Client(tokenA);
        var start = DateTime.UtcNow.AddDays(2);

        var schedule = await recruiter.PostAsJsonAsync("/api/recruiter/interviews", new
        {
            applicationId,
            startAtUtc = start,
            durationMinutes = 60,
            timeZoneId = "Asia/Colombo",
            interviewType = "Video",
            meetingLink = "https://meet.example/test",
            meetingInstructions = "Join 5 minutes early",
            internalNotes = "PRIVATE_INTERNAL_NOTE",
            participantUserIds = Array.Empty<int>()
        });
        schedule.EnsureSuccessStatusCode();
        var scheduled = await schedule.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(scheduled.GetProperty("scheduled").GetBoolean());
        var interviewId = scheduled.GetProperty("interview").GetProperty("id").GetInt32();
        Assert.Equal("Asia/Colombo", scheduled.GetProperty("interview").GetProperty("timeZoneId").GetString());
        Assert.Equal("NotConfigured", scheduled.GetProperty("interview").GetProperty("calendarSyncStatus").GetString());
        Assert.Equal(60, scheduled.GetProperty("interview").GetProperty("durationMinutes").GetInt32());
        Assert.True(scheduled.GetProperty("interview").TryGetProperty("startAtUtc", out _));

        // Candidate conflict
        var conflict = await recruiter.PostAsJsonAsync("/api/recruiter/interviews", new
        {
            applicationId,
            startAtUtc = start.AddMinutes(30),
            durationMinutes = 60,
            timeZoneId = "UTC",
            interviewType = "Video"
        });
        conflict.EnsureSuccessStatusCode();
        var conflictBody = await conflict.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.False(conflictBody.GetProperty("scheduled").GetBoolean());
        Assert.True(conflictBody.GetProperty("conflicts").GetArrayLength() > 0);

        // Cancelled interviews do not conflict
        (await recruiter.PatchAsJsonAsync($"/api/recruiter/interviews/{interviewId}/status", new { status = "Cancelled", notes = "cancel" }))
            .EnsureSuccessStatusCode();
        var rescheduleAfterCancel = await recruiter.PostAsJsonAsync("/api/recruiter/interviews", new
        {
            applicationId,
            startAtUtc = start.AddMinutes(30),
            durationMinutes = 60,
            timeZoneId = "UTC",
            interviewType = "Video",
            internalNotes = "second"
        });
        rescheduleAfterCancel.EnsureSuccessStatusCode();
        var afterCancel = await rescheduleAfterCancel.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(afterCancel.GetProperty("scheduled").GetBoolean());
        var interview2 = afterCancel.GetProperty("interview").GetProperty("id").GetInt32();

        var rescheduleReq = await Client(candToken).PostAsJsonAsync(
            $"/api/candidate/interviews/{interview2}/reschedule-request",
            new { reason = "Conflict with travel" });
        rescheduleReq.EnsureSuccessStatusCode();

        // Schedule a fresh interview to confirm (after previous moved to reschedule state)
        var third = await recruiter.PostAsJsonAsync("/api/recruiter/interviews", new
        {
            applicationId,
            startAtUtc = start.AddDays(3),
            durationMinutes = 45,
            timeZoneId = "Asia/Colombo",
            interviewType = "Video",
            internalNotes = "PRIVATE_INTERNAL_NOTE",
            forceDespiteConflicts = true
        });
        third.EnsureSuccessStatusCode();
        var thirdBody = await third.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(thirdBody.GetProperty("scheduled").GetBoolean());
        var interview3 = thirdBody.GetProperty("interview").GetProperty("id").GetInt32();

        var confirm = await Client(candToken).PostAsJsonAsync($"/api/candidate/interviews/{interview3}/confirm", new { });
        if (!confirm.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Confirm failed: {(int)confirm.StatusCode} {await confirm.Content.ReadAsStringAsync()}");
        }

        var detail = await recruiter.GetAsync($"/api/recruiter/interviews/{interview3}");
        detail.EnsureSuccessStatusCode();
        Assert.Equal("Confirmed", (await detail.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("candidateResponse").GetString());

        var candDetail = await Client(candToken).GetAsync($"/api/candidate/interviews/{interview3}");
        candDetail.EnsureSuccessStatusCode();
        var candJson = await candDetail.Content.ReadAsStringAsync();
        Assert.DoesNotContain("PRIVATE_INTERNAL_NOTE", candJson);
        Assert.DoesNotContain("internalNotes", candJson, StringComparison.OrdinalIgnoreCase);

        var cross = await Client(tokenB).GetAsync($"/api/recruiter/interviews/{interview3}");
        Assert.Equal(HttpStatusCode.NotFound, cross.StatusCode);

        var report = await recruiter.GetAsync($"/api/recruiter/reports/summary?jobId=");
        report.EnsureSuccessStatusCode();
        var reportBody = await report.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(reportBody.GetProperty("applicationsTotal").GetInt32() >= 1);

        var export = await recruiter.GetAsync("/api/recruiter/reports/export");
        export.EnsureSuccessStatusCode();
        var csv = Encoding.UTF8.GetString(await export.Content.ReadAsByteArrayAsync());
        Assert.Contains("ApplicationId", csv);
        Assert.DoesNotContain("PasswordHash", csv, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(candidate.FullName, csv);

        var otherReport = await Client(tokenB).GetAsync("/api/recruiter/reports/summary");
        otherReport.EnsureSuccessStatusCode();
        Assert.Equal(0, (await otherReport.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("applicationsTotal").GetInt32());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(await db.AuditLogs.AnyAsync(a => a.Action == "InterviewScheduled"));
        Assert.True(await db.Notifications.AnyAsync(n => n.UserId == candidate.Id && n.Category == "InterviewScheduled"));
        Assert.NotEqual(0, orgA);
        Assert.NotEqual(0, recruiterA.Id);
    }
}
