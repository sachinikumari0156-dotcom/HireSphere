using HireSphere.API.Data;
using HireSphere.API.DTOs.Candidate;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface ICandidateInterviewService
{
    Task<(bool Ok, string? Error, IReadOnlyList<CandidateInterviewListItemDto>? Result)> ListAsync();

    Task<(bool Ok, string? Error, CandidateInterviewDetailDto? Result)> GetAsync(int interviewId);

    Task<(bool Ok, string? Error, CandidateInterviewDetailDto? Result)> ConfirmAsync(int interviewId);

    Task<(bool Ok, string? Error, CandidateInterviewDetailDto? Result)> RequestRescheduleAsync(
        int interviewId,
        InterviewRescheduleRequestDto dto);

    Task<(bool Ok, string? Error, CandidateInterviewDetailDto? Result)> DeclineAsync(
        int interviewId,
        InterviewDeclineDto dto);
}

public sealed class CandidateInterviewService : ICandidateInterviewService
{
    private static readonly InterviewStatus[] RespondableStatuses =
    {
        InterviewStatus.Scheduled,
        InterviewStatus.Rescheduled,
        InterviewStatus.Proposed,
        InterviewStatus.Confirmed
    };

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationWriter _notifications;

    public CandidateInterviewService(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        INotificationWriter notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<CandidateInterviewListItemDto>? Result)> ListAsync()
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var interviews = await QueryOwned(profile.UserId)
            .AsNoTracking()
            .OrderBy(i => i.InterviewDate)
            .ToListAsync();

