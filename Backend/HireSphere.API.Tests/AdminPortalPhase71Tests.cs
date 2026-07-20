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

public class AdminPortalPhase71Tests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AdminPortalPhase71Tests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient Client(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<(User User, string Token, string Password)> SeedAsync(string role, string? orgName = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);

        int? orgId = null;
        if (orgName is not null)
        {
            var org = new Organization
            {
                Name = orgName,
                Code = $"C-{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
                Status = OrganizationStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Organizations.Add(org);
            await db.SaveChangesAsync();
            orgId = org.Id;
        }

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
        if (role == "Recruiter" && orgId is int oid)
        {
            db.RecruiterProfiles.Add(new RecruiterProfile
            {
                UserId = user.Id,
                OrganizationId = oid,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        if (role == "HiringManager" && orgId is int oid2)
        {
            db.HiringManagerProfiles.Add(new HiringManagerProfile
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
        return (user, payload!.GetProperty("token").GetString()!, password);
    }

    [Fact]
    public async Task Phase71_Governance_Authz_And_Protections()
    {
        var (admin, adminToken, _) = await SeedAsync("Admin");
        var (admin2, admin2Token, _) = await SeedAsync("Admin");
        var (cand, candToken, candPass) = await SeedAsync("Candidate");
        var (rec, recToken, _) = await SeedAsync("Recruiter", $"Org-{Guid.NewGuid():N}");
        var (hm, hmToken, _) = await SeedAsync("HiringManager", $"Org-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.Forbidden, (await Client(candToken).GetAsync("/api/admin/dashboard")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Client(recToken).GetAsync("/api/admin/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Client(hmToken).GetAsync("/api/admin/organizations")).StatusCode);

        var list = await Client(adminToken).GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var listBody = await list.Content.ReadAsStringAsync();
        Assert.DoesNotContain("passwordHash", listBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PasswordHash", listBody);

        var dash = await Client(adminToken).GetAsync("/api/admin/dashboard");
        Assert.Equal(HttpStatusCode.OK, dash.StatusCode);

        var selfDisable = await Client(adminToken).PatchAsJsonAsync($"/api/admin/users/{admin.Id}/status", new { status = "Inactive" });
        Assert.Equal(HttpStatusCode.BadRequest, selfDisable.StatusCode);

        var disableCand = await Client(adminToken).PatchAsJsonAsync($"/api/admin/users/{cand.Id}/status", new { status = "Inactive" });
        Assert.Equal(HttpStatusCode.OK, disableCand.StatusCode);
        var loginDisabled = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email = cand.Email, password = candPass });
        Assert.Equal(HttpStatusCode.Unauthorized, loginDisabled.StatusCode);

        // Leave one admin active; disable the other admin then try to disable last
        var disableAdmin2 = await Client(adminToken).PatchAsJsonAsync($"/api/admin/users/{admin2.Id}/status", new { status = "Inactive" });
        Assert.Equal(HttpStatusCode.OK, disableAdmin2.StatusCode);
        var lastAdmin = await Client(adminToken).PatchAsJsonAsync($"/api/admin/users/{admin.Id}/status", new { status = "Inactive" });
        // self already blocked; use a third approach: remove last admin role via delete
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // reactivate admin2 as inactive so only admin is active Admin
            var a2 = await db.Users.FirstAsync(u => u.Id == admin2.Id);
            a2.Status = UserStatus.Inactive;
            await db.SaveChangesAsync();
        }

        var adminRole = await Client(adminToken).GetAsync($"/api/admin/users/{admin.Id}/roles");
        Assert.Equal(HttpStatusCode.OK, adminRole.StatusCode);
        var roles = await adminRole.Content.ReadFromJsonAsync<JsonElement>();
        var adminRoleId = roles.EnumerateArray().First(x => x.GetProperty("roleName").GetString() == "Admin").GetProperty("roleId").GetInt32();
        var removeLast = await Client(adminToken).DeleteAsync($"/api/admin/users/{admin.Id}/roles/{adminRoleId}");
        Assert.Equal(HttpStatusCode.BadRequest, removeLast.StatusCode);

        var orgCreate = await Client(adminToken).PostAsJsonAsync("/api/admin/organizations", new
        {
            name = $"Org {Guid.NewGuid():N}",
            code = $"X{Guid.NewGuid():N}"[..8].ToUpperInvariant(),
            description = "Phase 7.1"
        });
        Assert.Equal(HttpStatusCode.OK, orgCreate.StatusCode);
        var org = await orgCreate.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = org.GetProperty("id").GetInt32();
        var code = org.GetProperty("code").GetString();

        var dup = await Client(adminToken).PostAsJsonAsync("/admin/organizations", new { name = "Dup", code });
        // wrong path without /api - fix
        dup = await Client(adminToken).PostAsJsonAsync("/api/admin/organizations", new { name = "Dup", code });
        Assert.Equal(HttpStatusCode.BadRequest, dup.StatusCode);

        var dept = await Client(adminToken).PostAsJsonAsync("/api/admin/departments", new
        {
            organizationId = orgId,
            name = "Engineering",
            code = "ENG"
        });
        Assert.Equal(HttpStatusCode.OK, dept.StatusCode);
        var deptId = (await dept.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var otherOrg = await Client(adminToken).PostAsJsonAsync("/api/admin/organizations", new
        {
            name = $"Other {Guid.NewGuid():N}",
            code = $"Y{Guid.NewGuid():N}"[..8].ToUpperInvariant()
        });
        var otherOrgId = (await otherOrg.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        var crossDept = await Client(adminToken).PutAsJsonAsync($"/api/admin/departments/{deptId}", new
        {
            organizationId = otherOrgId,
            name = "Engineering"
        });
        Assert.Equal(HttpStatusCode.BadRequest, crossDept.StatusCode);

        await Client(adminToken).PatchAsJsonAsync($"/api/admin/departments/{deptId}/status", new { status = "Archived" });
        var assignArchived = await Client(adminToken).PutAsJsonAsync($"/api/admin/users/{hm.Id}/organization", new
        {
            organizationId = orgId,
            departmentId = deptId
        });
        Assert.Equal(HttpStatusCode.BadRequest, assignArchived.StatusCode);

        // Recruiter request approve/reject
        int requestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var req = new RecruiterAccessRequest
            {
                FullName = "Req Rec",
                BusinessEmail = $"req-{Guid.NewGuid():N}@example.com",
                NormalizedBusinessEmail = $"REQ-{Guid.NewGuid():N}@EXAMPLE.COM".ToUpperInvariant(),
                OrganizationName = "Req Org",
                Status = RecruiterRequestStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            };
            req.NormalizedBusinessEmail = req.BusinessEmail.ToUpperInvariant();
            db.RecruiterAccessRequests.Add(req);
            await db.SaveChangesAsync();
            requestId = req.Id;
        }

        var approve = await Client(adminToken).PostAsJsonAsync($"/api/admin/recruiter-requests/{requestId}/approve", new { notes = "ok" });
        Assert.Equal(HttpStatusCode.OK, approve.StatusCode);
        var approveAgain = await Client(adminToken).PostAsJsonAsync($"/api/admin/recruiter-requests/{requestId}/approve", new { notes = "again" });
        Assert.Equal(HttpStatusCode.BadRequest, approveAgain.StatusCode);

        int rejectId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var req = new RecruiterAccessRequest
            {
                FullName = "Reject Me",
                BusinessEmail = $"rej-{Guid.NewGuid():N}@example.com",
                NormalizedBusinessEmail = "",
                OrganizationName = "Rej Org",
                Status = RecruiterRequestStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            };
            req.NormalizedBusinessEmail = req.BusinessEmail.ToUpperInvariant();
            db.RecruiterAccessRequests.Add(req);
            await db.SaveChangesAsync();
            rejectId = req.Id;
        }

        var reject = await Client(adminToken).PostAsJsonAsync($"/api/admin/recruiter-requests/{rejectId}/reject", new { notes = "Incomplete" });
        Assert.Equal(HttpStatusCode.OK, reject.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.True(await db.AuditLogs.AnyAsync(a => a.Action == "admin.recruiter-request.approve"));
            Assert.True(await db.AuditLogs.AnyAsync(a => a.Action == "admin.recruiter-request.reject"));
            Assert.True(await db.AuditLogs.AnyAsync(a => a.Action == "admin.user.status"));
        }

        // silence unused
        _ = admin2Token;
        _ = JsonOptions;
    }
}
