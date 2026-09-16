using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Infrastructure.Persistence.Seeders;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence;

public class DbInitializer : IDbInitializer
{
    private const int ConsumptionCount = 100;
    private const int DisposalCount = 100;
    private const int JobCount = 100;
    private const int LotsPerItem = 4;
    private readonly AppDbContext _dbContext;
    private readonly IdentitySeeder _identitySeeder;
    private readonly ILogger<DbInitializer> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly UserSeeder _userSeeder;

    public DbInitializer(
        AppDbContext dbContext,
        IdentitySeeder identitySeeder,
        ILogger<DbInitializer> logger,
        TimeProvider timeProvider,
        UserSeeder userSeeder)
    {
        _dbContext = dbContext;
        _identitySeeder = identitySeeder;
        _logger = logger;
        _timeProvider = timeProvider;
        _userSeeder = userSeeder;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Applying database migrations...");
        await _dbContext.Database.MigrateAsync();

        await _identitySeeder.SeedAsync();

        await _userSeeder.SeedAsync();

        var generatedItems = ItemSeeder.Generate();

        await SeedItemsAsync(generatedItems);

        var seededItemIds = await GetSeededItemIdsAsync(generatedItems);

        await SeedLotsAsync(seededItemIds);
        await SeedJobsAsync();

        await SeedConsumptionsAsync();
        await SeedDisposalsAsync();

        _logger.LogInformation(
            "Database initialization completed.");
    }
    private async Task SeedItemsAsync(
        IReadOnlyList<Item> generatedItems)
    {
        var existingItemCodes = (await _dbContext.Items
                .AsNoTracking()
                .Select(item => item.Code)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newItems = generatedItems
            .Where(item => !existingItemCodes.Contains(item.Code))
            .ToList();

        if (newItems.Count == 0)
        {
            _logger.LogInformation(
                "Item seed skipped; all {Count} item code(s) already exist.",
                generatedItems.Count);

            return;
        }

        await _dbContext.Items.AddRangeAsync(newItems);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Seeded {Count} new item(s).",
            newItems.Count);
    }

    private async Task<IReadOnlyList<Guid>> GetSeededItemIdsAsync(
        IReadOnlyList<Item> generatedItems)
    {
        var seedItemCodes = generatedItems
            .Select(item => item.Code)
            .ToArray();

        return await _dbContext.Items
            .AsNoTracking()
            .Where(item => seedItemCodes.Contains(item.Code))
            .OrderBy(item => item.Code)
            .Select(item => item.Id)
            .ToListAsync();
    }

    private async Task SeedLotsAsync(
        IReadOnlyList<Guid> itemIds)
    {
        if (itemIds.Count == 0)
        {
            _logger.LogWarning(
                "Lot seed skipped; no seeded item id was found.");

            return;
        }

        var existingLotNumbers = (await _dbContext.Lots
                .AsNoTracking()
                .Select(lot => lot.LotNumber)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var generatedLots = LotSeeder.Generate(
            itemIds,
            _timeProvider,
            lotsPerItem: LotsPerItem);

        var newLots = generatedLots
            .Where(lot => !existingLotNumbers.Contains(lot.LotNumber))
            .ToList();

        if (newLots.Count == 0)
        {
            _logger.LogInformation(
                "Lot seed skipped; all {Count} lot number(s) already exist.",
                generatedLots.Count);

            return;
        }

        await _dbContext.Lots.AddRangeAsync(newLots);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Seeded {Count} new lot(s).",
            newLots.Count);
    }
    private async Task SeedJobsAsync()
    {
        var existingJobNumbers = (await _dbContext.Jobs
                .AsNoTracking()
                .Select(job => job.JobNumber)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var generatedJobs = JobSeeder.Generate(
            _timeProvider,
            jobCount: JobCount);

        var newJobs = generatedJobs
            .Where(job => !existingJobNumbers.Contains(job.JobNumber))
            .ToList();

        if (newJobs.Count == 0)
        {
            _logger.LogInformation(
                "Job seed skipped; all {Count} job number(s) already exist.",
                generatedJobs.Count);

            return;
        }

        await _dbContext.Jobs.AddRangeAsync(newJobs);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Seeded {Count} new job(s).",
            newJobs.Count);
    }

    private async Task SeedConsumptionsAsync()
    {
        var existingConsumptionIds = (await _dbContext.Consumptions
                .AsNoTracking()
                .Select(consumption => consumption.Id)
                .ToListAsync())
            .ToHashSet();

        var jobs = await _dbContext.Jobs
            .AsNoTracking()
            .ToListAsync();

        var lots = await _dbContext.Lots
            .AsNoTracking()
            .ToListAsync();

        var actorIds = await _dbContext.Users
            .AsNoTracking()
            .Select(user => user.Id)
            .ToListAsync();

        if (jobs.Count == 0 || lots.Count == 0 || actorIds.Count == 0)
        {
            _logger.LogWarning(
                "Consumption seed skipped; job, lot, atau actor belum tersedia.");

            return;
        }

        var generatedConsumptions = ConsumptionSeeder.Generate(
            jobs,
            lots,
            actorIds,
            _timeProvider,
            consumptionCount: ConsumptionCount);

        var newConsumptions = generatedConsumptions
            .Where(consumption => !existingConsumptionIds.Contains(consumption.Id))
            .ToList();

        if (newConsumptions.Count == 0)
        {
            _logger.LogInformation(
                "Consumption seed skipped; all {Count} record(s) already exist.",
                generatedConsumptions.Count);

            return;
        }

        await _dbContext.Consumptions.AddRangeAsync(newConsumptions);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Seeded {Count} new consumption record(s).",
            newConsumptions.Count);
    }
    private async Task SeedDisposalsAsync()
    {
        var existingDisposalIds = (await _dbContext.Disposals
                .AsNoTracking()
                .Select(disposal => disposal.Id)
                .ToListAsync())
            .ToHashSet();

        var lots = await _dbContext.Lots.AsNoTracking().ToListAsync();

        // Kuota disposal dihitung setelah konsumsi, jadi konsumsi
        // yang sudah tersimpan harus ikut diperhitungkan (BR-03/BR-04).
        var consumptions = await _dbContext.Consumptions
            .AsNoTracking()
            .ToListAsync();

        var actorIds = await GetActorIdsAsync();

        if (lots.Count == 0 || actorIds.Count == 0)
        {
            _logger.LogWarning(
                "Disposal seed skipped; lot atau actor belum tersedia.");

            return;
        }

        var generatedDisposals = DisposalSeeder.Generate(
            lots,
            consumptions,
            actorIds,
            _timeProvider,
            disposalCount: DisposalCount);

        var newDisposals = generatedDisposals
            .Where(disposal => !existingDisposalIds.Contains(disposal.Id))
            .ToList();

        if (newDisposals.Count == 0)
        {
            _logger.LogInformation(
                "Disposal seed skipped; all {Count} record(s) already exist.",
                generatedDisposals.Count);

            return;
        }

        await _dbContext.Disposals.AddRangeAsync(newDisposals);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Seeded {Count} new disposal record(s).",
            newDisposals.Count);
    }
    private async Task<IReadOnlyList<Guid>> GetActorIdsAsync() =>
        await _dbContext.Users
            .AsNoTracking()
            .Select(user => user.Id)
            .ToListAsync();
}