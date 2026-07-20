using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Controllers;

/// <summary>
/// Development-only helpers for Phase 4 browser E2E. Disabled unless E2e:Enabled is true.
/// </summary>
[ApiController]
[Route("api/e2e")]
[AllowAnonymous]
public sealed class E2eSeedController : ControllerBase
{
    public const string CatalogJobTitle = "E2E Phase4 Full Stack Developer";

    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<E2eSeedController> _logger;

    public E2eSeedController(
        ApplicationDbContext db,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<E2eSeedController> logger)
    {
        _db = db;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    private bool IsE2eEnabled()
    {
        if (_environment.IsProduction())
        {
            return false;
        }

        return _configuration.GetValue<bool>("E2e:Enabled")
            || string.Equals(
                Environment.GetEnvironmentVariable("HIRESPHERE_E2E_SEED_ENABLED"),
                "true",
                StringComparison.OrdinalIgnoreCase);
    }

    private IActionResult Disabled() =>
        NotFound(new { message = "E2E seed endpoints are disabled." });

    [HttpPost("ensure-catalog")]
    public async Task<IActionResult> EnsureCatalog(CancellationToken cancellationToken)
    {
        if (!IsE2eEnabled())
        {
            return Disabled();
        }

        var job = await EnsureCatalogJobAsync(cancellationToken);
        return Ok(new
        {
            jobId = job.Id,
            title = job.Title,
            location = job.Location,
            employmentType = job.EmploymentType.ToString(),
            workArrangement = job.WorkArrangement.ToString(),
            screeningQuestionCount = job.ScreeningQuestions.Count,
            skillCount = job.JobSkills.Count
        });
    }

    public sealed class PrepareCandidateRequest
    {
        public string CandidateEmail { get; set; } = string.Empty;
    }

    [HttpPost("prepare-candidate-journey")]
    public async Task<IActionResult> PrepareCandidateJourney(
        [FromBody] PrepareCandidateRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsE2eEnabled())
        {
            return Disabled();
        }

        if (string.IsNullOrWhiteSpace(request.CandidateEmail))
        {
            return BadRequest(new { message = "CandidateEmail is required." });
        }

        var normalized = request.CandidateEmail.Trim().ToUpperInvariant();
        var candidate = await _db.Users.FirstOrDefaultAsync(
            u => u.NormalizedEmail == normalized && u.Role == "Candidate",
            cancellationToken);

        if (candidate is null)
        {
            return NotFound(new { message = "Candidate not found." });
        }

        var job = await EnsureCatalogJobAsync(cancellationToken);

        var application = await _db.Applications
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(
                a => a.CandidateId == candidate.Id && a.JobId == job.Id,
                cancellationToken);

        if (application is null)
        {
            application = new Application
            {
                CandidateId = candidate.Id,
                JobId = job.Id,
                AppliedDate = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow,
                Status = ApplicationStatus.Assessment,
                CoverLetter = "E2E prepared application"
            };
            application.StatusHistory.Add(new ApplicationStatusHistory
            {
                Status = ApplicationStatus.Pending,
                ChangedAtUtc = DateTime.UtcNow.AddHours(-2),
                Notes = "Submitted"
            });
            application.StatusHistory.Add(new ApplicationStatusHistory
            {
                Status = ApplicationStatus.Assessment,
                ChangedAtUtc = DateTime.UtcNow.AddHours(-1),
                Notes = "Assessment assigned"
            });
            _db.Applications.Add(application);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var assignment = await EnsureAssessmentAsync(candidate.Id, job.Id, application.Id, cancellationToken);
        var interviewId = await EnsureInterviewAsync(candidate.Id, application.Id, cancellationToken);
        await EnsureNotificationsAsync(candidate.Id, assignment.Id, interviewId, cancellationToken);

        _logger.LogInformation(
            "E2E journey prepared for candidate {Email}: job={JobId} app={AppId} assignment={AssignmentId} interview={InterviewId}",
            candidate.Email,
            job.Id,
            application.Id,
            assignment.Id,
            interviewId);

        return Ok(new
        {
            candidateId = candidate.Id,
            jobId = job.Id,
            applicationId = application.Id,
            assessmentAssignmentId = assignment.Id,
            interviewId
        });
    }

    private async Task<Job> EnsureCatalogJobAsync(CancellationToken cancellationToken)
    {
        var existing = await _db.Jobs
            .Include(j => j.ScreeningQuestions)
            .Include(j => j.JobSkills)
            .FirstOrDefaultAsync(j => j.Title == CatalogJobTitle, cancellationToken);

        if (existing is not null)
        {
            if (existing.Status != JobStatus.Open)
            {
                existing.Status = JobStatus.Open;
                existing.UpdatedAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
            }

            return existing;
        }

        var org = await _db.Organizations.FirstOrDefaultAsync(cancellationToken)
            ?? new Organization
            {
                Name = "HireSphere Demo Org",
                Description = "Demo organization",
                CreatedAtUtc = DateTime.UtcNow
            };

        if (org.Id == 0)
        {
            _db.Organizations.Add(org);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var dept = await _db.Departments.FirstOrDefaultAsync(d => d.OrganizationId == org.Id, cancellationToken)
            ?? new Department
            {
                OrganizationId = org.Id,
                Name = "Engineering",
                CreatedAtUtc = DateTime.UtcNow
            };

        if (dept.Id == 0)
        {
            _db.Departments.Add(dept);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var recruiter = await _db.Users.FirstOrDefaultAsync(u => u.Role == "Recruiter", cancellationToken);
        if (recruiter is null)
        {
            var email = $"e2e.recruiter.{Guid.NewGuid():N}@example.test";
            recruiter = new User
            {
                FullName = "E2E Recruiter",
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("E2eRecruiterPass123!"),
                Role = "Recruiter",
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.Users.Add(recruiter);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var skills = await _db.Skills
            .Where(s => s.Name == "C#" || s.Name == "React" || s.Name == "SQL Server" || s.Name == "JavaScript")
            .ToListAsync(cancellationToken);

        var job = new Job
        {
            Title = CatalogJobTitle,
            Description = "E2E catalog role for Phase 4 browser verification. Build APIs and React UIs.",
            RequiredSkills = "C#, React, SQL Server",
            Location = "Colombo",
            JobType = "Full-time",
            PostedDate = DateTime.UtcNow,
            RecruiterId = recruiter.Id,
            OrganizationId = org.Id,
            DepartmentId = dept.Id,
            Status = JobStatus.Open,
            EmploymentType = EmploymentType.FullTime,
            WorkArrangement = WorkArrangement.Hybrid,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var skill in skills)
        {
            job.JobSkills.Add(new JobSkill
            {
                SkillId = skill.Id,
                IsRequired = skill.Name is "C#" or "React",
                MinProficiencyLevel = "Intermediate"
            });
        }

        job.ScreeningQuestions.Add(new ScreeningQuestion
        {
            QuestionText = "How many years of full-stack experience do you have?",
            QuestionType = "Text",
            IsRequired = true,
            SortOrder = 1,
            CreatedAtUtc = DateTime.UtcNow
        });
        job.ScreeningQuestions.Add(new ScreeningQuestion
        {
            QuestionText = "Are you available to start within 30 days?",
            QuestionType = "Text",
            IsRequired = true,
            SortOrder = 2,
            CreatedAtUtc = DateTime.UtcNow
        });

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync(cancellationToken);
        return job;
    }

    private async Task<AssessmentAssignment> EnsureAssessmentAsync(
        int candidateId,
        int jobId,
        int applicationId,
        CancellationToken cancellationToken)
    {
        var existing = await _db.AssessmentAssignments
            .FirstOrDefaultAsync(
                a => a.CandidateId == candidateId && a.ApplicationId == applicationId,
                cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var assessment = new SkillAssessment
        {
            JobId = jobId,
            Title = "E2E C# Skills Check",
            Description = "Phase 4 browser assessment",
            DurationMinutes = 30,
            MaxAttempts = 3,
            PassingScorePercent = 50m,
            RevealResultsToCandidate = true,
            Status = AssessmentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
        assessment.Questions.Add(new AssessmentQuestion
        {
            QuestionText = "2 + 2 = ?",
            QuestionType = "MultipleChoice",
            Points = 10,
            SortOrder = 1,
            OptionsJson = "[\"3\",\"4\",\"5\"]",
            CorrectAnswerKey = "4"
        });
        assessment.Questions.Add(new AssessmentQuestion
        {
            QuestionText = "C# is statically typed.",
            QuestionType = "TrueFalse",
            Points = 5,
            SortOrder = 2,
            OptionsJson = "[\"True\",\"False\"]",
            CorrectAnswerKey = "True"
        });
        _db.SkillAssessments.Add(assessment);
        await _db.SaveChangesAsync(cancellationToken);

        var assignment = new AssessmentAssignment
        {
            SkillAssessmentId = assessment.Id,
            CandidateId = candidateId,
            ApplicationId = applicationId,
            AssignedAtUtc = DateTime.UtcNow,
            StartsAtUtc = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            MaxAttempts = 3,
            RevealResultsToCandidate = true,
            Status = AssessmentStatus.Pending
        };
        _db.AssessmentAssignments.Add(assignment);
        await _db.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    private async Task<int> EnsureInterviewAsync(
        int candidateId,
        int applicationId,
        CancellationToken cancellationToken)
    {
        var existing = await _db.Interviews
            .FirstOrDefaultAsync(i => i.ApplicationId == applicationId, cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        var interview = new Interview
        {
            ApplicationId = applicationId,
            InterviewDate = DateTime.UtcNow.AddDays(3),
            TimeZoneId = "Asia/Colombo",
            InterviewType = "Video",
            MeetingLink = "https://meet.example.test/e2e-room",
            MeetingInstructions = "Join five minutes early for E2E verification.",
            Status = InterviewStatus.Scheduled,
            CandidateResponse = InterviewCandidateResponse.Pending,
            RequireConfirmForMeetingInfo = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Interviews.Add(interview);

        var app = await _db.Applications.FirstAsync(a => a.Id == applicationId, cancellationToken);
        if (app.CandidateId != candidateId)
        {
            throw new InvalidOperationException("Application candidate mismatch.");
        }

        app.Status = ApplicationStatus.InterviewScheduled;
        await _db.SaveChangesAsync(cancellationToken);
        return interview.Id;
    }

    private async Task EnsureNotificationsAsync(
        int candidateId,
        int assignmentId,
        int interviewId,
        CancellationToken cancellationToken)
    {
        async Task EnsureOne(string title, string message, string category, string entityType, int entityId, string link)
        {
            var exists = await _db.Notifications.AnyAsync(
                n => n.UserId == candidateId && n.Title == title,
                cancellationToken);
            if (exists)
            {
                return;
            }

            _db.Notifications.Add(new Notification
            {
                UserId = candidateId,
                Title = title,
                Message = message,
                IsRead = false,
                Category = category,
                RelatedEntityType = entityType,
                RelatedEntityId = entityId,
                LinkPath = link,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await EnsureOne(
            "Assessment assigned",
            "Complete your E2E C# Skills Check.",
            "Assessment",
            "AssessmentAssignment",
            assignmentId,
            $"/candidate/assessments/{assignmentId}");

        await EnsureOne(
            "Interview scheduled",
            "A video interview has been scheduled for your application.",
            "Interview",
            "Interview",
            interviewId,
            $"/candidate/interviews/{interviewId}");

        await _db.SaveChangesAsync(cancellationToken);
    }

    public sealed class EnsureRecruiterPortalRequest
    {
        public string? OrganizationName { get; set; }
        public string? AdminEmail { get; set; }
        public string? AdminPassword { get; set; }
        public string? RecruiterEmail { get; set; }
        public string? RecruiterPassword { get; set; }
        public string? HiringManagerEmail { get; set; }
        public string? HiringManagerPassword { get; set; }
    }

    [HttpPost("ensure-recruiter-portal")]
    public async Task<IActionResult> EnsureRecruiterPortal(
        [FromBody] EnsureRecruiterPortalRequest? request,
        CancellationToken cancellationToken)
    {
        if (!IsE2eEnabled())
        {
            return Disabled();
        }

        var orgName = string.IsNullOrWhiteSpace(request?.OrganizationName)
            ? "E2E Recruiter Org"
            : request.OrganizationName.Trim();
        var adminEmail = request?.AdminEmail ?? "e2e-admin@hiresphere.local";
        var adminPassword = request?.AdminPassword
            ?? Environment.GetEnvironmentVariable("HIRESPHERE_E2E_ADMIN_PASSWORD")
            ?? "AdminE2ePass123!";
        var recruiterEmail = request?.RecruiterEmail ?? $"e2e-recruiter-{Guid.NewGuid():N}@hiresphere.local";
        var recruiterPassword = request?.RecruiterPassword
            ?? Environment.GetEnvironmentVariable("HIRESPHERE_E2E_RECRUITER_PASSWORD")
            ?? "RecruiterE2ePass123!";
        var hmEmail = request?.HiringManagerEmail ?? $"e2e-hm-{Guid.NewGuid():N}@hiresphere.local";
        var hmPassword = request?.HiringManagerPassword
            ?? Environment.GetEnvironmentVariable("HIRESPHERE_E2E_HM_PASSWORD")
            ?? "HiringMgrE2ePass123!";

        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Name == orgName, cancellationToken);
        if (org is null)
        {
            org = new Organization { Name = orgName, CreatedAtUtc = DateTime.UtcNow };
            _db.Organizations.Add(org);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var admin = await EnsureUserWithRoleAsync(adminEmail, adminPassword, "Admin", org.Id, cancellationToken);
        var recruiter = await EnsureUserWithRoleAsync(recruiterEmail, recruiterPassword, "Recruiter", org.Id, cancellationToken);
        var hm = await EnsureUserWithRoleAsync(hmEmail, hmPassword, "HiringManager", org.Id, cancellationToken);

        return Ok(new
        {
            organizationId = org.Id,
            adminEmail = admin.Email,
            recruiterEmail = recruiter.Email,
            hiringManagerEmail = hm.Email,
            hiringManagerUserId = hm.Id,
            passwordsFromRequestOrEnv = true
        });
    }

    private async Task<User> EnsureUserWithRoleAsync(
        string email,
        string password,
        string roleName,
        int organizationId,
        CancellationToken cancellationToken)
    {
        var normalized = email.Trim().ToUpperInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                FullName = $"E2E {roleName}",
                Email = email.Trim(),
                NormalizedEmail = normalized,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = roleName,
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.Status = UserStatus.Active;
            user.Role = roleName;
        }

        var role = await _db.Roles.FirstAsync(r => r.Name == roleName, cancellationToken);
        if (!await _db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken))
        {
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        }

        if (roleName == "Recruiter")
        {
            var profile = await _db.RecruiterProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id, cancellationToken);
            if (profile is null)
            {
                _db.RecruiterProfiles.Add(new RecruiterProfile
                {
                    UserId = user.Id,
                    OrganizationId = organizationId,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
            else
            {
                profile.OrganizationId = organizationId;
            }
        }
        else if (roleName == "HiringManager")
        {
            var profile = await _db.HiringManagerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id, cancellationToken);
            if (profile is null)
            {
                _db.HiringManagerProfiles.Add(new HiringManagerProfile
                {
                    UserId = user.Id,
                    OrganizationId = organizationId,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
            else
            {
                profile.OrganizationId = organizationId;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return user;
    }
}
