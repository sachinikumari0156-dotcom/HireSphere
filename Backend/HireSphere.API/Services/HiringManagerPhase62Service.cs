using System.Text.RegularExpressions;
using HireSphere.API.Data;
using HireSphere.API.DTOs.HiringManager;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface IHiringManagerPhase62Service
{
    Task<(bool Ok, string? Error, IReadOnlyList<HiringManagerInterviewListItemDto>? Result)> ListInterviewsAsync();

    Task<(bool Ok, string? Error, HiringManagerInterviewDetailDto? Result)> GetInterviewAsync(int interviewId);

    Task<(bool Ok, string? Error, HiringManagerInterviewFeedbackDto? Result)> UpsertFeedbackAsync(
        int interviewId,
        UpsertInterviewFeedbackDto dto,
        bool isUpdate);

    Task<(bool Ok, string? Error, HiringManagerEvaluationDto? Result)> GetEvaluationAsync(int applicationId);

    Task<(bool Ok, string? Error, HiringManagerEvaluationDto? Result)> UpsertEvaluationAsync(
        int applicationId,
        UpsertEvaluationDto dto,
        bool isUpdate);

    Task<(bool Ok, string? Error, HiringDecisionHistoryItemDto? Result)> SubmitRecommendationAsync(
        int applicationId,
        CreateRecommendationDto dto);

    Task<(bool Ok, string? Error, IReadOnlyList<HiringDecisionHistoryItemDto>? Result)> GetDecisionHistoryAsync(
        int applicationId);
}

