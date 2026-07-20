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

public class RecruiterPortalPhase52Tests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RecruiterPortalPhase52Tests(TestWebApplicationFactory factory) => _factory = factory;

    private async Task<(User Recruiter, int OrgId, string Token)> SeedRecruiterAsync(string orgName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);
        var org = new Organization { Name = orgName, CreatedAtUtc = DateTime.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();
        var email = $"rec52-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var user = new User
        {
            FullName = "Recruiter 52",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Recruiter",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = (await db.Roles.FirstAsync(r => r.Name == "Recruiter")).Id });
        db.RecruiterProfiles.Add(new RecruiterProfile { UserId = user.Id, OrganizationId = org.Id, CreatedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;
        return (user, org.Id, token);
    }

    private async Task<(User Candidate, string Token)> SeedCandidateAsync(string? skill = "C#")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);
        var email = $"cand52-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123!";
        var user = new User
        {
            FullName = "Candidate 52",
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Candidate",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = (await db.Roles.FirstAsync(r => r.Name == "Candidate")).Id });
        var profile = new CandidateProfile
        {
            UserId = user.Id,
            FullName = user.FullName,
            Summary = "Engineer",
            YearsOfExperience = 4,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.CandidateProfiles.Add(profile);
        await db.SaveChangesAsync();
        if (!string.IsNullOrWhiteSpace(skill))
        {
            var skillEntity = await db.Skills.FirstOrDefaultAsync(s => s.Name == skill)
                ?? db.Skills.Add(new Skill { Name = skill, CreatedAtUtc = DateTime.UtcNow }).Entity;
            await db.SaveChangesAsync();
            db.CandidateSkills.Add(new CandidateSkill
            {
                CandidateProfileId = profile.Id,
                SkillId = skillEntity.Id,
                ProficiencyLevel = "Advanced",
                YearsOfExperience = 3
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        return (user, (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!);
    }

    private HttpClient Client(string token)
    {
        var c = _factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    private async Task<(int JobId, int ApplicationId)> SeedPublishedJobWithApplicationAsync(
        string recruiterToken,
        string candidateToken,
        string title = "Ranked Role")
    {
        var recruiter = Client(recruiterToken);
        var create = await recruiter.PostAsJsonAsync("/api/recruiter/jobs", new
        {
            title,
            description = "Build things",
            location = "Colombo",
            vacancies = 1,
            skills = new[]
            {
                new { skillName = "C#", isRequired = true },
                new { skillName = "React", isRequired = false }
            }
        });
        create.EnsureSuccessStatusCode();
        var jobId = (await create.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();
        (await recruiter.PatchAsJsonAsync($"/api/recruiter/jobs/{jobId}/status", new { status = "Published" }))
            .EnsureSuccessStatusCode();

        var apply = await Client(candidateToken).PostAsJsonAsync("/api/candidate/applications", new
        {
            jobId,
            coverLetter = "Interested",
            termsAccepted = true,
            answers = Array.Empty<object>()
        });
        if (!apply.IsSuccessStatusCode)
        {
            var err = await apply.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Apply failed: {(int)apply.StatusCode} {err}");
        }
        var applicationId = (await apply.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();
        return (jobId, applicationId);
    }

    [Fact]
    public async Task Ranking_IsOrgScoped_Deterministic_WithNotice()
    {
        var (_, _, tokenA) = await SeedRecruiterAsync($"A-{Guid.NewGuid():N}");
        var (_, _, tokenB) = await SeedRecruiterAsync($"B-{Guid.NewGuid():N}");
        var (_, candToken) = await SeedCandidateAsync();
        var (_, applicationId) = await SeedPublishedJobWithApplicationAsync(tokenA, candToken);

        var ranking = await Client(tokenA).GetAsync($"/api/recruiter/applications/{applicationId}/ranking");
        ranking.EnsureSuccessStatusCode();
        var body = await ranking.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("Deterministic", body.GetProperty("providerName").GetString());
        Assert.Contains("reviewed by authorized users", body.GetProperty("humanReviewNotice").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Protected characteristics", body.GetProperty("explanation").GetString(), StringComparison.OrdinalIgnoreCase);
        var score1 = body.GetProperty("totalScore").GetDecimal();

        var ranking2 = await Client(tokenA).GetAsync($"/api/recruiter/applications/{applicationId}/ranking");
        ranking2.EnsureSuccessStatusCode();
        Assert.Equal(score1, (await ranking2.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("totalScore").GetDecimal());

        var blocked = await Client(tokenB).GetAsync($"/api/recruiter/applications/{applicationId}/ranking");
        Assert.Equal(HttpStatusCode.NotFound, blocked.StatusCode);

        var review = await Client(tokenA).PostAsJsonAsync(
            $"/api/recruiter/applications/{applicationId}/ranking/review",
            new { decision = "Proceed", notes = "Human reviewed fit", overrideScore = 80 });
        review.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(await db.AuditLogs.AnyAsync(a => a.Action == "RankingHumanReview" && a.EntityId == applicationId));
        Assert.Equal(RecruiterPhase52Service.HumanReviewNotice, body.GetProperty("humanReviewNotice").GetString());
    }

    [Fact]
    public async Task Screening_Assessment_Messaging_And_AnswerKeyProtection()
    {
        var (_, orgId, token) = await SeedRecruiterAsync($"Org52-{Guid.NewGuid():N}");
        var (candidate, candToken) = await SeedCandidateAsync();
        var (_, applicationId) = await SeedPublishedJobWithApplicationAsync(token, candToken, "Assess Role");
        var recruiter = Client(token);
        var candidateClient = Client(candToken);

        var decision = await recruiter.PostAsJsonAsync(
            $"/api/recruiter/applications/{applicationId}/screening-decision",
            new { status = "UnderReview", reason = "Passed required checks" });
        Assert.Equal(HttpStatusCode.NoContent, decision.StatusCode);

        var createAssessment = await recruiter.PostAsJsonAsync("/api/recruiter/assessments", new
        {
            title = "C# Quiz",
            passingScorePercent = 50,
            maxAttempts = 1,
            durationMinutes = 30,
            revealResultsToCandidate = true
        });
        createAssessment.EnsureSuccessStatusCode();
        var assessmentBody = await createAssessment.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var assessmentId = assessmentBody.GetProperty("id").GetInt32();
        Assert.Equal(orgId, assessmentBody.GetProperty("organizationId").GetInt32());

        var addQ = await recruiter.PostAsJsonAsync($"/api/recruiter/assessments/{assessmentId}/questions", new
        {
            questionText = "2+2?",
            questionType = "MultipleChoice",
            points = 10,
            sortOrder = 0,
            optionsJson = "[\"3\",\"4\",\"5\"]",
            correctAnswerKey = "4"
        });
        addQ.EnsureSuccessStatusCode();
        Assert.Equal("4", (await addQ.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("correctAnswerKey").GetString());

        var assign = await recruiter.PostAsJsonAsync(
            $"/api/recruiter/applications/{applicationId}/assessments",
            new { assessmentId, maxAttempts = 1 });
        assign.EnsureSuccessStatusCode();
        var assignmentId = (await assign.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();

        var candList = await candidateClient.GetAsync("/api/candidate/assessments");
        candList.EnsureSuccessStatusCode();
        var candJson = await candList.Content.ReadAsStringAsync();
        Assert.DoesNotContain("correctAnswerKey", candJson, StringComparison.OrdinalIgnoreCase);

        var detail = await candidateClient.GetAsync($"/api/candidate/assessments/{assignmentId}");
        detail.EnsureSuccessStatusCode();
        var detailJson = await detail.Content.ReadAsStringAsync();
        Assert.DoesNotContain("correctAnswerKey", detailJson, StringComparison.OrdinalIgnoreCase);

        var start = await candidateClient.PostAsJsonAsync($"/api/candidate/assessments/{assignmentId}/start", new { });
        start.EnsureSuccessStatusCode();
        var startBody = await start.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var attemptId = startBody.GetProperty("attemptId").GetInt32();
        var questionId = startBody.GetProperty("questions")[0].GetProperty("id").GetInt32();

        await candidateClient.PutAsJsonAsync($"/api/candidate/assessments/attempts/{attemptId}/answers", new
        {
            answers = new[] { new { questionId, answerValue = "4" } }
        });
        var submit = await candidateClient.PostAsJsonAsync($"/api/candidate/assessments/attempts/{attemptId}/submit", new { });
        submit.EnsureSuccessStatusCode();

        var review = await recruiter.GetAsync($"/api/recruiter/assessment-assignments/{assignmentId}/attempts");
        review.EnsureSuccessStatusCode();
        var attempts = await review.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(attempts.GetArrayLength() >= 1);
        var score = attempts[0].TryGetProperty("scorePercent", out var sp) && sp.ValueKind != JsonValueKind.Null
            ? sp.GetDecimal()
            : -1m;
        Assert.True(score >= 50, $"Expected scorePercent >= 50, got {score}. Payload: {attempts}");

        // attempt limit
        var start2 = await candidateClient.PostAsJsonAsync($"/api/candidate/assessments/{assignmentId}/start", new { });
        Assert.Equal(HttpStatusCode.BadRequest, start2.StatusCode);

        var msg = await recruiter.PostAsJsonAsync(
            $"/api/recruiter/applications/{applicationId}/messages",
            new { body = "Please confirm availability <b>tomorrow</b>" });
        msg.EnsureSuccessStatusCode();
        var sent = await msg.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.DoesNotContain("<b>", sent.GetProperty("body").GetString());

        var thread = await candidateClient.GetAsync($"/api/candidate/applications/{applicationId}/messages");
        thread.EnsureSuccessStatusCode();
        Assert.True((await thread.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("totalCount").GetInt32() >= 1);

        var reply = await candidateClient.PostAsJsonAsync(
            $"/api/candidate/applications/{applicationId}/messages",
            new { body = "Confirmed" });
        reply.EnsureSuccessStatusCode();

        var (_, otherCandToken) = await SeedCandidateAsync();
        var hijack = await Client(otherCandToken).PostAsJsonAsync(
            $"/api/candidate/applications/{applicationId}/messages",
            new { body = "Nope" });
        Assert.True(
            hijack.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest or HttpStatusCode.Forbidden,
            $"Unexpected status: {hijack.StatusCode}");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(await db.Notifications.AnyAsync(n =>
            n.UserId == candidate.Id && n.Category == NotificationCategories.AssessmentAssigned));
        Assert.True(await db.ApplicationStatusHistories.AnyAsync(h =>
            h.ApplicationId == applicationId && h.Status == ApplicationStatus.UnderReview));
        Assert.True(await db.AuditLogs.AnyAsync(a => a.Action == "AssessmentCreated"));
    }

    [Fact]
    public async Task CrossOrg_AssessmentReview_Blocked_And_ExpiryEnforced()
    {
        var (_, _, tokenA) = await SeedRecruiterAsync($"ExpA-{Guid.NewGuid():N}");
        var (_, _, tokenB) = await SeedRecruiterAsync($"ExpB-{Guid.NewGuid():N}");
        var (_, candToken) = await SeedCandidateAsync();
        var (_, applicationId) = await SeedPublishedJobWithApplicationAsync(tokenA, candToken, "Expiry Role");
        var recruiter = Client(tokenA);

        var create = await recruiter.PostAsJsonAsync("/api/recruiter/assessments", new
        {
            title = "Timed",
            passingScorePercent = 50,
            maxAttempts = 2
        });
        create.EnsureSuccessStatusCode();
        var assessmentId = (await create.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();
        await recruiter.PostAsJsonAsync($"/api/recruiter/assessments/{assessmentId}/questions", new
        {
            questionText = "Q",
            questionType = "ShortAnswer",
            points = 5,
            sortOrder = 0,
            correctAnswerKey = "yes"
        });

        var assign = await recruiter.PostAsJsonAsync(
            $"/api/recruiter/applications/{applicationId}/assessments",
            new
            {
                assessmentId,
                expiresAtUtc = DateTime.UtcNow.AddMinutes(-5),
                maxAttempts = 2
            });
        assign.EnsureSuccessStatusCode();
        var assignmentId = (await assign.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("id").GetInt32();

        var start = await Client(candToken).PostAsJsonAsync($"/api/candidate/assessments/{assignmentId}/start", new { });
        Assert.Equal(HttpStatusCode.BadRequest, start.StatusCode);

        var cross = await Client(tokenB).GetAsync($"/api/recruiter/assessment-assignments/{assignmentId}");
        Assert.Equal(HttpStatusCode.NotFound, cross.StatusCode);
    }
}
