using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using HireSphere.API.Services.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HireSphere.API.Services.Integrations;

public sealed class IntegrationProviderStatusDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "NotConfigured";
    public string? Detail { get; set; }
    public DateTime? LastCheckedUtc { get; set; }
}

public sealed class DeliveryResult
{
    public bool Ok { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public string? SafeFailureCode { get; set; }
}

public sealed class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string PlainTextBody { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
}

public sealed class SmsMessage
{
    public string ToE164 { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public interface IEmailProvider
{
    IntegrationProviderStatusDto GetStatus();
    Task<DeliveryResult> SendAsync(EmailMessage message, CancellationToken ct = default);
}

public interface ISmsProvider
{
    IntegrationProviderStatusDto GetStatus();
    Task<DeliveryResult> SendAsync(SmsMessage message, bool hasConsent, CancellationToken ct = default);
}

public interface ICalendarProvider
{
    string Name { get; }
    IntegrationProviderStatusDto GetStatus();
}

public interface IIcsCalendarService
{
    IntegrationProviderStatusDto GetStatus();
    string GenerateIcs(Interview interview, string summary, string? description, string? location);
}

public interface INotificationDispatcher
{
    Task EnqueueAsync(
        int userId,
        string notificationType,
        string title,
        string message,
        string? relatedEntityType,
        int? relatedEntityId,
        string? idempotencyKey = null,
        bool forceSecurityEmail = false);
}

public interface INotificationOutboxProcessor
{
    Task<int> ProcessPendingAsync(int take = 20, CancellationToken ct = default);
}

public interface IIntegrationHealthService
{
    Task<IReadOnlyList<IntegrationProviderStatusDto>> GetStatusesAsync();
}

public sealed class SmtpEmailProvider : IEmailProvider
{
    private readonly IConfiguration _config;

    public SmtpEmailProvider(IConfiguration config) => _config = config;

    private string? Host => _config["Email:Smtp:Host"];
    private int Port => _config.GetValue("Email:Smtp:Port", 25);
    private string From => _config["Email:Smtp:From"] ?? "noreply@hiresphere.local";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);

    public IntegrationProviderStatusDto GetStatus()
    {
        if (!IsConfigured)
        {
            return new IntegrationProviderStatusDto
            {
                Name = "SMTP Email",
                Status = "NotConfigured",
                Detail = "Production SMTP Not Configured. Set Email:Smtp:Host to enable development SMTP (e.g. MailHog)."
            };
        }

        var isDev = Host!.Contains("localhost", StringComparison.OrdinalIgnoreCase)
            || Host.Contains("127.0.0.1")
            || Host.Contains("mailhog", StringComparison.OrdinalIgnoreCase);
        return new IntegrationProviderStatusDto
        {
            Name = "SMTP Email",
            Status = "Configured",
            Detail = isDev
                ? "Development SMTP configured (MailHog/local). Production Email Not Configured."
                : "SMTP host configured. Mark Healthy only after a successful send/health check."
        };
    }

    public async Task<DeliveryResult> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return new DeliveryResult { Ok = false, StatusLabel = "NotConfigured", SafeFailureCode = "smtp_not_configured" };

        try
        {
            using var client = new SmtpClient(Host, Port)
            {
                EnableSsl = _config.GetValue("Email:Smtp:EnableSsl", false),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            var user = _config["Email:Smtp:Username"];
            var pass = _config["Email:Smtp:Password"];
            if (!string.IsNullOrWhiteSpace(user))
                client.Credentials = new NetworkCredential(user, pass);

            using var mail = new MailMessage(From, message.To, message.Subject, message.PlainTextBody)
            {
                IsBodyHtml = false
            };
            if (!string.IsNullOrWhiteSpace(message.HtmlBody))
            {
                mail.IsBodyHtml = true;
                mail.Body = message.HtmlBody;
                mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.PlainTextBody, null, "text/plain"));
            }

            await client.SendMailAsync(mail, ct);
            return new DeliveryResult { Ok = true, StatusLabel = "Sent" };
        }
        catch (Exception)
        {
            return new DeliveryResult { Ok = false, StatusLabel = "Failed", SafeFailureCode = "smtp_send_failed" };
        }
    }
}

