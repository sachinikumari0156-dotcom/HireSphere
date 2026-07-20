using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HireSphere.API.Data.Seed;

/// <summary>
/// Idempotent development seed. User accounts are created only when explicitly enabled
/// and when seed credentials are supplied via configuration or environment variables.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await SeedRolesAndPermissionsAsync(db, cancellationToken);
        await SeedOrganizationAndDepartmentsAsync(db, cancellationToken);
        await SeedSkillsAsync(db, cancellationToken);

        var seedEnabled = configuration.GetValue<bool>("Seed:Enabled")
            || configuration.GetValue<bool>("HIRESPHERE_SEED_ENABLED")
            || string.Equals(
                Environment.GetEnvironmentVariable("HIRESPHERE_SEED_ENABLED"),
                "true",
                StringComparison.OrdinalIgnoreCase);

        if (!seedEnabled)
        {
            logger.LogInformation(
                "Development user seeding skipped. Set Seed:Enabled=true (or HIRESPHERE_SEED_ENABLED=true) and supply seed credentials to enable.");
            return;
        }

        var adminEmail = FirstNonEmpty(
            configuration["Seed:AdminEmail"],
            configuration["HIRESPHERE_SEED_ADMIN_EMAIL"],
            Environment.GetEnvironmentVariable("HIRESPHERE_SEED_ADMIN_EMAIL"));

        var adminPassword = FirstNonEmpty(
            configuration["Seed:AdminPassword"],
            configuration["HIRESPHERE_SEED_ADMIN_PASSWORD"],
            Environment.GetEnvironmentVariable("HIRESPHERE_SEED_ADMIN_PASSWORD"));

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "Seed enabled but Seed:AdminEmail / Seed:AdminPassword (or HIRESPHERE_SEED_ADMIN_EMAIL / HIRESPHERE_SEED_ADMIN_PASSWORD) are missing. Skipping user seeding.");
            return;
        }

        if (adminPassword.Length < 12)
        {
            logger.LogWarning(
                "Seed admin password does not meet the minimum length policy (12+). Skipping user seeding.");
            return;
        }

        await SeedConfiguredUsersAsync(db, adminEmail.Trim(), adminPassword, logger, cancellationToken);
        await SeedSampleJobsAndApplicationsAsync(db, cancellationToken);
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

    private static async Task SeedRolesAndPermissionsAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var roleNames = new[] { "Candidate", "Recruiter", "HiringManager", "Admin" };
        foreach (var name in roleNames)
        {
            if (!await db.Roles.AnyAsync(r => r.Name == name, cancellationToken))
            {
                db.Roles.Add(new Role { Name = name, Description = $"{name} role" });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

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
            if (!await db.Permissions.AnyAsync(p => p.Code == code, cancellationToken))
            {
                db.Permissions.Add(new Permission { Code = code, Name = name });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var rolePermissionMap = new Dictionary<string, string[]>
        {
            ["Candidate"] = Array.Empty<string>(),
            ["Recruiter"] = new[] { "users.read", "jobs.manage", "applications.review" },
            ["HiringManager"] = new[] { "users.read", "applications.review", "hiring.decide" },
            ["Admin"] = new[] { "users.read", "jobs.manage", "applications.review", "hiring.decide", "admin.access" }
        };

        foreach (var (roleName, permCodes) in rolePermissionMap)
        {
            var role = await db.Roles.FirstAsync(r => r.Name == roleName, cancellationToken);
            foreach (var code in permCodes)
            {
                var permission = await db.Permissions.FirstAsync(p => p.Code == code, cancellationToken);
                if (!await db.RolePermissions.AnyAsync(
                        rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id,
                        cancellationToken))
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id
                    });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedOrganizationAndDepartmentsAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        if (!await db.Organizations.AnyAsync(o => o.Name == "HireSphere Demo Org", cancellationToken))
        {
            db.Organizations.Add(new Organization
            {
                Name = "HireSphere Demo Org",
                Description = "Demo organization for local development",
                Website = "https://hiresphere.local"
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        var org = await db.Organizations.FirstAsync(o => o.Name == "HireSphere Demo Org", cancellationToken);
        foreach (var deptName in new[] { "Engineering", "HR" })
        {
            if (!await db.Departments.AnyAsync(
                    d => d.OrganizationId == org.Id && d.Name == deptName,
                    cancellationToken))
            {
                db.Departments.Add(new Department { OrganizationId = org.Id, Name = deptName });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedSkillsAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        foreach (var skillName in new[] { "C#", "React", "SQL Server", "Communication", "JavaScript", "ASP.NET Core" })
        {
            if (!await db.Skills.AnyAsync(s => s.Name == skillName, cancellationToken))
            {
                db.Skills.Add(new Skill { Name = skillName, Category = "General" });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedConfiguredUsersAsync(
        ApplicationDbContext db,
        string adminEmail,
        string adminPassword,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
        var org = await db.Organizations.FirstAsync(o => o.Name == "HireSphere Demo Org", cancellationToken);
        var engineering = await db.Departments.FirstAsync(
            d => d.OrganizationId == org.Id && d.Name == "Engineering",
            cancellationToken);
        var hr = await db.Departments.FirstAsync(
            d => d.OrganizationId == org.Id && d.Name == "HR",
            cancellationToken);

        var emailLocalPart = adminEmail.Contains('@', StringComparison.Ordinal)
            ? adminEmail[..adminEmail.IndexOf('@')]
            : "admin";
        var domain = adminEmail.Contains('@', StringComparison.Ordinal)
            ? adminEmail[(adminEmail.IndexOf('@') + 1)..]
            : "hiresphere.local";

        string RoleEmail(string rolePrefix) =>
            string.Equals(rolePrefix, "admin", StringComparison.OrdinalIgnoreCase)
                ? adminEmail
                : $"{rolePrefix}.{emailLocalPart}@{domain}";

        await EnsureUserAsync(
            db,
            RoleEmail("candidate"),
            "Demo Candidate",
            "Candidate",
            passwordHash,
            async user =>
            {
                if (!await db.CandidateProfiles.AnyAsync(c => c.UserId == user.Id, cancellationToken))
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
            },
            cancellationToken);

        await EnsureUserAsync(
            db,
            RoleEmail("recruiter"),
            "Demo Recruiter",
            "Recruiter",
            passwordHash,
            async user =>
            {
                if (!await db.RecruiterProfiles.AnyAsync(r => r.UserId == user.Id, cancellationToken))
                {
                    db.RecruiterProfiles.Add(new RecruiterProfile
                    {
                        UserId = user.Id,
                        OrganizationId = org.Id,
                        DepartmentId = hr.Id,
                        Title = "Senior Recruiter"
                    });
                }
            },
            cancellationToken);

        await EnsureUserAsync(
            db,
            RoleEmail("manager"),
            "Demo Hiring Manager",
            "HiringManager",
            passwordHash,
            async user =>
            {
                if (!await db.HiringManagerProfiles.AnyAsync(h => h.UserId == user.Id, cancellationToken))
                {
                    db.HiringManagerProfiles.Add(new HiringManagerProfile
                    {
                        UserId = user.Id,
                        OrganizationId = org.Id,
                        DepartmentId = engineering.Id,
                        Title = "Engineering Manager"
                    });
                }
            },
            cancellationToken);

        await EnsureUserAsync(
            db,
            RoleEmail("admin"),
            "Demo Admin",
            "Admin",
            passwordHash,
            _ => Task.CompletedTask,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Configured development users were seeded or verified (passwords are hashed; values are never logged).");
    }

    private static async Task EnsureUserAsync(
        ApplicationDbContext db,
        string email,
        string fullName,
        string roleName,
        string passwordHash,
        Func<User, Task> configureProfile,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (user == null)
        {
            user = new User
            {
                FullName = fullName,
                Email = email.Trim(),
                NormalizedEmail = normalizedEmail,
                PasswordHash = passwordHash,
                Role = roleName,
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
        }

        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
        if (role != null
            && !await db.UserRoles.AnyAsync(
                ur => ur.UserId == user.Id && ur.RoleId == role.Id,
                cancellationToken))
        {
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        }

        await configureProfile(user);
    }

    private static async Task SeedSampleJobsAndApplicationsAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var recruiter = await db.Users
            .Where(u => u.Role == "Recruiter")
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var candidate = await db.Users
            .Where(u => u.Role == "Candidate")
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (recruiter == null || candidate == null)
        {
            return;
        }

        var org = await db.Organizations.FirstAsync(o => o.Name == "HireSphere Demo Org", cancellationToken);
        var engineering = await db.Departments.FirstAsync(
            d => d.OrganizationId == org.Id && d.Name == "Engineering",
            cancellationToken);

        if (!await db.Jobs.AnyAsync(
                j => j.Title == "Full Stack Developer" && j.RecruiterId == recruiter.Id,
                cancellationToken))
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
            await db.SaveChangesAsync(cancellationToken);
        }

        var job = await db.Jobs.FirstAsync(
            j => j.Title == "Full Stack Developer" && j.RecruiterId == recruiter.Id,
            cancellationToken);

        if (!await db.Applications.AnyAsync(
                a => a.CandidateId == candidate.Id && a.JobId == job.Id,
                cancellationToken))
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
            await db.SaveChangesAsync(cancellationToken);

            db.ApplicationStatusHistories.Add(new ApplicationStatusHistory
            {
                ApplicationId = application.Id,
                Status = ApplicationStatus.Pending,
                ChangedAtUtc = DateTime.UtcNow,
                Notes = "Application submitted"
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
