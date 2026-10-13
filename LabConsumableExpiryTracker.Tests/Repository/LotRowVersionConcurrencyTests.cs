using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Tests.Repository;


[TestFixture]
public class LotRowVersionConcurrencyTests
{
    private const string LotPrefix = "LOT-RV-";
    private const string ItemPrefix = "TEST-RV-";
    private const string JobPrefix = "JOB-RV-";

    private string _connectionString = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _connectionString = Connection.GetConnection("UnitTestConnection");
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync(CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await CleanupTestData();
    }

    [SetUp]
    public async Task SetUp()
    {
        await CleanupTestData();
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .EnableSensitiveDataLogging()
            .Options);

    [Test]
    public async Task Consume_WhenTwoWritersReadSameRowVersion_ShouldRejectStaleWriter()
    {
        var (job, lot) = await SeedPendingLotAsync(remainingQuantity: 100m);
        var now = DateTimeOffset.UtcNow;
        var consumedBy = Guid.NewGuid();

        await using var writerA = CreateContext();
        await using var writerB = CreateContext();

        // Both writers read the lot before either of them writes: same snapshot, same RowVersion.
        var lotA = await writerA.Lots.SingleAsync(x => x.Id == lot.Id, CancellationToken.None);
        var lotB = await writerB.Lots.SingleAsync(x => x.Id == lot.Id, CancellationToken.None);

        Assert.That(
            lotB.RowVersion,
            Is.EqualTo(lotA.RowVersion),
            "Both concurrent readers must observe the same RowVersion snapshot.");

        var staleRowVersion = lotB.RowVersion;

        // First writer wins.
        var consumptionA = lotA.Consume(job.Id, 10m, now, consumedBy);
        writerA.Consumptions.Add(consumptionA);
        await writerA.SaveChangesAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(lotA.RemainingQuantity, Is.EqualTo(90m));
            Assert.That(lotA.RowVersion, Is.Not.EqualTo(staleRowVersion));
        });

        // Second writer still holds the stale token and must be rejected.
        var consumptionB = lotB.Consume(job.Id, 10m, now, consumedBy);
        writerB.Consumptions.Add(consumptionB);

        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await writerB.SaveChangesAsync(CancellationToken.None),
            "Stale writer must not be allowed to overwrite the lot.");

        // The losing write is rolled back: quantity reflects only the winning consumption.
        await using var reader = CreateContext();
        var persistedLot = await reader.Lots
            .AsNoTracking()
            .SingleAsync(x => x.Id == lot.Id, CancellationToken.None);
        var persistedConsumptions = await reader.Consumptions
            .AsNoTracking()
            .CountAsync(x => x.LotId == lot.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(persistedLot.RemainingQuantity, Is.EqualTo(90m));
            Assert.That(persistedConsumptions, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Consume_WhenStaleWriterRefreshesRowVersion_ShouldSucceed()
    {
        var (job, lot) = await SeedPendingLotAsync(remainingQuantity: 100m);
        var now = DateTimeOffset.UtcNow;
        var consumedBy = Guid.NewGuid();

        await using var writerA = CreateContext();
        await using var writerB = CreateContext();

        var lotA = await writerA.Lots.SingleAsync(x => x.Id == lot.Id, CancellationToken.None);
        var lotB = await writerB.Lots.SingleAsync(x => x.Id == lot.Id, CancellationToken.None);

        // Writer B stages its consumption against the stale snapshot before writer A commits.
        writerB.Consumptions.Add(lotB.Consume(job.Id, 30m, now, consumedBy));

        // Writer A wins the race.
        writerA.Consumptions.Add(lotA.Consume(job.Id, 30m, now, consumedBy));
        await writerA.SaveChangesAsync(CancellationToken.None);

        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await writerB.SaveChangesAsync(CancellationToken.None));

        // Reload the fresh RowVersion, then retry the consumption.
        await using var retry = CreateContext();
        var freshLot = await retry.Lots.SingleAsync(x => x.Id == lot.Id, CancellationToken.None);
        Assert.That(freshLot.RowVersion, Is.Not.EqualTo(lotB.RowVersion));

        retry.Consumptions.Add(freshLot.Consume(job.Id, 30m, now, consumedBy));
        var affected = await retry.SaveChangesAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(affected, Is.EqualTo(2)); // 1 consumption insert + 1 lot update
            Assert.That(freshLot.RemainingQuantity, Is.EqualTo(40m));
        });
    }

    [Test]
    public async Task Consume_WhenTwoWritersRaceInParallel_ShouldAllowExactlyOneWinner()
    {
        var (job, lot) = await SeedPendingLotAsync(remainingQuantity: 100m);
        var now = DateTimeOffset.UtcNow;
        var consumedBy = Guid.NewGuid();

        // Both writers must finish reading before either writes, otherwise the race
        // is not actually exercised and both requests could succeed sequentially.
        using var bothHaveRead = new Barrier(participantCount: 2);

        var results = await Task.WhenAll(
            RaceConsumeAsync(bothHaveRead),
            RaceConsumeAsync(bothHaveRead));

        var succeeded = results.Count(result => result);
        var concurrencyFailures = results.Length - succeeded;

        await using var reader = CreateContext();
        var persistedLot = await reader.Lots
            .AsNoTracking()
            .SingleAsync(x => x.Id == lot.Id, CancellationToken.None);
        var persistedConsumptions = await reader.Consumptions
            .AsNoTracking()
            .CountAsync(x => x.LotId == lot.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(succeeded, Is.EqualTo(1), "Exactly one writer must win the race.");
            Assert.That(concurrencyFailures, Is.EqualTo(1), "The loser must fail on the stale RowVersion.");
            Assert.That(persistedLot.RemainingQuantity, Is.EqualTo(90m));
            Assert.That(persistedConsumptions, Is.EqualTo(1));
        });
        return;

        async Task<bool> RaceConsumeAsync(Barrier barrier)
        {
            await using var context = CreateContext();
            var racingLot = await context.Lots
                .SingleAsync(x => x.Id == lot.Id, CancellationToken.None);

            // Hold the row version before any writer commits.
            _ = racingLot.RowVersion;

            barrier.SignalAndWait();

            try
            {
                context.Consumptions.Add(racingLot.Consume(job.Id, 10m, now, consumedBy));
                await context.SaveChangesAsync(CancellationToken.None);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }
    }

    private async Task<(Job Job, Lot Lot)> SeedPendingLotAsync(decimal remainingQuantity)
    {
        await using var context = CreateContext();

        var item = new Item(
            Guid.NewGuid(),
            $"{ItemPrefix}{Guid.NewGuid():N}",
            $"Concurrency Test Item {Guid.NewGuid():N}",
            UnitOfMeasure.Unit,
            10m,
            30);

        var lot = new Lot(
            Guid.NewGuid(),
            item.Id,
            $"{LotPrefix}{Guid.NewGuid():N}",
            null,
            new DateTimeOffset(2030, 1, 1, 8, 0, 0, TimeSpan.Zero),
            "Concurrency Supplier",
            initialQuantity: 100m,
            remainingQuantity: remainingQuantity,
            new DateOnly(2030, 12, 31),
            "TEST-RACK",
            LotAdministrativeStatus.Active);

        var job = new Job(
            Guid.NewGuid(),
            $"{JobPrefix}{Guid.NewGuid():N}",
            "Concurrency Test Job");
        job.Start(DateTimeOffset.UtcNow);

        context.Items.Add(item);
        context.Lots.Add(lot);
        context.Jobs.Add(job);
        await context.SaveChangesAsync(CancellationToken.None);

        return (job, lot);
    }

    private async Task CleanupTestData()
    {
        await using var context = CreateContext();

        var lotIds = await context.Lots
            .Where(lot => lot.LotNumber.StartsWith(LotPrefix))
            .Select(lot => lot.Id)
            .ToListAsync(CancellationToken.None);

        var jobIds = await context.Jobs
            .Where(job => job.JobNumber != null && job.JobNumber.StartsWith(JobPrefix))
            .Select(job => job.Id)
            .ToListAsync(CancellationToken.None);

        var consumptions = await context.Consumptions
            .Where(consumption =>
                lotIds.Contains(consumption.LotId) ||
                jobIds.Contains(consumption.JobId))
            .ToListAsync(CancellationToken.None);

        if (consumptions.Count > 0)
        {
            context.Consumptions.RemoveRange(consumptions);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        var disposals = await context.Disposals
            .Where(disposal => lotIds.Contains(disposal.LotId))
            .ToListAsync(CancellationToken.None);

        if (disposals.Count > 0)
        {
            context.Disposals.RemoveRange(disposals);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        if (lotIds.Count > 0)
        {
            var lots = await context.Lots
                .Where(lot => lotIds.Contains(lot.Id))
                .ToListAsync(CancellationToken.None);
            context.Lots.RemoveRange(lots);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        if (jobIds.Count > 0)
        {
            var jobs = await context.Jobs
                .Where(job => jobIds.Contains(job.Id))
                .ToListAsync(CancellationToken.None);
            context.Jobs.RemoveRange(jobs);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        var items = await context.Items
            .Where(item => item.Code.StartsWith(ItemPrefix))
            .ToListAsync(CancellationToken.None);

        if (items.Count > 0)
        {
            context.Items.RemoveRange(items);
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
