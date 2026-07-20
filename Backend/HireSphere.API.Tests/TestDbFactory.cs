using HireSphere.API.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Tests;

public static class TestDbFactory
{
    public static (ApplicationDbContext Context, SqliteConnection Connection) CreateContext(string? databaseName = null)
    {
        var connection = new SqliteConnection($"Data Source={databaseName ?? Guid.NewGuid().ToString()};Mode=Memory;Cache=Shared");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return (context, connection);
    }
}
