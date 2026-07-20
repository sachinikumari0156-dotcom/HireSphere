using HireSphere.API.Models.Enums;

namespace HireSphere.API.Models;

public class NotificationOutbox
{
    public int Id { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public int UserId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string? DestinationMasked { get; set; }
    public string Provider { get; set; } = string.Empty;
    public OutboxDeliveryStatus Status { get; set; } = OutboxDeliveryStatus.Queued;
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? SafeFailureCode { get; set; }
    public string? BodySummary { get; set; }
    public DateTime QueuedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SentAtUtc { get; set; }
    public DateTime? FailedAtUtc { get; set; }
    public User User { get; set; } = null!;
}

public class UserNotificationPreference
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; }
    public bool InterviewReminders { get; set; } = true;
    public bool ApplicationUpdates { get; set; } = true;
    public bool AssessmentReminders { get; set; } = true;
    public bool SmsConsent { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
}
