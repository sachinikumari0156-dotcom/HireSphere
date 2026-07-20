using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using HireSphere.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HireSphere.API.Tests;

public class CandidateJobsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CandidateJobsControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(User User, int ProfileId)> SeedCandidateAsync(
        string? email = null,
        Action<CandidateProfile>? configureProfile = null)
    {
        email ??= $"cand-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);

        var user = new User
        {
            FullName = "Candidate User",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Candidate",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var role = await db.Roles.FirstAsync(r => r.Name == "Candidate");
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });

        var profile = new CandidateProfile
        {
            UserId = user.Id,
            FullName = user.FullName,
            CreatedAtUtc = DateTime.UtcNow
        };
        configureProfile?.Invoke(profile);
        db.CandidateProfiles.Add(profile);
        await db.SaveChangesAsync();

        return (user, profile.Id);
    }

    private async Task EnrichProfileForRecommendationsAsync(int profileId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var profile = await db.CandidateProfiles.FirstAsync(p => p.Id == profileId);
        profile.PhoneNumber = "555-0100";
        profile.Summary = "Experienced developer";
        profile.Location = "Colombo";
        profile.DesiredJobTitle = "Software Engineer";
        profile.PreferredWorkArrangement = WorkArrangement.Hybrid;
        profile.SalaryExpectation = 100000;
        profile.Availability = "Immediate";
        profile.LinkedInUrl = "https://linkedin.com/in/test";

        var skill = await db.Skills.FirstOrDefaultAsync(s => s.Name == "C#");
        if (skill == null)
        {
            skill = new Skill { Name = "C#", CreatedAtUtc = DateTime.UtcNow };
            db.Skills.Add(skill);
            await db.SaveChangesAsync();
        }

        if (!await db.CandidateSkills.AnyAsync(cs => cs.CandidateProfileId == profileId && cs.SkillId == skill.Id))
        {
            db.CandidateSkills.Add(new CandidateSkill
            {
                CandidateProfileId = profileId,
                SkillId = skill.Id,
                ProficiencyLevel = "Advanced",
                YearsOfExperience = 3
            });
        }

        db.WorkExperiences.Add(new WorkExperience
        {
            CandidateProfileId = profileId,
            CompanyName = "Acme",
            JobTitle = "Developer",
            StartDate = new DateTime(2020, 1, 1),
            IsCurrentRole = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        db.Educations.Add(new Education
        {
            CandidateProfileId = profileId,
            Institution = "Uni",
            Degree = "BSc",
            StartDate = new DateTime(2016, 1, 1),
            EndDate = new DateTime(2020, 1, 1),
            CreatedAtUtc = DateTime.UtcNow
        });

        db.Resumes.Add(new Resume
        {
            CandidateProfileId = profileId,
            FilePath = "resumes/test.pdf",
            FileName = "resume.pdf",
            IsPrimary = true,
            UploadedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        });

        profile.YearsOfExperience = 4;
        await db.SaveChangesAsync();
    }

    private async Task<(int OpenJobId, int ClosedJobId, int CsharpSkillId, int DeptId)> SeedJobsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var org = new Organization { Name = "HireSphere Org", CreatedAtUtc = DateTime.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var dept = new Department
        {
            OrganizationId = org.Id,
            Name = "Engineering",
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Departments.Add(dept);
        await db.SaveChangesAsync();

        var recruiterEmail = $"rec-{Guid.NewGuid():N}@example.com";
        var recruiter = new User
        {
            FullName = "Recruiter",
            Email = recruiterEmail,
            NormalizedEmail = recruiterEmail.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("SecurePass123!"),
            Role = "Recruiter",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(recruiter);
        await db.SaveChangesAsync();

        var csharp = await db.Skills.FirstOrDefaultAsync(s => s.Name == "C#");
        if (csharp == null)
        {
            csharp = new Skill { Name = "C#", CreatedAtUtc = DateTime.UtcNow };
            db.Skills.Add(csharp);
        }

        var sql = await db.Skills.FirstOrDefaultAsync(s => s.Name == "SQL");
        if (sql == null)
        {
            sql = new Skill { Name = "SQL", CreatedAtUtc = DateTime.UtcNow };
            db.Skills.Add(sql);
        }

        await db.SaveChangesAsync();

        var openJob = new Job
        {
            Title = "Senior C# Developer",
            Description = "Build APIs with C# and SQL",
            RequiredSkills = "C#, SQL",
            Location = "Colombo",
            JobType = "Full-time",
            PostedDate = DateTime.UtcNow.AddDays(-1),
            RecruiterId = recruiter.Id,
            OrganizationId = org.Id,
            DepartmentId = dept.Id,
            Status = JobStatus.Open,
            EmploymentType = EmploymentType.FullTime,
            WorkArrangement = WorkArrangement.Hybrid,
            CreatedAtUtc = DateTime.UtcNow
        };

        var openJobB = new Job
        {
            Title = "Junior Support Role",
            Description = "Customer support",
            RequiredSkills = "Communication",
            Location = "Kandy",
            JobType = "Part-time",
            PostedDate = DateTime.UtcNow.AddDays(-2),
            RecruiterId = recruiter.Id,
            OrganizationId = org.Id,
            DepartmentId = dept.Id,
            Status = JobStatus.Open,
            EmploymentType = EmploymentType.PartTime,
            WorkArrangement = WorkArrangement.OnSite,
            CreatedAtUtc = DateTime.UtcNow
        };

        var closedJob = new Job
        {
            Title = "Closed Architect",
            Description = "Should not be visible",
            RequiredSkills = "C#",
            Location = "Colombo",
            JobType = "Full-time",
            PostedDate = DateTime.UtcNow.AddDays(-3),
            RecruiterId = recruiter.Id,
            OrganizationId = org.Id,
            DepartmentId = dept.Id,
            Status = JobStatus.Closed,
            EmploymentType = EmploymentType.FullTime,
            WorkArrangement = WorkArrangement.Remote,
            CreatedAtUtc = DateTime.UtcNow
        };

        var draftJob = new Job
        {
            Title = "Draft Role",
            Description = "Draft only",
            RequiredSkills = "C#",
            Location = "Colombo",
            PostedDate = DateTime.UtcNow,
            RecruiterId = recruiter.Id,
            Status = JobStatus.Draft,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Jobs.AddRange(openJob, openJobB, closedJob, draftJob);
        await db.SaveChangesAsync();

        db.JobSkills.Add(new JobSkill
        {
            JobId = openJob.Id,
            SkillId = csharp.Id,
            IsRequired = true
        });
        db.JobSkills.Add(new JobSkill
        {
            JobId = openJob.Id,
            SkillId = sql.Id,
            IsRequired = true
        });

        db.ScreeningQuestions.Add(new ScreeningQuestion
        {
            JobId = openJob.Id,
            QuestionText = "Are you authorized to work?",
            QuestionType = "YesNo",
            IsRequired = true,
            SortOrder = 1,
            CreatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return (openJob.Id, closedJob.Id, csharp.Id, dept.Id);
    }

    private async Task<string> LoginTokenAsync(string email, string password = "SecurePass123!")
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        return payload.GetProperty("token").GetString()!;
    }

    private HttpClient ClientWithToken(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task SearchJobs_FiltersPaginationAndActiveOnly()
    {
        var (user, profileId) = await SeedCandidateAsync();
        await EnrichProfileForRecommendationsAsync(profileId);
        var (openJobId, closedJobId, skillId, deptId) = await SeedJobsAsync();
        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        var response = await client.GetAsync(
            $"/api/candidate/jobs?keyword=C%23&location=Colombo&departmentId={deptId}&employmentType=FullTime&workArrangement=Hybrid&skillId={skillId}&page=1&pageSize=10&sortBy=postedDate&sortDir=desc");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(body.GetProperty("totalCount").GetInt32() >= 1);
        Assert.Equal(1, body.GetProperty("page").GetInt32());

        var items = body.GetProperty("items").EnumerateArray().ToList();
        Assert.Contains(items, i => i.GetProperty("id").GetInt32() == openJobId);
        Assert.DoesNotContain(items, i => i.GetProperty("id").GetInt32() == closedJobId);
        Assert.DoesNotContain(items, i =>
            string.Equals(i.GetProperty("title").GetString(), "Draft Role", StringComparison.Ordinal));

        var pageResponse = await client.GetAsync("/api/candidate/jobs?page=1&pageSize=1&sortBy=title&sortDir=asc");
        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);
        var pageBody = await pageResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal(1, pageBody.GetProperty("pageSize").GetInt32());
        Assert.Single(pageBody.GetProperty("items").EnumerateArray());
        Assert.True(pageBody.GetProperty("totalCount").GetInt32() >= 2);
    }

    [Fact]
    public async Task ClosedAndDraftJobs_AreNotVisibleInDetail()
    {
        var (user, _) = await SeedCandidateAsync();
        var (_, closedJobId, _, _) = await SeedJobsAsync();
        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        var closed = await client.GetAsync($"/api/candidate/jobs/{closedJobId}");
        Assert.Equal(HttpStatusCode.NotFound, closed.StatusCode);
    }

    [Fact]
    public async Task DeterministicMatchScore_ReturnsMatchedAndMissingSkills()
    {
        var (user, profileId) = await SeedCandidateAsync();
        await EnrichProfileForRecommendationsAsync(profileId);
        var (openJobId, _, _, _) = await SeedJobsAsync();
        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        var response = await client.GetAsync($"/api/candidate/jobs/{openJobId}/match");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        Assert.Equal(DeterministicJobMatchingProvider.ProviderKey, body.GetProperty("provider").GetString());
        Assert.True(body.GetProperty("matchScore").GetDecimal() > 0);
        Assert.Contains(
            body.GetProperty("matchedSkills").EnumerateArray().Select(x => x.GetString()),
            s => string.Equals(s, "C#", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            body.GetProperty("missingSkills").EnumerateArray().Select(x => x.GetString()),
            s => string.Equals(s, "SQL", StringComparison.OrdinalIgnoreCase));
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("explanation").GetString()));
        Assert.Contains("AI-generated insight", body.GetProperty("humanReviewNotice").GetString()!, StringComparison.OrdinalIgnoreCase);
        Assert.False(
            string.Equals(body.GetProperty("provider").GetString(), "ExternalAI", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Recommendations_OrderByHighestMatch_AndIncompleteProfileHandled()
    {
        var (incompleteUser, _) = await SeedCandidateAsync();
        var incompleteToken = await LoginTokenAsync(incompleteUser.Email);
        var incompleteClient = ClientWithToken(incompleteToken);
        await SeedJobsAsync();

        var incompleteResponse = await incompleteClient.GetAsync("/api/candidate/recommendations");
        Assert.Equal(HttpStatusCode.OK, incompleteResponse.StatusCode);
        var incompleteBody = await incompleteResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.False(incompleteBody.GetProperty("profileCompleteEnough").GetBoolean());
        Assert.Empty(incompleteBody.GetProperty("jobs").EnumerateArray());
        Assert.False(string.IsNullOrWhiteSpace(incompleteBody.GetProperty("message").GetString()));

        var (readyUser, readyProfileId) = await SeedCandidateAsync();
        await EnrichProfileForRecommendationsAsync(readyProfileId);
        var readyToken = await LoginTokenAsync(readyUser.Email);
        var readyClient = ClientWithToken(readyToken);

        var readyResponse = await readyClient.GetAsync("/api/candidate/recommendations");
        Assert.Equal(HttpStatusCode.OK, readyResponse.StatusCode);
        var readyBody = await readyResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(readyBody.GetProperty("profileCompleteEnough").GetBoolean());

        var jobs = readyBody.GetProperty("jobs").EnumerateArray().ToList();
        Assert.True(jobs.Count >= 2);
        var scores = jobs.Select(j => j.GetProperty("matchScore").GetDecimal()).ToList();
        Assert.Equal(scores.OrderByDescending(s => s), scores);
        Assert.Contains(jobs, j => j.GetProperty("title").GetString()!.Contains("C#", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Apply_Succeeds_PersistsAnswersAndStatusHistory()
    {
        var (user, profileId) = await SeedCandidateAsync();
        await EnrichProfileForRecommendationsAsync(profileId);
        var (openJobId, _, _, _) = await SeedJobsAsync();

        int resumeId;
        int questionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            resumeId = await db.Resumes
                .Where(r => r.CandidateProfileId == profileId)
                .Select(r => r.Id)
                .FirstAsync();
            questionId = await db.ScreeningQuestions
                .Where(q => q.JobId == openJobId)
                .Select(q => q.Id)
                .FirstAsync();
        }

        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        var response = await client.PostAsJsonAsync("/api/candidate/applications", new
        {
            jobId = openJobId,
            resumeId,
            coverLetter = "I am excited to apply.",
            termsAccepted = true,
            screeningAnswers = new[]
            {
                new { screeningQuestionId = questionId, answerText = "Yes" }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("Pending", body.GetProperty("status").GetString());
        Assert.Equal(resumeId, body.GetProperty("resumeId").GetInt32());
        Assert.Single(body.GetProperty("answers").EnumerateArray());
        Assert.Equal("Yes", body.GetProperty("answers")[0].GetProperty("answerText").GetString());
        Assert.Single(body.GetProperty("statusHistory").EnumerateArray());
        Assert.Equal("Pending", body.GetProperty("statusHistory")[0].GetProperty("status").GetString());
    }

    [Fact]
    public async Task Apply_Duplicate_IsRejected()
    {
        var (user, profileId) = await SeedCandidateAsync();
        await EnrichProfileForRecommendationsAsync(profileId);
        var (openJobId, _, _, _) = await SeedJobsAsync();

        int questionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            questionId = await db.ScreeningQuestions
                .Where(q => q.JobId == openJobId)
                .Select(q => q.Id)
                .FirstAsync();
        }

        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);
        var payload = new
        {
            jobId = openJobId,
            coverLetter = "First",
            termsAccepted = true,
            screeningAnswers = new[]
            {
                new { screeningQuestionId = questionId, answerText = "Yes" }
            }
        };

        var first = await client.PostAsJsonAsync("/api/candidate/applications", payload);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/candidate/applications", payload);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        var body = await second.Content.ReadAsStringAsync();
        Assert.Contains("already applied", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Apply_ClosedJob_IsRejected()
    {
        var (user, profileId) = await SeedCandidateAsync();
        await EnrichProfileForRecommendationsAsync(profileId);
        var (_, closedJobId, _, _) = await SeedJobsAsync();
        var token = await LoginTokenAsync(user.Email);
        var client = ClientWithToken(token);

        var response = await client.PostAsJsonAsync("/api/candidate/applications", new
        {
            jobId = closedJobId,
            coverLetter = "Please consider me",
            termsAccepted = true,
            screeningAnswers = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("closed", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetApplication_AnotherCandidate_IsBlocked()
    {
        var (userA, profileA) = await SeedCandidateAsync();
        await EnrichProfileForRecommendationsAsync(profileA);
        var (userB, _) = await SeedCandidateAsync();
        var (openJobId, _, _, _) = await SeedJobsAsync();

        int questionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            questionId = await db.ScreeningQuestions
                .Where(q => q.JobId == openJobId)
                .Select(q => q.Id)
                .FirstAsync();
        }

        var tokenA = await LoginTokenAsync(userA.Email);
        var clientA = ClientWithToken(tokenA);
        var created = await clientA.PostAsJsonAsync("/api/candidate/applications", new
        {
            jobId = openJobId,
            coverLetter = "Mine",
            termsAccepted = true,
            screeningAnswers = new[]
            {
                new { screeningQuestionId = questionId, answerText = "Yes" }
            }
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var createdBody = await created.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var applicationId = createdBody.GetProperty("id").GetInt32();

        var tokenB = await LoginTokenAsync(userB.Email);
        var clientB = ClientWithToken(tokenB);
        var blocked = await clientB.GetAsync($"/api/candidate/applications/{applicationId}");
        Assert.Equal(HttpStatusCode.NotFound, blocked.StatusCode);
    }

    [Fact]
    public void DeterministicProvider_ScoresSkillOverlapConsistently()
    {
        var provider = new DeterministicJobMatchingProvider();
        var csharp = new Skill { Id = 1, Name = "C#" };
        var sql = new Skill { Id = 2, Name = "SQL" };

        var profile = new CandidateProfile
        {
            YearsOfExperience = 4,
            Location = "Colombo",
            PreferredWorkArrangement = WorkArrangement.Hybrid,
            CandidateSkills =
            {
                new CandidateSkill { SkillId = 1, Skill = csharp }
            },
            Educations = { new Education { Institution = "Uni", Degree = "BSc" } },
            WorkExperiences =
            {
                new WorkExperience
                {
                    CompanyName = "Acme",
                    JobTitle = "Dev",
                    StartDate = DateTime.UtcNow.AddYears(-3),
                    IsCurrentRole = true
                }
            }
        };

        var job = new Job
        {
            Id = 10,
            Location = "Colombo",
            WorkArrangement = WorkArrangement.Hybrid,
            JobSkills =
            {
                new JobSkill { SkillId = 1, Skill = csharp, IsRequired = true },
                new JobSkill { SkillId = 2, Skill = sql, IsRequired = true }
            }
        };

        var match = provider.ComputeMatch(profile, job);
        Assert.Equal(DeterministicJobMatchingProvider.ProviderKey, match.Provider);
        Assert.Contains("C#", match.MatchedSkills);
        Assert.Contains("SQL", match.MissingSkills);
        Assert.InRange(match.MatchScore, 1m, 100m);
    }
}
