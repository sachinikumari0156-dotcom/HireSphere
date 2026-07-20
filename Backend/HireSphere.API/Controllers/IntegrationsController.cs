using HireSphere.API.Data;
using HireSphere.API.DTOs.Integrations;
using HireSphere.API.Models;
using HireSphere.API.Services;
using HireSphere.API.Services.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class IntegrationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IIcsCalendarService _ics;
    private readonly IIntegrationHealthService _health;
    private readonly INotificationOutboxProcessor _processor;
    private readonly IEnumerable<ICalendarProvider> _calendars;

    public IntegrationsController(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        IIcsCalendarService ics,
        IIntegrationHealthService health,
        INotificationOutboxProcessor processor,
        IEnumerable<ICalendarProvider> calendars)
    {
        _db = db;
        _currentUser = currentUser;
        _ics = ics;
        _health = health;
        _processor = processor;
        _calendars = calendars;
    }

    [HttpGet("users/notification-preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        if (_currentUser.UserId is not int userId) return Unauthorized();
        var prefs = await EnsurePrefsAsync(userId);
        return Ok(MapPrefs(prefs));
    }

    [HttpPut("users/notification-preferences")]
    public async Task<IActionResult> PutPreferences([FromBody] NotificationPreferencesDto dto)
    {
        if (_currentUser.UserId is not int userId) return Unauthorized();
        var prefs = await EnsurePrefsAsync(userId);
        prefs.EmailEnabled = dto.EmailEnabled;
        prefs.SmsEnabled = dto.SmsEnabled;
        prefs.InterviewReminders = dto.InterviewReminders;
        prefs.ApplicationUpdates = dto.ApplicationUpdates;
        prefs.AssessmentReminders = dto.AssessmentReminders;
        prefs.SmsConsent = dto.SmsConsent;
        prefs.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(MapPrefs(prefs));
    }

    [HttpGet("notifications/deliveries")]
    public async Task<IActionResult> MyDeliveries()
    {
        if (_currentUser.UserId is not int userId) return Unauthorized();
        var items = await _db.NotificationOutbox.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.QueuedAtUtc)
            .Take(50)
            .Select(x => new NotificationDeliveryDto
            {
                Id = x.Id,
                NotificationType = x.NotificationType,
                Channel = x.Channel.ToString(),
                Status = x.Status.ToString(),
                Provider = x.Provider,
                DestinationMasked = x.DestinationMasked,
                AttemptCount = x.AttemptCount,
                SafeFailureCode = x.SafeFailureCode,
                QueuedAtUtc = x.QueuedAtUtc,
                SentAtUtc = x.SentAtUtc
            }).ToListAsync();
        return Ok(items);
    }

    [HttpGet("interviews/{id:int}/calendar.ics")]
    public async Task<IActionResult> DownloadIcs(int id)
    {
        if (_currentUser.UserId is not int userId) return Unauthorized();
        var interview = await _db.Interviews.AsNoTracking()
            .Include(i => i.Application).ThenInclude(a => a.Job)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (interview is null) return NotFound(new { message = "Interview not found." });

        var allowed = interview.RecruiterUserId == userId
            || interview.HiringManagerUserId == userId
            || interview.Application.CandidateId == userId
            || await _db.InterviewParticipants.AnyAsync(p => p.InterviewId == id && p.UserId == userId);
        if (!allowed) return Forbid();

        var ics = _ics.GenerateIcs(
            interview,
            $"Interview: {interview.Application.Job.Title}",
            $"HireSphere interview ({interview.InterviewType}). Time zone: {interview.TimeZoneId}.",
            interview.MeetingLink);
        return File(System.Text.Encoding.UTF8.GetBytes(ics), "text/calendar", $"interview-{id}.ics");
    }

    [HttpPost("interviews/{id:int}/calendar/sync")]
    public async Task<IActionResult> SyncCalendar(int id)
    {
        if (_currentUser.UserId is not int userId) return Unauthorized();
        var interview = await _db.Interviews.FirstOrDefaultAsync(i => i.Id == id);
        if (interview is null) return NotFound(new { message = "Interview not found." });
        if (interview.RecruiterUserId != userId && interview.HiringManagerUserId != userId)
            return Forbid();

        var google = _calendars.First(c => c.Name.Contains("Google", StringComparison.OrdinalIgnoreCase)).GetStatus();
        var outlook = _calendars.First(c => c.Name.Contains("Outlook", StringComparison.OrdinalIgnoreCase)).GetStatus();
        interview.CalendarSyncStatus = "InternalSynced";
        await _db.SaveChangesAsync();
        return Ok(new
        {
            internalCalendar = "Healthy",
            googleCalendar = google.Status,
            outlookCalendar = outlook.Status,
            calendarSyncStatus = interview.CalendarSyncStatus
        });
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpGet("admin/integrations/status")]
    public async Task<IActionResult> AdminStatus()
    {
        var statuses = await _health.GetStatusesAsync();
        var json = System.Text.Json.JsonSerializer.Serialize(statuses);
        if (json.Contains("Password", StringComparison.OrdinalIgnoreCase)
            || json.Contains("ApiKey", StringComparison.OrdinalIgnoreCase)
            || json.Contains("Secret", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(500, new { message = "Status payload failed secret scrub." });
        }
        return Ok(statuses);
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpPost("admin/integrations/{provider}/health-check")]
    public async Task<IActionResult> HealthCheck(string provider)
    {
        var statuses = await _health.GetStatusesAsync();
        var match = statuses.FirstOrDefault(s =>
            s.Name.Contains(provider, StringComparison.OrdinalIgnoreCase));
        return match is null ? NotFound(new { message = "Provider not found." }) : Ok(match);
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpGet("admin/notifications/failed")]
    public async Task<IActionResult> FailedNotifications()
    {
        var items = await _db.NotificationOutbox.AsNoTracking()
            .Where(x => x.Status == Models.Enums.OutboxDeliveryStatus.Failed)
            .OrderByDescending(x => x.FailedAtUtc)
            .Take(100)
            .Select(x => new NotificationDeliveryDto
            {
                Id = x.Id,
                NotificationType = x.NotificationType,
                Channel = x.Channel.ToString(),
                Status = x.Status.ToString(),
                Provider = x.Provider,
                DestinationMasked = x.DestinationMasked,
                AttemptCount = x.AttemptCount,
                SafeFailureCode = x.SafeFailureCode,
                QueuedAtUtc = x.QueuedAtUtc,
                SentAtUtc = x.SentAtUtc
            }).ToListAsync();
        return Ok(items);
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpPost("admin/notifications/{id:int}/retry")]
    public async Task<IActionResult> Retry(int id)
    {
        var item = await _db.NotificationOutbox.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound(new { message = "Delivery not found." });
        item.Status = Models.Enums.OutboxDeliveryStatus.Queued;
        item.SafeFailureCode = null;
        await _db.SaveChangesAsync();
        var processed = await _processor.ProcessPendingAsync(5);
        return Ok(new { retried = true, processed });
    }

    private async Task<UserNotificationPreference> EnsurePrefsAsync(int userId)
    {
        var prefs = await _db.UserNotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
        if (prefs is not null) return prefs;
        prefs = new UserNotificationPreference { UserId = userId, UpdatedAtUtc = DateTime.UtcNow };
        _db.UserNotificationPreferences.Add(prefs);
        await _db.SaveChangesAsync();
        return prefs;
    }

    private static NotificationPreferencesDto MapPrefs(UserNotificationPreference p) => new()
    {
        EmailEnabled = p.EmailEnabled,
        SmsEnabled = p.SmsEnabled,
        InterviewReminders = p.InterviewReminders,
        ApplicationUpdates = p.ApplicationUpdates,
        AssessmentReminders = p.AssessmentReminders,
        SmsConsent = p.SmsConsent
    };
}
