namespace HireSphere.API.DTOs.Integrations;

public sealed class NotificationPreferencesDto
{
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; }
    public bool InterviewReminders { get; set; } = true;
    public bool ApplicationUpdates { get; set; } = true;
    public bool AssessmentReminders { get; set; } = true;
    public bool SmsConsent { get; set; }
}

public sealed class NotificationDeliveryDto
{
    public int Id { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? DestinationMasked { get; set; }
    public int AttemptCount { get; set; }
    public string? SafeFailureCode { get; set; }
    public DateTime QueuedAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
}
