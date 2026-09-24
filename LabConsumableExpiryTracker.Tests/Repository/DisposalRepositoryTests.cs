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
    public async Task OneTimeTearDown()
    {
        await _context.DisposeAsync();
    }
    private async Task<Item> SeedItemAsync()
    {
        return await SeedItemAsync(
            code: $"ITEM-{Guid.NewGuid():N}"[..15],
            name: $"Test Item {Guid.NewGuid():N}"[..25],
            baseUnit: UnitOfMeasure.Milliliter,
            minimumStock: 10,
            expDays: 30
        );
    }
    private async Task<Item> SeedItemAsync(string code, string name, UnitOfMeasure baseUnit, int minimumStock, int expDays)
    {
        var item = new Item(Guid.NewGuid(), code, name, baseUnit, minimumStock, expDays);
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        _seededItemIds.Add(item.Id);
        return item;
    }
    private async Task<Lot> SeedLotAsync(Guid itemId)
    {
        var lot = Lot.Create(
            itemId: itemId,
            lotNumber: $"LOT-D-{Guid.NewGuid():N}"[..20],
            supplierLotNumber: null,
            receivedAt: DateTimeOffset.UtcNow.AddDays(-90),
            supplierName: null,
            initialQuantity: 50m,
            expiryDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), 
            storageLocation: "Shelf-B"
        );
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync();
        _seededLotIds.Add(lot.Id);
        return lot;
    }
    private async Task SeedDisposalAsync(Guid lotId, decimal quantity, DateTimeOffset? disposedAt = null)
    {
    var testActorId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    var disposal = new Disposal(
        id: Guid.NewGuid(),
        lotId: lotId,
        quantity: quantity,
        reason: "Expired",
        disposedAt: disposedAt ?? DateTimeOffset.UtcNow,
        disposedBy: testActorId
    );

    _context.Disposals.Add(disposal);
    await _context.SaveChangesAsync();
    _seededDisposalIds.Add(disposal.Id);
    
    }
    [Test]
    public async Task AddAsync_PersistsDisposal_And_GetByIdAsync_ReturnsIt()
    {
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var testActorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var disposal = new Disposal(
            id: Guid.NewGuid(),
            lotId: lot.Id,
            quantity: 15m,
            reason: "Contaminated",
            disposedAt: DateTimeOffset.UtcNow,
            disposedBy: testActorId
        );
        await _repository.AddAsync(disposal);
        await _context.SaveChangesAsync();
        _seededDisposalIds.Add(disposal.Id);
        var fetched = await _repository.GetByIdAsync(disposal.Id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.LotId, Is.EqualTo(lot.Id));
        Assert.That(fetched.Quantity, Is.EqualTo(15m));
        Assert.That(fetched.DisposedBy, Is.EqualTo(testActorId));
        Assert.That(fetched.Reason, Is.EqualTo("Contaminated"));
    }
    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }
    [Test]
public async Task SearchAsync_FilterByFrom_ReturnsDisposalsOnOrAfterDate()
{
    var item = await SeedItemAsync();
    var lot = await SeedLotAsync(item.Id);
    var now = new DateTimeOffset(2023, 10, 27, 10, 0, 0, TimeSpan.Zero);
    await SeedDisposalAsync(lot.Id, 1m, disposedAt: now.AddDays(-10));
    await SeedDisposalAsync(lot.Id, 2m, disposedAt: now.AddDays(-3));
    await SeedDisposalAsync(lot.Id, 3m, disposedAt: now);
    var results = await _repository.SearchAsync(
        itemId: null, lotId: null, from: now.AddDays(-3), to: null, ct: CancellationToken.None);
    Assert.That(results, Has.Count.EqualTo(2));
    Assert.That(results.Select(r => r.Quantity), Is.EquivalentTo(new[] { 2m, 3m }));
}
[Test]
public async Task SearchAsync_FilterByTo_ReturnsDisposalsBeforeDate()
{
    var item = await SeedItemAsync();
    var lot = await SeedLotAsync(item.Id);
    var now = new DateTimeOffset(2023, 10, 27, 10, 0, 0, TimeSpan.Zero);
    await SeedDisposalAsync(lot.Id, 1m, disposedAt: now.AddDays(-5));
    await SeedDisposalAsync(lot.Id, 2m, disposedAt: now.AddDays(-1));
    await SeedDisposalAsync(lot.Id, 3m, disposedAt: now.AddDays(2));
    var results = await _repository.SearchAsync(
        itemId: null, lotId: null, from: null, to: now, ct: CancellationToken.None);
    Assert.That(results, Has.Count.EqualTo(2));
    Assert.That(results.Select(r => r.Quantity), Is.EquivalentTo(new[] { 1m, 2m }));
}
[Test]
public async Task SearchAsync_FilterByFromAndTo_ReturnsDisposalsInRange()
{
    var item = await SeedItemAsync();
    var lot = await SeedLotAsync(item.Id);
    var now = new DateTimeOffset(2023, 10, 27, 10, 0, 0, TimeSpan.Zero);
    await SeedDisposalAsync(lot.Id, 1m, disposedAt: now.AddDays(-10));
    await SeedDisposalAsync(lot.Id, 2m, disposedAt: now.AddDays(-5));
    await SeedDisposalAsync(lot.Id, 3m, disposedAt: now.AddDays(-1));
    await SeedDisposalAsync(lot.Id, 4m, disposedAt: now.AddDays(2));
    var results = await _repository.SearchAsync(
        itemId: null, lotId: null, from: now.AddDays(-7), to: now, ct: CancellationToken.None);
    Assert.That(results, Has.Count.EqualTo(2));
    Assert.That(results.Select(r => r.Quantity), Is.EquivalentTo(new[] { 2m, 3m }));
}
[Test]
public async Task SearchAsync_CombinedFilters_ReturnsOnlyMatchingDisposals()
{
    var item1 = await SeedItemAsync();
    var item2 = await SeedItemAsync();
    var lot1 = await SeedLotAsync(item1.Id);
    var lot2 = await SeedLotAsync(item2.Id);
    var now = new DateTimeOffset(2023, 10, 27, 10, 0, 0, TimeSpan.Zero);
    await SeedDisposalAsync(lot1.Id, 1m, disposedAt: now.AddDays(-2));
    await SeedDisposalAsync(lot1.Id, 2m, disposedAt: now.AddDays(-10));
    await SeedDisposalAsync(lot2.Id, 3m, disposedAt: now.AddDays(-2));
    var results = await _repository.SearchAsync(
        itemId: item1.Id, lotId: null, from: now.AddDays(-5), to: null, ct: CancellationToken.None);
    Assert.That(results, Has.Count.EqualTo(1));
    Assert.That(results[0].Quantity, Is.EqualTo(1m));
}
}