using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LabConsumableExpiryTracker.Tests.Repository;

[TestFixture]
public class ItemRepositoryTests
{
    private static string _connectionString = null!;
    private AppDbContext _context = null!;
    private ItemRepository _repository = null!;

    private readonly List<Guid> _seededItemIds = [];
    private readonly List<Guid> _seededLotIds = [];

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _connectionString = Connection.GetConnection("UnitTestConnection");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;

        _context = new AppDbContext(options);
        _repository = new ItemRepository(_context, NullLogger<ItemRepository>.Instance);
        await _context.Database.CanConnectAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        // Bersihkan dari child ke parent untuk mencegah FK violation
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

    // ──────────────────────────────────────────────
    //  HELPERS (DDD Compliant)
    // ──────────────────────────────────────────────

    private async Task<Item> SeedItemAsync(string code, string name)
    {
        var item = new Item(Guid.NewGuid(), code, name, UnitOfMeasure.Milliliter, 10m, 30);
        await _repository.AddAsync(item);
        await _context.SaveChangesAsync();
        _seededItemIds.Add(item.Id);
        return item;
    }

    private async Task SeedLotAsync(Guid itemId, string lotNumber, decimal initialQty, decimal remainingQty, DateOnly expiryDate, LotAdministrativeStatus status)
    {
        var lot = new Lot(
            id: Guid.NewGuid(),
            itemId: itemId,
            lotNumber: lotNumber,
            supplierLotNumber: null,
            receivedAt: DateTimeOffset.Now,
            supplierName: null,
            initialQuantity: initialQty,
            remainingQuantity: remainingQty,
            expiryDate: expiryDate,
            storageLocation: "Shelf-A",
            administrativeStatus: status);

        _context.Lots.Add(lot);
        await _context.SaveChangesAsync();
        _seededLotIds.Add(lot.Id);

        // Hapus baris: return lot;
    }

    // ──────────────────────────────────────────────
    //  BASE REPOSITORY TESTS
    // ──────────────────────────────────────────────

    [Test]
    public async Task AddAsync_And_GetByIdAsync()
    {
        var item = new Item(Guid.NewGuid(), "ITM-BASE-1", "Base Item 1", UnitOfMeasure.Gram, 5m, 15);
        await _repository.AddAsync(item);
        await _context.SaveChangesAsync();
        _seededItemIds.Add(item.Id);

        var fetched = await _repository.GetByIdAsync(item.Id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Code, Is.EqualTo("ITM-BASE-1"));
    }

    [Test]
    public async Task GetAllAsync_ReturnsAll()
    {
        await SeedItemAsync("ITM-ALL-1", "All 1");
        await SeedItemAsync("ITM-ALL-2", "All 2");

        var all = await _repository.GetAllAsync();
        Assert.That(all.Count, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public async Task UpdateAsync_ModifiesEntity()
    {
        var item = await SeedItemAsync("ITM-UPD", "Update Me");
        item.UpdateDetails("Updated Name", UnitOfMeasure.Gram, 20m, 45);
        await _repository.UpdateAsync(item);
        await _context.SaveChangesAsync();

        var fetched = await _repository.GetByIdAsync(item.Id);
        Assert.That(fetched!.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    public async Task DeleteAsync_RemovesEntity()
    {
        var item = await SeedItemAsync("ITM-DEL", "Delete Me");
        var result = await _repository.DeleteAsync(item.Id);
        Assert.That(result, Is.True);
        await _context.SaveChangesAsync();

        var fetched = await _repository.GetByIdAsync(item.Id);
        Assert.That(fetched, Is.Null);
        _seededItemIds.Remove(item.Id); // Cegah error saat TearDown
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());
        Assert.That(result, Is.False);
    }

    // ──────────────────────────────────────────────
    //  ITEM REPOSITORY SPECIFIC TESTS
    // ──────────────────────────────────────────────

    [Test]
    public async Task GetAllWithLotsAsync_IncludesLots()
    {
        var item = await SeedItemAsync("ITM-WL-1", "With Lots 1");
        await SeedLotAsync(item.Id, "LOT-IGNORED", 100m, 100m, new DateOnly(9999, 12, 31), LotAdministrativeStatus.Active);

        var items = await _repository.GetAllWithLotsAsync();
        var fetchedItem = items.FirstOrDefault(i => i.Id == item.Id);

        Assert.That(fetchedItem, Is.Not.Null);
        Assert.That(fetchedItem!.Lots, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetAllWithLotsAsyncPaged_ReturnsPagedResult()
    {
        await SeedItemAsync("ITM-PG-1", "Paged 1");
        await SeedItemAsync("ITM-PG-2", "Paged 2");
        await SeedItemAsync("ITM-PG-3", "Paged 3");

        var page1 = await _repository.GetAllWithLotsAsyncPaged(1, 2);
        Assert.That(page1.Items, Has.Count.EqualTo(2));
        Assert.That(page1.TotalCount, Is.GreaterThanOrEqualTo(3));
        Assert.That(page1.Page, Is.EqualTo(1));
        Assert.That(page1.PageSize, Is.EqualTo(2));
    }

    [Test]
    public void GetAllWithLotsAsyncPaged_Throws_WhenPageInvalid()
    {
        // page < 0 akan melempar ArgumentOutOfRangeException
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await _repository.GetAllWithLotsAsyncPaged(-1, 10));
    }

    [Test]
    public void GetAllWithLotsAsyncPaged_Throws_WhenPageSizeInvalid()
    {
        // pageSize < 1 atau > 100 akan melempar ArgumentOutOfRangeException
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await _repository.GetAllWithLotsAsyncPaged(1, 0));

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await _repository.GetAllWithLotsAsyncPaged(1, 101));
    }

    [Test]
    public async Task GetByCodeAsync_ReturnsItem_WhenExists()
    {
        await SeedItemAsync("ITM-CODE-1", "By Code 1");

        var fetched = await _repository.GetByCodeAsync("ITM-CODE-1");
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Name, Is.EqualTo("By Code 1"));
    }

    [Test]
    public async Task GetByCodeAsync_ReturnsNull_WhenNotExists()
    {
        var fetched = await _repository.GetByCodeAsync("NON-EXISTENT-CODE");
        Assert.That(fetched, Is.Null);
    }

    [Test]
    public async Task GetByIdWithLotsAsync_IncludesLots()
    {
        var item = await SeedItemAsync("ITM-IDWL", "ById With Lots");
        await SeedLotAsync(item.Id, "LOT-IGNORED", 100m, 100m, new DateOnly(9999, 12, 31), LotAdministrativeStatus.Active);

        var fetched = await _repository.GetByIdWithLotsAsync(item.Id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Lots, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetByIdWithLotsAsync_ReturnsNull_WhenNotExists()
    {
        var fetched = await _repository.GetByIdWithLotsAsync(Guid.NewGuid());
        Assert.That(fetched, Is.Null);
    }
}