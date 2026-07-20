using HireSphere.API.Data;
using HireSphere.API.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace HireSphere.API.Tests;

/// <summary>
/// Optional SQL Server verification. Runs only when ConnectionStrings__DefaultConnection is set.
/// </summary>
public class SqlServerVerificationTests
{
    [Fact]
    public async Task CatalogSeed_IsIdempotent_WhenUsersDisabled()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var db = new ApplicationDbContext(options);
        Assert.True(await db.Database.CanConnectAsync());

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:Enabled"] = "false"
            })
            .Build();

        await DbSeeder.SeedAsync(db, config, NullLogger.Instance);
        var rolesAfterFirst = await db.Roles.CountAsync();
        var skillsAfterFirst = await db.Skills.CountAsync();

        await DbSeeder.SeedAsync(db, config, NullLogger.Instance);
        var rolesAfterSecond = await db.Roles.CountAsync();
        var skillsAfterSecond = await db.Skills.CountAsync();

        Assert.Equal(rolesAfterFirst, rolesAfterSecond);
        Assert.Equal(skillsAfterFirst, skillsAfterSecond);
        Assert.True(rolesAfterFirst >= 4);
    }

    [Fact]
    public async Task EnabledUserSeed_HashesPasswords_And_IsIdempotent()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        var seedEmail = Environment.GetEnvironmentVariable("HIRESPHERE_SEED_ADMIN_EMAIL");
        var seedPassword = Environment.GetEnvironmentVariable("HIRESPHERE_SEED_ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(connectionString)
            || string.IsNullOrWhiteSpace(seedEmail)
            || string.IsNullOrWhiteSpace(seedPassword))
        {
            return;
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var db = new ApplicationDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:Enabled"] = "true",
                ["Seed:AdminEmail"] = seedEmail,
                ["Seed:AdminPassword"] = seedPassword
            })
            .Build();

        await DbSeeder.SeedAsync(db, config, NullLogger.Instance);
        var usersAfterFirst = await db.Users.CountAsync();

        await DbSeeder.SeedAsync(db, config, NullLogger.Instance);
        var usersAfterSecond = await db.Users.CountAsync();

        Assert.Equal(usersAfterFirst, usersAfterSecond);
        Assert.True(usersAfterFirst >= 1);

        var hashes = await db.Users.AsNoTracking().Select(u => u.PasswordHash).ToListAsync();
        Assert.All(hashes, hash =>
        {
            Assert.False(string.Equals(hash, seedPassword, StringComparison.Ordinal));
            Assert.StartsWith("$2", hash);
        });
    }

    [Fact]
    public async Task RequiredTables_Exist()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var db = new ApplicationDbContext(options);
        var tables = await db.Database
            .SqlQueryRaw<string>("SELECT TABLE_NAME AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'")
            .ToListAsync();

        string[] required =
        [
            "Users", "Roles", "Permissions", "UserRoles", "Organizations", "Departments",
            "CandidateProfiles", "RecruiterProfiles", "HiringManagerProfiles", "Skills", "Jobs",
            "Applications", "SkillAssessments", "Interviews", "CandidateEvaluations",
            "HiringDecisions", "AuditLogs"
        ];

        foreach (var table in required)
        {
            Assert.Contains(table, tables);
        }

        var migrations = await db.Database.GetAppliedMigrationsAsync();
        Assert.Contains(migrations, m => m.Contains("InitialSqlServerCoreModel", StringComparison.Ordinal));
    }
}
