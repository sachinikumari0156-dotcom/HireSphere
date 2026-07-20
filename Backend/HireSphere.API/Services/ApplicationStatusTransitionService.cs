using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface IApplicationStatusTransitionService
{
    Task<(bool Ok, string? Error)> TransitionAsync(
        int applicationId,
        ApplicationStatus to,
        int actingUserId,
        string? notes,
        bool notifyCandidate = true);
}

public sealed class ApplicationStatusTransitionService : IApplicationStatusTransitionService
{
    private static readonly Dictionary<ApplicationStatus, HashSet<ApplicationStatus>> Allowed = new()
    {
        [ApplicationStatus.Pending] = new()
        {
            ApplicationStatus.UnderReview,
            ApplicationStatus.ManualReview,
            ApplicationStatus.Assessment,
            ApplicationStatus.Shortlisted,
            ApplicationStatus.Rejected,
            ApplicationStatus.Withdrawn
        },
        [ApplicationStatus.UnderReview] = new()
        {
            ApplicationStatus.ManualReview,
            ApplicationStatus.Assessment,
            ApplicationStatus.Shortlisted,
            ApplicationStatus.Rejected,
            ApplicationStatus.InterviewScheduled
        },
        [ApplicationStatus.ManualReview] = new()
        {
            ApplicationStatus.UnderReview,
            ApplicationStatus.Assessment,
            ApplicationStatus.Shortlisted,
            ApplicationStatus.Rejected
        },
        [ApplicationStatus.Assessment] = new()
        {
            ApplicationStatus.Shortlisted,
            ApplicationStatus.Rejected,
            ApplicationStatus.InterviewScheduled,
            ApplicationStatus.UnderReview
        },
        [ApplicationStatus.Shortlisted] = new()
        {
            ApplicationStatus.InterviewScheduled,
            ApplicationStatus.Rejected,
            ApplicationStatus.Offered,
            ApplicationStatus.UnderReview
        },
        [ApplicationStatus.InterviewScheduled] = new()
        {
            ApplicationStatus.Interviewed,
            ApplicationStatus.Rejected,
            ApplicationStatus.Shortlisted
        },
        [ApplicationStatus.Interviewed] = new()
        {
            ApplicationStatus.Offered,
            ApplicationStatus.Rejected,
            ApplicationStatus.Hired
        },
        [ApplicationStatus.Offered] = new()
        {
            ApplicationStatus.Hired,
            ApplicationStatus.Rejected
        },
        [ApplicationStatus.Hired] = new(),
        [ApplicationStatus.Rejected] = new() { ApplicationStatus.UnderReview },
        [ApplicationStatus.Withdrawn] = new()
    };

    private readonly ApplicationDbContext _db;
    private readonly INotificationWriter _notifications;

    public ApplicationStatusTransitionService(ApplicationDbContext db, INotificationWriter notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<(bool Ok, string? Error)> TransitionAsync(
        int applicationId,
        ApplicationStatus to,
        int actingUserId,
        string? notes,
        bool notifyCandidate = true)
    {
        var application = await _db.Applications
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application is null)
        {
            return (false, "Application not found.");
        }

        var from = application.Status;
        if (from == to)
        {
            return (true, null);
        }

        if (!Allowed.TryGetValue(from, out var next) || !next.Contains(to))
        {
            return (false, $"Cannot transition application from {from} to {to}.");
        }

        application.Status = to;
        application.UpdatedAtUtc = DateTime.UtcNow;
        application.StatusHistory.Add(new ApplicationStatusHistory
        {
            Status = to,
            ChangedAtUtc = DateTime.UtcNow,
            ChangedByUserId = actingUserId,
            Notes = Sanitize(notes)
        });

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = actingUserId,
            Action = "ApplicationStatusChanged",
            EntityType = "Application",
            EntityId = application.Id,
            Details = $"{from} → {to}",
            CreatedAtUtc = DateTime.UtcNow
        });

        if (notifyCandidate)
        {
            await _notifications.CreateAsync(
                application.CandidateId,
                NotificationCategories.ApplicationStatusUpdated,
                "Application status updated",
                $"Your application for {application.Job.Title} is now {to}.",
                "Application",
                application.Id,
                $"/candidate/applications/{application.Id}",
                saveChanges: false);
        }

        await _db.SaveChangesAsync();
        return (true, null);
    }

    private static string? Sanitize(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var trimmed = notes.Trim();
        return trimmed.Length > 2000 ? trimmed[..2000] : trimmed;
    }
}
