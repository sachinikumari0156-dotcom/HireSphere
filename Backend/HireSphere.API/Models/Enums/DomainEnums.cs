namespace HireSphere.API.Models.Enums;

public enum UserStatus
{
    Active,
    Inactive,
    PendingApproval,
    Suspended
}

public enum JobStatus
{
    Draft = 0,
    /// <summary>Legacy published state; treated as publicly visible with <see cref="Published"/>.</summary>
    Open = 1,
    Closed = 2,
    Archived = 3,
    PendingApproval = 4,
    Paused = 5,
    Published = 6
}

/// <summary>
/// Candidate-facing application pipeline statuses.
/// Display mapping: Pending≈Submitted, UnderReview≈Screening, Offered≈Offer.
/// </summary>
public enum ApplicationStatus
{
    Pending,
    UnderReview,
    Assessment,
    Shortlisted,
    InterviewScheduled,
    Interviewed,
    Offered,
    Hired,
    Rejected,
    Withdrawn,
    ManualReview
}

public enum InterviewStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    Rescheduled = 4,
    NoShow = 5,
    Proposed = 6,
    Confirmed = 7,
    RescheduleRequested = 8
}

public enum InterviewCandidateResponse
{
    Pending,
    Confirmed,
    RescheduleRequested,
    Declined
}

public enum AssessmentStatus
{
    Pending,
    InProgress,
    Completed,
    Expired,
    Cancelled
}

public enum HiringDecisionStatus
{
    Pending,
    Approved,
    Rejected,
    OnHold,
    Withdrawn
}

public enum DocumentType
{
    Resume,
    CoverLetter,
    Certificate,
    Identification,
    Portfolio,
    Other
}

public enum WorkArrangement
{
    OnSite,
    Remote,
    Hybrid
}

public enum EmploymentType
{
    FullTime,
    PartTime,
    Contract,
    Internship,
    Temporary
}

public enum RecruiterRequestStatus
{
    Pending,
    Approved,
    Rejected
}
