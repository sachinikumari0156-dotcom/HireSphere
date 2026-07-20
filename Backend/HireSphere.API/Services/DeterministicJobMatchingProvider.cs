using System.Text;
using HireSphere.API.DTOs.Candidate;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;

namespace HireSphere.API.Services;

public interface IJobMatchingProvider
{
    string ProviderName { get; }

    JobMatchResultDto ComputeMatch(CandidateProfile profile, Job job);
}

/// <summary>
/// Deterministic rules-based job matcher. Not an external AI provider.
/// </summary>
public sealed class DeterministicJobMatchingProvider : IJobMatchingProvider
{
    public const string ProviderKey = "Deterministic";

    public string ProviderName => ProviderKey;

    public JobMatchResultDto ComputeMatch(CandidateProfile profile, Job job)
    {
        var computedAt = DateTime.UtcNow;
        var candidateSkillNames = GetCandidateSkillNames(profile);
        var (requiredSkills, preferredSkills) = GetJobSkillNames(job);

        var allJobSkills = requiredSkills
            .Concat(preferredSkills)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var matched = allJobSkills
            .Where(s => candidateSkillNames.Contains(s, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var missingRequired = requiredSkills
            .Where(s => !candidateSkillNames.Contains(s, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var missingPreferred = preferredSkills
            .Where(s => !candidateSkillNames.Contains(s, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var missingAll = missingRequired
            .Concat(missingPreferred)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();

        decimal skillScore;
        if (requiredSkills.Count == 0 && preferredSkills.Count == 0)
        {
            skillScore = candidateSkillNames.Count > 0 ? 55m : 35m;
        }
        else if (requiredSkills.Count > 0)
        {
            var requiredMatchRatio = (decimal)requiredSkills.Count(s =>
                candidateSkillNames.Contains(s, StringComparer.OrdinalIgnoreCase)) / requiredSkills.Count;
            var preferredMatchRatio = preferredSkills.Count == 0
                ? 1m
                : (decimal)preferredSkills.Count(s =>
                    candidateSkillNames.Contains(s, StringComparer.OrdinalIgnoreCase)) / preferredSkills.Count;
            skillScore = Math.Round((requiredMatchRatio * 45m) + (preferredMatchRatio * 10m), 2);
        }
        else
        {
            var preferredMatchRatio = (decimal)preferredSkills.Count(s =>
                candidateSkillNames.Contains(s, StringComparer.OrdinalIgnoreCase)) / preferredSkills.Count;
            skillScore = Math.Round(preferredMatchRatio * 55m, 2);
        }

        var experience = ScoreExperience(profile);
        var education = ScoreEducation(profile);
        var location = ScoreLocation(profile, job);
        var arrangement = ScoreWorkArrangement(profile, job);

        var total = Math.Clamp(
            Math.Round(
                skillScore
                + experience.FactorScore
                + education.FactorScore
                + location.FactorScore
                + arrangement.FactorScore,
                2),
            0m,
            100m);

        var explanation = BuildExplanation(
            matched,
            missingRequired,
            experience,
            education,
            location,
            arrangement,
            total);

        return new JobMatchResultDto
        {
            JobId = job.Id,
            MatchScore = total,
            MatchedSkills = matched,
            MissingSkills = missingAll,
            Experience = experience,
            Education = education,
            Location = location,
            WorkArrangement = arrangement,
            Explanation = explanation,
            Provider = ProviderKey,
            ComputedAtUtc = computedAt
        };
    }

    private static HashSet<string> GetCandidateSkillNames(CandidateProfile profile)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (profile.CandidateSkills != null)
        {
            foreach (var cs in profile.CandidateSkills)
            {
                if (cs.Skill?.Name is { Length: > 0 } name)
                {
                    names.Add(name.Trim());
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(profile.Skills))
        {
            foreach (var part in profile.Skills.Split(
                         new[] { ',', ';', '|' },
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                names.Add(part);
            }
        }

        return names;
    }

    private static (List<string> Required, List<string> Preferred) GetJobSkillNames(Job job)
    {
        var required = new List<string>();
        var preferred = new List<string>();

        if (job.JobSkills is { Count: > 0 })
        {
            foreach (var js in job.JobSkills)
            {
                var name = js.Skill?.Name?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if (js.IsRequired)
                {
                    required.Add(name);
                }
                else
                {
                    preferred.Add(name);
                }
            }
        }
        else if (!string.IsNullOrWhiteSpace(job.RequiredSkills))
        {
            foreach (var part in job.RequiredSkills.Split(
                         new[] { ',', ';', '|' },
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                required.Add(part);
            }
        }

        return (
            required.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            preferred.Distinct(StringComparer.OrdinalIgnoreCase).ToList());
    }

    private static ExperienceComparisonDto ScoreExperience(CandidateProfile profile)
    {
        var years = profile.YearsOfExperience;
        if (!years.HasValue && profile.WorkExperiences is { Count: > 0 })
        {
            years = EstimateYearsFromExperiences(profile.WorkExperiences);
        }

        decimal factor;
        string assessment;
        if (!years.HasValue || years.Value <= 0)
        {
            factor = 3m;
            assessment = "No years of experience recorded on the profile.";
        }
        else if (years.Value < 2)
        {
            factor = 8m;
            assessment = $"Candidate reports {years.Value} year(s) of experience (early career).";
        }
        else if (years.Value < 5)
        {
            factor = 12m;
            assessment = $"Candidate reports {years.Value} years of experience (mid-level range).";
        }
        else
        {
            factor = 15m;
            assessment = $"Candidate reports {years.Value} years of experience (strong experience signal).";
        }

        return new ExperienceComparisonDto
        {
            CandidateYears = years,
            Assessment = assessment,
            FactorScore = factor
        };
    }

    private static int EstimateYearsFromExperiences(ICollection<WorkExperience> experiences)
    {
        double totalDays = 0;
        foreach (var exp in experiences)
        {
            var end = exp.IsCurrentRole || exp.EndDate is null ? DateTime.UtcNow : exp.EndDate.Value;
            if (end < exp.StartDate)
            {
                continue;
            }

            totalDays += (end - exp.StartDate).TotalDays;
        }

        return (int)Math.Max(0, Math.Round(totalDays / 365.25));
    }

    private static EducationComparisonDto ScoreEducation(CandidateProfile profile)
    {
        var count = profile.Educations?.Count ?? 0;
        decimal factor;
        string assessment;
        if (count == 0)
        {
            factor = 2m;
            assessment = "No education entries on the profile.";
        }
        else if (count == 1)
        {
            factor = 7m;
            assessment = "One education entry recorded.";
        }
        else
        {
            factor = 10m;
            assessment = $"{count} education entries recorded.";
        }

        return new EducationComparisonDto
        {
            CandidateEducationCount = count,
            Assessment = assessment,
            FactorScore = factor
        };
    }

    private static LocationFactorDto ScoreLocation(CandidateProfile profile, Job job)
    {
        var candidateLoc = profile.Location?.Trim();
        var jobLoc = job.Location?.Trim() ?? string.Empty;
        var isMatch = !string.IsNullOrWhiteSpace(candidateLoc)
                      && !string.IsNullOrWhiteSpace(jobLoc)
                      && (jobLoc.Contains(candidateLoc, StringComparison.OrdinalIgnoreCase)
                          || candidateLoc.Contains(jobLoc, StringComparison.OrdinalIgnoreCase));

        // Remote jobs are location-flexible
        if (job.WorkArrangement == WorkArrangement.Remote)
        {
            isMatch = true;
        }

        return new LocationFactorDto
        {
            CandidateLocation = candidateLoc,
            JobLocation = jobLoc,
            IsMatch = isMatch,
            FactorScore = isMatch ? 10m : (string.IsNullOrWhiteSpace(candidateLoc) ? 3m : 4m)
        };
    }

    private static WorkArrangementFactorDto ScoreWorkArrangement(CandidateProfile profile, Job job)
    {
        var preference = profile.PreferredWorkArrangement;
        var isMatch = preference.HasValue && preference.Value == job.WorkArrangement;

        return new WorkArrangementFactorDto
        {
            CandidatePreference = preference,
            JobArrangement = job.WorkArrangement,
            IsMatch = isMatch,
            FactorScore = isMatch ? 10m : (preference.HasValue ? 4m : 5m)
        };
    }

    private static string BuildExplanation(
        IReadOnlyList<string> matched,
        IReadOnlyList<string> missingRequired,
        ExperienceComparisonDto experience,
        EducationComparisonDto education,
        LocationFactorDto location,
        WorkArrangementFactorDto arrangement,
        decimal total)
    {
        var sb = new StringBuilder();
        sb.Append($"Overall match score {total:0.##}/100 (deterministic). ");
        sb.Append(matched.Count > 0
            ? $"Matched skills: {string.Join(", ", matched)}. "
            : "No overlapping skills found. ");
        if (missingRequired.Count > 0)
        {
            sb.Append($"Missing required skills: {string.Join(", ", missingRequired)}. ");
        }

        sb.Append(experience.Assessment).Append(' ');
        sb.Append(education.Assessment).Append(' ');
        sb.Append(location.IsMatch
            ? "Location/work mode aligns with the role. "
            : "Location alignment is limited. ");
        sb.Append(arrangement.IsMatch
            ? "Preferred work arrangement matches the job."
            : "Preferred work arrangement differs from the job.");
        return sb.ToString().Trim();
    }
}
