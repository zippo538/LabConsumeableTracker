using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
namespace LabConsumableExpiryTracker.Tests.Repository;

[TestFixture]
public class ConsumptionHistoryRepositoryTests
{
    private static string _connectionString = null!;
    private AppDbContext _context = null!;
    private ConsumptionHistoryRepository _repository = null!;
    private readonly List<Guid> _seededConsumptionIds = [];
    private readonly List<Guid> _seededJobIds = [];
    private readonly List<Guid> _seededLotIds = [];
    private readonly List<Guid> _seededItemIds = [];
    private readonly Guid _user1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _user2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _connectionString = Connection.GetConnection("UnitTestConnection");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;
        _context = new AppDbContext(options);
        _repository = new ConsumptionHistoryRepository(_context);
        await _context.Database.CanConnectAsync();
    }
    [TearDown]
    public async Task TearDown()
    {
        if (_seededConsumptionIds.Count > 0)
        {
            var items = await _context.Consumptions.Where(c => _seededConsumptionIds.Contains(c.Id)).ToListAsync();
            _context.Consumptions.RemoveRange(items);
            await _context.SaveChangesAsync();
            _seededConsumptionIds.Clear();
        }
        if (_seededJobIds.Count > 0)
        {
            var items = await _context.Jobs.Where(j => _seededJobIds.Contains(j.Id)).ToListAsync();
            _context.Jobs.RemoveRange(items);
            await _context.SaveChangesAsync();
            _seededJobIds.Clear();
        }
        if (_seededLotIds.Count > 0)
        {
            var items = await _context.Lots.Where(l => _seededLotIds.Contains(l.Id)).ToListAsync();
            _context.Lots.RemoveRange(items);
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
    public async Task OneTimeTearDown() => await _context.DisposeAsync();
    private async Task<Item> SeedItemAsync(string code)
    {
        var item = new Item(Guid.NewGuid(), code, $"Item {code}", UnitOfMeasure.Milliliter, 10m, 30);
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        _seededItemIds.Add(item.Id);
        return item;
    }
    private async Task<Lot> SeedLotAsync(Guid itemId, string lotNumber)
    {
        var lot = Lot.Create(itemId, lotNumber, null, DateTimeOffset.UtcNow, null, 100m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)), "Shelf-A");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync();
        _seededLotIds.Add(lot.Id);
        return lot;
    }
    private async Task<Job> SeedJobAsync(string jobNumber)
    {
        var job = new Job(Guid.NewGuid(), jobNumber, "Desc");
        job.Start(DateTimeOffset.UtcNow);
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
        _seededJobIds.Add(job.Id);
        return job;
    }
    private async Task<Consumption> SeedConsumptionAsync(Guid jobId, Guid lotId, Guid consumedBy, decimal qty, DateTimeOffset consumedAt)
    {
        var c = new Consumption(Guid.NewGuid(), jobId, lotId, qty, consumedAt, consumedBy);
        _context.Consumptions.Add(c);
        await _context.SaveChangesAsync();
        _seededConsumptionIds.Add(c.Id);
        return c;
    }
    [Test]
    public async Task GetPagedAsync_FiltersByConsumedBy()
    {
        var item = await SeedItemAsync("ITM-1");
        var lot = await SeedLotAsync(item.Id, "LOT-1");
        var job = await SeedJobAsync("JOB-1");
        var now = DateTimeOffset.UtcNow;
        await SeedConsumptionAsync(job.Id, lot.Id, _user1, 5m, now);
        await SeedConsumptionAsync(job.Id, lot.Id, _user2, 10m, now);
        var (items, totalCount) = await _repository.GetPagedAsync(_user1, null, null, null, null, null, null, 1, 10);
        Assert.Multiple(() =>
        {
            Assert.That(totalCount, Is.EqualTo(1));
            Assert.That(items[0].Quantity, Is.EqualTo(5m));
        });
    }
    [Test]
    public async Task GetPagedAsync_FiltersByLotNumber_CaseInsensitive()
    {
        var item = await SeedItemAsync("ITM-2");
        var lot1 = await SeedLotAsync(item.Id, "LOT-ABC-123");
        var lot2 = await SeedLotAsync(item.Id, "LOT-XYZ-999");
        var job = await SeedJobAsync("JOB-2");
        var now = DateTimeOffset.UtcNow;
        await SeedConsumptionAsync(job.Id, lot1.Id, _user1, 1m, now);
        await SeedConsumptionAsync(job.Id, lot2.Id, _user1, 2m, now);
        var (items, _) = await _repository.GetPagedAsync(_user1, null, null, null, "lot-abc", null, null, 1, 10);
        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].LotNumber, Is.EqualTo("LOT-ABC-123"));
    }
    [Test]
    public async Task GetPagedAsync_PaginatesCorrectly()
    {
        var item = await SeedItemAsync("ITM-3");
        var lot = await SeedLotAsync(item.Id, "LOT-3");
        var job = await SeedJobAsync("JOB-3");
        var now = DateTimeOffset.UtcNow;
        for (var i = 0; i < 5; i++)
            await SeedConsumptionAsync(job.Id, lot.Id, _user1, 1m, now.AddMinutes(i));
        var (page1, count1) = await _repository.GetPagedAsync(_user1, null, null, null, null, null, null, 1, 2);
        var (page2, _) = await _repository.GetPagedAsync(_user1, null, null, null, null, null, null, 2, 2);
        Assert.Multiple(() =>
        {
            Assert.That(count1, Is.EqualTo(5));
            Assert.That(page1, Has.Count.EqualTo(2));
            Assert.That(page2, Has.Count.EqualTo(2));
        });
    }
    [Test]
    public async Task GetDetailAsync_ReturnsDetail_WhenExistsAndMatchesUser()
    {
        var item = await SeedItemAsync("ITM-4");
        var lot = await SeedLotAsync(item.Id, "LOT-4");
        var job = await SeedJobAsync("JOB-4");
        var c = await SeedConsumptionAsync(job.Id, lot.Id, _user1, 7m, DateTimeOffset.UtcNow);
        var detail = await _repository.GetDetailAsync(c.Id, _user1);
        Assert.That(detail, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(detail!.Quantity, Is.EqualTo(7m));
            Assert.That(detail.ItemCode, Is.EqualTo("ITM-4"));
            Assert.That(detail.JobNumber, Is.EqualTo("JOB-4"));
        });
    }
    [Test]
    public async Task GetDetailAsync_ReturnsNull_WhenUserMismatch()
    {
        var item = await SeedItemAsync("ITM-5");
        var lot = await SeedLotAsync(item.Id, "LOT-5");
        var job = await SeedJobAsync("JOB-5");
        var c = await SeedConsumptionAsync(job.Id, lot.Id, _user1, 7m, DateTimeOffset.UtcNow);
        var detail = await _repository.GetDetailAsync(c.Id, _user2);
        Assert.That(detail, Is.Null);
    }
}