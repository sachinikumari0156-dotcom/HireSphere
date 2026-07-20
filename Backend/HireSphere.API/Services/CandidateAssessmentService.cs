using System.Text.Json;
using HireSphere.API.Data;
using HireSphere.API.DTOs.Candidate;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public interface ICandidateAssessmentService
{
    Task<(bool Ok, string? Error, IReadOnlyList<CandidateAssessmentListItemDto>? Result)> ListAsync();

    Task<(bool Ok, string? Error, CandidateAssessmentDetailDto? Result)> GetAssignmentAsync(int assignmentId);

    Task<(bool Ok, string? Error, CandidateAssessmentAttemptDto? Result)> StartAsync(int assignmentId);

    Task<(bool Ok, string? Error, CandidateAssessmentAttemptDto? Result)> GetAttemptAsync(int attemptId);

    Task<(bool Ok, string? Error, CandidateAssessmentAttemptDto? Result)> SaveAnswersAsync(
        int attemptId,
        SaveAssessmentAnswersDto dto);

    Task<(bool Ok, string? Error, CandidateAssessmentAttemptDto? Result)> SubmitAsync(int attemptId);
}

public sealed class CandidateAssessmentService : ICandidateAssessmentService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CandidateAssessmentService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<(bool Ok, string? Error, IReadOnlyList<CandidateAssessmentListItemDto>? Result)> ListAsync()
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var assignments = await LoadAssignmentsQuery(profile.UserId)
            .AsNoTracking()
            .OrderByDescending(a => a.AssignedAtUtc)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var items = assignments.Select(a => MapListItem(a, now)).ToList();
        return (true, null, items);
    }

    public async Task<(bool Ok, string? Error, CandidateAssessmentDetailDto? Result)> GetAssignmentAsync(
        int assignmentId)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var assignment = await LoadAssignmentsQuery(profile.UserId)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment == null)
        {
            return (false, "Assessment not found.", null);
        }

        return (true, null, MapDetail(assignment, DateTime.UtcNow));
    }

    public async Task<(bool Ok, string? Error, CandidateAssessmentAttemptDto? Result)> StartAsync(int assignmentId)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var assignment = await LoadAssignmentsQuery(profile.UserId)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment == null)
        {
            return (false, "Assessment not found.", null);
        }

        var now = DateTime.UtcNow;
        var (canStart, blockReason) = EvaluateCanStart(assignment, now);
        if (!canStart)
        {
            return (false, blockReason, null);
        }

        var active = assignment.Attempts.FirstOrDefault(a => a.Status == AssessmentStatus.InProgress);
        if (active != null)
        {
            if (active.AttemptExpiresAtUtc is DateTime exp && exp <= now)
            {
                ExpireAttempt(active, now);
                await _db.SaveChangesAsync();
            }
            else
            {
                return await GetAttemptAsync(active.Id);
            }
        }

        var duration = assignment.SkillAssessment.DurationMinutes;
        DateTime? attemptExpiry = null;
        if (duration is int minutes && minutes > 0)
        {
            attemptExpiry = now.AddMinutes(minutes);
        }

        if (assignment.ExpiresAtUtc is DateTime assignmentExpiry)
        {
            attemptExpiry = attemptExpiry is null
                ? assignmentExpiry
                : (attemptExpiry < assignmentExpiry ? attemptExpiry : assignmentExpiry);
        }

        var attempt = new AssessmentAttempt
        {
            SkillAssessmentId = assignment.SkillAssessmentId,
            AssessmentAssignmentId = assignment.Id,
            CandidateId = profile.UserId,
            StartedAtUtc = now,
            AttemptExpiresAtUtc = attemptExpiry,
            Status = AssessmentStatus.InProgress
        };

        assignment.Status = AssessmentStatus.InProgress;
        assignment.Attempts.Add(attempt);

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = profile.UserId,
            Action = "AssessmentAttemptStarted",
            EntityType = nameof(AssessmentAttempt),
            Details = $"Assignment {assignment.Id} started.",
            CreatedAtUtc = now
        });

        await _db.SaveChangesAsync();
        return await GetAttemptAsync(attempt.Id);
    }

    public async Task<(bool Ok, string? Error, CandidateAssessmentAttemptDto? Result)> GetAttemptAsync(int attemptId)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var attempt = await _db.AssessmentAttempts
            .Include(a => a.Answers)
            .Include(a => a.Result)
            .Include(a => a.AssessmentAssignment)
            .Include(a => a.SkillAssessment)
                .ThenInclude(s => s.Questions)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.CandidateId == profile.UserId);

        if (attempt == null)
        {
            return (false, "Attempt not found.", null);
        }

        var now = DateTime.UtcNow;
        if (attempt.Status == AssessmentStatus.InProgress
            && attempt.AttemptExpiresAtUtc is DateTime exp
            && exp <= now)
        {
            ExpireAttempt(attempt, now);
            await _db.SaveChangesAsync();
        }

        return (true, null, MapAttempt(attempt));
    }

    public async Task<(bool Ok, string? Error, CandidateAssessmentAttemptDto? Result)> SaveAnswersAsync(
        int attemptId,
        SaveAssessmentAnswersDto dto)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var attempt = await _db.AssessmentAttempts
            .Include(a => a.Answers)
            .Include(a => a.Result)
            .Include(a => a.AssessmentAssignment)
            .Include(a => a.SkillAssessment)
                .ThenInclude(s => s.Questions)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.CandidateId == profile.UserId);

        if (attempt == null)
        {
            return (false, "Attempt not found.", null);
        }

        var now = DateTime.UtcNow;
        if (attempt.Status != AssessmentStatus.InProgress)
        {
            return (false, "This attempt is no longer open for answers.", null);
        }

        if (attempt.AttemptExpiresAtUtc is DateTime exp && exp <= now)
        {
            ExpireAttempt(attempt, now);
            await _db.SaveChangesAsync();
            return (false, "This attempt has expired.", null);
        }

        var questionIds = attempt.SkillAssessment.Questions.Select(q => q.Id).ToHashSet();
        foreach (var input in dto.Answers ?? Array.Empty<AssessmentAnswerInputDto>())
        {
            if (!questionIds.Contains(input.QuestionId))
            {
                return (false, "One or more answers refer to an invalid question.", null);
            }

            var existing = attempt.Answers.FirstOrDefault(a => a.AssessmentQuestionId == input.QuestionId);
            var value = input.AnswerValue?.Trim() ?? string.Empty;
            if (existing == null)
            {
                attempt.Answers.Add(new AssessmentAnswer
                {
                    AssessmentQuestionId = input.QuestionId,
                    AnswerValue = value,
                    CreatedAtUtc = now
                });
            }
            else
            {
                existing.AnswerValue = value;
                existing.UpdatedAtUtc = now;
            }
        }

        await _db.SaveChangesAsync();
        return await GetAttemptAsync(attempt.Id);
    }

    public async Task<(bool Ok, string? Error, CandidateAssessmentAttemptDto? Result)> SubmitAsync(int attemptId)
    {
        var (profileOk, profileError, profile) = await RequireProfileAsync();
        if (!profileOk || profile == null)
        {
            return (false, profileError, null);
        }

        var attempt = await _db.AssessmentAttempts
            .Include(a => a.Answers)
            .Include(a => a.Result)
            .Include(a => a.AssessmentAssignment)
            .Include(a => a.SkillAssessment)
                .ThenInclude(s => s.Questions)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.CandidateId == profile.UserId);

        if (attempt == null)
        {
            return (false, "Attempt not found.", null);
        }

        var now = DateTime.UtcNow;
        if (attempt.Status != AssessmentStatus.InProgress)
        {
            return (false, "This attempt has already been submitted or closed.", null);
        }

        if (attempt.AttemptExpiresAtUtc is DateTime exp && exp <= now)
        {
            ExpireAttempt(attempt, now);
            await _db.SaveChangesAsync();
            return (false, "This attempt has expired.", null);
        }

        ScoreAttempt(attempt, now);

        attempt.Status = AssessmentStatus.Completed;
        attempt.CompletedAtUtc = now;
        attempt.AssessmentAssignment.Status = AssessmentStatus.Completed;

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = profile.UserId,
            Action = "AssessmentAttemptSubmitted",
            EntityType = nameof(AssessmentAttempt),
            EntityId = attempt.Id,
            Details = $"Score {attempt.Result?.Score}/{attempt.Result?.MaxScore}",
            CreatedAtUtc = now
        });

        await _db.SaveChangesAsync();
        return await GetAttemptAsync(attempt.Id);
    }

    private static void ScoreAttempt(AssessmentAttempt attempt, DateTime now)
    {
        decimal score = 0;
        decimal maxScore = 0;

        foreach (var question in attempt.SkillAssessment.Questions)
        {
            maxScore += question.Points;
            var answer = attempt.Answers.FirstOrDefault(a => a.AssessmentQuestionId == question.Id);
            var awarded = 0m;
            if (answer != null
                && !string.IsNullOrWhiteSpace(answer.AnswerValue)
                && AnswersMatch(answer.AnswerValue, question.CorrectAnswerKey))
            {
                awarded = question.Points;
                score += awarded;
            }

            if (answer != null)
            {
                answer.AwardedPoints = awarded;
                answer.UpdatedAtUtc = now;
            }
        }

        var percent = maxScore <= 0 ? 0 : Math.Round(score / maxScore * 100m, 2);
        var passed = percent >= attempt.SkillAssessment.PassingScorePercent;

        attempt.Result = new AssessmentResult
        {
            Score = score,
            MaxScore = maxScore,
            Passed = passed,
            Feedback = passed
                ? "You met the passing score for this assessment."
                : "You did not meet the passing score for this assessment.",
            CreatedAtUtc = now
        };
    }

    private static bool AnswersMatch(string submitted, string correctKey)
    {
        return string.Equals(
            submitted.Trim(),
            correctKey.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    private static void ExpireAttempt(AssessmentAttempt attempt, DateTime now)
    {
        if (attempt.Status != AssessmentStatus.InProgress)
        {
            return;
        }

        attempt.Status = AssessmentStatus.Expired;
        attempt.CompletedAtUtc = now;
        if (attempt.AssessmentAssignment.Status == AssessmentStatus.InProgress)
        {
            attempt.AssessmentAssignment.Status = AssessmentStatus.Expired;
        }
    }

    private IQueryable<AssessmentAssignment> LoadAssignmentsQuery(int candidateId) =>
        _db.AssessmentAssignments
            .Include(a => a.SkillAssessment)
                .ThenInclude(s => s.Questions)
            .Include(a => a.SkillAssessment)
                .ThenInclude(s => s.Job)
            .Include(a => a.Attempts)
                .ThenInclude(t => t.Result)
            .Where(a => a.CandidateId == candidateId);

    private static (bool CanStart, string? BlockReason) EvaluateCanStart(
        AssessmentAssignment assignment,
        DateTime now)
    {
        if (assignment.Status == AssessmentStatus.Cancelled)
        {
            return (false, "This assessment has been cancelled.");
        }

        if (assignment.StartsAtUtc is DateTime start && start > now)
        {
            return (false, "This assessment is not available yet.");
        }

        if (assignment.ExpiresAtUtc is DateTime expiry && expiry <= now)
        {
            return (false, "This assessment has expired.");
        }

        var used = assignment.Attempts.Count(a =>
            a.Status is AssessmentStatus.Completed or AssessmentStatus.Expired or AssessmentStatus.InProgress);
        var inProgress = assignment.Attempts.FirstOrDefault(a => a.Status == AssessmentStatus.InProgress);
        if (inProgress != null)
        {
            if (inProgress.AttemptExpiresAtUtc is DateTime ae && ae <= now)
            {
                // treated as expired by caller
            }
            else
            {
                return (true, null);
            }

            used = assignment.Attempts.Count(a =>
                a.Status is AssessmentStatus.Completed or AssessmentStatus.Expired);
        }

        if (used >= assignment.MaxAttempts)
        {
            return (false, "No attempts remaining for this assessment.");
        }

        return (true, null);
    }

    private static CandidateAssessmentListItemDto MapListItem(AssessmentAssignment a, DateTime now)
    {
        var (canStart, block) = EvaluateCanStart(a, now);
        var used = CountUsedAttempts(a);
        var active = a.Attempts
            .Where(t => t.Status == AssessmentStatus.InProgress)
            .OrderByDescending(t => t.StartedAtUtc)
            .FirstOrDefault();
        var latest = a.Attempts.OrderByDescending(t => t.StartedAtUtc).FirstOrDefault();

        return new CandidateAssessmentListItemDto
        {
            AssignmentId = a.Id,
            SkillAssessmentId = a.SkillAssessmentId,
            Title = a.SkillAssessment.Title,
            Description = a.SkillAssessment.Description,
            DurationMinutes = a.SkillAssessment.DurationMinutes,
            MaxAttempts = a.MaxAttempts,
            AttemptsUsed = used,
            AttemptsRemaining = Math.Max(0, a.MaxAttempts - used),
            AssignedAtUtc = a.AssignedAtUtc,
            StartsAtUtc = a.StartsAtUtc,
            ExpiresAtUtc = a.ExpiresAtUtc,
            Status = a.Status,
            CanStart = canStart,
            BlockReason = canStart ? null : block,
            ActiveAttemptId = active?.Id,
            LatestAttemptId = latest?.Id,
            ApplicationId = a.ApplicationId,
            JobTitle = a.SkillAssessment.Job?.Title
        };
    }

    private static CandidateAssessmentDetailDto MapDetail(AssessmentAssignment a, DateTime now)
    {
        var list = MapListItem(a, now);
        return new CandidateAssessmentDetailDto
        {
            AssignmentId = list.AssignmentId,
            SkillAssessmentId = list.SkillAssessmentId,
            Title = list.Title,
            Description = list.Description,
            DurationMinutes = list.DurationMinutes,
            MaxAttempts = list.MaxAttempts,
            AttemptsUsed = list.AttemptsUsed,
            AttemptsRemaining = list.AttemptsRemaining,
            AssignedAtUtc = list.AssignedAtUtc,
            StartsAtUtc = list.StartsAtUtc,
            ExpiresAtUtc = list.ExpiresAtUtc,
            Status = list.Status,
            CanStart = list.CanStart,
            BlockReason = list.BlockReason,
            RevealResultsToCandidate = a.RevealResultsToCandidate,
            ActiveAttemptId = list.ActiveAttemptId,
            LatestAttemptId = list.LatestAttemptId,
            ApplicationId = list.ApplicationId,
            JobTitle = list.JobTitle,
            Questions = MapQuestions(a.SkillAssessment.Questions)
        };
    }

    private static CandidateAssessmentAttemptDto MapAttempt(AssessmentAttempt attempt)
    {
        var reveal = attempt.AssessmentAssignment.RevealResultsToCandidate
            && attempt.Status == AssessmentStatus.Completed
            && attempt.Result != null;

        CandidateAssessmentResultDto? resultDto = null;
        if (reveal && attempt.Result != null)
        {
            var max = attempt.Result.MaxScore;
            var percent = max <= 0 ? 0 : Math.Round(attempt.Result.Score / max * 100m, 2);
            resultDto = new CandidateAssessmentResultDto
            {
                Score = attempt.Result.Score,
                MaxScore = attempt.Result.MaxScore,
                ScorePercent = percent,
                Passed = attempt.Result.Passed,
                Feedback = attempt.Result.Feedback
            };
        }

        return new CandidateAssessmentAttemptDto
        {
            AttemptId = attempt.Id,
            AssignmentId = attempt.AssessmentAssignmentId,
            SkillAssessmentId = attempt.SkillAssessmentId,
            Title = attempt.SkillAssessment.Title,
            Status = attempt.Status,
            StartedAtUtc = attempt.StartedAtUtc,
            CompletedAtUtc = attempt.CompletedAtUtc,
            AttemptExpiresAtUtc = attempt.AttemptExpiresAtUtc,
            ResultsVisible = reveal,
            Result = resultDto,
            Questions = MapQuestions(attempt.SkillAssessment.Questions),
            Answers = attempt.Answers
                .Select(ans => new CandidateAssessmentAnswerDto
                {
                    QuestionId = ans.AssessmentQuestionId,
                    AnswerValue = ans.AnswerValue,
                    AwardedPoints = reveal ? ans.AwardedPoints : null
                })
                .ToList()
        };
    }

    private static IReadOnlyList<CandidateAssessmentQuestionDto> MapQuestions(
        IEnumerable<AssessmentQuestion> questions)
    {
        return questions
            .OrderBy(q => q.SortOrder)
            .ThenBy(q => q.Id)
            .Select(q => new CandidateAssessmentQuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Points = q.Points,
                SortOrder = q.SortOrder,
                Options = ParseOptions(q.OptionsJson)
            })
            .ToList();
    }

    private static IReadOnlyList<string> ParseOptions(string? optionsJson)
    {
        if (string.IsNullOrWhiteSpace(optionsJson))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(optionsJson) ?? new List<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private static int CountUsedAttempts(AssessmentAssignment assignment) =>
        assignment.Attempts.Count(a =>
            a.Status is AssessmentStatus.Completed or AssessmentStatus.Expired or AssessmentStatus.InProgress);

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
