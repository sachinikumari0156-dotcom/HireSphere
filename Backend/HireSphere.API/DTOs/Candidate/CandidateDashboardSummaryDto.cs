namespace HireSphere.API.DTOs.Candidate;

public class CandidateDashboardSummaryDto
{
    public int ProfileCompletionPercent { get; set; }

    public int LatestApplicationsCount { get; set; }

    public int InterviewsCount { get; set; }

    public int AssessmentsCount { get; set; }

    public int RecommendationsCount { get; set; }

    public int UnreadNotificationsCount { get; set; }

    public string ResumeAnalysisStatus { get; set; } = "NotAvailable";
}
