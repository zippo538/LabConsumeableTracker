using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
namespace LabConsumableExpiryTracker.Tests.Repository;

[TestFixture]
public class ConsumptionRepositoryTests
{
    private static string _connectionString = null!;
    private AppDbContext _context = null!;
    private ConsumptionRepository _repository = null!;
    private readonly List<Guid> _seededConsumptionIds = [];
    private readonly List<Guid> _seededJobIds = [];
    private readonly List<Guid> _seededLotIds = [];
    private readonly List<Guid> _seededItemIds = [];
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _connectionString = Connection.GetConnection("UnitTestConnection");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;
        _context = new AppDbContext(options);
        _repository = new ConsumptionRepository(_context);
        await _context.Database.CanConnectAsync();
    }
    [TearDown]
    public async Task TearDown()
    {
        if (_seededConsumptionIds.Count > 0)
        {
            var consumptions = await _context.Consumptions.Where(c => _seededConsumptionIds.Contains(c.Id)).ToListAsync();
            _context.Consumptions.RemoveRange(consumptions);
            await _context.SaveChangesAsync();
            _seededConsumptionIds.Clear();
        }
        if (_seededJobIds.Count > 0)
        {
            var jobs = await _context.Jobs.Where(j => _seededJobIds.Contains(j.Id)).ToListAsync();
            _context.Jobs.RemoveRange(jobs);
            await _context.SaveChangesAsync();
            _seededJobIds.Clear();
        }
        if (_seededLotIds.Count > 0)
        {
            var lots = await _context.Lots.Where(l => _seededLotIds.Contains(l.Id)).ToListAsync();
            _context.Lots.RemoveRange(lots);
            await _context.SaveChangesAsync();
            _seededLotIds.Clear();
        }
        if (_seededItemIds.Count > 0)
        {
            var items = await _context.Items.Where(i => _seededItemIds.Contains(i.Id)).ToListAsync();
            _context.Items.RemoveRange(items);
            await _context.SaveChangesAsync();
            _seededItemIds.Clear();
        }
    }
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _context.DisposeAsync();
    }
    private async Task<Item> SeedItemAsync()
    {
        var item = new Item(
            id: Guid.NewGuid(),
            code: $"ITEM-C-{Guid.NewGuid():N}"[..20],
            name: $"Test Item {Guid.NewGuid():N}"[..30],
            baseUnit: UnitOfMeasure.Milliliter,
            minimumStock: 10m,
            expiringSoonDays: 30
        );
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        _seededItemIds.Add(item.Id);
        return item;
    }
    private async Task<Lot> SeedLotAsync(Guid itemId)
    {
        var lot = new Lot(
            id: Guid.NewGuid(),
            itemId: itemId,
            lotNumber: $"LOT-{Guid.NewGuid():N}"[..20],
            supplierLotNumber: null,
            receivedAt: DateTimeOffset.UtcNow.AddDays(-10),
            supplierName: null,
            initialQuantity: 100m,
            remainingQuantity: 80m,
            expiryDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)),
            storageLocation: "Shelf-A",
            administrativeStatus: LotAdministrativeStatus.Active
        );
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync();
        _seededLotIds.Add(lot.Id);
        return lot;
    }
    private async Task<Job> SeedJobAsync()
    {
        var job = new Job(
            id: Guid.NewGuid(),
            jobNumber: $"JOB-{Guid.NewGuid():N}"[..20],
            description: "Test Job Description"
        );
        job.Start(DateTimeOffset.UtcNow);
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
        _seededJobIds.Add(job.Id);
        return job;
    }
    private async Task SeedConsumptionAsync(
    Guid jobId, Guid lotId, decimal quantity, DateTimeOffset? consumedAt = null)
    {
        var testActorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var consumption = new Consumption(
            id: Guid.NewGuid(),
            jobId: jobId,
            lotId: lotId,
            quantity: quantity,
            consumedAt: consumedAt ?? DateTimeOffset.UtcNow,
            consumedBy: testActorId
        );
        await _repository.AddAsync(consumption);
        await _context.SaveChangesAsync();
        _seededConsumptionIds.Add(consumption.Id);
    }
    [Test]
    public async Task AddAsync_PersistsConsumption_And_GetByIdAsync_ReturnsIt()
    {
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var job = await SeedJobAsync();
        var testActorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var consumption = new Consumption(
            id: Guid.NewGuid(),
            jobId: job.Id,
            lotId: lot.Id,
            quantity: 5m,
            consumedAt: DateTimeOffset.UtcNow,
            consumedBy: testActorId
        );
        await _repository.AddAsync(consumption);
        await _context.SaveChangesAsync();
        _seededConsumptionIds.Add(consumption.Id);
        var fetched = await _repository.GetByIdAsync(consumption.Id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.JobId, Is.EqualTo(job.Id));
        Assert.That(fetched.LotId, Is.EqualTo(lot.Id));
        Assert.That(fetched.Quantity, Is.EqualTo(5m));
        Assert.That(fetched.ConsumedBy, Is.EqualTo(testActorId));
    }
    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }
    [Test]
    public async Task SearchAsync_FilterByJobId_ReturnsMatchingConsumptions()
    {
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var job1 = await SeedJobAsync();
        var job2 = await SeedJobAsync();
        await SeedConsumptionAsync(job1.Id, lot.Id, 3m);
        await SeedConsumptionAsync(job1.Id, lot.Id, 7m);
        await SeedConsumptionAsync(job2.Id, lot.Id, 2m);
        var results = await _repository.SearchAsync(
            jobId: job1.Id, itemId: null, lotId: null,
            from: null, to: null, ct: CancellationToken.None);
        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results.All(c => c.JobId == job1.Id), Is.True);
    }
    [Test]
    public async Task SearchAsync_FilterByLotId_ReturnsMatchingConsumptions()
    {
        var item = await SeedItemAsync();
        var lot1 = await SeedLotAsync(item.Id);
        var lot2 = await SeedLotAsync(item.Id);
        var job = await SeedJobAsync();
        await SeedConsumptionAsync(job.Id, lot1.Id, 4m);
        await SeedConsumptionAsync(job.Id, lot2.Id, 6m);
        var results = await _repository.SearchAsync(
            jobId: null, itemId: null, lotId: lot1.Id,
            from: null, to: null, ct: CancellationToken.None);
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].LotId, Is.EqualTo(lot1.Id));
    }
    [Test]
    public async Task SearchAsync_FilterByItemId_ReturnsConsumptionsForThatItem()
    {
        var item1 = await SeedItemAsync();
        var item2 = await SeedItemAsync();
        var lot1 = await SeedLotAsync(item1.Id);
        var lot2 = await SeedLotAsync(item2.Id);
        var job = await SeedJobAsync();
        await SeedConsumptionAsync(job.Id, lot1.Id, 1m);
        await SeedConsumptionAsync(job.Id, lot2.Id, 2m);
        var results = await _repository.SearchAsync(
            jobId: null, itemId: item1.Id, lotId: null,
            from: null, to: null, ct: CancellationToken.None);
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].LotId, Is.EqualTo(lot1.Id));
    }
    [Test]
    public async Task SearchAsync_FilterByDateRange_ReturnsConsumptionsInRange()
    {
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var job = await SeedJobAsync();
        var now = DateTimeOffset.UtcNow;
        await SeedConsumptionAsync(job.Id, lot.Id, 1m, consumedAt: now.AddDays(-5));
        await SeedConsumptionAsync(job.Id, lot.Id, 2m, consumedAt: now.AddDays(-1));
        await SeedConsumptionAsync(job.Id, lot.Id, 3m, consumedAt: now.AddDays(1));
        var results = await _repository.SearchAsync(
            jobId: null, itemId: null, lotId: null,
            from: now.AddDays(-3), to: now,
            ct: CancellationToken.None);
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Quantity, Is.EqualTo(2m));
    }
    [Test]
    public async Task SearchAsync_NoFilters_ReturnsNonEmptyList()
    {
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var job = await SeedJobAsync();
        await SeedConsumptionAsync(job.Id, lot.Id, 10m);
        var results = await _repository.SearchAsync(
            jobId: null, itemId: null, lotId: null,
            from: null, to: null, ct: CancellationToken.None);
        Assert.That(results, Is.Not.Empty);
    }
    [Test]
    public async Task SearchAsync_ImpossibleFilter_ReturnsEmptyList()
    {
        var results = await _repository.SearchAsync(
            jobId: Guid.NewGuid(), itemId: null, lotId: null,
            from: null, to: null, ct: CancellationToken.None);
        Assert.That(results, Is.Empty);
    }
}