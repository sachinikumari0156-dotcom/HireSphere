using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Services.Integrations;

namespace HireSphere.API.Services;

public interface INotificationWriter
{
    Task CreateAsync(
        int userId,
        string category,
        string title,
        string message,
        string? relatedEntityType = null,
        int? relatedEntityId = null,
        string? linkPath = null,
        bool saveChanges = true);
}

public sealed class NotificationWriter : INotificationWriter
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationDispatcher _dispatcher;

    public NotificationWriter(ApplicationDbContext db, INotificationDispatcher dispatcher)
    {
        _db = db;
        _dispatcher = dispatcher;
    }

    public async Task CreateAsync(
        int userId,
        string category,
        string title,
        string message,
        string? relatedEntityType = null,
        int? relatedEntityId = null,
        string? linkPath = null,
        bool saveChanges = true)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Category = category,
            Title = title,
            Message = message,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            LinkPath = linkPath,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        });

        if (saveChanges)
        {
            await _db.SaveChangesAsync();
        }

        // Outbox enqueue is best-effort after in-app notification; failures must not corrupt business state.
        try
        {
            var forceSecurity = category.Contains("Password", StringComparison.OrdinalIgnoreCase)
                || category.Contains("Security", StringComparison.OrdinalIgnoreCase);
            await _dispatcher.EnqueueAsync(
                userId,
                category,
                title,
                message,
                relatedEntityType,
                relatedEntityId,
                idempotencyKey: $"{category}:{userId}:{relatedEntityType}:{relatedEntityId}:Email",
                forceSecurityEmail: forceSecurity);
        }
        catch
        {
            // Prefer durable in-app notification over failing the parent workflow.
        }
    }
}

public static class NotificationCategories
{
    public const string ApplicationSubmitted = "ApplicationSubmitted";
    public const string ApplicationStatusUpdated = "ApplicationStatusUpdated";
    public const string AssessmentAssigned = "AssessmentAssigned";
    public const string InterviewScheduled = "InterviewScheduled";
    public const string InterviewUpdated = "InterviewUpdated";
}
