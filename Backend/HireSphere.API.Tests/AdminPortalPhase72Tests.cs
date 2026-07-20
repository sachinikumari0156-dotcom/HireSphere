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

public class AdminPortalPhase72Tests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AdminPortalPhase72Tests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient Client(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<(User User, string Token)> SeedUserAsync(string role, int? orgId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);

        var email = $"{role}-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var user = new User
        {
            FullName = role,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            Status = UserStatus.Active,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var r = await db.Roles.FirstAsync(x => x.Name == role);
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = r.Id });
        if (role == "HiringManager" && orgId is int oid)
        {
            db.HiringManagerProfiles.Add(new HiringManagerProfile
            {
                UserId = user.Id,
                OrganizationId = oid,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        if (role == "Recruiter" && orgId is int oid2)
        {
            db.RecruiterProfiles.Add(new RecruiterProfile
            {
                UserId = user.Id,
                OrganizationId = oid2,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();

        var login = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var payload = await login.Content.ReadFromJsonAsync<JsonElement>();
        return (user, payload!.GetProperty("token").GetString()!);
    }

    private async Task<(int AppId, int OrgId, User Candidate, User Hm, string HmToken)> SeedPendingFinalAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);

        var org = new Organization
        {
            Name = $"Org-{Guid.NewGuid():N}",
            Code = $"C-{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
            Status = OrganizationStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var (hm, hmToken) = await SeedUserAsync("HiringManager", org.Id);
        var (rec, _) = await SeedUserAsync("Recruiter", org.Id);
        var (cand, _) = await SeedUserAsync("Candidate");

        db.CandidateProfiles.Add(new CandidateProfile
        {
            UserId = cand.Id,
            FullName = cand.FullName,
            CreatedAtUtc = DateTime.UtcNow
        });
        var job = new Job
        {
            Title = "Engineer",
            Description = "d",
            Location = "Colombo",
            RecruiterId = rec.Id,
            HiringManagerUserId = hm.Id,
            OrganizationId = org.Id,
            Status = JobStatus.Published,
            PostedDate = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            Vacancies = 1
        };
        db.Jobs.Add(job);
        await db.SaveChangesAsync();

        var app = new Application
        {
            CandidateId = cand.Id,
            JobId = job.Id,
            Status = ApplicationStatus.Interviewed,
            AppliedDate = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.Applications.Add(app);
        await db.SaveChangesAsync();

        db.HiringDecisions.Add(new HiringDecision
        {
            ApplicationId = app.Id,
            DecisionByUserId = hm.Id,
            DecisionType = HiringDecisionType.RecommendHire,
            IsFinal = false,
            Status = HiringDecisionStatus.Pending,
            Reason = "Strong hire",
            PriorApplicationStatus = ApplicationStatus.Interviewed,
            DecisionDateUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        });
        db.AuditLogs.Add(new AuditLog
        {
            UserId = hm.Id,
            Action = "hm.recommend",
            EntityType = nameof(Application),
            EntityId = app.Id,
            Details = "RecommendHire",
            Success = true,
            ActorRole = "HiringManager",
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return (app.Id, org.Id, cand, hm, hmToken);
    }

    [Fact]
    public async Task Phase72_Audit_Monitoring_Analytics_Final_And_Exports()
    {
        var (admin, adminToken) = await SeedUserAsync("Admin");
        var (cand, candToken) = await SeedUserAsync("Candidate");
        var (rec, recToken) = await SeedUserAsync("Recruiter", null);
        var (hmUser, hmToken) = await SeedUserAsync("HiringManager", null);
        var (appId, orgId, _, _, _) = await SeedPendingFinalAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await Client(candToken).GetAsync("/api/admin/audit-logs")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Client(recToken).GetAsync("/api/admin/monitoring/summary")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Client(hmToken).GetAsync("/api/admin/final-decisions/pending")).StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.AuditLogs.Add(new AuditLog
            {
                UserId = admin.Id,
                Action = "admin.test",
                EntityType = "System",
                Details = "=CMD|calc",
                Success = true,
                ActorRole = "Admin",
                CorrelationId = "corr-phase72",
                CreatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var audits = await Client(adminToken).GetAsync("/api/admin/audit-logs?action=admin.test");
        Assert.Equal(HttpStatusCode.OK, audits.StatusCode);
        var auditBody = await audits.Content.ReadAsStringAsync();
        Assert.DoesNotContain("PasswordHash", auditBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", auditBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("admin.test", auditBody);

        var export = await Client(adminToken).GetAsync("/api/admin/audit-logs/export?action=admin.test");
        Assert.Equal(HttpStatusCode.OK, export.StatusCode);
        var csv = Encoding.UTF8.GetString(await export.Content.ReadAsByteArrayAsync());
        Assert.Contains("'=CMD|calc", csv);
        Assert.DoesNotContain("PasswordHash", csv, StringComparison.OrdinalIgnoreCase);

        var monitoring = await Client(adminToken).GetAsync("/api/admin/monitoring/summary");
        Assert.Equal(HttpStatusCode.OK, monitoring.StatusCode);
        var mon = await monitoring.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Connected", mon.GetProperty("databaseConnectivity").GetString());
        Assert.Equal("NotConfigured", mon.GetProperty("emailProviderStatus").GetString());
        Assert.Equal("NotConfigured", mon.GetProperty("smsProviderStatus").GetString());

        var analytics = await Client(adminToken).GetAsync($"/api/admin/analytics/recruitment?organizationId={orgId}");
        Assert.Equal(HttpStatusCode.OK, analytics.StatusCode);

        var emptySkills = await Client(adminToken).GetAsync("/api/admin/analytics/skills?organizationId=999999");
        Assert.Equal(HttpStatusCode.OK, emptySkills.StatusCode);
        var skills = await emptySkills.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, skills.GetProperty("skillDemandFromJobs").GetArrayLength());

        var pending = await Client(adminToken).GetAsync("/api/admin/final-decisions/pending");
        Assert.Equal(HttpStatusCode.OK, pending.StatusCode);
        var pendingList = await pending.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(pendingList.EnumerateArray(), x => x.GetProperty("applicationId").GetInt32() == appId);

        var detail = await Client(adminToken).GetAsync($"/api/admin/final-decisions/{appId}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        var detailBody = await detail.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("RecommendHire", detailBody.GetProperty("latestRecommendation").GetString());

        var hmFinal = await Client(hmToken).PostAsJsonAsync($"/api/admin/final-decisions/{appId}", new
        {
            decisionType = "FinalHire",
            reason = "HM cannot finalize"
        });
        Assert.Equal(HttpStatusCode.Forbidden, hmFinal.StatusCode);

        var hire = await Client(adminToken).PostAsJsonAsync($"/api/admin/final-decisions/{appId}", new
        {
            decisionType = "FinalHire",
            reason = "Approved after panel review"
        });
        Assert.Equal(HttpStatusCode.OK, hire.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var app = await db.Applications.Include(a => a.StatusHistory).FirstAsync(a => a.Id == appId);
            Assert.Equal(ApplicationStatus.Hired, app.Status);
            Assert.Contains(app.StatusHistory, h => h.Status == ApplicationStatus.Hired);
            Assert.True(await db.HiringDecisions.AnyAsync(d => d.ApplicationId == appId && d.IsFinal));
            Assert.True(await db.AuditLogs.AnyAsync(a => a.Action == "admin.hiring.final" && a.EntityId == appId));
            Assert.True(await db.Notifications.AnyAsync(n => n.UserId == app.CandidateId && n.Category == "HiringDecision"));
        }

        var dup = await Client(adminToken).PostAsJsonAsync($"/api/admin/final-decisions/{appId}", new
        {
            decisionType = "FinalReject",
            reason = "Duplicate"
        });
        Assert.Equal(HttpStatusCode.BadRequest, dup.StatusCode);

        // Withdrawn application rejects final decision
        var (withdrawnId, _, _, _, _) = await SeedPendingFinalAsync();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var a = await db.Applications.FirstAsync(x => x.Id == withdrawnId);
            a.Status = ApplicationStatus.Withdrawn;
            await db.SaveChangesAsync();
        }

        var withdrawn = await Client(adminToken).PostAsJsonAsync($"/api/admin/final-decisions/{withdrawnId}", new
        {
            decisionType = "FinalHire",
            reason = "Should fail"
        });
        Assert.Equal(HttpStatusCode.BadRequest, withdrawn.StatusCode);

        // Stale update
        var (staleId, _, _, _, _) = await SeedPendingFinalAsync();
        DateTime expected;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var a = await db.Applications.FirstAsync(x => x.Id == staleId);
            expected = a.UpdatedAtUtc ?? DateTime.UtcNow.AddHours(-1);
            a.UpdatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        var stale = await Client(adminToken).PostAsJsonAsync($"/api/admin/final-decisions/{staleId}", new
        {
            decisionType = "FinalHire",
            reason = "Stale",
            expectedUpdatedAtUtc = expected
        });
        Assert.Equal(HttpStatusCode.BadRequest, stale.StatusCode);

        var usersExport = await Client(adminToken).GetAsync("/api/admin/exports/users");
        Assert.Equal(HttpStatusCode.OK, usersExport.StatusCode);
        var usersCsv = Encoding.UTF8.GetString(await usersExport.Content.ReadAsByteArrayAsync());
        Assert.DoesNotContain("PasswordHash", usersCsv, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Bearer ", usersCsv);

        var security = await Client(adminToken).GetAsync($"/api/admin/security/users/{cand.Id}");
        Assert.Equal(HttpStatusCode.OK, security.StatusCode);
        var secBody = await security.Content.ReadAsStringAsync();
        Assert.DoesNotContain("PasswordHash", secBody, StringComparison.OrdinalIgnoreCase);
    }
}