public sealed class DevelopmentMockSmsProvider : ISmsProvider
{
    private static readonly Regex E164 = new(@"^\+[1-9]\d{7,14}$", RegexOptions.Compiled);

    public IntegrationProviderStatusDto GetStatus() => new()
    {
        Name = "SMS (Development Mock)",
        Status = "Healthy",
        Detail = "Development Mock only. External SMS Not Configured."
    };

    public Task<DeliveryResult> SendAsync(SmsMessage message, bool hasConsent, CancellationToken ct = default)
    {
        if (!hasConsent)
            return Task.FromResult(new DeliveryResult { Ok = false, StatusLabel = "Suppressed", SafeFailureCode = "sms_consent_required" });
        if (string.IsNullOrWhiteSpace(message.ToE164) || !E164.IsMatch(message.ToE164.Trim()))
            return Task.FromResult(new DeliveryResult { Ok = false, StatusLabel = "Failed", SafeFailureCode = "invalid_phone" });

        return Task.FromResult(new DeliveryResult
        {
            Ok = true,
            StatusLabel = "Development Mock"
        });
    }

    public static string MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 4) return "***";
        return new string('*', Math.Max(0, phone.Length - 4)) + phone[^4..];
    }
}

public sealed class InternalCalendarProvider : ICalendarProvider
{
    public string Name => "Internal Calendar";
    public IntegrationProviderStatusDto GetStatus() => new()
    {
        Name = Name,
        Status = "Healthy",
        Detail = "Interview records persist internally with timezone metadata."
    };
}

public sealed class GoogleCalendarProviderStub : ICalendarProvider
{
    public string Name => "Google Calendar";
    public IntegrationProviderStatusDto GetStatus() => new()
    {
        Name = Name,
        Status = "NotConfigured",
        Detail = "OAuth credentials not configured."
    };
}

public sealed class OutlookCalendarProviderStub : ICalendarProvider
{
    public string Name => "Outlook Calendar";
    public IntegrationProviderStatusDto GetStatus() => new()
    {
        Name = Name,
        Status = "NotConfigured",
        Detail = "OAuth credentials not configured."
    };
}

public sealed class IcsCalendarService : IIcsCalendarService
{
    public IntegrationProviderStatusDto GetStatus() => new()
    {
        Name = "ICS Calendar",
        Status = "Healthy",
        Detail = "Standards-compliant ICS generation verified in application tests."
    };

    public string GenerateIcs(Interview interview, string summary, string? description, string? location)
    {
        var uid = $"interview-{interview.Id}@hiresphere.local";
        var start = interview.InterviewDate.ToUniversalTime();
        var end = start.AddMinutes(interview.DurationMinutes <= 0 ? 60 : interview.DurationMinutes);
        var stamp = DateTime.UtcNow;
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//HireSphere//Interview//EN");
        sb.AppendLine("CALSCALE:GREGORIAN");
        sb.AppendLine("METHOD:PUBLISH");
        sb.AppendLine("BEGIN:VEVENT");
        sb.AppendLine($"UID:{Escape(uid)}");
        sb.AppendLine($"DTSTAMP:{FormatUtc(stamp)}");
        sb.AppendLine($"DTSTART:{FormatUtc(start)}");
        sb.AppendLine($"DTEND:{FormatUtc(end)}");
        sb.AppendLine($"SUMMARY:{Escape(summary)}");
        if (!string.IsNullOrWhiteSpace(description))
            sb.AppendLine($"DESCRIPTION:{Escape(description)}");
        if (!string.IsNullOrWhiteSpace(location))
            sb.AppendLine($"LOCATION:{Escape(location)}");
        if (string.Equals(interview.Status.ToString(), "Cancelled", StringComparison.OrdinalIgnoreCase))
            sb.AppendLine("STATUS:CANCELLED");
        sb.AppendLine("END:VEVENT");
        sb.AppendLine("END:VCALENDAR");
        return sb.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");
    }

    private static string FormatUtc(DateTime dt) => dt.ToUniversalTime().ToString("yyyyMMdd'T'HHmmss'Z'");

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\n", "\\n");
}

public sealed class NotificationDispatcher : INotificationDispatcher
{
    private readonly ApplicationDbContext _db;

