using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence;

public class DbInitializer : IDbInitializer
{
    private readonly AppDbContext _dbContext;
    private readonly IdentitySeeder _identitySeeder;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        AppDbContext dbContext,
        IdentitySeeder identitySeeder,
        ILogger<DbInitializer> logger)
    {
        _dbContext = dbContext;
        _identitySeeder = identitySeeder;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Applying database migrations...");

        await _dbContext.Database.MigrateAsync();

        await _identitySeeder.SeedAsync();

        _logger.LogInformation(
            "Database initialization completed.");
    }
}