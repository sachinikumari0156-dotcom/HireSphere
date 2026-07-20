using HireSphere.API.Models.Enums;
using System.Text;

namespace HireSphere.API.DTOs.Recruiter;

public sealed class ScheduleInterviewDto
{
    public int ApplicationId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public string TimeZoneId { get; set; } = "UTC";
    public string InterviewType { get; set; } = "Video";
    public string? MeetingLink { get; set; }
    public string? MeetingInstructions { get; set; }
    public string? PhysicalLocation { get; set; }
    public string? InternalNotes { get; set; }
    public int? HiringManagerUserId { get; set; }
    public List<int> ParticipantUserIds { get; set; } = new();
    public bool ForceDespiteConflicts { get; set; }
}

public sealed class RescheduleInterviewDto
{
    public DateTime StartAtUtc { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public string TimeZoneId { get; set; } = "UTC";
    public string? Reason { get; set; }
    public bool ForceDespiteConflicts { get; set; }
}

public sealed class ChangeInterviewStatusDto
{
    public InterviewStatus Status { get; set; }
    public string? Notes { get; set; }
}

public sealed class InterviewConflictDto
{
    public string ConflictType { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public int? ConflictingInterviewId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class RecruiterInterviewDetailDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public string TimeZoneId { get; set; } = "UTC";
    public string InterviewType { get; set; } = string.Empty;
    public string? MeetingLink { get; set; }
    public string? MeetingInstructions { get; set; }
    public string? PhysicalLocation { get; set; }
    public string? InternalNotes { get; set; }
    public InterviewStatus Status { get; set; }
    public InterviewCandidateResponse CandidateResponse { get; set; }
    public string? CandidateResponseReason { get; set; }
    public int? HiringManagerUserId { get; set; }
    public string? HiringManagerName { get; set; }
    public IReadOnlyList<int> ParticipantUserIds { get; set; } = Array.Empty<int>();
    public string CalendarSyncStatus { get; set; } = "NotConfigured";
    public IReadOnlyList<InterviewConflictDto> Conflicts { get; set; } = Array.Empty<InterviewConflictDto>();
}

public sealed class ScheduleInterviewResultDto
{
    public RecruiterInterviewDetailDto? Interview { get; set; }
    public IReadOnlyList<InterviewConflictDto> Conflicts { get; set; } = Array.Empty<InterviewConflictDto>();
    public bool Scheduled { get; set; }
}

public sealed class ReportFilterQuery
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int? JobId { get; set; }
    public int? DepartmentId { get; set; }
    public int? RecruiterUserId { get; set; }
    public ApplicationStatus? ApplicationStatus { get; set; }
}

public sealed class RecruiterReportSummaryDto
{
    public int ApplicationsTotal { get; set; }
    public int NewApplicants { get; set; }
    public int Shortlisted { get; set; }
    public int Rejected { get; set; }
    public double ShortlistRate { get; set; }
    public double RejectionRate { get; set; }
    public int AssessmentAssignments { get; set; }
    public int AssessmentCompletions { get; set; }
    public int InterviewsScheduled { get; set; }
    public double? AverageDaysToScreen { get; set; }
    public double? AverageDaysToShortlist { get; set; }
    public double? AverageDaysToInterview { get; set; }
    public IReadOnlyList<NamedCountDto> ApplicationsByJob { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<NamedCountDto> ApplicationsByStatus { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<NamedCountDto> ApplicationsOverTime { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<NamedCountDto> ScreeningOutcomes { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<NamedCountDto> AssessmentScoreRanges { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<NamedCountDto> InterviewStatuses { get; set; } = Array.Empty<NamedCountDto>();
}

public sealed class NamedCountDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class CsvExportResult
{
    public string FileName { get; set; } = "recruiter-report.csv";
    public string ContentType { get; set; } = "text/csv";
    public byte[] Content { get; set; } = Array.Empty<byte>();
}

public static class CsvEscaper
{
    public static string Escape(string? value)
    {
        var text = value ?? string.Empty;
        if (text.Contains('"') || text.Contains(',') || text.Contains('\n') || text.Contains('\r'))
        {
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        return text;
    }

    public static byte[] ToUtf8Csv(IEnumerable<string[]> rows)
    {
        var sb = new StringBuilder();
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", row.Select(Escape)));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