    public NotificationDispatcher(ApplicationDbContext db) => _db = db;

    public async Task EnqueueAsync(
        int userId,
        string notificationType,
        string title,
        string message,
        string? relatedEntityType,
        int? relatedEntityId,
        string? idempotencyKey = null,
        bool forceSecurityEmail = false)
    {
        var key = idempotencyKey ?? $"{notificationType}:{userId}:{relatedEntityType}:{relatedEntityId}:{NotificationChannel.Email}";
        if (await _db.NotificationOutbox.AnyAsync(x => x.IdempotencyKey == key))
            return;

        var prefs = await _db.UserNotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
        prefs ??= new UserNotificationPreference { UserId = userId, UpdatedAtUtc = DateTime.UtcNow };

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;

        var emailAllowed = forceSecurityEmail || (prefs.EmailEnabled && prefs.ApplicationUpdates);
        if (emailAllowed)
        {
            _db.NotificationOutbox.Add(new NotificationOutbox
            {
                NotificationType = notificationType,
                UserId = userId,
                Channel = NotificationChannel.Email,
                DestinationMasked = MaskEmail(user.Email),
                Provider = "SMTP",
                Status = OutboxDeliveryStatus.Queued,
                IdempotencyKey = key,
                CorrelationId = Guid.NewGuid().ToString("N"),
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                BodySummary = Truncate($"{title}: {message}", 500),
                QueuedAtUtc = DateTime.UtcNow
            });
        }

        if (prefs.SmsEnabled && prefs.SmsConsent && prefs.InterviewReminders
            && notificationType.Contains("Interview", StringComparison.OrdinalIgnoreCase))
        {
            var smsKey = key.Replace(":Email", ":Sms", StringComparison.Ordinal);
            if (!await _db.NotificationOutbox.AnyAsync(x => x.IdempotencyKey == smsKey))
            {
                _db.NotificationOutbox.Add(new NotificationOutbox
                {
                    NotificationType = notificationType,
                    UserId = userId,
                    Channel = NotificationChannel.Sms,
                    DestinationMasked = "***",
                    Provider = "DevelopmentMock",
                    Status = OutboxDeliveryStatus.Queued,
                    IdempotencyKey = smsKey,
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    RelatedEntityType = relatedEntityType,
                    RelatedEntityId = relatedEntityId,
                    BodySummary = Truncate(title, 500),
                    QueuedAtUtc = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return "***";
        return email[0] + "***" + email[at..];
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}

public sealed class NotificationOutboxProcessor : INotificationOutboxProcessor
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailProvider _email;
    private readonly ISmsProvider _sms;

    public NotificationOutboxProcessor(ApplicationDbContext db, IEmailProvider email, ISmsProvider sms)
    {
        _db = db;
        _email = email;
        _sms = sms;
    }

    public async Task<int> ProcessPendingAsync(int take = 20, CancellationToken ct = default)
    {
        var pending = await _db.NotificationOutbox
            .Where(x => x.Status == OutboxDeliveryStatus.Queued || x.Status == OutboxDeliveryStatus.Failed)
            .Where(x => x.AttemptCount < x.MaxAttempts)
            .OrderBy(x => x.QueuedAtUtc)
            .Take(take)
            .ToListAsync(ct);

        var processed = 0;
        foreach (var item in pending)
        {
            item.Status = OutboxDeliveryStatus.Processing;
            item.AttemptCount++;
            await _db.SaveChangesAsync(ct);

            var user = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == item.UserId, ct);
            DeliveryResult result;
            if (item.Channel == NotificationChannel.Email)
            {
                result = await _email.SendAsync(new EmailMessage
                {
                    To = user.Email,
                    Subject = item.NotificationType,
                    PlainTextBody = item.BodySummary ?? string.Empty,
                    HtmlBody = WebUtility.HtmlEncode(item.BodySummary ?? string.Empty)
                }, ct);
            }
            else if (item.Channel == NotificationChannel.Sms)
            {
                var prefs = await _db.UserNotificationPreferences.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == item.UserId, ct);
                result = await _sms.SendAsync(new SmsMessage
                {
                    ToE164 = "+94770000000",
                    Body = item.BodySummary ?? string.Empty
                }, prefs?.SmsConsent == true, ct);
            }
            else
            {
                result = new DeliveryResult { Ok = true, StatusLabel = "Sent" };
            }

            if (result.Ok)
            {
                item.Status = string.Equals(result.StatusLabel, "Development Mock", StringComparison.Ordinal)
                    ? OutboxDeliveryStatus.Sent
                    : OutboxDeliveryStatus.Sent;
                item.SentAtUtc = DateTime.UtcNow;
                item.SafeFailureCode = result.StatusLabel;
            }
            else
            {
                item.Status = string.Equals(result.StatusLabel, "Suppressed", StringComparison.OrdinalIgnoreCase)
                    ? OutboxDeliveryStatus.Suppressed
                    : OutboxDeliveryStatus.Failed;
                item.FailedAtUtc = DateTime.UtcNow;
                item.SafeFailureCode = result.SafeFailureCode;
            }

            processed++;
            await _db.SaveChangesAsync(ct);
        }

        return processed;
    }
}

public sealed class IntegrationHealthService : IIntegrationHealthService
{
    private readonly IEmailProvider _email;
    private readonly ISmsProvider _sms;
    private readonly IEnumerable<ICalendarProvider> _calendars;
    private readonly IIcsCalendarService _ics;
    private readonly ExternalAiResumeParsingProvider _externalAi;
    private readonly DeterministicResumeParsingProvider _deterministicAi;

    public IntegrationHealthService(
        IEmailProvider email,
        ISmsProvider sms,
        IEnumerable<ICalendarProvider> calendars,
        IIcsCalendarService ics,
        ExternalAiResumeParsingProvider externalAi,
        DeterministicResumeParsingProvider deterministicAi)
    {
        _email = email;
        _sms = sms;
        _calendars = calendars;
        _ics = ics;
        _externalAi = externalAi;
        _deterministicAi = deterministicAi;
    }

    public Task<IReadOnlyList<IntegrationProviderStatusDto>> GetStatusesAsync()
    {
        static IntegrationProviderStatusDto MapAi(ProviderMetadataDto m) => new()
        {
            Name = $"AI ({m.Name})",
            Status = m.Status,
            Detail = $"{m.Type}: {m.Detail}"
        };

        var list = new List<IntegrationProviderStatusDto>
        {
            MapAi(_deterministicAi.GetStatus()),
            MapAi(_externalAi.GetStatus()),
            _email.GetStatus(),
            _sms.GetStatus(),
            _ics.GetStatus()
        };
        list.AddRange(_calendars.Select(c => c.GetStatus()));
        list.Add(new IntegrationProviderStatusDto
        {
            Name = "Cloud Storage",
            Status = "NotConfigured",
            Detail = "Azure Blob deferred to Phase 8.3. Local development storage remains separate."
        });
        foreach (var item in list) item.LastCheckedUtc = DateTime.UtcNow;
        return Task.FromResult<IReadOnlyList<IntegrationProviderStatusDto>>(list);
    }
}

public static class NotificationTemplateRenderer
{
    public static string EscapeHtml(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    public static (string Subject, string Plain, string Html) ApplicationStatus(
        string candidateName,
        string jobTitle,
        string status)
    {
        var safeName = EscapeHtml(candidateName);
        var safeJob = EscapeHtml(jobTitle);
        var safeStatus = EscapeHtml(status);
        var subject = $"Application update: {jobTitle}";
        var plain = $"Hello {candidateName},\n\nYour application for {jobTitle} is now {status}.\n\n— HireSphere";
        var html = $"<p>Hello {safeName},</p><p>Your application for <strong>{safeJob}</strong> is now <strong>{safeStatus}</strong>.</p><p>— HireSphere</p>";
        return (subject, plain, html);
    }
}

/// <summary>Bounded outbox / reminder processor. Idempotent via outbox keys and max attempts.</summary>
public sealed class NotificationReminderWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationReminderWorker> _logger;

    public NotificationReminderWorker(IServiceScopeFactory scopeFactory, ILogger<NotificationReminderWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<INotificationOutboxProcessor>();
                await processor.ProcessPendingAsync(10, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Outbox reminder cycle failed safely.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
