using HireSphere.API.Data;
using HireSphere.API.DTOs.Recruiter;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface IRecruiterPhase53Service
{
    Task<(bool Ok, string? Error, IReadOnlyList<RecruiterInterviewDetailDto>? Result)> ListInterviewsAsync();

    Task<(bool Ok, string? Error, RecruiterInterviewDetailDto? Result)> GetInterviewAsync(int id);

    Task<(bool Ok, string? Error, ScheduleInterviewResultDto? Result)> ScheduleAsync(ScheduleInterviewDto dto);

    Task<(bool Ok, string? Error, ScheduleInterviewResultDto? Result)> RescheduleAsync(int id, RescheduleInterviewDto dto);

    Task<(bool Ok, string? Error, RecruiterInterviewDetailDto? Result)> ChangeStatusAsync(
        int id,
        ChangeInterviewStatusDto dto);

    Task<(bool Ok, string? Error, RecruiterInterviewDetailDto? Result)> ResolveRescheduleAsync(
        int id,
        bool approve,
        RescheduleInterviewDto? newSlot);

    Task<(bool Ok, string? Error, RecruiterReportSummaryDto? Result)> GetReportSummaryAsync(ReportFilterQuery filter);

    Task<(bool Ok, string? Error, CsvExportResult? Result)> ExportReportCsvAsync(ReportFilterQuery filter);
}

public sealed class RecruiterPhase53Service : IRecruiterPhase53Service
{
    private static readonly InterviewStatus[] ActiveConflictStatuses =
    {
        InterviewStatus.Proposed,
        InterviewStatus.Scheduled,
        InterviewStatus.Confirmed,
        InterviewStatus.RescheduleRequested,
        InterviewStatus.Rescheduled,
        InterviewStatus.InProgress
    };

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IResourceAuthorizationService _authz;
    private readonly IApplicationStatusTransitionService _transitions;
    private readonly INotificationWriter _notifications;

    public RecruiterPhase53Service(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        IResourceAuthorizationService authz,
        IApplicationStatusTransitionService transitions,
        INotificationWriter notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _authz = authz;
        _transitions = transitions;
        _notifications = notifications;
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<RecruiterInterviewDetailDto>? Result)> ListInterviewsAsync()
    {
        if (_currentUser.OrganizationId is not int orgId || _currentUser.UserId is not int userId)
        {
            return (false, "Organization context is required.", null);
        }

        var interviews = await OrgInterviewsQuery(orgId, userId)
            .AsNoTracking()
            .Include(i => i.Application).ThenInclude(a => a.Candidate)
            .Include(i => i.Application).ThenInclude(a => a.Job)
            .Include(i => i.HiringManager)
            .Include(i => i.Participants)
            .OrderBy(i => i.InterviewDate)
            .Take(200)
            .ToListAsync();

        return (true, null, interviews.Select(MapInterview).ToList());
    }

    public async Task<(bool Ok, string? Error, RecruiterInterviewDetailDto? Result)> GetInterviewAsync(int id)
    {
        var interview = await LoadOwnedInterviewAsync(id);
        if (interview is null)
        {
            return (false, "Interview not found or access denied.", null);
        }

        return (true, null, MapInterview(interview));
    }

