using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
namespace LabConsumableExpiryTracker.Tests.Repository;

[TestFixture]
public class DisposalRepositoryTests
{
    private static string _connectionString = null!;
    private AppDbContext _context = null!;
    private DisposalRepository _repository = null!;
    private readonly List<Guid> _seededDisposalIds = [];
    private readonly List<Guid> _seededLotIds = [];
    private readonly List<Guid> _seededItemIds = [];
    private readonly DateTimeOffset _baseTime = new(2023, 10, 27, 10, 0, 0, TimeSpan.Zero);
    private readonly Guid _testActorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _connectionString = Connection.GetConnection("UnitTestConnection");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;
        _context = new AppDbContext(options);
        _repository = new DisposalRepository(_context, NullLogger<DisposalRepository>.Instance);
        await _context.Database.CanConnectAsync();
    }
    [TearDown]
    public async Task TearDown()
    {
        if (_seededDisposalIds.Count > 0)
        {
            var disposals = await _context.Disposals.Where(d => _seededDisposalIds.Contains(d.Id)).ToListAsync();
            _context.Disposals.RemoveRange(disposals);
            await _context.SaveChangesAsync();
            _seededDisposalIds.Clear();
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
        var lot = new Lot(
            Guid.NewGuid(), itemId, lotNumber, null, _baseTime, null,
            100m, 100m, DateOnly.FromDateTime(_baseTime.UtcDateTime.AddDays(60)),
            "Shelf-A", LotAdministrativeStatus.Active);
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync();
        _seededLotIds.Add(lot.Id);
        return lot;
    }
    private async Task<Disposal> SeedDisposalAsync(Guid lotId, decimal quantity, DateTimeOffset disposedAt, string reason = "Expired")
    {
        var disposal = new Disposal(Guid.NewGuid(), lotId, quantity, reason, disposedAt, _testActorId);
        await _repository.AddAsync(disposal);
        await _context.SaveChangesAsync();
        _seededDisposalIds.Add(disposal.Id);
        return disposal;
    }
    [Test]
    public async Task AddAsync_And_GetByIdAsync()
    {
        var item = await SeedItemAsync("ITM-BASE");
        var lot = await SeedLotAsync(item.Id, "LOT-BASE");
        var disposal = new Disposal(Guid.NewGuid(), lot.Id, 10m, "Test Reason", _baseTime, _testActorId);
        await _repository.AddAsync(disposal);
        await _context.SaveChangesAsync();
        _seededDisposalIds.Add(disposal.Id);
        var fetched = await _repository.GetByIdAsync(disposal.Id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Quantity, Is.EqualTo(10m));
    }
    [Test]
    public async Task GetAllAsync_ReturnsAll()
    {
        var item = await SeedItemAsync("ITM-ALL");
        var lot = await SeedLotAsync(item.Id, "LOT-ALL");
        await SeedDisposalAsync(lot.Id, 5m, _baseTime);
        await SeedDisposalAsync(lot.Id, 10m, _baseTime);
        var all = await _repository.GetAllAsync();
        Assert.That(all, Has.Count.GreaterThanOrEqualTo(2));
    }
    [Test]
    public async Task DeleteAsync_RemovesEntity()
    {
        var item = await SeedItemAsync("ITM-DEL");
        var lot = await SeedLotAsync(item.Id, "LOT-DEL");
        var disposal = await SeedDisposalAsync(lot.Id, 5m, _baseTime);
        var result = await _repository.DeleteAsync(disposal.Id);
        Assert.That(result, Is.True);
        await _context.SaveChangesAsync();
        var fetched = await _repository.GetByIdAsync(disposal.Id);
        Assert.That(fetched, Is.Null);
        _seededDisposalIds.Remove(disposal.Id);
    }
    [Test]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());
        Assert.That(result, Is.False);
    }
    [Test]
    public async Task GetByLotIdAsync_ReturnsDisposalsForLot_OrderedByDateDesc()
    {
        var item = await SeedItemAsync("ITM-LID");
        var lot1 = await SeedLotAsync(item.Id, "LOT-LID-1");
        var lot2 = await SeedLotAsync(item.Id, "LOT-LID-2");
        await SeedDisposalAsync(lot1.Id, 5m, _baseTime);
        await SeedDisposalAsync(lot1.Id, 10m, _baseTime.AddHours(1));
        await SeedDisposalAsync(lot2.Id, 15m, _baseTime);
        var result = await _repository.GetByLotIdAsync(lot1.Id);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(result.All(d => d.LotId == lot1.Id), Is.True);
            Assert.That(result[0].DisposedAt, Is.GreaterThan(result[1].DisposedAt));
        });
    }
    [Test]
    public async Task GetByLotIdAsync_ReturnsEmpty_WhenNoDisposals()
    {
        var item = await SeedItemAsync("ITM-EMPTY");
        var lot = await SeedLotAsync(item.Id, "LOT-EMPTY");
        var result = await _repository.GetByLotIdAsync(lot.Id);
        Assert.That(result, Is.Empty);
    }
    [Test]
    public async Task SearchAsync_FilterByLotId()
    {
        var item = await SeedItemAsync("ITM-SLID");
        var lot1 = await SeedLotAsync(item.Id, "LOT-SLID-1");
        var lot2 = await SeedLotAsync(item.Id, "LOT-SLID-2");
        await SeedDisposalAsync(lot1.Id, 5m, _baseTime);
        await SeedDisposalAsync(lot2.Id, 10m, _baseTime);
        var result = await _repository.SearchAsync(null, lot1.Id, null, null, CancellationToken.None);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].LotId, Is.EqualTo(lot1.Id));
    }
    [Test]
    public async Task SearchAsync_FilterByItemId_IncludesNavigation()
    {
        var item1 = await SeedItemAsync("ITM-SIID-1");
        var item2 = await SeedItemAsync("ITM-SIID-2");
        var lot1 = await SeedLotAsync(item1.Id, "LOT-SIID-1");
        var lot2 = await SeedLotAsync(item2.Id, "LOT-SIID-2");
        await SeedDisposalAsync(lot1.Id, 5m, _baseTime);
        await SeedDisposalAsync(lot2.Id, 10m, _baseTime);
        var result = await _repository.SearchAsync(item1.Id, null, null, null, CancellationToken.None);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Lot, Is.Not.Null);
        Assert.That(result[0].Lot.Item, Is.Not.Null);
        Assert.That(result[0].Lot.Item.Id, Is.EqualTo(item1.Id));
    }
    [Test]
    public async Task SearchAsync_FilterByFrom()
    {
        var item = await SeedItemAsync("ITM-SFR");
        var lot = await SeedLotAsync(item.Id, "LOT-SFR");
        await SeedDisposalAsync(lot.Id, 1m, _baseTime.AddDays(-5));
        await SeedDisposalAsync(lot.Id, 2m, _baseTime.AddDays(-2));
        await SeedDisposalAsync(lot.Id, 3m, _baseTime.AddDays(1));
        var result = await _repository.SearchAsync(null, null, _baseTime.AddDays(-3), null, CancellationToken.None);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(d => d.DisposedAt >= _baseTime.AddDays(-3)), Is.True);
    }
    [Test]
    public async Task SearchAsync_FilterByTo()
    {
        var item = await SeedItemAsync("ITM-STO");
        var lot = await SeedLotAsync(item.Id, "LOT-STO");
        await SeedDisposalAsync(lot.Id, 1m, _baseTime.AddDays(-5));
        await SeedDisposalAsync(lot.Id, 2m, _baseTime.AddDays(-1));
        await SeedDisposalAsync(lot.Id, 3m, _baseTime.AddDays(2));
        var result = await _repository.SearchAsync(null, null, null, _baseTime, CancellationToken.None);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(d => d.DisposedAt <= _baseTime), Is.True);
    }
    [Test]
    public async Task SearchAsync_CombinedFilters()
    {
        var item1 = await SeedItemAsync("ITM-SCB-1");
        var item2 = await SeedItemAsync("ITM-SCB-2");
        var lot1 = await SeedLotAsync(item1.Id, "LOT-SCB-1");
        var lot2 = await SeedLotAsync(item2.Id, "LOT-SCB-2");
        await SeedDisposalAsync(lot1.Id, 1m, _baseTime.AddDays(-2));
        await SeedDisposalAsync(lot1.Id, 2m, _baseTime.AddDays(-10));
        await SeedDisposalAsync(lot2.Id, 3m, _baseTime.AddDays(-2));
        var result = await _repository.SearchAsync(item1.Id, null, _baseTime.AddDays(-5), null, CancellationToken.None);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Quantity, Is.EqualTo(1m));
    }
    [Test]
    public async Task SearchAsync_NoFilters_ReturnsAll()
    {
        var item = await SeedItemAsync("ITM-SNF");
        var lot = await SeedLotAsync(item.Id, "LOT-SNF");
        await SeedDisposalAsync(lot.Id, 10m, _baseTime);
        var result = await _repository.SearchAsync(null, null, null, null, CancellationToken.None);
        Assert.That(result, Is.Not.Empty);
    }
    [Test]
    public async Task SearchAsync_ImpossibleFilter_ReturnsEmpty()
    {
        var result = await _repository.SearchAsync(Guid.NewGuid(), null, null, null, CancellationToken.None);
        Assert.That(result, Is.Empty);
    }
}