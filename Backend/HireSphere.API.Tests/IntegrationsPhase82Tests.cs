using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using HireSphere.API.Services;
using HireSphere.API.Services.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HireSphere.API.Tests;

public class IntegrationsPhase82Tests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public IntegrationsPhase82Tests(TestWebApplicationFactory factory) => _factory = factory;

    private HttpClient Client(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<(User User, string Token)> SeedUserAsync(string role)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);
        var email = $"{role.ToLowerInvariant()}-int-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var user = new User
        {
            FullName = $"{role} Integrations",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role == "Admin" ? "Admin" : role,
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var roleEntity = await db.Roles.FirstAsync(r => r.Name == (role == "Admin" ? "Admin" : role));
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleEntity.Id });
        await db.SaveChangesAsync();

        var login = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var payload = await login.Content.ReadFromJsonAsync<JsonElement>();
        return (user, payload!.GetProperty("token").GetString()!);
    }

    [Fact]
    public async Task Outbox_IsCreated_WithBusinessNotification()
    {
        var (user, _) = await SeedUserAsync("Candidate");
        using var scope = _factory.Services.CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<HireSphere.API.Services.INotificationWriter>();
        await writer.CreateAsync(user.Id, NotificationCategories.ApplicationSubmitted, "Received", "Your application was received.", "Application", 42);

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var outbox = await db.NotificationOutbox.FirstOrDefaultAsync(x => x.UserId == user.Id && x.RelatedEntityId == 42);
        Assert.NotNull(outbox);
        Assert.Equal(OutboxDeliveryStatus.Queued, outbox!.Status);
    }

    [Fact]
    public async Task Duplicate_IdempotencyKey_IsRejected()
    {
        var (user, _) = await SeedUserAsync("Candidate");
        using var scope = _factory.Services.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
        await dispatcher.EnqueueAsync(user.Id, "Test", "T", "M", "X", 1, "idem-dup-key");
        await dispatcher.EnqueueAsync(user.Id, "Test", "T", "M", "X", 1, "idem-dup-key");
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await db.NotificationOutbox.CountAsync(x => x.IdempotencyKey == "idem-dup-key"));
    }

    [Fact]
    public void Html_Template_Escapes_User_Content()
    {
        var (_, plain, html) = NotificationTemplateRenderer.ApplicationStatus("<script>x</script>", "Job & Co", "Review");
        Assert.Contains("&lt;script&gt;", html);
        Assert.DoesNotContain("<script>", html);
        Assert.Contains("Job & Co", plain);
    }

    [Fact]
    public void Plain_Text_Email_Is_Generated()
    {
        var (_, plain, _) = NotificationTemplateRenderer.ApplicationStatus("Ada", "Engineer", "Shortlisted");
        Assert.Contains("Ada", plain);
        Assert.Contains("Engineer", plain);
        Assert.Contains("Shortlisted", plain);
    }

    [Fact]
    public void Production_Smtp_Is_Not_Reported_Verified_Without_Config()
    {
        using var scope = _factory.Services.CreateScope();
        var email = scope.ServiceProvider.GetRequiredService<IEmailProvider>();
        var status = email.GetStatus();
        Assert.Equal("NotConfigured", status.Status);
        Assert.Contains("Production", status.Detail!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Sms_Mock_Records_Development_Mock_Status()
    {
        using var scope = _factory.Services.CreateScope();
        var sms = scope.ServiceProvider.GetRequiredService<ISmsProvider>();
        var result = await sms.SendAsync(new SmsMessage { ToE164 = "+94771234567", Body = "Hi" }, hasConsent: true);
        Assert.True(result.Ok);
        Assert.Equal("Development Mock", result.StatusLabel);
    }

    [Fact]
    public async Task Sms_Consent_Is_Enforced()
    {
        using var scope = _factory.Services.CreateScope();
        var sms = scope.ServiceProvider.GetRequiredService<ISmsProvider>();
        var result = await sms.SendAsync(new SmsMessage { ToE164 = "+94771234567", Body = "Hi" }, hasConsent: false);
        Assert.False(result.Ok);
        Assert.Equal("sms_consent_required", result.SafeFailureCode);
    }

    [Fact]
    public async Task Invalid_Phone_Is_Rejected()
    {
        using var scope = _factory.Services.CreateScope();
        var sms = scope.ServiceProvider.GetRequiredService<ISmsProvider>();
        var result = await sms.SendAsync(new SmsMessage { ToE164 = "077123", Body = "Hi" }, hasConsent: true);
        Assert.False(result.Ok);
        Assert.Equal("invalid_phone", result.SafeFailureCode);
    }

    [Fact]
    public void Full_Phone_Is_Not_Logged_In_Mask()
    {
        var masked = DevelopmentMockSmsProvider.MaskPhone("+94771234567");
        Assert.DoesNotContain("771234567", masked);
        Assert.EndsWith("4567", masked);
    }

    [Fact]
    public async Task User_Preferences_Are_Enforced()
    {
        var (user, token) = await SeedUserAsync("Candidate");
        var client = Client(token);
        var put = await client.PutAsJsonAsync("/api/users/notification-preferences", new
        {
            emailEnabled = false,
            smsEnabled = false,
            interviewReminders = false,
            applicationUpdates = false,
            assessmentReminders = false,
            smsConsent = false
        });
        put.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
        await dispatcher.EnqueueAsync(user.Id, "ApplicationStatusUpdated", "T", "M", "Application", 9);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(0, await db.NotificationOutbox.CountAsync(x => x.UserId == user.Id && x.RelatedEntityId == 9));
    }

    [Fact]
    public async Task Mandatory_Security_Notification_Bypasses_Email_Preference()
    {
        var (user, token) = await SeedUserAsync("Candidate");
        var client = Client(token);
        await client.PutAsJsonAsync("/api/users/notification-preferences", new
        {
            emailEnabled = false,
            smsEnabled = false,
            interviewReminders = false,
            applicationUpdates = false,
            assessmentReminders = false,
            smsConsent = false
        });

        using var scope = _factory.Services.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
        await dispatcher.EnqueueAsync(user.Id, "SecurityPasswordReset", "Reset", "Reset foundation", null, null, "sec-key", forceSecurityEmail: true);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await db.NotificationOutbox.CountAsync(x => x.IdempotencyKey == "sec-key"));
    }

    [Fact]
    public async Task Failed_Delivery_Retries_Are_Bounded()
    {
        var (user, _) = await SeedUserAsync("Candidate");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var key = $"bound-{Guid.NewGuid():N}";
        db.NotificationOutbox.Add(new NotificationOutbox
        {
            NotificationType = "Test",
            UserId = user.Id,
            Channel = NotificationChannel.Email,
            Provider = "SMTP",
            Status = OutboxDeliveryStatus.Failed,
            AttemptCount = 3,
            MaxAttempts = 3,
            IdempotencyKey = key,
            QueuedAtUtc = DateTime.UtcNow,
            DestinationMasked = "a***@example.com"
        });
        await db.SaveChangesAsync();
        var processor = scope.ServiceProvider.GetRequiredService<INotificationOutboxProcessor>();
        await processor.ProcessPendingAsync(20);
        var item = await db.NotificationOutbox.AsNoTracking().FirstAsync(x => x.IdempotencyKey == key);
        Assert.Equal(3, item.AttemptCount);
        Assert.Equal(OutboxDeliveryStatus.Failed, item.Status);
    }

    [Fact]
    public void Ics_Contains_Valid_Uid_Start_End()
    {
        using var scope = _factory.Services.CreateScope();
        var icsSvc = scope.ServiceProvider.GetRequiredService<IIcsCalendarService>();
        var interview = new Interview
        {
            Id = 99,
            InterviewDate = new DateTime(2026, 7, 22, 10, 0, 0, DateTimeKind.Utc),
            DurationMinutes = 45,
            InterviewType = "Video",
            Status = InterviewStatus.Scheduled,
            TimeZoneId = "UTC"
        };
        var ics = icsSvc.GenerateIcs(interview, "Interview: Backend", "HireSphere interview", "https://meet.example");
        Assert.Contains("UID:interview-99@hiresphere.local", ics);
        Assert.Contains("DTSTART:20260722T100000Z", ics);
        Assert.Contains("DTEND:20260722T104500Z", ics);
        Assert.DoesNotContain("InternalNotes", ics);
    }

    [Fact]
    public void Ics_Excludes_Private_Internal_Notes()
    {
        using var scope = _factory.Services.CreateScope();
        var icsSvc = scope.ServiceProvider.GetRequiredService<IIcsCalendarService>();
        var interview = new Interview
        {
            Id = 7,
            InterviewDate = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 30,
            InternalNotes = "SECRET_PRIVATE_NOTE",
            Status = InterviewStatus.Scheduled
        };
        var ics = icsSvc.GenerateIcs(interview, "Interview", "Public desc", null);
        Assert.DoesNotContain("SECRET_PRIVATE_NOTE", ics);
    }

    [Fact]
    public async Task Google_And_Outlook_Show_NotConfigured()
    {
        var (_, token) = await SeedUserAsync("Admin");
        var client = Client(token);
        var response = await client.GetAsync("/api/admin/integrations/status");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("Google Calendar", json);
        Assert.Contains("Outlook Calendar", json);
        Assert.Contains("NotConfigured", json);
        Assert.DoesNotContain("ApiKey", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"secret\"", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Provider_Tokens_Are_Not_Serialized_On_Stubs()
    {
        var google = new GoogleCalendarProviderStub().GetStatus();
        var outlook = new OutlookCalendarProviderStub().GetStatus();
        Assert.Equal("NotConfigured", google.Status);
        Assert.Equal("NotConfigured", outlook.Status);
        Assert.Null(google.GetType().GetProperty("AccessToken"));
        Assert.Null(outlook.GetType().GetProperty("RefreshToken"));
    }

    [Fact]
    public async Task Cross_User_Delivery_Access_Is_Blocked()
    {
        var (userA, tokenA) = await SeedUserAsync("Candidate");
        var (userB, tokenB) = await SeedUserAsync("Candidate");
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.NotificationOutbox.Add(new NotificationOutbox
            {
                NotificationType = "Private",
                UserId = userA.Id,
                Channel = NotificationChannel.Email,
                Provider = "SMTP",
                Status = OutboxDeliveryStatus.Sent,
                IdempotencyKey = $"priv-{Guid.NewGuid():N}",
                QueuedAtUtc = DateTime.UtcNow,
                DestinationMasked = "a***@example.com"
            });
            await db.SaveChangesAsync();
        }

        var clientB = Client(tokenB);
        var response = await clientB.GetAsync("/api/notifications/deliveries");
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.Equal(0, items.GetArrayLength());
        _ = userA;
        _ = tokenA;
    }

    [Fact]
    public async Task Candidate_Cannot_Access_Admin_Integration_Status()
    {
        var (_, token) = await SeedUserAsync("Candidate");
        var client = Client(token);
        var response = await client.GetAsync("/api/admin/integrations/status");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reminder_Worker_Processor_Is_Idempotent_For_Sent()
    {
        var (user, _) = await SeedUserAsync("Candidate");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var key = $"sent-{Guid.NewGuid():N}";
        db.NotificationOutbox.Add(new NotificationOutbox
        {
            NotificationType = "InterviewReminder",
            UserId = user.Id,
            Channel = NotificationChannel.Email,
            Provider = "SMTP",
            Status = OutboxDeliveryStatus.Sent,
            AttemptCount = 1,
            MaxAttempts = 3,
            IdempotencyKey = key,
            QueuedAtUtc = DateTime.UtcNow,
            SentAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var processor = scope.ServiceProvider.GetRequiredService<INotificationOutboxProcessor>();
        await processor.ProcessPendingAsync(50);
        var item = await db.NotificationOutbox.AsNoTracking().FirstAsync(x => x.IdempotencyKey == key);
        Assert.Equal(OutboxDeliveryStatus.Sent, item.Status);
        Assert.Equal(1, item.AttemptCount);
    }

    [Fact]
    public async Task Preferences_Endpoint_RoundTrips()
    {
        var (_, token) = await SeedUserAsync("Candidate");
        var client = Client(token);
        var put = await client.PutAsJsonAsync("/api/users/notification-preferences", new
        {
            emailEnabled = true,
            smsEnabled = true,
            interviewReminders = true,
            applicationUpdates = true,
            assessmentReminders = false,
            smsConsent = true
        });
        put.EnsureSuccessStatusCode();
        var get = await client.GetAsync("/api/users/notification-preferences");
        get.EnsureSuccessStatusCode();
        var body = await get.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(body.GetProperty("smsConsent").GetBoolean());
        Assert.False(body.GetProperty("assessmentReminders").GetBoolean());
    }
}
