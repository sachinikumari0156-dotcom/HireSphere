namespace HireSphere.API.Models.Enums;

public enum NotificationChannel
{
    InApp = 0,
    Email = 1,
    Sms = 2
}

public enum OutboxDeliveryStatus
{
    Queued = 0,
    Processing = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Cancelled = 5,
    Suppressed = 6
}
