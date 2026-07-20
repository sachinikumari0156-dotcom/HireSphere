using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using HireSphere.API.Services.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HireSphere.API.Tests;

public class StoragePhase83Tests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public StoragePhase83Tests(TestWebApplicationFactory factory) => _factory = factory;

    private HttpClient Client(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static byte[] MinimalPdf()
    {
        var text = "%PDF-1.4\n1 0 obj<<>>endobj\ntrailer<<>>\n%%EOF\n";
        return Encoding.ASCII.GetBytes(text);
    }

    private static byte[] MinimalDocx()
    {
        // PK ZIP signature is enough for signature check; content may fail OpenXML elsewhere but upload only checks PK.
        var bytes = new byte[32];
        bytes[0] = 0x50;
        bytes[1] = 0x4B;
        bytes[2] = 0x03;
        bytes[3] = 0x04;
        return bytes;
    }

    private async Task<(User User, string Token)> SeedCandidateAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);
        var email = $"cand-st-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var user = new User
        {
            FullName = "Storage Candidate",
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
        db.CandidateProfiles.Add(new CandidateProfile
        {
            UserId = user.Id,
            FullName = user.FullName,
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var login = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var payload = await login.Content.ReadFromJsonAsync<JsonElement>();
        return (user, payload!.GetProperty("token").GetString()!);
    }

    private async Task<(User User, string Token)> SeedAdminAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);
        var email = $"admin-st-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var user = new User
        {
            FullName = "Storage Admin",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Admin",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var role = await db.Roles.FirstAsync(r => r.Name == "Admin");
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await db.SaveChangesAsync();
        var login = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var payload = await login.Content.ReadFromJsonAsync<JsonElement>();
        return (user, payload!.GetProperty("token").GetString()!);
    }

    private static MultipartFormDataContent FileContent(byte[] bytes, string fileName, string contentType)
    {
        var form = new MultipartFormDataContent();
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(content, "file", fileName);
        return form;
    }

    [Fact]
    public async Task Candidate_Uploads_Allowed_Pdf()
    {
        var (_, token) = await SeedCandidateAsync();
        var client = Client(token);
        using var form = FileContent(MinimalPdf(), "cv.pdf", "application/pdf");
        var response = await client.PostAsync("/api/candidate/resumes", form);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(body.TryGetProperty("id", out _));
        Assert.False(body.TryGetProperty("storageKey", out _));
        Assert.False(body.TryGetProperty("filePath", out _));
    }

    [Fact]
    public async Task Candidate_Uploads_Allowed_Docx()
    {
        var (_, token) = await SeedCandidateAsync();
        var client = Client(token);
        using var form = FileContent(MinimalDocx(), "cv.docx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        var response = await client.PostAsync("/api/candidate/resumes", form);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Executable_Upload_Is_Rejected()
    {
        var (_, token) = await SeedCandidateAsync();
        var client = Client(token);
        var exe = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };
        using var form = FileContent(exe, "malware.exe", "application/octet-stream");
        var response = await client.PostAsync("/api/candidate/resumes", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Signature_Mismatch_Is_Rejected()
    {
        var (_, token) = await SeedCandidateAsync();
        var client = Client(token);
        using var form = FileContent(Encoding.ASCII.GetBytes("not-a-pdf"), "fake.pdf", "application/pdf");
        var response = await client.PostAsync("/api/candidate/resumes", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Zero_Byte_Upload_Is_Rejected()
    {
        var (_, token) = await SeedCandidateAsync();
        var client = Client(token);
        using var form = FileContent(Array.Empty<byte>(), "empty.pdf", "application/pdf");
        var response = await client.PostAsync("/api/candidate/resumes", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void Filename_Path_Traversal_Is_Neutralized()
    {
        var safe = FileUploadValidator.SanitizeDisplayName(@"..\..\evil.pdf");
        Assert.DoesNotContain("..", safe);
        Assert.Equal("evil.pdf", safe);
    }

    [Fact]
    public async Task Storage_Key_Uses_Random_Tenant_Path()
    {
        var (user, token) = await SeedCandidateAsync();
        var client = Client(token);
        using var form = FileContent(MinimalPdf(), "cv.pdf", "application/pdf");
        await client.PostAsync("/api/candidate/resumes", form);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var profile = await db.CandidateProfiles.FirstAsync(p => p.UserId == user.Id);
        var resume = await db.Resumes.FirstAsync(r => r.CandidateProfileId == profile.Id);
        Assert.StartsWith("tenant/", resume.FilePath);
        Assert.Contains($"/candidate/{user.Id}/", resume.FilePath);
        Assert.DoesNotContain("cv.pdf", resume.FilePath);
    }

    [Fact]
    public async Task Candidate_Cannot_Download_Another_Candidates_Document()
    {
        var (userA, tokenA) = await SeedCandidateAsync();
        var (_, tokenB) = await SeedCandidateAsync();
        var clientA = Client(tokenA);
        using (var form = FileContent(MinimalPdf(), "a.pdf", "application/pdf"))
        {
            form.Add(new StringContent("Other"), "documentType");
            var upload = await clientA.PostAsync("/api/candidate/documents", form);
            upload.EnsureSuccessStatusCode();
        }

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var profile = await db.CandidateProfiles.FirstAsync(p => p.UserId == userA.Id);
        var doc = await db.CandidateDocuments.FirstAsync(d => d.CandidateProfileId == profile.Id);

        var clientB = Client(tokenB);
        var denied = await clientB.GetAsync($"/api/candidate/documents/{doc.Id}/download");
        Assert.True(denied.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden or HttpStatusCode.BadRequest);
        var denied2 = await clientB.GetAsync($"/api/documents/{doc.Id}/download");
        Assert.Equal(HttpStatusCode.Forbidden, denied2.StatusCode);
    }

    [Fact]
    public async Task Quarantined_Document_Cannot_Be_Downloaded()
    {
        var (user, token) = await SeedCandidateAsync();
        var client = Client(token);
        using (var form = FileContent(MinimalPdf(), "q.pdf", "application/pdf"))
        {
            form.Add(new StringContent("Other"), "documentType");
            (await client.PostAsync("/api/candidate/documents", form)).EnsureSuccessStatusCode();
        }

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var profile = await db.CandidateProfiles.FirstAsync(p => p.UserId == user.Id);
        var doc = await db.CandidateDocuments.FirstAsync(d => d.CandidateProfileId == profile.Id);
        doc.ValidationStatus = "Quarantined";
        await db.SaveChangesAsync();

        var response = await client.GetAsync($"/api/candidate/documents/{doc.Id}/download");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logical_Delete_Creates_Audit_And_Blocks_Download()
    {
        var (user, token) = await SeedCandidateAsync();
        var client = Client(token);
        using (var form = FileContent(MinimalPdf(), "d.pdf", "application/pdf"))
        {
            form.Add(new StringContent("Other"), "documentType");
            (await client.PostAsync("/api/candidate/documents", form)).EnsureSuccessStatusCode();
        }

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var profile = await db.CandidateProfiles.FirstAsync(p => p.UserId == user.Id);
        var doc = await db.CandidateDocuments.FirstAsync(d => d.CandidateProfileId == profile.Id);
        var delete = await client.DeleteAsync($"/api/candidate/documents/{doc.Id}");
        delete.EnsureSuccessStatusCode();
        Assert.True(await db.AuditLogs.AnyAsync(a => a.Action == "DocumentLogicalDelete" && a.EntityId == doc.Id));
        var download = await client.GetAsync($"/api/candidate/documents/{doc.Id}/download");
        Assert.True(download.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Dry_Run_Migration_Does_Not_Change_Data()
    {
        var (_, token) = await SeedAdminAsync();
        var client = Client(token);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var before = await db.Resumes.CountAsync();
        var response = await client.PostAsync("/api/admin/storage/migrations/dry-run", null);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.False(body.GetProperty("changed").GetBoolean());
        Assert.Equal(before, await db.Resumes.CountAsync());
    }

    [Fact]
    public async Task Azure_Provider_Is_NotConfigured_Without_Credentials()
    {
        using var scope = _factory.Services.CreateScope();
        var azure = scope.ServiceProvider.GetRequiredService<AzureBlobStorageProvider>();
        Assert.Equal("NotConfigured", azure.GetStatus().Status);
    }

    [Fact]
    public async Task Antivirus_Is_NotConfigured()
    {
        using var scope = _factory.Services.CreateScope();
        var av = scope.ServiceProvider.GetRequiredService<IAntivirusScanner>();
        Assert.Equal("NotConfigured", av.GetStatus().Status);
    }

    [Fact]
    public async Task Admin_Storage_Status_Exposes_No_Secrets()
    {
        var (_, token) = await SeedAdminAsync();
        var client = Client(token);
        var response = await client.GetAsync("/api/admin/storage/status");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("ConnectionString", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AccountKey", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("NotConfigured", json);
    }

    [Fact]
    public void Checksum_Validation_Works()
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes("hello"));
        var hash = FileUploadValidator.ComputeSha256(ms);
        Assert.Equal(64, hash.Length);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("hello"))).ToLowerInvariant(), hash);
    }

    [Fact]
    public async Task Candidate_Cannot_Access_Admin_Storage_Status()
    {
        var (_, token) = await SeedCandidateAsync();
        var client = Client(token);
        var response = await client.GetAsync("/api/admin/storage/status");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
