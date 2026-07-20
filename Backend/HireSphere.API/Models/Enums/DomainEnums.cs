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
    Draft,
    Open,
    Closed,
    Archived
}

public enum ApplicationStatus
{
    Pending,
    UnderReview,
    Shortlisted,
    InterviewScheduled,
    Offered,
    Hired,
    Rejected,
    Withdrawn
}

public enum InterviewStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Rescheduled,
    NoShow
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
