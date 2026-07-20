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

public class CandidatePortalControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CandidatePortalControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(User User, int ProfileId)> SeedCandidateAsync(string? email = null)
    {
        email ??= $"cand-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);

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
            CreatedAtUtc = DateTime.UtcNow
        };
        db.CandidateProfiles.Add(profile);
        await db.SaveChangesAsync();

        return (user, profile.Id);
    }

    private async Task<string> LoginTokenAsync(string email, string password = "SecurePass123!")
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
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

    private static ByteArrayContent CreatePdfContent(byte[] bytes)
    {
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        return content;
    }

    [Fact]
    public async Task GetProfile_ReturnsOwnProfile()
    {
        var (user, _) = await SeedCandidateAsync();
        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        var response = await client.GetAsync("/api/candidate/profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal(user.Id, body.GetProperty("userId").GetInt32());
        Assert.Equal(user.FullName, body.GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task GetProfile_DoesNotExposePasswordHash()
    {
        var (user, _) = await SeedCandidateAsync();
        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        var response = await client.GetAsync("/api/candidate/profile");
        var json = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PasswordHash", json);
    }

    [Fact]
    public async Task CrossCandidateExperienceUpdate_IsBlocked()
    {
        var (userA, _) = await SeedCandidateAsync();
        var (userB, profileBId) = await SeedCandidateAsync();

        int experienceBId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var experience = new WorkExperience
            {
                CandidateProfileId = profileBId,
                CompanyName = "Other Corp",
                JobTitle = "Engineer",
                StartDate = new DateTime(2022, 1, 1),
                CreatedAtUtc = DateTime.UtcNow
            };
            db.WorkExperiences.Add(experience);
            await db.SaveChangesAsync();
            experienceBId = experience.Id;
        }

        var token = await LoginTokenAsync(userA.Email);
        var client = ClientWithToken(token);
        var response = await client.PutAsJsonAsync($"/api/candidate/experience/{experienceBId}", new
        {
            companyName = "Hacked Corp",
            jobTitle = "CEO",
            startDate = "2020-01-01",
            endDate = "2024-01-01",
            isCurrentRole = false
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ExperienceEndBeforeStart_IsRejected()
    {
        var (user, _) = await SeedCandidateAsync();
        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        var response = await client.PostAsJsonAsync("/api/candidate/experience", new
        {
            companyName = "Acme",
            jobTitle = "Developer",
            startDate = "2024-06-01",
            endDate = "2023-01-01",
            isCurrentRole = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("End date cannot be before start date", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DuplicateSkill_IsBlocked()
    {
        var (user, profileId) = await SeedCandidateAsync();

        int skillId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var skill = new Skill { Name = "C#", CreatedAtUtc = DateTime.UtcNow };
            db.Skills.Add(skill);
            await db.SaveChangesAsync();
            skillId = skill.Id;
        }

        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        var first = await client.PostAsJsonAsync("/api/candidate/skills", new { skillId });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/candidate/skills", new { skillId });
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        var body = await second.Content.ReadAsStringAsync();
        Assert.Contains("already on your profile", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UploadResume_ReturnsSafeMetadataWithoutAbsolutePath()
    {
        var (user, _) = await SeedCandidateAsync();
        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        using var form = new MultipartFormDataContent();
        var pdfBytes = Encoding.UTF8.GetBytes("%PDF-1.4 test resume content");
        form.Add(CreatePdfContent(pdfBytes), "file", "resume.pdf");

        var response = await client.PostAsync("/api/candidate/resumes", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.False(body.TryGetProperty("storageKey", out _));
        Assert.False(body.TryGetProperty("filePath", out _));
        Assert.Equal("resume.pdf", body.GetProperty("fileName").GetString());
        Assert.True(body.TryGetProperty("id", out _));
    }

    [Fact]
    public async Task UploadUnsupportedFile_IsRejected()
    {
        var (user, _) = await SeedCandidateAsync();
        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        using var form = new MultipartFormDataContent();
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("plain text"));
        content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(content, "file", "notes.txt");

        var response = await client.PostAsync("/api/candidate/resumes", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadExecutableFile_IsRejected()
    {
        var (user, _) = await SeedCandidateAsync();
        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        using var form = new MultipartFormDataContent();
        var exeBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };
        var content = new ByteArrayContent(exeBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(content, "file", "malware.pdf");

        var response = await client.PostAsync("/api/candidate/resumes", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Executable", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UploadOversizedFile_IsRejected()
    {
        var (user, _) = await SeedCandidateAsync();
        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        using var form = new MultipartFormDataContent();
        var oversized = new byte[5 * 1024 * 1024 + 1];
        Array.Fill(oversized, (byte)'A');
        form.Add(CreatePdfContent(oversized), "file", "large.pdf");

        var response = await client.PostAsync("/api/candidate/resumes", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("5 MB", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CandidatePortal_RejectsRecruiter()
    {
        var email = $"rec-{Guid.NewGuid():N}@example.com";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            TestWebApplicationFactory.SeedRoles(db);
            var user = new User
            {
                FullName = "Recruiter User",
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SecurePass123!"),
                Role = "Recruiter",
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var role = await db.Roles.FirstAsync(r => r.Name == "Recruiter");
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            await db.SaveChangesAsync();
        }

        var token = await LoginTokenAsync(email);
        var client = ClientWithToken(token);
        var response = await client.GetAsync("/api/candidate/profile");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
