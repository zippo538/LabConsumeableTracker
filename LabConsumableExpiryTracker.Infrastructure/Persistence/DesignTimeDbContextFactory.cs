using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence;

/// <summary>
/// Provides a design-time factory for <see cref="AppDbContext"/> so that
/// <c>dotnet ef</c> commands can create the context without needing the
/// full host application to be built and run.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
        public AppDbContext CreateDbContext(string[] args)
        {
            // Determine the path to the project directory. When the EF tools
            // invoke the factory the working directory is the project folder,
            // so we can safely use <c>Directory.GetCurrentDirectory()</c>.
            var basePath = Path.Combine(Directory.GetCurrentDirectory(),"../LabConsumableExpiryTracker.API");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection belum diatur di appsettings.json.");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

            return new AppDbContext(optionsBuilder.Options);
        }
}
