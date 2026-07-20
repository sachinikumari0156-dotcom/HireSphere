using System.Text;
using HireSphere.API.Data;
using HireSphere.API.DTOs.Admin;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using CsvExportResult = HireSphere.API.DTOs.Recruiter.CsvExportResult;
using CsvEscaper = HireSphere.API.DTOs.Recruiter.CsvEscaper;

namespace HireSphere.API.Services;

public interface IAdminPhase72Service
{
    Task<(bool Ok, string? Error, AdminPagedResultDto<AdminAuditListItemDto>? Result)> ListAuditLogsAsync(AdminAuditLogQuery query);
    Task<(bool Ok, string? Error, CsvExportResult? Result)> ExportAuditLogsAsync(AdminAuditLogQuery query);
    Task<(bool Ok, string? Error, AdminMonitoringSummaryDto? Result)> GetMonitoringAsync();
    Task<(bool Ok, string? Error, AdminUserAnalyticsDto? Result)> GetUserAnalyticsAsync(AdminAnalyticsFilter filter);
    Task<(bool Ok, string? Error, AdminRecruitmentAnalyticsDto? Result)> GetRecruitmentAnalyticsAsync(AdminAnalyticsFilter filter);
    Task<(bool Ok, string? Error, AdminDepartmentAnalyticsDto? Result)> GetDepartmentAnalyticsAsync(AdminAnalyticsFilter filter);
    Task<(bool Ok, string? Error, AdminSkillAnalyticsDto? Result)> GetSkillAnalyticsAsync(AdminAnalyticsFilter filter);
    Task<(bool Ok, string? Error, IReadOnlyList<AdminFinalDecisionListItemDto>? Result)> ListPendingFinalDecisionsAsync();
    Task<(bool Ok, string? Error, AdminFinalDecisionDetailDto? Result)> GetFinalDecisionDetailAsync(int applicationId);
    Task<(bool Ok, string? Error, HiringDecisionHistoryItemLiteDto? Result)> RecordFinalDecisionAsync(int applicationId, AdminFinalDecisionRequestDto dto);
    Task<(bool Ok, string? Error, AdminSecurityUserDto? Result)> GetSecurityUserAsync(int userId);
    Task<(bool Ok, string? Error, CsvExportResult? Result)> ExportAsync(string type, AdminAnalyticsFilter filter);
}