        return (true, null, interviews.Select(MapListItem).ToList());
    }

    public async Task<(bool Ok, string? Error, CandidateInterviewDetailDto? Result)> GetAsync(int interviewId)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var interview = await QueryOwned(profile.UserId)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == interviewId);

        if (interview == null)
        {
            return (false, "Interview not found.", null);
        }

        return (true, null, MapDetail(interview));
    }

    public async Task<(bool Ok, string? Error, CandidateInterviewDetailDto? Result)> ConfirmAsync(int interviewId)
    {
        return await RespondAsync(
            interviewId,
            InterviewCandidateResponse.Confirmed,
            reason: null,
            preferredNote: null);
    }

    public async Task<(bool Ok, string? Error, CandidateInterviewDetailDto? Result)> RequestRescheduleAsync(
        int interviewId,
        InterviewRescheduleRequestDto dto)
    {
        var reason = dto.Reason?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(reason))
        {
            return (false, "A reason is required to request a reschedule.", null);
        }

        var preferred = dto.PreferredTimesNote?.Trim();
        var combined = string.IsNullOrWhiteSpace(preferred)
            ? reason
            : $"{reason}\nPreferred times: {preferred}";

        return await RespondAsync(
            interviewId,
            InterviewCandidateResponse.RescheduleRequested,
            reason: combined,
            preferredNote: preferred);
    }

    public async Task<(bool Ok, string? Error, CandidateInterviewDetailDto? Result)> DeclineAsync(
        int interviewId,
        InterviewDeclineDto dto)
    {
        var reason = dto.Reason?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(reason))
        {
            return (false, "A reason is required to decline an interview.", null);
        }

        return await RespondAsync(
            interviewId,
            InterviewCandidateResponse.Declined,
            reason: reason,
            preferredNote: null);
    }

    private async Task<(bool Ok, string? Error, CandidateInterviewDetailDto? Result)> RespondAsync(
        int interviewId,
        InterviewCandidateResponse response,
        string? reason,
        string? preferredNote)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var interview = await QueryOwned(profile.UserId)
            .FirstOrDefaultAsync(i => i.Id == interviewId);

        if (interview == null)
        {
            return (false, "Interview not found.", null);
        }

        if (!RespondableStatuses.Contains(interview.Status))
        {
            return (false, "This interview can no longer be updated.", null);
        }

        if (interview.CandidateResponse is InterviewCandidateResponse.Confirmed
            or InterviewCandidateResponse.Declined)
        {
            return (false, "You have already responded to this interview.", null);
        }

        var now = DateTime.UtcNow;
        interview.CandidateResponse = response;
        interview.CandidateResponseReason = reason;
        interview.CandidateRespondedAtUtc = now;
        interview.UpdatedAtUtc = now;

        if (response == InterviewCandidateResponse.Declined)
        {
            interview.Status = InterviewStatus.Cancelled;
        }
        else if (response == InterviewCandidateResponse.RescheduleRequested)
        {
            interview.Status = InterviewStatus.Rescheduled;
        }

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = profile.UserId,
            Action = $"Interview{response}",
            EntityType = nameof(Interview),
            EntityId = interview.Id,
            Details = reason,
            CreatedAtUtc = now
        });

        var title = response switch
        {
            InterviewCandidateResponse.Confirmed => "Interview confirmed",
            InterviewCandidateResponse.RescheduleRequested => "Interview reschedule requested",
            InterviewCandidateResponse.Declined => "Interview declined",
            _ => "Interview updated"
        };

        await _notifications.CreateAsync(
            profile.UserId,
            NotificationCategories.InterviewUpdated,
            title,
            $"Your response for the interview on {interview.InterviewDate:u} ({interview.TimeZoneId}) was recorded.",
            nameof(Interview),
            interview.Id,
            $"/candidate/interviews/{interview.Id}",
            saveChanges: false);

        await _db.SaveChangesAsync();
        return await GetAsync(interview.Id);
    }

    private IQueryable<Interview> QueryOwned(int candidateUserId) =>
        _db.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a.Job)
            .Where(i => i.Application.CandidateId == candidateUserId);

    private static bool MeetingInfoAvailable(Interview interview) =>
        !interview.RequireConfirmForMeetingInfo
        || interview.CandidateResponse == InterviewCandidateResponse.Confirmed
        || interview.Status is InterviewStatus.InProgress or InterviewStatus.Completed;

    private static bool CanRespond(Interview interview) =>
        RespondableStatuses.Contains(interview.Status)
        && interview.CandidateResponse is InterviewCandidateResponse.Pending
            or InterviewCandidateResponse.RescheduleRequested;

    private static CandidateInterviewListItemDto MapListItem(Interview i) => new()
    {
        Id = i.Id,
        ApplicationId = i.ApplicationId,
        JobTitle = i.Application.Job.Title,
        InterviewDateUtc = i.InterviewDate,
        TimeZoneId = i.TimeZoneId,
        InterviewType = i.InterviewType,
        Status = i.Status,
        CandidateResponse = i.CandidateResponse,
        MeetingInfoAvailable = MeetingInfoAvailable(i)
    };

    private static CandidateInterviewDetailDto MapDetail(Interview i)
    {
        var meetingOk = MeetingInfoAvailable(i);
        var canRespond = CanRespond(i);

        return new CandidateInterviewDetailDto
        {
            Id = i.Id,
            ApplicationId = i.ApplicationId,
            JobTitle = i.Application.Job.Title,
            InterviewDateUtc = i.InterviewDate,
            TimeZoneId = i.TimeZoneId,
            InterviewType = i.InterviewType,
            Status = i.Status,
            CandidateResponse = i.CandidateResponse,
            CandidateResponseReason = i.CandidateResponseReason,
            CandidateRespondedAtUtc = i.CandidateRespondedAtUtc,
            MeetingInfoAvailable = meetingOk,
            MeetingLink = meetingOk ? NullIfEmpty(i.MeetingLink) : null,
            MeetingInstructions = meetingOk ? i.MeetingInstructions : null,
            CanConfirm = canRespond && i.CandidateResponse != InterviewCandidateResponse.Confirmed,
            CanRequestReschedule = canRespond,
            CanDecline = canRespond && i.CandidateResponse != InterviewCandidateResponse.Declined
        };
    }

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private async Task<(bool Ok, string? Error, CandidateProfile? Profile)> RequireProfileAsync()
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        var profile = await _db.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            return (false, "Candidate profile not found.", null);
        }

        return (true, null, profile);
    }
}