    public async Task<(bool Ok, string? Error, ScheduleInterviewResultDto? Result)> ScheduleAsync(
        ScheduleInterviewDto dto)
    {
        if (_currentUser.UserId is not int userId || _currentUser.OrganizationId is not int orgId)
        {
            return (false, "Organization context is required.", null);
        }

        if (!await _authz.RecruiterCanAccessApplicationAsync(dto.ApplicationId))
        {
            return (false, "Application not found or access denied.", null);
        }

        if (dto.DurationMinutes < 15)
        {
            return (false, "Duration must be at least 15 minutes.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.TimeZoneId))
        {
            return (false, "Timezone is required.", null);
        }

        var application = await _db.Applications
            .Include(a => a.Job)
            .Include(a => a.Candidate)
            .FirstAsync(a => a.Id == dto.ApplicationId);

        if (dto.HiringManagerUserId is int hmId)
        {
            var hmOk = await _db.HiringManagerProfiles.AnyAsync(p =>
                p.UserId == hmId && p.OrganizationId == orgId);
            if (!hmOk)
            {
                return (false, "Hiring Manager is not authorized for this organization.", null);
            }
        }

        var start = DateTime.SpecifyKind(dto.StartAtUtc, DateTimeKind.Utc);
        var end = start.AddMinutes(dto.DurationMinutes);
        var participants = dto.ParticipantUserIds.Distinct().ToList();
        var conflicts = await DetectConflictsAsync(
            application.CandidateId,
            userId,
            dto.HiringManagerUserId,
            participants,
            start,
            end,
            excludeInterviewId: null);

        if (conflicts.Count > 0 && !dto.ForceDespiteConflicts)
        {
            return (true, null, new ScheduleInterviewResultDto
            {
                Scheduled = false,
                Conflicts = conflicts
            });
        }

        var interview = new Interview
        {
            ApplicationId = dto.ApplicationId,
            RecruiterUserId = userId,
            HiringManagerUserId = dto.HiringManagerUserId,
            InterviewDate = start,
            DurationMinutes = dto.DurationMinutes,
            TimeZoneId = dto.TimeZoneId.Trim(),
            InterviewType = string.IsNullOrWhiteSpace(dto.InterviewType) ? "Video" : dto.InterviewType.Trim(),
            MeetingLink = dto.MeetingLink?.Trim() ?? string.Empty,
            MeetingInstructions = Sanitize(dto.MeetingInstructions),
            PhysicalLocation = Sanitize(dto.PhysicalLocation),
            InternalNotes = Sanitize(dto.InternalNotes),
            Status = InterviewStatus.Scheduled,
            CalendarSyncStatus = "NotConfigured",
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var participantId in participants)
        {
            interview.Participants.Add(new InterviewParticipant
            {
                UserId = participantId,
                ParticipantRole = "Participant"
            });
        }

        _db.Interviews.Add(interview);
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "InterviewScheduled",
            EntityType = "Interview",
            EntityId = null,
            Details = $"Application {dto.ApplicationId} at {start:O}",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _notifications.CreateAsync(
            application.CandidateId,
            NotificationCategories.InterviewScheduled,
            "Interview invitation",
            $"An interview was scheduled for {application.Job.Title}.",
            "Interview",
            null,
            "/candidate/interviews",
            saveChanges: false);

        if (application.Status is ApplicationStatus.Shortlisted
            or ApplicationStatus.Assessment
            or ApplicationStatus.UnderReview
            or ApplicationStatus.ManualReview)
        {
            await _transitions.TransitionAsync(
                application.Id,
                ApplicationStatus.InterviewScheduled,
                userId,
                "Interview scheduled",
                notifyCandidate: false);
        }
        else
        {
            await _db.SaveChangesAsync();
        }

        var loaded = await LoadOwnedInterviewAsync(interview.Id);
        return (true, null, new ScheduleInterviewResultDto
        {
            Scheduled = true,
            Conflicts = conflicts,
            Interview = MapInterview(loaded!)
        });
    }

    public async Task<(bool Ok, string? Error, ScheduleInterviewResultDto? Result)> RescheduleAsync(
        int id,
        RescheduleInterviewDto dto)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        var interview = await LoadOwnedInterviewAsync(id, tracking: true);
        if (interview is null)
        {
            return (false, "Interview not found or access denied.", null);
        }

        var start = DateTime.SpecifyKind(dto.StartAtUtc, DateTimeKind.Utc);
        var end = start.AddMinutes(dto.DurationMinutes);
        var participantIds = interview.Participants.Select(p => p.UserId).ToList();
        var conflicts = await DetectConflictsAsync(
            interview.Application.CandidateId,
            interview.RecruiterUserId ?? userId,
            interview.HiringManagerUserId,
            participantIds,
            start,
            end,
            excludeInterviewId: id);

        if (conflicts.Count > 0 && !dto.ForceDespiteConflicts)
        {
            return (true, null, new ScheduleInterviewResultDto
            {
                Scheduled = false,
                Conflicts = conflicts
            });
        }

        interview.InterviewDate = start;
        interview.DurationMinutes = dto.DurationMinutes;
        interview.TimeZoneId = string.IsNullOrWhiteSpace(dto.TimeZoneId) ? interview.TimeZoneId : dto.TimeZoneId.Trim();
        interview.Status = InterviewStatus.Rescheduled;
        interview.CandidateResponse = InterviewCandidateResponse.Pending;
        interview.CandidateResponseReason = null;
        interview.CandidateRespondedAtUtc = null;
        interview.UpdatedAtUtc = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "InterviewRescheduled",
            EntityType = "Interview",
            EntityId = id,
            Details = dto.Reason,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _notifications.CreateAsync(
            interview.Application.CandidateId,
            NotificationCategories.InterviewUpdated,
            "Interview rescheduled",
            "Your interview time was updated. Please review and respond.",
            "Interview",
            id,
            $"/candidate/interviews/{id}",
            saveChanges: false);

        await _db.SaveChangesAsync();
        return (true, null, new ScheduleInterviewResultDto
        {
            Scheduled = true,
            Conflicts = conflicts,
            Interview = MapInterview(interview)
        });
    }

    public async Task<(bool Ok, string? Error, RecruiterInterviewDetailDto? Result)> ChangeStatusAsync(
        int id,
        ChangeInterviewStatusDto dto)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        var interview = await LoadOwnedInterviewAsync(id, tracking: true);
        if (interview is null)
        {
            return (false, "Interview not found or access denied.", null);
        }

        interview.Status = dto.Status;
        interview.UpdatedAtUtc = DateTime.UtcNow;
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "InterviewStatusChanged",
            EntityType = "Interview",
            EntityId = id,
            Details = $"{dto.Status}: {dto.Notes}",
            CreatedAtUtc = DateTime.UtcNow
        });

        if (dto.Status == InterviewStatus.Cancelled)
        {
            await _notifications.CreateAsync(
                interview.Application.CandidateId,
                NotificationCategories.InterviewUpdated,
                "Interview cancelled",
                "Your interview was cancelled by the recruiter.",
                "Interview",
                id,
                $"/candidate/interviews/{id}",
                saveChanges: false);
        }

        await _db.SaveChangesAsync();
        return (true, null, MapInterview(interview));
    }

    public async Task<(bool Ok, string? Error, RecruiterInterviewDetailDto? Result)> ResolveRescheduleAsync(
        int id,
        bool approve,
        RescheduleInterviewDto? newSlot)
    {
        var interview = await LoadOwnedInterviewAsync(id, tracking: true);
        if (interview is null)
        {
            return (false, "Interview not found or access denied.", null);
        }

        if (interview.CandidateResponse != InterviewCandidateResponse.RescheduleRequested
            && interview.Status != InterviewStatus.RescheduleRequested)
        {
            return (false, "No pending reschedule request.", null);
        }

        if (approve)
        {
            if (newSlot is null)
            {
                return (false, "New slot is required to approve a reschedule.", null);
            }

            var (ok, error, scheduled) = await RescheduleAsync(id, newSlot);
            if (!ok)
            {
                return (false, error, null);
            }

            return (true, null, scheduled?.Interview);
        }

        interview.Status = InterviewStatus.Scheduled;
        interview.CandidateResponse = InterviewCandidateResponse.Pending;
        interview.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, null, MapInterview(interview));
    }

    public async Task<(bool Ok, string? Error, RecruiterReportSummaryDto? Result)> GetReportSummaryAsync(
        ReportFilterQuery filter)
    {
        if (_currentUser.OrganizationId is not int orgId || _currentUser.UserId is not int userId)
        {
            return (false, "Organization context is required.", null);
        }

        var (from, to, rangeError) = NormalizeRange(filter);
        if (rangeError is not null)
        {
            return (false, rangeError, null);
        }

        var appsQuery = _db.Applications.AsNoTracking()
            .Include(a => a.Job)
            .Include(a => a.StatusHistory)
            .Include(a => a.Interviews)
            .Where(a => a.Job.OrganizationId == orgId || a.Job.RecruiterId == userId);

        if (filter.JobId is int jobId) appsQuery = appsQuery.Where(a => a.JobId == jobId);
        if (filter.DepartmentId is int deptId) appsQuery = appsQuery.Where(a => a.Job.DepartmentId == deptId);
        if (filter.RecruiterUserId is int recruiterId) appsQuery = appsQuery.Where(a => a.Job.RecruiterId == recruiterId);
        if (filter.ApplicationStatus is ApplicationStatus status) appsQuery = appsQuery.Where(a => a.Status == status);
        appsQuery = appsQuery.Where(a => a.AppliedDate >= from && a.AppliedDate <= to);

        var apps = await appsQuery.ToListAsync();
        var appIds = apps.Select(a => a.Id).ToList();

        var assignments = await _db.AssessmentAssignments.AsNoTracking()
            .Include(a => a.Attempts).ThenInclude(t => t.Result)
            .Where(a => a.ApplicationId != null && appIds.Contains(a.ApplicationId.Value))
            .ToListAsync();

        var shortlisted = apps.Count(a => a.Status == ApplicationStatus.Shortlisted
            || a.StatusHistory.Any(h => h.Status == ApplicationStatus.Shortlisted));
        var rejected = apps.Count(a => a.Status == ApplicationStatus.Rejected);
        var total = apps.Count;

        double? AvgDays(Func<Application, DateTime?> selector)
        {
            var values = apps
                .Select(a =>
                {
                    var target = selector(a);
                    return target is DateTime t ? (t - a.AppliedDate).TotalDays : (double?)null;
                })
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();
            return values.Count == 0 ? null : Math.Round(values.Average(), 2);
        }

        var summary = new RecruiterReportSummaryDto
        {
            ApplicationsTotal = total,
            NewApplicants = apps.Count(a => a.AppliedDate >= DateTime.UtcNow.AddDays(-7)),
            Shortlisted = shortlisted,
            Rejected = rejected,
            ShortlistRate = total == 0 ? 0 : Math.Round(shortlisted * 100.0 / total, 2),
            RejectionRate = total == 0 ? 0 : Math.Round(rejected * 100.0 / total, 2),
            AssessmentAssignments = assignments.Count,
            AssessmentCompletions = assignments.Count(a =>
                a.Attempts.Any(t => t.Status == AssessmentStatus.Completed)),
            InterviewsScheduled = apps.SelectMany(a => a.Interviews)
                .Count(i => i.Status != InterviewStatus.Cancelled),
            AverageDaysToScreen = AvgDays(a => a.StatusHistory
                .Where(h => h.Status is ApplicationStatus.UnderReview or ApplicationStatus.ManualReview)
                .Select(h => (DateTime?)h.ChangedAtUtc)
                .FirstOrDefault()),
            AverageDaysToShortlist = AvgDays(a => a.StatusHistory
                .Where(h => h.Status == ApplicationStatus.Shortlisted)
                .Select(h => (DateTime?)h.ChangedAtUtc)
                .FirstOrDefault()),
            AverageDaysToInterview = AvgDays(a => a.Interviews
                .OrderBy(i => i.CreatedAtUtc)
                .Select(i => (DateTime?)i.CreatedAtUtc)
                .FirstOrDefault()),
            ApplicationsByJob = apps.GroupBy(a => a.Job.Title)
                .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),
            ApplicationsByStatus = apps.GroupBy(a => a.Status.ToString())
                .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
                .ToList(),
            ApplicationsOverTime = apps.GroupBy(a => a.AppliedDate.Date.ToString("yyyy-MM-dd"))
                .OrderBy(g => g.Key)
                .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
                .ToList(),
            ScreeningOutcomes = apps.SelectMany(a => a.StatusHistory)
                .Where(h => h.Status is ApplicationStatus.UnderReview or ApplicationStatus.ManualReview
                    or ApplicationStatus.Shortlisted or ApplicationStatus.Rejected)
                .GroupBy(h => h.Status.ToString())
                .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
                .ToList(),
            AssessmentScoreRanges = BuildScoreRanges(assignments),
            InterviewStatuses = apps.SelectMany(a => a.Interviews)
                .GroupBy(i => i.Status.ToString())
                .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
                .ToList()
        };

        return (true, null, summary);
    }

    public async Task<(bool Ok, string? Error, CsvExportResult? Result)> ExportReportCsvAsync(
        ReportFilterQuery filter)
    {
        var (ok, error, summary) = await GetReportSummaryAsync(filter);
        if (!ok || summary is null)
        {
            return (false, error, null);
        }

        if (_currentUser.OrganizationId is not int orgId || _currentUser.UserId is not int userId)
        {
            return (false, "Organization context is required.", null);
        }

        var (from, to, rangeError) = NormalizeRange(filter);
        if (rangeError is not null)
        {
            return (false, rangeError, null);
        }

        var apps = await _db.Applications.AsNoTracking()
            .Include(a => a.Job)
            .Include(a => a.Candidate)
            .Where(a => (a.Job.OrganizationId == orgId || a.Job.RecruiterId == userId)
                && a.AppliedDate >= from && a.AppliedDate <= to)
            .Where(a => filter.JobId == null || a.JobId == filter.JobId)
            .ToListAsync();

        var rows = new List<string[]>
        {
            new[] { "ApplicationId", "CandidateName", "JobTitle", "Status", "AppliedAtUtc" }
        };

        foreach (var app in apps)
        {
            rows.Add(new[]
            {
                app.Id.ToString(),
                app.Candidate.FullName,
                app.Job.Title,
                app.Status.ToString(),
                app.AppliedDate.ToString("O")
            });
        }

        // Never include password hashes or secrets
        var csv = CsvEscaper.ToUtf8Csv(rows);
        var text = System.Text.Encoding.UTF8.GetString(csv);
        if (text.Contains("PasswordHash", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Jwt", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Export blocked: sensitive fields detected.", null);
        }

        return (true, null, new CsvExportResult
        {
            FileName = $"recruiter-report-{DateTime.UtcNow:yyyyMMddHHmmss}.csv",
            Content = csv
        });
    }

    private async Task<List<InterviewConflictDto>> DetectConflictsAsync(
        int candidateUserId,
        int recruiterUserId,
        int? hiringManagerUserId,
        IReadOnlyList<int> participantUserIds,
        DateTime startUtc,
        DateTime endUtc,
        int? excludeInterviewId)
    {
        var conflicts = new List<InterviewConflictDto>();
        var existing = await _db.Interviews
            .AsNoTracking()
            .Include(i => i.Application)
            .Include(i => i.Participants)
            .Where(i => ActiveConflictStatuses.Contains(i.Status)
                && (excludeInterviewId == null || i.Id != excludeInterviewId))
            .ToListAsync();

        bool Overlaps(Interview i)
        {
            var iEnd = i.InterviewDate.AddMinutes(i.DurationMinutes <= 0 ? 60 : i.DurationMinutes);
            return startUtc < iEnd && endUtc > i.InterviewDate;
        }

        foreach (var interview in existing.Where(Overlaps))
        {
            if (interview.Application.CandidateId == candidateUserId)
            {
                conflicts.Add(new InterviewConflictDto
                {
                    ConflictType = "Candidate",
                    UserId = candidateUserId,
                    ConflictingInterviewId = interview.Id,
                    Message = "Candidate already has an overlapping interview."
                });
            }

            if (interview.RecruiterUserId == recruiterUserId)
            {
                conflicts.Add(new InterviewConflictDto
                {
                    ConflictType = "Recruiter",
                    UserId = recruiterUserId,
                    ConflictingInterviewId = interview.Id,
                    Message = "Recruiter already has an overlapping interview."
                });
            }

            if (hiringManagerUserId is int hm
                && interview.HiringManagerUserId == hm)
            {
                conflicts.Add(new InterviewConflictDto
                {
                    ConflictType = "HiringManager",
                    UserId = hm,
                    ConflictingInterviewId = interview.Id,
                    Message = "Hiring Manager already has an overlapping interview."
                });
            }

            foreach (var participantId in participantUserIds)
            {
                if (interview.Participants.Any(p => p.UserId == participantId)
                    || interview.RecruiterUserId == participantId
                    || interview.HiringManagerUserId == participantId)
                {
                    conflicts.Add(new InterviewConflictDto
                    {
                        ConflictType = "Participant",
                        UserId = participantId,
                        ConflictingInterviewId = interview.Id,
                        Message = "Participant already has an overlapping interview."
                    });
                }
            }
        }

        return conflicts
            .GroupBy(c => new { c.ConflictType, c.UserId, c.ConflictingInterviewId })
            .Select(g => g.First())
            .ToList();
    }

    private IQueryable<Interview> OrgInterviewsQuery(int orgId, int userId) =>
        _db.Interviews.Where(i =>
            i.Application.Job.OrganizationId == orgId
            || i.Application.Job.RecruiterId == userId
            || i.RecruiterUserId == userId);

    private async Task<Interview?> LoadOwnedInterviewAsync(int id, bool tracking = false)
    {
        if (_currentUser.OrganizationId is not int orgId || _currentUser.UserId is not int userId)
        {
            return null;
        }

        var query = tracking ? _db.Interviews.AsQueryable() : _db.Interviews.AsNoTracking();
        var interview = await query
            .Include(i => i.Application).ThenInclude(a => a.Candidate)
            .Include(i => i.Application).ThenInclude(a => a.Job)
            .Include(i => i.HiringManager)
            .Include(i => i.Participants)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (interview is null)
        {
            return null;
        }

        if (interview.Application.Job.OrganizationId == orgId
            || interview.Application.Job.RecruiterId == userId
            || interview.RecruiterUserId == userId
            || _currentUser.IsInRole("Admin"))
        {
            return interview;
        }

        return null;
    }

    private static RecruiterInterviewDetailDto MapInterview(Interview i) => new()
    {
        Id = i.Id,
        ApplicationId = i.ApplicationId,
        CandidateName = i.Application.Candidate?.FullName ?? string.Empty,
        JobTitle = i.Application.Job?.Title ?? string.Empty,
        StartAtUtc = i.InterviewDate,
        EndAtUtc = i.EndsAtUtc,
        DurationMinutes = i.DurationMinutes,
        TimeZoneId = i.TimeZoneId,
        InterviewType = i.InterviewType,
        MeetingLink = i.MeetingLink,
        MeetingInstructions = i.MeetingInstructions,
        PhysicalLocation = i.PhysicalLocation,
        InternalNotes = i.InternalNotes,
        Status = i.Status,
        CandidateResponse = i.CandidateResponse,
        CandidateResponseReason = i.CandidateResponseReason,
        HiringManagerUserId = i.HiringManagerUserId,
        HiringManagerName = i.HiringManager?.FullName,
        ParticipantUserIds = i.Participants.Select(p => p.UserId).ToList(),
        CalendarSyncStatus = string.IsNullOrWhiteSpace(i.CalendarSyncStatus) ? "NotConfigured" : i.CalendarSyncStatus
    };

    private static (DateTime From, DateTime To, string? Error) NormalizeRange(ReportFilterQuery filter)
    {
        var to = filter.ToUtc ?? DateTime.UtcNow;
        var from = filter.FromUtc ?? to.AddDays(-90);
        if (from > to)
        {
            return (default, default, "FromUtc cannot be after ToUtc.");
        }

        if ((to - from).TotalDays > 366)
        {
            return (default, default, "Date range cannot exceed 366 days.");
        }

        return (DateTime.SpecifyKind(from, DateTimeKind.Utc), DateTime.SpecifyKind(to, DateTimeKind.Utc), null);
    }

    private static List<NamedCountDto> BuildScoreRanges(List<AssessmentAssignment> assignments)
    {
        var percents = assignments
            .SelectMany(a => a.Attempts)
            .Where(t => t.Result != null && t.Result.MaxScore > 0)
            .Select(t => t.Result!.Score / t.Result.MaxScore * 100m)
            .ToList();

        return new List<NamedCountDto>
        {
            new() { Name = "0-49", Count = percents.Count(p => p < 50) },
            new() { Name = "50-69", Count = percents.Count(p => p is >= 50 and < 70) },
            new() { Name = "70-84", Count = percents.Count(p => p is >= 70 and < 85) },
            new() { Name = "85-100", Count = percents.Count(p => p >= 85) }
        };
    }

    private static string? Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        if (trimmed.Length > 4000) trimmed = trimmed[..4000];
        return trimmed.Replace("<", string.Empty).Replace(">", string.Empty);
    }
}
