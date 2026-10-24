using Microsoft.Extensions.Configuration;
namespace LabConsumableExpiryTracker.Tests.Helpers;

public abstract class Connection
{
    private static string _connectionString = string.Empty;
    public static string GetConnection(string connectionAppSettings)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: false)
            .Build();
        _connectionString = configuration.GetConnectionString(connectionAppSettings) ?? throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection belum diatur di appsettings.Development.json.");
        return _connectionString;
    }

}