public sealed class AdminPhase72Service : IAdminPhase72Service
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationWriter _notifications;

    public AdminPhase72Service(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        INotificationWriter notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    private bool TryAdmin(out int adminId, out string? error)
    {
        if (_currentUser.UserId is not int id || !_currentUser.IsInRole("Admin"))
        {
            adminId = 0;
            error = "Unauthorized.";
            return false;
        }

        adminId = id;
        error = null;
        return true;
    }

    private static string FormulaSafe(string? value)
    {
        var text = value ?? string.Empty;
        if (text.Length > 0 && "=+-@".Contains(text[0]))
        {
            text = "'" + text;
        }

        return text;
    }

    public async Task<(bool Ok, string? Error, AdminPagedResultDto<AdminAuditListItemDto>? Result)> ListAuditLogsAsync(
        AdminAuditLogQuery query)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 50 : query.PageSize, 1, 200);
        var q = FilterAudit(_db.AuditLogs.AsNoTracking(), query);
        var total = await q.CountAsync();
        var rows = await q.OrderByDescending(a => a.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();
        var items = rows.Select(MapAudit).ToList();
        return (true, null, new AdminPagedResultDto<AdminAuditListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        });
    }

    private static IQueryable<AuditLog> FilterAudit(IQueryable<AuditLog> q, AdminAuditLogQuery query)
    {
        if (query.FromUtc is DateTime from) q = q.Where(a => a.CreatedAtUtc >= from);
        if (query.ToUtc is DateTime to) q = q.Where(a => a.CreatedAtUtc <= to);
        if (query.ActorUserId is int actor) q = q.Where(a => a.UserId == actor);
        if (!string.IsNullOrWhiteSpace(query.Action)) q = q.Where(a => a.Action.Contains(query.Action));
        if (!string.IsNullOrWhiteSpace(query.EntityType)) q = q.Where(a => a.EntityType == query.EntityType);
        if (query.Success is bool s) q = q.Where(a => a.Success == s);
        if (!string.IsNullOrWhiteSpace(query.CorrelationId))
            q = q.Where(a => a.CorrelationId == query.CorrelationId);
        return q;
    }

    private static AdminAuditListItemDto MapAudit(AuditLog a) => new()
    {
        Id = a.Id,
        Action = a.Action,
        EntityType = a.EntityType,
        EntityId = a.EntityId,
        ActorUserId = a.UserId,
        ActorRole = a.ActorRole,
        Details = a.Details,
        Success = a.Success,
        CorrelationId = a.CorrelationId,
        CreatedAtUtc = a.CreatedAtUtc
    };

    public async Task<(bool Ok, string? Error, CsvExportResult? Result)> ExportAuditLogsAsync(AdminAuditLogQuery query)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var rows = new List<string[]>
        {
            new[] { "Id", "Action", "EntityType", "EntityId", "ActorUserId", "ActorRole", "Success", "Details", "CreatedAtUtc" }
        };
        var items = await FilterAudit(_db.AuditLogs.AsNoTracking(), query)
            .OrderByDescending(a => a.CreatedAtUtc).Take(5000).ToListAsync();
        foreach (var a in items)
        {
            rows.Add(new[]
            {
                a.Id.ToString(),
                FormulaSafe(a.Action),
                FormulaSafe(a.EntityType),
                a.EntityId?.ToString() ?? "",
                a.UserId?.ToString() ?? "",
                FormulaSafe(a.ActorRole),
                a.Success.ToString(),
                FormulaSafe(a.Details),
                a.CreatedAtUtc.ToString("O")
            });
        }

        return (true, null, new CsvExportResult
        {
            FileName = "admin-audit-logs.csv",
            Content = CsvEscaper.ToUtf8Csv(rows)
        });
    }

    public async Task<(bool Ok, string? Error, AdminMonitoringSummaryDto? Result)> GetMonitoringAsync()
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        string dbStatus;
        try
        {
            dbStatus = await _db.Database.CanConnectAsync() ? "Connected" : "Unavailable";
        }
        catch
        {
            dbStatus = "Unavailable";
        }

        var now = DateTime.UtcNow;
        return (true, null, new AdminMonitoringSummaryDto
        {
            ApiHealth = "Operational",
            DatabaseConnectivity = dbStatus,
            PendingRecruiterRequests = await _db.RecruiterAccessRequests.CountAsync(r => r.Status == RecruiterRequestStatus.Pending),
            DisabledAccounts = await _db.Users.CountAsync(u => u.Status == UserStatus.Inactive || u.Status == UserStatus.Suspended),
            PendingAssessments = await _db.AssessmentAssignments.CountAsync(a =>
                a.Status == AssessmentStatus.Pending || a.Status == AssessmentStatus.InProgress),
            UpcomingInterviews = await _db.Interviews.CountAsync(i =>
                i.InterviewDate >= now && i.Status != InterviewStatus.Cancelled && i.Status != InterviewStatus.Completed),
            PendingInterviewFeedback = await _db.Interviews.CountAsync(i =>
                i.InterviewDate >= now && !i.Feedbacks.Any()),
            PendingFinalDecisions = await _db.HiringDecisions.CountAsync(d =>
                !d.IsFinal && (d.DecisionType == HiringDecisionType.RecommendHire
                    || d.DecisionType == HiringDecisionType.RecommendReject)),
            StorageProviderStatus = "NotConfigured",
            EmailProviderStatus = "NotConfigured",
            SmsProviderStatus = "NotConfigured",
            CalendarProviderStatus = "NotConfigured"
        });
    }

    public async Task<(bool Ok, string? Error, AdminUserAnalyticsDto? Result)> GetUserAnalyticsAsync(AdminAnalyticsFilter filter)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var users = _db.Users.AsNoTracking().AsQueryable();
        if (filter.FromUtc is DateTime from) users = users.Where(u => u.CreatedAtUtc >= from);
        if (filter.ToUtc is DateTime to) users = users.Where(u => u.CreatedAtUtc <= to);
        var byRole = await users.GroupBy(u => u.Role).Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() }).ToListAsync();
        var byStatus = await users.GroupBy(u => u.Status.ToString())
            .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() }).ToListAsync();
        return (true, null, new AdminUserAnalyticsDto { UsersByRole = byRole, UsersByStatus = byStatus });
    }

    public async Task<(bool Ok, string? Error, AdminRecruitmentAnalyticsDto? Result)> GetRecruitmentAnalyticsAsync(
        AdminAnalyticsFilter filter)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var jobs = _db.Jobs.AsNoTracking().AsQueryable();
        var apps = _db.Applications.AsNoTracking().AsQueryable();
        if (filter.OrganizationId is int oid)
        {
            jobs = jobs.Where(j => j.OrganizationId == oid);
            apps = apps.Where(a => a.Job.OrganizationId == oid);
        }

        if (filter.FromUtc is DateTime from) apps = apps.Where(a => a.AppliedDate >= from);
        if (filter.ToUtc is DateTime to) apps = apps.Where(a => a.AppliedDate <= to);

        var jobsByStatus = await jobs.GroupBy(j => j.Status.ToString())
            .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() }).ToListAsync();
        var appsByStatus = await apps.GroupBy(a => a.Status.ToString())
            .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() }).ToListAsync();
        var overTimeRaw = await apps
            .GroupBy(a => a.AppliedDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();
        var overTime = overTimeRaw
            .Select(g => new NamedCountDto { Name = g.Key.ToString("yyyy-MM-dd"), Count = g.Count })
            .ToList();

        return (true, null, new AdminRecruitmentAnalyticsDto
        {
            JobsByStatus = jobsByStatus,
            ApplicationsByStatus = appsByStatus,
            ApplicationsOverTime = overTime,
            Shortlisted = await apps.CountAsync(a => a.Status == ApplicationStatus.Shortlisted),
            Rejected = await apps.CountAsync(a => a.Status == ApplicationStatus.Rejected),
            Hired = await apps.CountAsync(a => a.Status == ApplicationStatus.Hired)
        });
    }

    public async Task<(bool Ok, string? Error, AdminDepartmentAnalyticsDto? Result)> GetDepartmentAnalyticsAsync(
        AdminAnalyticsFilter filter)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var depts = _db.Departments.AsNoTracking().Include(d => d.Organization).AsQueryable();
        if (filter.OrganizationId is int oid) depts = depts.Where(d => d.OrganizationId == oid);
        var list = await depts.ToListAsync();
        var jobs = new List<NamedCountDto>();
        var users = new List<NamedCountDto>();
        foreach (var d in list)
        {
            jobs.Add(new NamedCountDto
            {
                Name = $"{d.Organization.Name}/{d.Name}",
                Count = await _db.Jobs.CountAsync(j => j.DepartmentId == d.Id)
            });
            users.Add(new NamedCountDto
            {
                Name = $"{d.Organization.Name}/{d.Name}",
                Count = await _db.RecruiterProfiles.CountAsync(p => p.DepartmentId == d.Id)
                    + await _db.HiringManagerProfiles.CountAsync(p => p.DepartmentId == d.Id)
            });
        }

        return (true, null, new AdminDepartmentAnalyticsDto { JobsByDepartment = jobs, UsersByDepartment = users });
    }

    public async Task<(bool Ok, string? Error, AdminSkillAnalyticsDto? Result)> GetSkillAnalyticsAsync(
        AdminAnalyticsFilter filter)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var jobSkills = _db.JobSkills.AsNoTracking().Include(js => js.Skill).AsQueryable();
        if (filter.OrganizationId is int oid)
            jobSkills = jobSkills.Where(js => js.Job.OrganizationId == oid);

        var demand = await jobSkills.GroupBy(js => js.Skill.Name)
            .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).Take(20).ToListAsync();
        var availability = await _db.CandidateSkills.AsNoTracking().Include(cs => cs.Skill)
            .GroupBy(cs => cs.Skill.Name)
            .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).Take(20).ToListAsync();
        return (true, null, new AdminSkillAnalyticsDto
        {
            SkillDemandFromJobs = demand,
            CandidateSkillAvailability = availability
        });
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<AdminFinalDecisionListItemDto>? Result)> ListPendingFinalDecisionsAsync()
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var recs = await _db.HiringDecisions.AsNoTracking()
            .Include(d => d.Application).ThenInclude(a => a.Candidate)
            .Include(d => d.Application).ThenInclude(a => a.Job)
            .Where(d => !d.IsFinal
                && (d.DecisionType == HiringDecisionType.RecommendHire || d.DecisionType == HiringDecisionType.RecommendReject)
                && d.Application.Status != ApplicationStatus.Withdrawn
                && d.Application.Status != ApplicationStatus.Hired
                && d.Application.Status != ApplicationStatus.Rejected)
            .OrderByDescending(d => d.DecisionDateUtc)
            .ToListAsync();

        var items = recs
            .GroupBy(d => d.ApplicationId)
            .Select(g =>
            {
                var latest = g.OrderByDescending(x => x.DecisionDateUtc).First();
                return new AdminFinalDecisionListItemDto
                {
                    ApplicationId = latest.ApplicationId,
                    CandidateName = latest.Application.Candidate.FullName,
                    JobTitle = latest.Application.Job.Title,
                    ApplicationStatus = latest.Application.Status.ToString(),
                    LatestRecommendation = latest.DecisionType.ToString(),
                    RecommendationDateUtc = latest.DecisionDateUtc
                };
            }).ToList();
        return (true, null, items);
    }

    public async Task<(bool Ok, string? Error, AdminFinalDecisionDetailDto? Result)> GetFinalDecisionDetailAsync(int applicationId)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var app = await _db.Applications.AsNoTracking()
            .Include(a => a.Candidate)
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == applicationId);
        if (app is null) return (false, "Application not found.", null);

        var history = await _db.HiringDecisions.AsNoTracking()
            .Where(d => d.ApplicationId == applicationId)
            .OrderByDescending(d => d.DecisionDateUtc)
            .Select(d => new HiringDecisionHistoryItemLiteDto
            {
                Id = d.Id,
                DecisionType = d.DecisionType.ToString(),
                IsFinal = d.IsFinal,
                Reason = d.Reason,
                DecisionDateUtc = d.DecisionDateUtc
            }).ToListAsync();

        var latestRec = history.FirstOrDefault(h => !h.IsFinal);
        var warnings = new List<string>();
        if (app.Status == ApplicationStatus.Withdrawn) warnings.Add("Application is withdrawn.");
        if (history.Any(h => h.IsFinal)) warnings.Add("A final decision already exists.");
        if (latestRec is null) warnings.Add("No Hiring Manager recommendation recorded.");

        return (true, null, new AdminFinalDecisionDetailDto
        {
            ApplicationId = app.Id,
            CandidateName = app.Candidate.FullName,
            JobTitle = app.Job.Title,
            ApplicationStatus = app.Status.ToString(),
            LatestRecommendation = latestRec?.DecisionType,
            RecommendationReason = latestRec?.Reason,
            DecisionHistory = history,
            Warnings = warnings
        });
    }

    public async Task<(bool Ok, string? Error, HiringDecisionHistoryItemLiteDto? Result)> RecordFinalDecisionAsync(
        int applicationId,
        AdminFinalDecisionRequestDto dto)
    {
        if (!TryAdmin(out var adminId, out var error)) return (false, error, null);
        if (!Enum.TryParse<HiringDecisionType>(dto.DecisionType, true, out var type))
            return (false, "Invalid decision type.", null);
        if (string.IsNullOrWhiteSpace(dto.Reason)) return (false, "Reason is required.", null);

        var allowed = new[]
        {
            HiringDecisionType.FinalHire,
            HiringDecisionType.FinalReject,
            HiringDecisionType.Hold,
            HiringDecisionType.RequestAdditionalInterview,
            HiringDecisionType.RequestAdditionalAssessment
        };
        if (!allowed.Contains(type)) return (false, "Decision type not permitted for Administrator final workflow.", null);

        var application = await _db.Applications.FirstOrDefaultAsync(a => a.Id == applicationId);
        if (application is null) return (false, "Application not found.", null);
        if (application.Status == ApplicationStatus.Withdrawn)
            return (false, "Cannot record a decision for a withdrawn application.", null);

        if (dto.ExpectedUpdatedAtUtc is DateTime expected
            && application.UpdatedAtUtc is DateTime updated
            && updated.ToUniversalTime() != expected.ToUniversalTime())
        {
            return (false, "Stale decision update rejected. Reload the application and retry.", null);
        }

        var isFinal = type is HiringDecisionType.FinalHire or HiringDecisionType.FinalReject;
        if (isFinal)
        {
            var hasFinal = await _db.HiringDecisions.AnyAsync(d =>
                d.ApplicationId == applicationId && d.IsFinal
                && (d.DecisionType == HiringDecisionType.FinalHire || d.DecisionType == HiringDecisionType.FinalReject));
            if (hasFinal) return (false, "A final decision already exists for this application.", null);
        }

        var prior = application.Status;
        ApplicationStatus? resulting = null;
        var decision = new HiringDecision
        {
            ApplicationId = applicationId,
            DecisionByUserId = adminId,
            DecisionType = type,
            IsFinal = isFinal,
            Status = isFinal
                ? (type == HiringDecisionType.FinalHire ? HiringDecisionStatus.Approved : HiringDecisionStatus.Rejected)
                : HiringDecisionStatus.Pending,
            Reason = dto.Reason.Trim(),
            Notes = dto.Notes,
            PriorApplicationStatus = prior,
            DecisionDateUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

        if (isFinal)
        {
            var target = type == HiringDecisionType.FinalHire ? ApplicationStatus.Hired : ApplicationStatus.Rejected;
            if (application.Status != target)
            {
                var from = application.Status;
                // Administrator final decision may complete the pipeline with explicit reason (override of intermediate stages).
                application.Status = target;
                application.UpdatedAtUtc = DateTime.UtcNow;
                _db.ApplicationStatusHistories.Add(new ApplicationStatusHistory
                {
                    ApplicationId = application.Id,
                    Status = target,
                    ChangedAtUtc = DateTime.UtcNow,
                    ChangedByUserId = adminId,
                    Notes = $"Admin final override ({from} → {target}): {dto.Reason.Trim()}"
                });
            }

            resulting = target;
            decision.ResultingApplicationStatus = resulting;
        }

        _db.HiringDecisions.Add(decision);
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminId,
            Action = isFinal ? "admin.hiring.final" : "admin.hiring.decision",
            EntityType = nameof(HiringDecision),
            EntityId = applicationId,
            Details = $"{type}: {dto.Reason}",
            Success = true,
            ActorRole = "Admin",
            CorrelationId = Guid.NewGuid().ToString("N"),
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        if (isFinal)
        {
            await _notifications.CreateAsync(
                application.CandidateId,
                "HiringDecision",
                type == HiringDecisionType.FinalHire ? "Offer update" : "Application update",
                type == HiringDecisionType.FinalHire
                    ? "A final hiring decision has been recorded for your application."
                    : "A final decision has been recorded for your application.",
                nameof(Application),
                applicationId,
                $"/candidate/applications/{applicationId}");
        }

        return (true, null, new HiringDecisionHistoryItemLiteDto
        {
            Id = decision.Id,
            DecisionType = decision.DecisionType.ToString(),
            IsFinal = decision.IsFinal,
            Reason = decision.Reason,
            DecisionDateUtc = decision.DecisionDateUtc
        });
    }

    public async Task<(bool Ok, string? Error, AdminSecurityUserDto? Result)> GetSecurityUserAsync(int userId)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return (false, "User not found.", null);
        var eventRows = await _db.AuditLogs.AsNoTracking()
            .Where(a => a.EntityType == nameof(User) && a.EntityId == userId)
            .OrderByDescending(a => a.CreatedAtUtc).Take(20)
            .ToListAsync();
        return (true, null, new AdminSecurityUserDto
        {
            UserId = user.Id,
            Status = user.Status.ToString(),
            CreatedAtUtc = user.CreatedAtUtc,
            MustChangePassword = user.MustChangePassword,
            RecentSecurityEvents = eventRows.Select(MapAudit).ToList()
        });
    }

    public async Task<(bool Ok, string? Error, CsvExportResult? Result)> ExportAsync(string type, AdminAnalyticsFilter filter)
    {
        if (!TryAdmin(out _, out var error)) return (false, error, null);
        var rows = new List<string[]>();
        switch (type.ToLowerInvariant())
        {
            case "users":
                rows.Add(new[] { "UserId", "FullName", "Email", "Role", "Status" });
                foreach (var u in await _db.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync())
                {
                    rows.Add(new[]
                    {
                        u.Id.ToString(),
                        FormulaSafe(u.FullName),
                        FormulaSafe(u.Email),
                        FormulaSafe(u.Role),
                        FormulaSafe(u.Status.ToString())
                    });
                }

                return (true, null, new CsvExportResult { FileName = "admin-users.csv", Content = CsvEscaper.ToUtf8Csv(rows) });
            case "organizations":
                rows.Add(new[] { "Id", "Name", "Code", "Status" });
                foreach (var o in await _db.Organizations.AsNoTracking().ToListAsync())
                    rows.Add(new[] { o.Id.ToString(), FormulaSafe(o.Name), FormulaSafe(o.Code), FormulaSafe(o.Status.ToString()) });
                return (true, null, new CsvExportResult { FileName = "admin-organizations.csv", Content = CsvEscaper.ToUtf8Csv(rows) });
            case "departments":
                rows.Add(new[] { "Id", "OrganizationId", "Name", "Status" });
                foreach (var d in await _db.Departments.AsNoTracking().ToListAsync())
                    rows.Add(new[] { d.Id.ToString(), d.OrganizationId.ToString(), FormulaSafe(d.Name), FormulaSafe(d.Status.ToString()) });
                return (true, null, new CsvExportResult { FileName = "admin-departments.csv", Content = CsvEscaper.ToUtf8Csv(rows) });
            case "audit":
                return await ExportAuditLogsAsync(new AdminAuditLogQuery());
            default:
                return (false, "Unknown export type.", null);
        }
    }
}
