using HireSphere.API.Data;
using HireSphere.API.Models;

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

    public NotificationWriter(ApplicationDbContext db)
    {
        _db = db;
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
