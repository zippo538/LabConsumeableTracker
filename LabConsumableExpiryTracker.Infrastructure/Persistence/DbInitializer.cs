using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence;

public class DbInitializer : IDbInitializer
{
    private readonly AppDbContext _dbContext;
    private readonly IdentitySeeder _identitySeeder;
    private readonly ILogger<DbInitializer> _logger;
    private readonly TimeProvider _timeProvider;

    public DbInitializer(
        AppDbContext dbContext,
        IdentitySeeder identitySeeder,
        ILogger<DbInitializer> logger,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _identitySeeder = identitySeeder;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Applying database migrations...");

        await _dbContext.Database.MigrateAsync();

        await _identitySeeder.SeedAsync();
        var generatedItems = ItemSeeder.Generate();

        var existingItemCodes = (await _dbContext.Items
                .AsNoTracking()
                .Select(item => item.Code)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newItems = generatedItems
            .Where(item => !existingItemCodes.Contains(item.Code))
            .ToList();

        if (newItems.Count > 0)
        {
            await _dbContext.Items.AddRangeAsync(newItems);
            await _dbContext.SaveChangesAsync();
        }

        var seedItemCodes = generatedItems
            .Select(item => item.Code)
            .ToArray();

        var itemIds = await _dbContext.Items
            .AsNoTracking()
            .Where(item => seedItemCodes.Contains(item.Code))
            .Select(item => item.Id)
            .ToListAsync();

        var itemIdsWithLots = (await _dbContext.Lots
                .AsNoTracking()
                .Select(lot => lot.ItemId)
                .Distinct()
                .ToListAsync())
            .ToHashSet();

        var itemIdsWithoutLots = itemIds
            .Where(itemId => !itemIdsWithLots.Contains(itemId))
            .ToArray();

        if (itemIdsWithoutLots.Length > 0)
        {
            var lots = LotSeeder.Generate(
                itemIdsWithoutLots,
                _timeProvider,
                lotsPerItem: 4);

            await _dbContext.Lots.AddRangeAsync(lots);
            await _dbContext.SaveChangesAsync();
        }


        _logger.LogInformation(
            "Database initialization completed.");
    }
}