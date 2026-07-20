using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HireSphere.API.Tests;

public class HiringManagerPortalPhase62Tests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public HiringManagerPortalPhase62Tests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient Client(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<(User Hm, User Candidate, int AppId, int InterviewId, string HmToken, string CandToken, string RecToken)> SeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestWebApplicationFactory.SeedRoles(db);

        var org = new Organization { Name = $"Org-{Guid.NewGuid():N}", CreatedAtUtc = DateTime.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        async Task<(User u, string token)> SeedUser(string role, int? orgId)
        {
            var email = $"{role}-{Guid.NewGuid():N}@example.com";
            const string password = "SecurePass123!";
            var u = new User
            {
                FullName = role,
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Users.Add(u);
            await db.SaveChangesAsync();
            var r = await db.Roles.FirstAsync(x => x.Name == role);
            db.UserRoles.Add(new UserRole { UserId = u.Id, RoleId = r.Id });
            if (role == "HiringManager")
            {
                db.HiringManagerProfiles.Add(new HiringManagerProfile
                {
                    UserId = u.Id,
                    OrganizationId = orgId,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
            if (role == "Recruiter")
            {
                db.RecruiterProfiles.Add(new RecruiterProfile
                {
                    UserId = u.Id,
                    OrganizationId = orgId,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();
            var login = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password });
            login.EnsureSuccessStatusCode();
            var payload = await login.Content.ReadFromJsonAsync<JsonElement>();
            return (u, payload!.GetProperty("token").GetString()!);
        }

        var (hm, hmToken) = await SeedUser("HiringManager", org.Id);
        var (rec, recToken) = await SeedUser("Recruiter", org.Id);
        var (cand, candToken) = await SeedUser("Candidate", null);
        db.CandidateProfiles.Add(new CandidateProfile
        {
            UserId = cand.Id,
            FullName = cand.FullName,
            CreatedAtUtc = DateTime.UtcNow
        });
        var job = new Job
        {
            Title = "Role",
            Description = "d",
            Location = "Colombo",
            RecruiterId = rec.Id,
            HiringManagerUserId = hm.Id,
            OrganizationId = org.Id,
            Status = JobStatus.Published,
            PostedDate = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            Vacancies = 1
        };
        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        var app = new Application
        {
            CandidateId = cand.Id,
            JobId = job.Id,
            Status = ApplicationStatus.InterviewScheduled,
            AppliedDate = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Applications.Add(app);
        await db.SaveChangesAsync();
        var interview = new Interview
        {
            ApplicationId = app.Id,
            RecruiterUserId = rec.Id,
            HiringManagerUserId = hm.Id,
            InterviewDate = DateTime.UtcNow.AddDays(2),
            DurationMinutes = 60,
            TimeZoneId = "Asia/Colombo",
            InterviewType = "Video",
            Status = InterviewStatus.Scheduled,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Interviews.Add(interview);
        await db.SaveChangesAsync();
        return (hm, cand, app.Id, interview.Id, hmToken, candToken, recToken);
    }

    [Fact]
    public async Task Feedback_Evaluation_Recommendation_Authorization_And_Final_Separation()
    {
        var (_, cand, appId, interviewId, hmToken, candToken, recToken) = await SeedAsync();
        var (otherHm, _, _, otherInterview, otherToken, _, _) = await SeedAsync();

        var view = await Client(hmToken).GetAsync($"/api/hiring-manager/interviews/{interviewId}");
        Assert.Equal(HttpStatusCode.OK, view.StatusCode);

        var deniedView = await Client(otherToken).GetAsync($"/api/hiring-manager/interviews/{interviewId}");
        Assert.Equal(HttpStatusCode.NotFound, deniedView.StatusCode);

        var feedback = await Client(hmToken).PostAsJsonAsync($"/api/hiring-manager/interviews/{interviewId}/feedback", new
        {
            technicalCompetency = 4,
            communication = 4,
            problemSolving = 5,
            roleKnowledge = 4,
            teamwork = 4,
            leadership = 3,
            culturalContribution = 4,
            recommendation = "Advance",
            privatePanelComments = "Panel only"
        });
        Assert.Equal(HttpStatusCode.OK, feedback.StatusCode);

        var dup = await Client(hmToken).PostAsJsonAsync($"/api/hiring-manager/interviews/{interviewId}/feedback", new
        {
            recommendation = "Again"
        });
        Assert.Equal(HttpStatusCode.BadRequest, dup.StatusCode);

        var badRating = await Client(hmToken).PutAsJsonAsync($"/api/hiring-manager/interviews/{interviewId}/feedback", new
        {
            technicalCompetency = 9,
            recommendation = "x"
        });
        Assert.Equal(HttpStatusCode.BadRequest, badRating.StatusCode);

        var unauthFeedback = await Client(otherToken).PostAsJsonAsync($"/api/hiring-manager/interviews/{interviewId}/feedback", new
        {
            recommendation = "nope"
        });
        Assert.Equal(HttpStatusCode.NotFound, unauthFeedback.StatusCode);

        var draft = await Client(hmToken).PostAsJsonAsync($"/api/hiring-manager/applications/{appId}/evaluation", new
        {
            submit = false,
            requiredSkillsAlignment = 80,
            justification = "Draft notes"
        });
        Assert.Equal(HttpStatusCode.OK, draft.StatusCode);
        Assert.Equal("Draft", (await draft.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("submissionStatus").GetString());

        var submitEval = await Client(hmToken).PutAsJsonAsync($"/api/hiring-manager/applications/{appId}/evaluation", new
        {
            submit = true,
            requiredSkillsAlignment = 85,
            justification = "Ready for hire recommendation"
        });
        Assert.Equal(HttpStatusCode.OK, submitEval.StatusCode);

        var noJustify = await Client(hmToken).PutAsJsonAsync($"/api/hiring-manager/applications/{appId}/evaluation", new
        {
            submit = true,
            requiredSkillsAlignment = 85
        });
        Assert.Equal(HttpStatusCode.BadRequest, noJustify.StatusCode);

        var rec = await Client(hmToken).PostAsJsonAsync($"/api/hiring-manager/applications/{appId}/recommendation", new
        {
            decisionType = "RecommendHire",
            reason = "Strong panel feedback"
        });
        Assert.Equal(HttpStatusCode.OK, rec.StatusCode);
        var recBody = await rec.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(recBody.GetProperty("isFinal").GetBoolean());

        var finalDenied = await Client(hmToken).PostAsJsonAsync($"/api/hiring-manager/applications/{appId}/recommendation", new
        {
            decisionType = "FinalHire",
            reason = "Should fail"
        });
        Assert.Equal(HttpStatusCode.BadRequest, finalDenied.StatusCode);

        var history = await Client(hmToken).GetAsync($"/api/hiring-manager/applications/{appId}/decision-history");
        Assert.Equal(HttpStatusCode.OK, history.StatusCode);

        // Candidate cannot see private panel comments via HM APIs
        var candHm = await Client(candToken).GetAsync($"/api/hiring-manager/interviews/{interviewId}");
        Assert.Equal(HttpStatusCode.Forbidden, candHm.StatusCode);

        var recruiterDenied = await Client(recToken).GetAsync("/api/hiring-manager/interviews");
        Assert.Equal(HttpStatusCode.Forbidden, recruiterDenied.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var app = await db.Applications.FirstAsync(a => a.Id == appId);
            app.Status = ApplicationStatus.Withdrawn;
            await db.SaveChangesAsync();
        }

        var withdrawn = await Client(hmToken).PostAsJsonAsync($"/api/hiring-manager/applications/{appId}/recommendation", new
        {
            decisionType = "RecommendReject",
            reason = "After withdraw"
        });
        Assert.Equal(HttpStatusCode.BadRequest, withdrawn.StatusCode);

        _ = cand;
        _ = otherHm;
        _ = otherInterview;
    }
}
