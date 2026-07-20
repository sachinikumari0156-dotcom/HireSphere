using HireSphere.API.Data;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Tests;

/// <summary>
/// LocalDB migration checks. Skips when ConnectionStrings__DefaultConnection is unset.
/// </summary>
public class MigrationVerificationTests
{
    [Fact]
    public async Task LocalDb_Can_Connect_And_Has_Migration_History()
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(cs))
        {
            // Default coursework LocalDB when env not set — only run if reachable.
            cs = "Server=(localdb)\\MSSQLLocalDB;Database=HireSphereDev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(cs)
            .Options;

        await using var db = new ApplicationDbContext(options);
        if (!await db.Database.CanConnectAsync())
        {
            return;
        }

        var applied = await db.Database.GetAppliedMigrationsAsync();
        Assert.Contains(applied, m => m.Contains("InitialSqlServerCoreModel"));
        Assert.Contains(applied, m => m.Contains("AddStoragePortalPhase83"));
    }

    [Fact]
    public async Task LocalDb_Update_Is_Idempotent()
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                 ?? "Server=(localdb)\\MSSQLLocalDB;Database=HireSphereDev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(cs)
            .Options;

        await using var db = new ApplicationDbContext(options);
        if (!await db.Database.CanConnectAsync())
        {
            return;
        }

        var before = (await db.Database.GetAppliedMigrationsAsync()).Count();
        await db.Database.MigrateAsync();
        var after = (await db.Database.GetAppliedMigrationsAsync()).Count();
        Assert.Equal(before, after);
    }
}
