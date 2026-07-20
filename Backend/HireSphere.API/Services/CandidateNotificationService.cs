using HireSphere.API.Data;
using HireSphere.API.DTOs.Candidate;
using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface ICandidateNotificationService
{
    Task<(bool Ok, string? Error, CandidateNotificationListDto? Result)> ListAsync(int? take = null);

    Task<(bool Ok, string? Error, CandidateNotificationDto? Result)> MarkReadAsync(int notificationId);

    Task<(bool Ok, string? Error, CandidateNotificationListDto? Result)> MarkAllReadAsync();
}

public sealed class CandidateNotificationService : ICandidateNotificationService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CandidateNotificationService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<(bool Ok, string? Error, CandidateNotificationListDto? Result)> ListAsync(int? take = null)
    {
        var (ok, error, userId) = RequireUserId();
        if (!ok)
        {
            return (false, error, null);
        }

        var limit = Math.Clamp(take ?? 50, 1, 100);
        var rows = await _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(limit)
            .ToListAsync();

        var unread = await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        return (true, null, new CandidateNotificationListDto
        {
            UnreadCount = unread,
            Items = rows.Select(Map).ToList()
        });
    }

    public async Task<(bool Ok, string? Error, CandidateNotificationDto? Result)> MarkReadAsync(int notificationId)
    {
        var (ok, error, userId) = RequireUserId();
        if (!ok)
        {
            return (false, error, null);
        }

        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            return (false, "Notification not found.", null);
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _db.SaveChangesAsync();
        }

        return (true, null, Map(notification));
    }

    public async Task<(bool Ok, string? Error, CandidateNotificationListDto? Result)> MarkAllReadAsync()
    {
        var (ok, error, userId) = RequireUserId();
        if (!ok)
        {
            return (false, error, null);
        }

        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
        }

        if (unread.Count > 0)
        {
            await _db.SaveChangesAsync();
        }

        return await ListAsync();
    }

    private (bool Ok, string? Error, int UserId) RequireUserId()
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", 0);
        }

        return (true, null, userId);
    }

    private static CandidateNotificationDto Map(Notification n) => new()
    {
        Id = n.Id,
        Title = n.Title,
        Message = n.Message,
        Category = n.Category,
        RelatedEntityType = n.RelatedEntityType,
        RelatedEntityId = n.RelatedEntityId,
        LinkPath = n.LinkPath,
        IsRead = n.IsRead,
        CreatedAtUtc = n.CreatedAtUtc
    };
}
