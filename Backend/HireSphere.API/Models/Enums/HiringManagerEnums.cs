namespace HireSphere.API.Models.Enums;

public enum EvaluationSubmissionStatus
{
    Draft = 0,
    Submitted = 1
}

public enum HiringDecisionType
{
    RecommendHire = 0,
    RecommendReject = 1,
    Hold = 2,
    RequestAdditionalInterview = 3,
    RequestAdditionalAssessment = 4,
    FinalHire = 5,
    FinalReject = 6
}
