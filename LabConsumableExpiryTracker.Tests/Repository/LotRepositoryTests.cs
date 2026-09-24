using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using LabConsumableExpiryTracker.Tests.Helpers;
namespace LabConsumableExpiryTracker.Tests.Repository;
[TestFixture]
public class LotRepositoryTests
{
    private AppDbContext _context = null!;
    private LotRepository _repository = null!;
    private CancellationToken _ct;
    private static string _connectionString = string.Empty;
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _connectionString = Connection.GetConnection("UnitTestConnection");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;
        _context = new AppDbContext(options);
        _repository = new LotRepository(_context, NullLogger<LotRepository>.Instance);
        await _context.Database.CanConnectAsync(_ct);
    }
    [SetUp]
    public async Task SetUp()
    {
        _ct = CancellationToken.None;
        await CleanupTestData();
        _context.ChangeTracker.Clear();
    }
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await CleanupTestData();
        await _context.DisposeAsync();
    }
    [Test]
    public async Task GetAllAsync_WhenLotsExist_ShouldReturnAllLots()
    {
        var itemId = await CreateItemAsync();
        var lot1 = CreateLot(
            itemId,
            "LOT-001",
            expiryDate: new DateOnly(2030, 1, 1));
        var lot2 = CreateLot(
            itemId,
            "LOT-002",
            expiryDate: new DateOnly(2030, 2, 1));
        _context.Lots.AddRange(lot1, lot2);
        await _context.SaveChangesAsync(_ct);
        var result = await _repository.GetAllAsync(_ct);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(
            result.Select(x => x.LotNumber),
            Is.EquivalentTo(new[]
            {
                "LOT-001",
                "LOT-002"
            }));
    }
    [Test]
    public async Task GetAllAsync_WhenNoLotsExist_ShouldReturnEmptyCollection()
    {
        var result = await _repository.GetAllAsync(_ct);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }
    [Test]
    public async Task GetByIdAsync_WhenLotExists_ShouldReturnLot()
    {
        var itemId = await CreateItemAsync();
        var lot = CreateLot(
            itemId,
            "LOT-GET-BY-ID");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();
        var result = await _repository.GetByIdAsync(
            lot.Id,
            _ct);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(lot.Id));
        Assert.That(result.ItemId, Is.EqualTo(itemId));
        Assert.That(
            result.LotNumber,
            Is.EqualTo("LOT-GET-BY-ID"));
    }
    [Test]
    public async Task GetByIdAsync_WhenLotDoesNotExist_ShouldReturnNull()
    {
        var lotId = Guid.NewGuid();
        var result = await _repository.GetByIdAsync(
            lotId,
            _ct);
        Assert.That(result, Is.Null);
    }
    [Test]
    public async Task AddAsync_ShouldAddLotToDbSet()
    {
        var itemId = await CreateItemAsync();
        var lot = CreateLot(
            itemId,
            "LOT-ADD-001");
        var result = await _repository.AddAsync(
            lot,
            _ct);
        await _context.SaveChangesAsync(_ct);
        Assert.That(result, Is.SameAs(lot));
        var savedLot = await _context.Lots
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == lot.Id,
                _ct);
        Assert.That(savedLot, Is.Not.Null);
        Assert.That(
            savedLot!.LotNumber,
            Is.EqualTo("LOT-ADD-001"));
        Assert.That(
            savedLot.ItemId,
            Is.EqualTo(itemId));
    }
    [Test]
    public async Task UpdateAsync_ShouldPersistUpdatedLot()
    {
        var itemId = await CreateItemAsync();
        var lot = CreateLot(
            itemId,
            "LOT-UPDATE-001",
            storageLocation: "RACK-A-01");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();
        var existing = await _context.Lots
            .SingleAsync(x => x.Id == lot.Id, _ct);
        existing.UpdateStorageLocation("RACK-B-02");
        var result = await _repository.UpdateAsync(
            existing,
            _ct);
        await _context.SaveChangesAsync(_ct);
        Assert.That(result, Is.SameAs(existing));
        _context.ChangeTracker.Clear();
        var updated = await _context.Lots
            .AsNoTracking()
            .SingleAsync(x => x.Id == lot.Id, _ct);
        Assert.That(
            updated.StorageLocation,
            Is.EqualTo("RACK-B-02"));
    }
    [Test]
    public async Task DeleteAsync_WhenLotExists_ShouldRemoveLot()
    {
        var itemId = await CreateItemAsync();
        var lot = CreateLot(
            itemId,
            "LOT-DELETE-001");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        var result = await _repository.DeleteAsync(
            lot.Id,
            _ct);
        await _context.SaveChangesAsync(_ct);
        Assert.That(result, Is.True);
        var deletedLot = await _context.Lots
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == lot.Id,
                _ct);
        Assert.That(deletedLot, Is.Null);
    }
    [Test]
    public async Task DeleteAsync_WhenLotDoesNotExist_ShouldReturnFalse()
    {
        var lotId = Guid.NewGuid();
        var result = await _repository.DeleteAsync(
            lotId,
            _ct);
        Assert.That(result, Is.False);
    }
    [Test]
    public async Task ExistsAsync_WhenMatchingLotExists_ShouldReturnTrue()
    {
        var itemId = await CreateItemAsync();
        var expiryDate = new DateOnly(2030, 12, 31);
        var lot = CreateLot(
            itemId,
            "LOT-EXISTS-001",
            expiryDate: expiryDate);
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        var result = await _repository.ExistsAsync(
            itemId,
            "LOT-EXISTS-001",
            expiryDate,
            _ct);
        Assert.That(result, Is.True);
    }
    [Test]
    public async Task ExistsAsync_WhenLotNumberHasWhitespace_ShouldTrimLotNumber()
    {
        var itemId = await CreateItemAsync();
        var expiryDate = new DateOnly(2030, 12, 31);
        var lot = CreateLot(
            itemId,
            "LOT-TRIM-001",
            expiryDate: expiryDate);
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        var result = await _repository.ExistsAsync(
            itemId,
            "   LOT-TRIM-001   ",
            expiryDate,
            _ct);
        Assert.That(result, Is.True);
    }
    [Test]
    public async Task ExistsAsync_WhenItemIsDifferent_ShouldReturnFalse()
    {
        var itemId = await CreateItemAsync();
        var differentItemId = await CreateItemAsync();
        var expiryDate = new DateOnly(2030, 12, 31);
        var lot = CreateLot(
            itemId,
            "LOT-EXISTS-002",
            expiryDate: expiryDate);
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        var result = await _repository.ExistsAsync(
            differentItemId,
            "LOT-EXISTS-002",
            expiryDate,
            _ct);
        Assert.That(result, Is.False);
    }
    [Test]
    public async Task ExistsAsync_WhenExpiryDateIsDifferent_ShouldReturnFalse()
    {
        var itemId = await CreateItemAsync();
        var actualExpiryDate = new DateOnly(2030, 12, 31);
        var differentExpiryDate = new DateOnly(2031, 12, 31);
        var lot = CreateLot(
            itemId,
            "LOT-EXISTS-003",
            expiryDate: actualExpiryDate);
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        var result = await _repository.ExistsAsync(
            itemId,
            "LOT-EXISTS-003",
            differentExpiryDate,
            _ct);
        Assert.That(result, Is.False);
    }
    [Test]
    public async Task GetByItemIdAsync_ShouldReturnOnlyLotsForRequestedItem()
    {
        var itemId = await CreateItemAsync();
        var differentItemId = await CreateItemAsync();
        var requestedItemLot = CreateLot(
            itemId,
            "LOT-ITEM-001");
        var otherItemLot = CreateLot(
            differentItemId,
            "LOT-OTHER-001");
        _context.Lots.AddRange(
            requestedItemLot,
            otherItemLot);
        await _context.SaveChangesAsync(_ct);
        var result = (
            await _repository.GetByItemIdAsync(
                itemId,
                _ct))
            .ToList();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(
            result[0].LotNumber,
            Is.EqualTo("LOT-ITEM-001"));
        Assert.That(
            result[0].ItemId,
            Is.EqualTo(itemId));
    }
    [Test]
    public async Task GetByItemIdAsync_ShouldOrderByExpiryDateThenReceivedAt()
    {
        var itemId = await CreateItemAsync();
        var receivedFirst = new DateTimeOffset(
            2030,
            1,
            1,
            8,
            0,
            0,
            TimeSpan.Zero);
        var receivedSecond = new DateTimeOffset(
            2030,
            1,
            1,
            10,
            0,
            0,
            TimeSpan.Zero);
        var lotA = CreateLot(
            itemId,
            "LOT-A",
            receivedAt: receivedSecond,
            expiryDate: new DateOnly(2030, 6, 1));
        var lotB = CreateLot(
            itemId,
            "LOT-B",
            receivedAt: receivedFirst,
            expiryDate: new DateOnly(2030, 6, 1));
        var lotC = CreateLot(
            itemId,
            "LOT-C",
            receivedAt: receivedFirst,
            expiryDate: new DateOnly(2030, 5, 1));
        _context.Lots.AddRange(
            lotA,
            lotB,
            lotC);
        await _context.SaveChangesAsync(_ct);
        var result = (
            await _repository.GetByItemIdAsync(
                itemId,
                _ct))
            .ToList();
        Assert.That(
            result.Select(x => x.LotNumber),
            Is.EqualTo(new[]
            {
                "LOT-C",
                "LOT-B",
                "LOT-A"
            }));
    }
    [Test]
    public async Task GetTotalUsableQuantityAsync_ShouldSumEligibleActiveLots()
    {
        var itemId = await CreateItemAsync();
        var today = new DateOnly(2030, 1, 10);
        var activeUsableLot1 = CreateLot(
            itemId,
            "LOT-USABLE-001",
            initialQuantity: 100m,
            remainingQuantity: 40m,
            expiryDate: new DateOnly(2030, 1, 20),
            status: LotAdministrativeStatus.Active);
        var activeUsableLot2 = CreateLot(
            itemId,
            "LOT-USABLE-002",
            initialQuantity: 100m,
            remainingQuantity: 30m,
            expiryDate: today,
            status: LotAdministrativeStatus.Active);
        var expiredLot = CreateLot(
            itemId,
            "LOT-EXPIRED",
            initialQuantity: 100m,
            remainingQuantity: 50m,
            expiryDate: new DateOnly(2030, 1, 9),
            status: LotAdministrativeStatus.Active);
        var zeroQuantityLot = CreateLot(
            itemId,
            "LOT-ZERO",
            initialQuantity: 100m,
            remainingQuantity: 0m,
            expiryDate: new DateOnly(2030, 1, 20),
            status: LotAdministrativeStatus.Active);
        var inactiveLot = CreateLot(
            itemId,
            "LOT-INACTIVE",
            initialQuantity: 100m,
            remainingQuantity: 80m,
            expiryDate: new DateOnly(2030, 1, 20),
            status: LotAdministrativeStatus.Disposed);
        _context.Lots.AddRange(
            activeUsableLot1,
            activeUsableLot2,
            expiredLot,
            zeroQuantityLot,
            inactiveLot);
        await _context.SaveChangesAsync(_ct);
        var result =
            await _repository.GetTotalUsableQuantityAsync(
                itemId,
                today,
                _ct);
        Assert.That(result, Is.EqualTo(70m));
    }
    [Test]
    public async Task GetTotalUsableQuantityAsync_WhenNoUsableLotExists_ShouldReturnZero()
    {
        var itemId = await CreateItemAsync();
        var today = new DateOnly(2030, 1, 10);
        var expiredLot = CreateLot(
            itemId,
            "LOT-NO-USABLE",
            initialQuantity: 100m,
            remainingQuantity: 50m,
            expiryDate: new DateOnly(2030, 1, 9),
            status: LotAdministrativeStatus.Active);
        _context.Lots.Add(expiredLot);
        await _context.SaveChangesAsync(_ct);
        var result =
            await _repository.GetTotalUsableQuantityAsync(
                itemId,
                today,
                _ct);
        Assert.That(result, Is.EqualTo(0m));
    }
    [Test]
    public async Task GetUsableQuantityByItemIdsAsync_ShouldGroupUsableQuantityByItem()
    {
        var item1 = await CreateItemAsync();
        var item2 = await CreateItemAsync();
        var item3 = await CreateItemAsync();
        var today = new DateOnly(2030, 1, 10);
        _context.Lots.AddRange(
            CreateLot(
                item1,
                "ITEM1-LOT1",
                initialQuantity: 100m,
                remainingQuantity: 20m,
                expiryDate: new DateOnly(2030, 1, 20),
                status: LotAdministrativeStatus.Active),
            CreateLot(
                item1,
                "ITEM1-LOT2",
                initialQuantity: 100m,
                remainingQuantity: 30m,
                expiryDate: new DateOnly(2030, 2, 1),
                status: LotAdministrativeStatus.Active),
            CreateLot(
                item1,
                "ITEM1-EXPIRED",
                initialQuantity: 100m,
                remainingQuantity: 50m,
                expiryDate: new DateOnly(2030, 1, 9),
                status: LotAdministrativeStatus.Active),
            CreateLot(
                item2,
                "ITEM2-LOT1",
                initialQuantity: 100m,
                remainingQuantity: 40m,
                expiryDate: new DateOnly(2030, 1, 20),
                status: LotAdministrativeStatus.Active),
            CreateLot(
                item2,
                "ITEM2-DISPOSED",
                initialQuantity: 100m,
                remainingQuantity: 60m,
                expiryDate: new DateOnly(2030, 1, 20),
                status: LotAdministrativeStatus.Disposed),
            CreateLot(
                item3,
                "ITEM3-ZERO",
                initialQuantity: 100m,
                remainingQuantity: 0m,
                expiryDate: new DateOnly(2030, 1, 20),
                status: LotAdministrativeStatus.Active)
        );
        await _context.SaveChangesAsync(_ct);
        var result =
            await _repository.GetUsableQuantityByItemIdsAsync(
                [
                    item1,
                    item2,
                    item3
                ],
                today,
                _ct);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(
            result[item1],
            Is.EqualTo(50m));
        Assert.That(
            result[item2],
            Is.EqualTo(40m));
        Assert.That(
            result.ContainsKey(item3),
            Is.False);
    }
    [Test]
    public async Task GetUsableQuantityByItemIdsAsync_ShouldIgnoreEmptyAndDuplicateIds()
    {
        var itemId = await CreateItemAsync();
        var today = new DateOnly(2030, 1, 10);
        var lot = CreateLot(
            itemId,
            "LOT-DISTINCT",
            initialQuantity: 100m,
            remainingQuantity: 25m,
            expiryDate: new DateOnly(2030, 1, 20),
            status: LotAdministrativeStatus.Active);
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        var result =
            await _repository.GetUsableQuantityByItemIdsAsync(
                [
                    Guid.Empty,
                    itemId,
                    itemId,
                    Guid.Empty
                ],
                today,
                _ct);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(
            result[itemId],
            Is.EqualTo(25m));
    }
    [Test]
    public async Task GetUsableQuantityByItemIdsAsync_WhenIdsAreEmpty_ShouldReturnEmptyDictionary()
    {
        var result =
            await _repository.GetUsableQuantityByItemIdsAsync(
                [
                    Guid.Empty,
                    Guid.Empty
                ],
                new DateOnly(2030, 1, 10),
                _ct);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }
    private async Task<Guid> CreateItemAsync()
    {
        var itemId = Guid.NewGuid();
        var item = new Item(
            itemId,
            $"TEST-{Guid.NewGuid():N}",
            $"Test Item {Guid.NewGuid():N}",
            UnitOfMeasure.Unit,
            10m,
            30);
        _context.Items.Add(item);
        await _context.SaveChangesAsync(_ct);
        return itemId;
    }
    private static Lot CreateLot(
        Guid itemId,
        string lotNumber,
        decimal initialQuantity = 100m,
        decimal remainingQuantity = 100m,
        DateOnly? expiryDate = null,
        DateTimeOffset? receivedAt = null,
        string storageLocation = "TEST-RACK",
        LotAdministrativeStatus status = LotAdministrativeStatus.Active)
    {
        return new Lot(
            Guid.NewGuid(),
            itemId,
            lotNumber,
            null,
            receivedAt ?? new DateTimeOffset(
                2030,
                1,
                1,
                8,
                0,
                0,
                TimeSpan.Zero),
            "Test Supplier",
            initialQuantity,
            remainingQuantity,
            expiryDate ?? new DateOnly(2030, 12, 31),
            storageLocation,
            status);
    }
    private async Task CleanupTestData()
    {
    var testLots = await _context.Lots
        .Where(x =>
            x.LotNumber.StartsWith("LOT-") ||
            x.LotNumber.StartsWith("ITEM1-") ||
            x.LotNumber.StartsWith("ITEM2-") ||
            x.LotNumber.StartsWith("ITEM3-"))
        .ToListAsync(_ct); 
    if (testLots.Count > 0)
    {
        _context.Lots.RemoveRange(testLots);
        await _context.SaveChangesAsync(_ct);
    }
    var itemIds = await _context.Items
        .Where(x => x.Code.StartsWith("TEST-"))
        .Select(x => x.Id)
        .ToListAsync(_ct); 
    if (itemIds.Count > 0)
    {
        var items = await _context.Items
            .Where(x => itemIds.Contains(x.Id))
            .ToListAsync(_ct); 
        _context.Items.RemoveRange(items);
        await _context.SaveChangesAsync(_ct);
    }
    _context.ChangeTracker.Clear();
    
    }
}
