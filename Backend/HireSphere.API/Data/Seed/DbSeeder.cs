using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await SeedRolesAndPermissionsAsync(db);
        await SeedOrganizationAndDepartmentsAsync(db);
        await SeedSkillsAsync(db);
        await SeedDevUsersAsync(db);
        await SeedSampleJobsAndApplicationsAsync(db);
    }

    private static async Task SeedRolesAndPermissionsAsync(ApplicationDbContext db)
    {
        var roleNames = new[] { "Candidate", "Recruiter", "HiringManager", "Admin" };
        foreach (var name in roleNames)
        {
            if (!await db.Roles.AnyAsync(r => r.Name == name))
            {
                db.Roles.Add(new Role { Name = name, Description = $"{name} role" });
            }
        }
        await db.SaveChangesAsync();

        var permissions = new (string Code, string Name)[]
        {
            ("users.read", "Read users"),
            ("jobs.manage", "Manage jobs"),
            ("applications.review", "Review applications"),
            ("hiring.decide", "Make hiring decisions"),
            ("admin.access", "Admin access")
        };

        foreach (var (code, name) in permissions)
        {
            if (!await db.Permissions.AnyAsync(p => p.Code == code))
            {
                db.Permissions.Add(new Permission { Code = code, Name = name });
            }
        }
        await db.SaveChangesAsync();

        var rolePermissionMap = new Dictionary<string, string[]>
        {
            ["Candidate"] = Array.Empty<string>(),
            ["Recruiter"] = new[] { "users.read", "jobs.manage", "applications.review" },
            ["HiringManager"] = new[] { "users.read", "applications.review", "hiring.decide" },
            ["Admin"] = new[] { "users.read", "jobs.manage", "applications.review", "hiring.decide", "admin.access" }
        };

        foreach (var (roleName, permCodes) in rolePermissionMap)
        {
            var role = await db.Roles.FirstAsync(r => r.Name == roleName);
            foreach (var code in permCodes)
            {
                var permission = await db.Permissions.FirstAsync(p => p.Code == code);
                if (!await db.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id))
                {
                    db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
                }
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedOrganizationAndDepartmentsAsync(ApplicationDbContext db)
    {
        if (!await db.Organizations.AnyAsync(o => o.Name == "HireSphere Demo Org"))
        {
            db.Organizations.Add(new Organization
            {
                Name = "HireSphere Demo Org",
                Description = "Demo organization for local development",
                Website = "https://hiresphere.local"
            });
            await db.SaveChangesAsync();
        }

        var org = await db.Organizations.FirstAsync(o => o.Name == "HireSphere Demo Org");
        foreach (var deptName in new[] { "Engineering", "HR" })
        {
            if (!await db.Departments.AnyAsync(d => d.OrganizationId == org.Id && d.Name == deptName))
            {
                db.Departments.Add(new Department { OrganizationId = org.Id, Name = deptName });
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedSkillsAsync(ApplicationDbContext db)
    {
        foreach (var skillName in new[] { "C#", "React", "SQL Server", "Communication", "JavaScript", "ASP.NET Core" })
        {
            if (!await db.Skills.AnyAsync(s => s.Name == skillName))
            {
                db.Skills.Add(new Skill { Name = skillName, Category = "General" });
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedDevUsersAsync(ApplicationDbContext db)
    {
        // Local development seed password is intentional and documented in DbSeeder only.
        // Change immediately outside local coursework environments. Do not publish in README.
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe!DevOnly1");
        var org = await db.Organizations.FirstAsync(o => o.Name == "HireSphere Demo Org");
        var engineering = await db.Departments.FirstAsync(d => d.OrganizationId == org.Id && d.Name == "Engineering");
        var hr = await db.Departments.FirstAsync(d => d.OrganizationId == org.Id && d.Name == "HR");

        await EnsureUserAsync(db, "candidate@hiresphere.local", "Demo Candidate", "Candidate", passwordHash, async user =>
        {
            if (!await db.CandidateProfiles.AnyAsync(c => c.UserId == user.Id))
            {
                db.CandidateProfiles.Add(new CandidateProfile
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    PhoneNumber = "555-0100",
                    Location = "Remote",
                    Skills = "C#, React, SQL Server"
                });
            }
        });

        await EnsureUserAsync(db, "recruiter@hiresphere.local", "Demo Recruiter", "Recruiter", passwordHash, async user =>
        {
            if (!await db.RecruiterProfiles.AnyAsync(r => r.UserId == user.Id))
            {
                db.RecruiterProfiles.Add(new RecruiterProfile
                {
                    UserId = user.Id,
                    OrganizationId = org.Id,
                    DepartmentId = hr.Id,
                    Title = "Senior Recruiter"
                });
            }
        });

        await EnsureUserAsync(db, "manager@hiresphere.local", "Demo Hiring Manager", "HiringManager", passwordHash, async user =>
        {
            if (!await db.HiringManagerProfiles.AnyAsync(h => h.UserId == user.Id))
            {
                db.HiringManagerProfiles.Add(new HiringManagerProfile
                {
                    UserId = user.Id,
                    OrganizationId = org.Id,
                    DepartmentId = engineering.Id,
                    Title = "Engineering Manager"
                });
            }
        });

        await EnsureUserAsync(db, "admin@hiresphere.local", "Demo Admin", "Admin", passwordHash, _ => Task.CompletedTask);
        await db.SaveChangesAsync();
    }

    private static async Task EnsureUserAsync(
        ApplicationDbContext db,
        string email,
        string fullName,
        string roleName,
        string passwordHash,
        Func<User, Task> configureProfile)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        if (user == null)
        {
            user = new User
            {
                FullName = fullName,
                Email = email,
                NormalizedEmail = normalizedEmail,
                PasswordHash = passwordHash,
                Role = roleName,
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role != null && !await db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id))
        {
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        }

        await configureProfile(user);
    }

    private static async Task SeedSampleJobsAndApplicationsAsync(ApplicationDbContext db)
    {
        var recruiter = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == "RECRUITER@HIRESPHERE.LOCAL");
        var candidate = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == "CANDIDATE@HIRESPHERE.LOCAL");
        if (recruiter == null || candidate == null)
        {
            return;
        }

        var org = await db.Organizations.FirstAsync(o => o.Name == "HireSphere Demo Org");
        var engineering = await db.Departments.FirstAsync(d => d.OrganizationId == org.Id && d.Name == "Engineering");

        if (!await db.Jobs.AnyAsync(j => j.Title == "Full Stack Developer" && j.RecruiterId == recruiter.Id))
        {
            db.Jobs.Add(new Job
            {
                Title = "Full Stack Developer",
                Description = "Build and maintain HireSphere platform features.",
                RequiredSkills = "C#, React, SQL Server",
                Location = "Remote",
                JobType = "FullTime",
                PostedDate = DateTime.UtcNow,
                RecruiterId = recruiter.Id,
                OrganizationId = org.Id,
                DepartmentId = engineering.Id,
                Status = JobStatus.Open,
                EmploymentType = EmploymentType.FullTime,
                WorkArrangement = WorkArrangement.Remote,
                CreatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var job = await db.Jobs.FirstAsync(j => j.Title == "Full Stack Developer" && j.RecruiterId == recruiter.Id);

        if (!await db.Applications.AnyAsync(a => a.CandidateId == candidate.Id && a.JobId == job.Id))
        {
            var application = new Application
            {
                CandidateId = candidate.Id,
                JobId = job.Id,
                AppliedDate = DateTime.UtcNow,
                Status = ApplicationStatus.Pending,
                CoverLetter = "I am excited to apply for this role.",
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Applications.Add(application);
            await db.SaveChangesAsync();

            db.ApplicationStatusHistories.Add(new ApplicationStatusHistory
            {
                ApplicationId = application.Id,
                Status = ApplicationStatus.Pending,
                ChangedAtUtc = DateTime.UtcNow,
                Notes = "Application submitted"
            });
            await db.SaveChangesAsync();
        }
    }
}