public sealed class HiringManagerPhase62Service : IHiringManagerPhase62Service
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IResourceAuthorizationService _authz;
    private readonly IApplicationStatusTransitionService _transitions;

    public HiringManagerPhase62Service(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        IResourceAuthorizationService authz,
        IApplicationStatusTransitionService transitions)
    {
        _db = db;
        _currentUser = currentUser;
        _authz = authz;
        _transitions = transitions;
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<HiringManagerInterviewListItemDto>? Result)> ListInterviewsAsync()
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        var items = await _db.Interviews.AsNoTracking()
            .Include(i => i.Application).ThenInclude(a => a.Candidate)
            .Include(i => i.Application).ThenInclude(a => a.Job)
            .Include(i => i.Recruiter)
            .Where(i => i.HiringManagerUserId == userId
                || i.Application.Job.HiringManagerUserId == userId
                || i.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(i => i.InterviewDate)
            .Select(i => new HiringManagerInterviewListItemDto
            {
                Id = i.Id,
                ApplicationId = i.ApplicationId,
                CandidateName = i.Application.Candidate.FullName,
                JobTitle = i.Application.Job.Title,
                RecruiterName = i.Recruiter != null ? i.Recruiter.FullName : null,
                InterviewType = i.InterviewType,
                InterviewDateUtc = i.InterviewDate,
                TimeZoneId = i.TimeZoneId,
                DurationMinutes = i.DurationMinutes,
                Status = i.Status.ToString(),
                CandidateResponse = i.CandidateResponse.ToString()
            })
            .ToListAsync();

        return (true, null, items);
    }

    public async Task<(bool Ok, string? Error, HiringManagerInterviewDetailDto? Result)> GetInterviewAsync(int interviewId)
    {
        if (!await _authz.HiringManagerCanAccessInterviewAsync(interviewId))
        {
            return (false, "Interview not found or access denied.", null);
        }

        var interview = await _db.Interviews.AsNoTracking()
            .Include(i => i.Application).ThenInclude(a => a.Candidate)
            .Include(i => i.Application).ThenInclude(a => a.Job)
            .Include(i => i.Recruiter)
            .Include(i => i.Feedbacks)
            .FirstOrDefaultAsync(i => i.Id == interviewId);

        if (interview is null)
        {
            return (false, "Interview not found or access denied.", null);
        }

        var mine = interview.Feedbacks.FirstOrDefault(f => f.InterviewerId == _currentUser.UserId);
        return (true, null, new HiringManagerInterviewDetailDto
        {
            Id = interview.Id,
            ApplicationId = interview.ApplicationId,
            CandidateName = interview.Application.Candidate.FullName,
            JobTitle = interview.Application.Job.Title,
            RecruiterName = interview.Recruiter?.FullName,
            InterviewType = interview.InterviewType,
            InterviewDateUtc = interview.InterviewDate,
            TimeZoneId = interview.TimeZoneId,
            DurationMinutes = interview.DurationMinutes,
            Status = interview.Status.ToString(),
            CandidateResponse = interview.CandidateResponse.ToString(),
            CandidateResponseReason = interview.CandidateResponseReason,
            MeetingLink = interview.MeetingLink,
            MeetingInstructions = interview.MeetingInstructions,
            PhysicalLocation = interview.PhysicalLocation,
            MyFeedback = mine is null ? null : MapFeedback(mine)
        });
    }

    public async Task<(bool Ok, string? Error, HiringManagerInterviewFeedbackDto? Result)> UpsertFeedbackAsync(
        int interviewId,
        UpsertInterviewFeedbackDto dto,
        bool isUpdate)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        if (!await _authz.HiringManagerCanAccessInterviewAsync(interviewId))
        {
            return (false, "Interview not found or access denied.", null);
        }

        var ratings = new[]
        {
            dto.TechnicalCompetency, dto.Communication, dto.ProblemSolving, dto.RoleKnowledge,
            dto.Teamwork, dto.Leadership, dto.CulturalContribution
        };
        foreach (var r in ratings.Where(x => x.HasValue))
        {
            if (r is < 1 or > 5)
            {
                return (false, "Ratings must be between 1 and 5.", null);
            }
        }

        if (string.IsNullOrWhiteSpace(dto.Recommendation))
        {
            return (false, "Recommendation is required.", null);
        }

        var existing = await _db.InterviewFeedbacks
            .FirstOrDefaultAsync(f => f.InterviewId == interviewId && f.InterviewerId == userId);

        if (existing is null && isUpdate)
        {
            return (false, "Feedback not found.", null);
        }

        if (existing is not null && !isUpdate)
        {
            return (false, "Feedback already submitted. Use update to revise.", null);
        }

        var overall = ratings.Where(x => x.HasValue).Select(x => x!.Value).DefaultIfEmpty(3).Average();

        if (existing is null)
        {
            existing = new InterviewFeedback
            {
                InterviewId = interviewId,
                InterviewerId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.InterviewFeedbacks.Add(existing);
        }

        existing.TechnicalCompetency = dto.TechnicalCompetency;
        existing.Communication = dto.Communication;
        existing.ProblemSolving = dto.ProblemSolving;
        existing.RoleKnowledge = dto.RoleKnowledge;
        existing.Teamwork = dto.Teamwork;
        existing.Leadership = dto.Leadership;
        existing.CulturalContribution = dto.CulturalContribution;
        existing.Strengths = Sanitize(dto.Strengths);
        existing.Concerns = Sanitize(dto.Concerns);
        existing.Recommendation = Sanitize(dto.Recommendation)!;
        existing.Comments = Sanitize(dto.Comments);
        existing.PrivatePanelComments = Sanitize(dto.PrivatePanelComments);
        existing.Rating = Math.Round(overall, 2);
        existing.SubmittedAtUtc = DateTime.UtcNow;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = isUpdate ? "InterviewFeedbackUpdated" : "InterviewFeedbackCreated",
            EntityType = "InterviewFeedback",
            EntityId = null,
            Details = $"Interview {interviewId}",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return (true, null, MapFeedback(existing));
    }

    public async Task<(bool Ok, string? Error, HiringManagerEvaluationDto? Result)> GetEvaluationAsync(int applicationId)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        if (!await _authz.HiringManagerCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.", null);
        }

        var eval = await _db.CandidateEvaluations.AsNoTracking()
            .FirstOrDefaultAsync(e => e.ApplicationId == applicationId && e.EvaluatorUserId == userId);

        return eval is null
            ? (true, null, null)
            : (true, null, MapEvaluation(eval));
    }

    public async Task<(bool Ok, string? Error, HiringManagerEvaluationDto? Result)> UpsertEvaluationAsync(
        int applicationId,
        UpsertEvaluationDto dto,
        bool isUpdate)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        if (!await _authz.HiringManagerCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.", null);
        }

        var scores = new[]
        {
            dto.RequiredSkillsAlignment, dto.PreferredSkillsAlignment, dto.RelevantExperience,
            dto.EducationRequirement, dto.AssessmentPerformance, dto.InterviewPerformance,
            dto.Communication, dto.ProblemSolving, dto.RoleReadiness
        };
        foreach (var s in scores.Where(x => x.HasValue))
        {
            if (s is < 0 or > 100)
            {
                return (false, "Criterion scores must be between 0 and 100.", null);
            }
        }

        if (dto.Submit && string.IsNullOrWhiteSpace(dto.Justification))
        {
            return (false, "Justification is required to submit an evaluation.", null);
        }

        var eval = await _db.CandidateEvaluations
            .FirstOrDefaultAsync(e => e.ApplicationId == applicationId && e.EvaluatorUserId == userId);

        if (eval is null && isUpdate)
        {
            return (false, "Evaluation not found.", null);
        }

        if (eval is null)
        {
            eval = new CandidateEvaluation
            {
                ApplicationId = applicationId,
                EvaluatorUserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.CandidateEvaluations.Add(eval);
        }

        eval.RequiredSkillsAlignment = dto.RequiredSkillsAlignment;
        eval.PreferredSkillsAlignment = dto.PreferredSkillsAlignment;
        eval.RelevantExperience = dto.RelevantExperience;
        eval.EducationRequirement = dto.EducationRequirement;
        eval.AssessmentPerformance = dto.AssessmentPerformance;
        eval.InterviewPerformance = dto.InterviewPerformance;
        eval.Communication = dto.Communication;
        eval.ProblemSolving = dto.ProblemSolving;
        eval.RoleReadiness = dto.RoleReadiness;
        eval.Strengths = Sanitize(dto.Strengths);
        eval.Weaknesses = Sanitize(dto.Weaknesses);
        eval.DocumentedRisks = Sanitize(dto.DocumentedRisks);
        eval.Justification = Sanitize(dto.Justification);
        eval.Recommendation = Sanitize(dto.Recommendation);
        eval.OverallScore = Math.Round(scores.Where(x => x.HasValue).Select(x => x!.Value).DefaultIfEmpty(0).Average(), 2);
        eval.UpdatedAtUtc = DateTime.UtcNow;

        if (dto.Submit)
        {
            eval.SubmissionStatus = EvaluationSubmissionStatus.Submitted;
            eval.SubmittedAtUtc = DateTime.UtcNow;
        }
        else
        {
            eval.SubmissionStatus = EvaluationSubmissionStatus.Draft;
        }

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = dto.Submit ? "CandidateEvaluationSubmitted" : "CandidateEvaluationSaved",
            EntityType = "CandidateEvaluation",
            EntityId = null,
            Details = $"Application {applicationId} ({eval.SubmissionStatus})",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return (true, null, MapEvaluation(eval));
    }

    public async Task<(bool Ok, string? Error, HiringDecisionHistoryItemDto? Result)> SubmitRecommendationAsync(
        int applicationId,
        CreateRecommendationDto dto)
    {
        if (_currentUser.UserId is not int userId)
        {
            return (false, "Unauthorized.", null);
        }

        if (!await _authz.HiringManagerCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            return (false, "Reason is required.", null);
        }

        var isFinal = dto.DecisionType is HiringDecisionType.FinalHire or HiringDecisionType.FinalReject;
        if (isFinal && !_currentUser.IsInRole("Admin") && !_currentUser.IsInRole("Recruiter"))
        {
            return (false, "Hiring Managers may submit recommendations only. Final decisions require Recruiter or Administrator.", null);
        }

        var application = await _db.Applications.FirstOrDefaultAsync(a => a.Id == applicationId);
        if (application is null)
        {
            return (false, "Application not found or access denied.", null);
        }

        if (application.Status == ApplicationStatus.Withdrawn)
        {
            return (false, "Cannot record a decision for a withdrawn application.", null);
        }

        if (isFinal)
        {
            var hasFinal = await _db.HiringDecisions.AnyAsync(d =>
                d.ApplicationId == applicationId && d.IsFinal
                && (d.DecisionType == HiringDecisionType.FinalHire || d.DecisionType == HiringDecisionType.FinalReject));
            if (hasFinal)
            {
                return (false, "A final decision already exists for this application.", null);
            }
        }

        var prior = application.Status;
        ApplicationStatus? resulting = null;

        var decision = new HiringDecision
        {
            ApplicationId = applicationId,
            DecisionByUserId = userId,
            DecisionType = dto.DecisionType,
            IsFinal = isFinal,
            Status = isFinal
                ? (dto.DecisionType == HiringDecisionType.FinalHire
                    ? HiringDecisionStatus.Approved
                    : HiringDecisionStatus.Rejected)
                : HiringDecisionStatus.Pending,
            Reason = Sanitize(dto.Reason)!,
            Notes = Sanitize(dto.Notes),
            PriorApplicationStatus = prior,
            DecisionDateUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

        if (isFinal)
        {
            var target = dto.DecisionType == HiringDecisionType.FinalHire
                ? ApplicationStatus.Hired
                : ApplicationStatus.Rejected;
            var (ok, error) = await _transitions.TransitionAsync(
                applicationId,
                target,
                userId,
                $"Final decision: {dto.DecisionType}");
            if (!ok)
            {
                return (false, error, null);
            }

            resulting = target;
            decision.ResultingApplicationStatus = target;
        }

        _db.HiringDecisions.Add(decision);
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = isFinal ? "HiringDecisionFinal" : "HiringRecommendationCreated",
            EntityType = "HiringDecision",
            EntityId = null,
            Details = $"{dto.DecisionType} on application {applicationId}",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        var user = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
        return (true, null, new HiringDecisionHistoryItemDto
        {
            Id = decision.Id,
            DecisionType = decision.DecisionType.ToString(),
            Status = decision.Status.ToString(),
            IsFinal = decision.IsFinal,
            Reason = decision.Reason,
            Notes = decision.Notes,
            DecisionByUserId = userId,
            DecisionByName = user.FullName,
            DecisionDateUtc = decision.DecisionDateUtc,
            PriorApplicationStatus = prior.ToString(),
            ResultingApplicationStatus = resulting?.ToString()
        });
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<HiringDecisionHistoryItemDto>? Result)> GetDecisionHistoryAsync(
        int applicationId)
    {
        if (!await _authz.HiringManagerCanAccessApplicationAsync(applicationId))
        {
            return (false, "Application not found or access denied.", null);
        }

        var items = await _db.HiringDecisions.AsNoTracking()
            .Include(d => d.DecisionByUser)
            .Where(d => d.ApplicationId == applicationId)
            .OrderByDescending(d => d.DecisionDateUtc)
            .Select(d => new HiringDecisionHistoryItemDto
            {
                Id = d.Id,
                DecisionType = d.DecisionType.ToString(),
                Status = d.Status.ToString(),
                IsFinal = d.IsFinal,
                Reason = d.Reason,
                Notes = d.Notes,
                DecisionByUserId = d.DecisionByUserId,
                DecisionByName = d.DecisionByUser.FullName,
                DecisionDateUtc = d.DecisionDateUtc,
                PriorApplicationStatus = d.PriorApplicationStatus.HasValue
                    ? d.PriorApplicationStatus.Value.ToString()
                    : null,
                ResultingApplicationStatus = d.ResultingApplicationStatus.HasValue
                    ? d.ResultingApplicationStatus.Value.ToString()
                    : null
            })
            .ToListAsync();

        return (true, null, items);
    }

    private static HiringManagerInterviewFeedbackDto MapFeedback(InterviewFeedback f) => new()
    {
        Id = f.Id,
        Rating = f.Rating,
        TechnicalCompetency = f.TechnicalCompetency,
        Communication = f.Communication,
        ProblemSolving = f.ProblemSolving,
        RoleKnowledge = f.RoleKnowledge,
        Teamwork = f.Teamwork,
        Leadership = f.Leadership,
        CulturalContribution = f.CulturalContribution,
        Strengths = f.Strengths,
        Concerns = f.Concerns,
        Recommendation = f.Recommendation,
        Comments = f.Comments,
        PrivatePanelComments = f.PrivatePanelComments,
        SubmittedAtUtc = f.SubmittedAtUtc
    };

    private static HiringManagerEvaluationDto MapEvaluation(CandidateEvaluation e) => new()
    {
        Id = e.Id,
        ApplicationId = e.ApplicationId,
        SubmissionStatus = e.SubmissionStatus.ToString(),
        OverallScore = e.OverallScore,
        RequiredSkillsAlignment = e.RequiredSkillsAlignment,
        PreferredSkillsAlignment = e.PreferredSkillsAlignment,
        RelevantExperience = e.RelevantExperience,
        EducationRequirement = e.EducationRequirement,
        AssessmentPerformance = e.AssessmentPerformance,
        InterviewPerformance = e.InterviewPerformance,
        Communication = e.Communication,
        ProblemSolving = e.ProblemSolving,
        RoleReadiness = e.RoleReadiness,
        Strengths = e.Strengths,
        Weaknesses = e.Weaknesses,
        DocumentedRisks = e.DocumentedRisks,
        Justification = e.Justification,
        Recommendation = e.Recommendation,
        CreatedAtUtc = e.CreatedAtUtc,
        SubmittedAtUtc = e.SubmittedAtUtc
    };

    private static string? Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = Regex.Replace(value.Trim(), "<.*?>", string.Empty);
        return trimmed.Length > 4000 ? trimmed[..4000] : trimmed;
    }
}
