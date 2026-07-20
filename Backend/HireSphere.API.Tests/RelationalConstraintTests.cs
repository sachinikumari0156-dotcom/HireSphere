using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Tests;
public class RelationalConstraintTests
{
    [Fact]
    public async Task DuplicateNormalizedEmail_IsRejected()
    {
        var (context, connection) = TestDbFactory.CreateContext();
        await using (context)
        await using (connection)
        {

        context.Users.Add(new User
        {
            FullName = "First User",
            Email = "dup@example.com",
            NormalizedEmail = "DUP@EXAMPLE.COM",
            PasswordHash = "hash",
            Role = "Candidate",
            Status = UserStatus.Active
        });
        await context.SaveChangesAsync();

        context.Users.Add(new User
        {
            FullName = "Second User",
            Email = "dup@example.com",
            NormalizedEmail = "DUP@EXAMPLE.COM",
            PasswordHash = "hash2",
            Role = "Candidate",
            Status = UserStatus.Active
        });

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task OneCandidateProfilePerUser_IsEnforced()
    {
        var (context, connection) = TestDbFactory.CreateContext();
        await using (context)
        await using (connection)
        {

        var user = new User
        {
            FullName = "Candidate One",
            Email = "candidate@example.com",
            NormalizedEmail = "CANDIDATE@EXAMPLE.COM",
            PasswordHash = "hash",
            Role = "Candidate",
            Status = UserStatus.Active
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.CandidateProfiles.Add(new CandidateProfile { UserId = user.Id, FullName = user.FullName });
        await context.SaveChangesAsync();

        context.CandidateProfiles.Add(new CandidateProfile { UserId = user.Id, FullName = "Duplicate Profile" });

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return;
            }

            var profileCount = await context.CandidateProfiles.CountAsync(p => p.UserId == user.Id);
            Assert.Equal(1, profileCount);
        }
    }

    [Fact]
    public async Task DuplicateApplication_SameCandidateAndJob_IsRejected()
    {
        var (context, connection) = TestDbFactory.CreateContext();
        await using (context)
        await using (connection)
        {

        var candidate = new User
        {
            FullName = "Applicant",
            Email = "applicant@example.com",
            NormalizedEmail = "APPLICANT@EXAMPLE.COM",
            PasswordHash = "hash",
            Role = "Candidate",
            Status = UserStatus.Active
        };
        var recruiter = new User
        {
            FullName = "Recruiter",
            Email = "recruiter@example.com",
            NormalizedEmail = "RECRUITER@EXAMPLE.COM",
            PasswordHash = "hash",
            Role = "Recruiter",
            Status = UserStatus.Active
        };
        context.Users.AddRange(candidate, recruiter);
        await context.SaveChangesAsync();

        var job = new Job
        {
            Title = "Engineer",
            Description = "Build things",
            RecruiterId = recruiter.Id,
            PostedDate = DateTime.UtcNow,
            Status = JobStatus.Open
        };
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        context.Applications.Add(new Application
        {
            CandidateId = candidate.Id,
            JobId = job.Id,
            Status = ApplicationStatus.Pending
        });
        await context.SaveChangesAsync();

        context.Applications.Add(new Application
        {
            CandidateId = candidate.Id,
            JobId = job.Id,
            Status = ApplicationStatus.UnderReview
        });

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task DuplicateCandidateSkill_IsRejected()
    {
        var (context, connection) = TestDbFactory.CreateContext();
        await using (context)
        await using (connection)
        {

        var user = new User
        {
            FullName = "Skill Candidate",
            Email = "skills@example.com",
            NormalizedEmail = "SKILLS@EXAMPLE.COM",
            PasswordHash = "hash",
            Role = "Candidate",
            Status = UserStatus.Active
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var profile = new CandidateProfile { UserId = user.Id, FullName = user.FullName };
        context.CandidateProfiles.Add(profile);

        var skill = new Skill { Name = "C#" };
        context.Skills.Add(skill);
        await context.SaveChangesAsync();

        context.CandidateSkills.Add(new CandidateSkill
        {
            CandidateProfileId = profile.Id,
            SkillId = skill.Id,
            ProficiencyLevel = "Intermediate"
        });
        await context.SaveChangesAsync();

        context.CandidateSkills.Add(new CandidateSkill
        {
            CandidateProfileId = profile.Id,
            SkillId = skill.Id,
            ProficiencyLevel = "Advanced"
        });

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task DuplicateJobSkill_IsRejected()
    {
        var (context, connection) = TestDbFactory.CreateContext();
        await using (context)
        await using (connection)
        {

        var recruiter = new User
        {
            FullName = "Job Recruiter",
            Email = "jobrec@example.com",
            NormalizedEmail = "JOBREC@EXAMPLE.COM",
            PasswordHash = "hash",
            Role = "Recruiter",
            Status = UserStatus.Active
        };
        context.Users.Add(recruiter);
        await context.SaveChangesAsync();

        var job = new Job
        {
            Title = "Backend Dev",
            Description = "API work",
            RecruiterId = recruiter.Id,
            PostedDate = DateTime.UtcNow,
            Status = JobStatus.Open
        };
        context.Jobs.Add(job);

        var skill = new Skill { Name = "SQL" };
        context.Skills.Add(skill);
        await context.SaveChangesAsync();

        context.JobSkills.Add(new JobSkill
        {
            JobId = job.Id,
            SkillId = skill.Id,
            MinProficiencyLevel = "Intermediate"
        });
        await context.SaveChangesAsync();

        context.JobSkills.Add(new JobSkill
        {
            JobId = job.Id,
            SkillId = skill.Id,
            MinProficiencyLevel = "Advanced"
        });

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
    }
}
