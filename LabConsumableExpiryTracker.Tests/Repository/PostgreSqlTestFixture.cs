using LabConsumableExpiryTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Tests.Repository;

[TestFixture]
public class PostgreSqlTestFixture : IDisposable
{
    public AppDbContext Context { get; }

    public PostgreSqlTestFixture()
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Environment variable LAB_TRACKER_TEST_CONNECTION belum diset.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        Context = new AppDbContext(options);

        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}