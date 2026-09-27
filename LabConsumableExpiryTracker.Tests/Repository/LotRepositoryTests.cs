using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LabConsumableExpiryTracker.Tests.Repository;

[TestFixture]
public class LotRepositoryTests
{
    private static readonly string[] ExpectedAllLotNumbers = ["LOT-001", "LOT-002"];
    private static readonly string[] ExpectedOrderedLotNumbers = ["LOT-C", "LOT-B", "LOT-A"];
    private static readonly string[] ExpectedRequestedLotNumbers = ["LOT-IDS-001", "LOT-IDS-003"];
    private static readonly string[] ExpectedFirstPageLotNumbers = ["LOT-PAGE-A", "LOT-PAGE-B"];
    private static readonly string[] ExpectedSecondPageLotNumbers = ["LOT-PAGE-C"];
    private static readonly string[] ExpectedSearchByLotNumber = ["SEQ-101"];
    private static readonly string[] ExpectedSearchByName = ["LOT-SEARCH-901"];
    private static readonly string[] ExpectedSearchByCode = ["LOT-SEARCH-CODE"];

    private static string _connectionString = null!;
    private AppDbContext _context = null!;
    private LotRepository _repository = null!;
    private CancellationToken _ct;

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
        await _context.Database.CanConnectAsync(CancellationToken.None);
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
        var lot1 = CreateLot(itemId, "LOT-001", expiryDate: new DateOnly(2030, 1, 1));
        var lot2 = CreateLot(itemId, "LOT-002", expiryDate: new DateOnly(2030, 2, 1));
        _context.Lots.AddRange(lot1, lot2);
        await _context.SaveChangesAsync(_ct);

        var result = await _repository.GetAllAsync(_ct);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(
                result.Select(x => x.LotNumber),
                Is.EquivalentTo(ExpectedAllLotNumbers));
        });
    }

    [Test]
    public async Task GetAllAsync_WhenNoLotsExist_ShouldReturnEmptyCollection()
    {
        var result = await _repository.GetAllAsync(_ct);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        });
    }

    [Test]
    public async Task GetByIdAsync_WhenLotExists_ShouldReturnLot()
    {
        var itemId = await CreateItemAsync();
        var lot = CreateLot(itemId, "LOT-GET-BY-ID");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetByIdAsync(lot.Id, _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(lot.Id));
            Assert.That(result.ItemId, Is.EqualTo(itemId));
            Assert.That(result.LotNumber, Is.EqualTo("LOT-GET-BY-ID"));
        });
    }

    [Test]
    public async Task GetByIdAsync_WhenLotDoesNotExist_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), _ct);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task AddAsync_ShouldAddLotToDbSet()
    {
        var itemId = await CreateItemAsync();
        var lot = CreateLot(itemId, "LOT-ADD-001");

        var result = await _repository.AddAsync(lot, _ct);
        await _context.SaveChangesAsync(_ct);

        var savedLot = await _context.Lots
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == lot.Id, _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(lot));
            Assert.That(savedLot, Is.Not.Null);
            Assert.That(savedLot!.LotNumber, Is.EqualTo("LOT-ADD-001"));
            Assert.That(savedLot.ItemId, Is.EqualTo(itemId));
        });
    }

    [Test]
    public async Task UpdateAsync_ShouldPersistUpdatedLot()
    {
        var itemId = await CreateItemAsync();
        var lot = CreateLot(itemId, "LOT-UPDATE-001", storageLocation: "RACK-A-01");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var existing = await _context.Lots.SingleAsync(x => x.Id == lot.Id, _ct);
        existing.UpdateStorageLocation("RACK-B-02");
        var result = await _repository.UpdateAsync(existing);
        await _context.SaveChangesAsync(_ct);

        _context.ChangeTracker.Clear();
        var updated = await _context.Lots
            .AsNoTracking()
            .SingleAsync(x => x.Id == lot.Id, _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(existing));
            Assert.That(updated.StorageLocation, Is.EqualTo("RACK-B-02"));
        });
    }

    [Test]
    public async Task DeleteAsync_WhenLotExists_ShouldRemoveLot()
    {
        var itemId = await CreateItemAsync();
        var lot = CreateLot(itemId, "LOT-DELETE-001");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);

        var result = await _repository.DeleteAsync(lot.Id, _ct);
        await _context.SaveChangesAsync(_ct);

        var deletedLot = await _context.Lots
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == lot.Id, _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(deletedLot, Is.Null);
        });
    }

    [Test]
    public async Task DeleteAsync_WhenLotDoesNotExist_ShouldReturnFalse()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid(), _ct);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ExistsAsync_WhenMatchingLotExists_ShouldReturnTrue()
    {
        var itemId = await CreateItemAsync();
        var expiryDate = new DateOnly(2030, 12, 31);
        var lot = CreateLot(itemId, "LOT-EXISTS-001", expiryDate: expiryDate);
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);

        var result = await _repository.ExistsAsync(itemId, "LOT-EXISTS-001", expiryDate, _ct);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExistsAsync_WhenLotNumberHasWhitespace_ShouldTrimLotNumber()
    {
        var itemId = await CreateItemAsync();
        var expiryDate = new DateOnly(2030, 12, 31);
        var lot = CreateLot(itemId, "LOT-TRIM-001", expiryDate: expiryDate);
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);

        var result = await _repository.ExistsAsync(itemId, "   LOT-TRIM-001   ", expiryDate, _ct);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExistsAsync_WhenItemIsDifferent_ShouldReturnFalse()
    {
        var itemId = await CreateItemAsync();
        var differentItemId = await CreateItemAsync();
        var expiryDate = new DateOnly(2030, 12, 31);
        var lot = CreateLot(itemId, "LOT-EXISTS-002", expiryDate: expiryDate);
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);

        var result = await _repository.ExistsAsync(differentItemId, "LOT-EXISTS-002", expiryDate, _ct);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ExistsAsync_WhenExpiryDateIsDifferent_ShouldReturnFalse()
    {
        var itemId = await CreateItemAsync();
        var actualExpiryDate = new DateOnly(2030, 12, 31);
        var differentExpiryDate = new DateOnly(2031, 12, 31);
        var lot = CreateLot(itemId, "LOT-EXISTS-003", expiryDate: actualExpiryDate);
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);

        var result = await _repository.ExistsAsync(itemId, "LOT-EXISTS-003", differentExpiryDate, _ct);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetByItemIdAsync_ShouldReturnOnlyLotsForRequestedItem()
    {
        var itemId = await CreateItemAsync();
        var differentItemId = await CreateItemAsync();
        var requestedItemLot = CreateLot(itemId, "LOT-ITEM-001");
        var otherItemLot = CreateLot(differentItemId, "LOT-OTHER-001");
        _context.Lots.AddRange(requestedItemLot, otherItemLot);
        await _context.SaveChangesAsync(_ct);

        var result = (await _repository.GetByItemIdAsync(itemId, _ct)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].LotNumber, Is.EqualTo("LOT-ITEM-001"));
            Assert.That(result[0].ItemId, Is.EqualTo(itemId));
        });
    }

    [Test]
    public async Task GetByItemIdAsync_ShouldOrderByExpiryDateThenReceivedAt()
    {
        var itemId = await CreateItemAsync();
        var receivedFirst = new DateTimeOffset(2030, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var receivedSecond = new DateTimeOffset(2030, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var lotA = CreateLot(itemId, "LOT-A", receivedAt: receivedSecond, expiryDate: new DateOnly(2030, 6, 1));
        var lotB = CreateLot(itemId, "LOT-B", receivedAt: receivedFirst, expiryDate: new DateOnly(2030, 6, 1));
        var lotC = CreateLot(itemId, "LOT-C", receivedAt: receivedFirst, expiryDate: new DateOnly(2030, 5, 1));
        _context.Lots.AddRange(lotA, lotB, lotC);
        await _context.SaveChangesAsync(_ct);

        var result = (await _repository.GetByItemIdAsync(itemId, _ct)).ToList();

        Assert.That(
            result.Select(x => x.LotNumber),
            Is.EqualTo(ExpectedOrderedLotNumbers));
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

        var result = await _repository.GetTotalUsableQuantityAsync(itemId, today, _ct);

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

        var result = await _repository.GetTotalUsableQuantityAsync(itemId, today, _ct);

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
                status: LotAdministrativeStatus.Active));
        await _context.SaveChangesAsync(_ct);

        var result = await _repository.GetUsableQuantityByItemIdsAsync([item1, item2, item3], today, _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[item1], Is.EqualTo(50m));
            Assert.That(result[item2], Is.EqualTo(40m));
            Assert.That(result.ContainsKey(item3), Is.False);
        });
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

        var result = await _repository.GetUsableQuantityByItemIdsAsync(
            [Guid.Empty, itemId, itemId, Guid.Empty],
            today,
            _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[itemId], Is.EqualTo(25m));
        });
    }

    [Test]
    public async Task GetUsableQuantityByItemIdsAsync_WhenIdsAreEmpty_ShouldReturnEmptyDictionary()
    {
        var result = await _repository.GetUsableQuantityByItemIdsAsync(
            [Guid.Empty, Guid.Empty],
            new DateOnly(2030, 1, 10),
            _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        });
    }

    [Test]
    public async Task GetAllWithItemAsync_ShouldReturnAllLotsWithItemLoaded()
    {
        var item = await SeedItemAsync();
        var lot = CreateLot(item.Id, "LOT-WITH-ITEM-001");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemAsync(_ct);

        var matched = result.Single(x => x.Id == lot.Id);
        Assert.Multiple(() =>
        {
            Assert.That(matched.Item, Is.Not.Null);
            Assert.That(matched.Item.Id, Is.EqualTo(item.Id));
        });
    }

    [Test]
    public async Task GetAllWithItemAsync_WhenNoLotsExist_ShouldReturnEmptyCollection()
    {
        var result = await _repository.GetAllWithItemAsync(_ct);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        });
    }

    [Test]
    public async Task GetByIdWithItemAsync_WhenLotExists_ShouldReturnLotWithItem()
    {
        var item = await SeedItemAsync();
        var lot = CreateLot(item.Id, "LOT-BY-ID-ITEM");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetByIdWithItemAsync(lot.Id, _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(lot.Id));
            Assert.That(result.Item, Is.Not.Null);
            Assert.That(result.Item.Code, Is.EqualTo(item.Code));
        });
    }

    [Test]
    public async Task GetByIdWithItemAsync_WhenLotDoesNotExist_ShouldReturnNull()
    {
        var result = await _repository.GetByIdWithItemAsync(Guid.NewGuid(), _ct);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdsAsync_ShouldReturnOnlyRequestedLots()
    {
        var item = await SeedItemAsync();
        var lot1 = CreateLot(item.Id, "LOT-IDS-001");
        var lot2 = CreateLot(item.Id, "LOT-IDS-002");
        var lot3 = CreateLot(item.Id, "LOT-IDS-003");
        _context.Lots.AddRange(lot1, lot2, lot3);
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetByIdsAsync([lot1.Id, lot3.Id], _ct);

        Assert.Multiple(() =>
        {
            Assert.That(
                result.Select(x => x.LotNumber),
                Is.EquivalentTo(ExpectedRequestedLotNumbers));
            Assert.That(result, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task GetByIdsAsync_WhenIdsAreEmpty_ShouldReturnEmptyCollection()
    {
        var result = await _repository.GetByIdsAsync([], _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        });
    }

    [Test]
    public async Task GetAllWithItemAsyncPage_ShouldReturnRequestedPageOrderedByExpiryThenLotNumber()
    {
        var item = await SeedItemAsync();
        _context.Lots.AddRange(
            CreateLot(item.Id, "LOT-PAGE-C", expiryDate: new DateOnly(2030, 3, 1)),
            CreateLot(item.Id, "LOT-PAGE-A", expiryDate: new DateOnly(2030, 1, 1)),
            CreateLot(item.Id, "LOT-PAGE-B", expiryDate: new DateOnly(2030, 2, 1)));
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var firstPage = await _repository.GetAllWithItemAsyncPage(
            page: 1,
            pageSize: 2,
            itemId: item.Id,
            ct: _ct);

        var secondPage = await _repository.GetAllWithItemAsyncPage(
            page: 2,
            pageSize: 2,
            itemId: item.Id,
            ct: _ct);

        Assert.Multiple(() =>
        {
            Assert.That(firstPage.TotalCount, Is.EqualTo(3));
            Assert.That(firstPage.Page, Is.EqualTo(1));
            Assert.That(firstPage.PageSize, Is.EqualTo(2));
            Assert.That(
                firstPage.Items.Select(x => x.LotNumber),
                Is.EqualTo(ExpectedFirstPageLotNumbers));
            Assert.That(
                secondPage.Items.Select(x => x.LotNumber),
                Is.EqualTo(ExpectedSecondPageLotNumbers));
        });
    }

    [Test]
    public async Task GetAllWithItemAsyncPage_ShouldFilterByItemId()
    {
        var item = await SeedItemAsync();
        var otherItem = await SeedItemAsync();
        _context.Lots.AddRange(
            CreateLot(item.Id, "LOT-FILTER-MINE"),
            CreateLot(otherItem.Id, "LOT-FILTER-OTHER"));
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemAsyncPage(
            page: 1,
            pageSize: 20,
            itemId: item.Id,
            ct: _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.Single().LotNumber, Is.EqualTo("LOT-FILTER-MINE"));
        });
    }

    [Test]
    public async Task GetAllWithItemAsyncPage_WhenSearchMatchesLotNumber_ShouldReturnLot()
    {
        var item = await SeedItemAsync();
        var lot = CreateLot(item.Id, "SEQ-101");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemAsyncPage(
            page: 1,
            pageSize: 20,
            search: "SEQ-101",
            ct: _ct);

        Assert.That(
            result.Items.Select(x => x.LotNumber),
            Is.EqualTo(ExpectedSearchByLotNumber));
    }

    [Test]
    public async Task GetAllWithItemAsyncPage_WhenSearchMatchesItemName_ShouldReturnLot()
    {
        var item = await SeedItemAsync(
            code: $"TEST-{Guid.NewGuid():N}",
            name: "Searchable Item");
        var lot = CreateLot(item.Id, "LOT-SEARCH-901");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemAsyncPage(
            page: 1,
            pageSize: 20,
            search: "Searchable Item",
            ct: _ct);

        Assert.That(
            result.Items.Select(x => x.LotNumber),
            Is.EqualTo(ExpectedSearchByName));
    }

    [Test]
    public async Task GetAllWithItemAsyncPage_WhenSearchMatchesItemCode_ShouldReturnLot()
    {
        var item = await SeedItemAsync(
            code: "SRCH-CODE",
            name: $"Test Item {Guid.NewGuid():N}");
        var otherItem = await SeedItemAsync();
        _context.Lots.AddRange(
            CreateLot(item.Id, "LOT-SEARCH-CODE"),
            CreateLot(otherItem.Id, "LOT-SEARCH-NO-MATCH"));
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemAsyncPage(
            page: 1,
            pageSize: 20,
            search: "SRCH-CODE",
            ct: _ct);

        Assert.That(
            result.Items.Select(x => x.LotNumber),
            Is.EqualTo(ExpectedSearchByCode));
    }

    [Test]
    public async Task GetAllWithItemAsyncPage_ShouldFilterByAdministrativeStatus()
    {
        var item = await SeedItemAsync();
        _context.Lots.AddRange(
            CreateLot(item.Id, "LOT-STATUS-ACTIVE", status: LotAdministrativeStatus.Active),
            CreateLot(item.Id, "LOT-STATUS-QUARANTINED", status: LotAdministrativeStatus.Quarantined));
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemAsyncPage(
            page: 1,
            pageSize: 20,
            itemId: item.Id,
            administrativeStatus: nameof(LotAdministrativeStatus.Quarantined),
            ct: _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.Single().LotNumber, Is.EqualTo("LOT-STATUS-QUARANTINED"));
        });
    }

    [Test]
    public async Task GetAllWithItemAsyncPage_WhenAdministrativeStatusIsUnknown_ShouldIgnoreFilter()
    {
        var item = await SeedItemAsync();
        _context.Lots.Add(CreateLot(item.Id, "LOT-STATUS-UNKNOWN"));
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemAsyncPage(
            page: 1,
            pageSize: 20,
            itemId: item.Id,
            administrativeStatus: "NotARealStatus",
            ct: _ct);

        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllWithItemAsyncPage_WhenExpiryConditionIsExpired_ShouldReturnOnlyPastExpiry()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var item = await SeedItemAsync(expiringSoonDays: 30);
        _context.Lots.AddRange(
            CreateLot(item.Id, "LOT-EXP-CONDITION", expiryDate: today.AddDays(-1)),
            CreateLot(item.Id, "LOT-VALID-CONDITION", expiryDate: today.AddDays(60)));
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemAsyncPage(
            page: 1,
            pageSize: 20,
            itemId: item.Id,
            expiryCondition: nameof(ExpiryCondition.Expired),
            ct: _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.Single().LotNumber, Is.EqualTo("LOT-EXP-CONDITION"));
        });
    }

    [Test]
    public async Task GetAllWithItemAsyncPage_WhenExpiryConditionIsExpiringSoon_ShouldReturnOnlyWithinWarningWindow()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var item = await SeedItemAsync(expiringSoonDays: 30);
        _context.Lots.AddRange(
            CreateLot(item.Id, "LOT-SOON-CONDITION", expiryDate: today.AddDays(10)),
            CreateLot(item.Id, "LOT-FAR-CONDITION", expiryDate: today.AddDays(90)));
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemAsyncPage(
            page: 1,
            pageSize: 20,
            itemId: item.Id,
            expiryCondition: nameof(ExpiryCondition.ExpiringSoon),
            ct: _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.Single().LotNumber, Is.EqualTo("LOT-SOON-CONDITION"));
        });
    }

    [Test]
    public async Task GetAllWithItemAsyncPage_WhenExpiryConditionIsValid_ShouldReturnOnlyBeyondWarningWindow()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var item = await SeedItemAsync(expiringSoonDays: 30);
        _context.Lots.AddRange(
            CreateLot(item.Id, "LOT-SOON-VALID", expiryDate: today.AddDays(10)),
            CreateLot(item.Id, "LOT-VALID-VALID", expiryDate: today.AddDays(90)));
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemAsyncPage(
            page: 1,
            pageSize: 20,
            itemId: item.Id,
            expiryCondition: nameof(ExpiryCondition.Valid),
            ct: _ct);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.Single().LotNumber, Is.EqualTo("LOT-VALID-VALID"));
        });
    }

    [Test]
    public async Task GetAllWithItemAsyncPage_WhenExpiryConditionIsUnknown_ShouldIgnoreFilter()
    {
        var item = await SeedItemAsync();
        _context.Lots.Add(CreateLot(item.Id, "LOT-CONDITION-UNKNOWN"));
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemAsyncPage(
            page: 1,
            pageSize: 20,
            itemId: item.Id,
            expiryCondition: "NotARealCondition",
            ct: _ct);

        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void GetAllWithItemAsyncPage_WhenPageIsBelowOne_ShouldThrow(int page)
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await _repository.GetAllWithItemAsyncPage(page: page, pageSize: 20, ct: _ct);
        });
    }

    [TestCase(0)]
    [TestCase(101)]
    public void GetAllWithItemAsyncPage_WhenPageSizeIsOutOfRange_ShouldThrow(int pageSize)
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await _repository.GetAllWithItemAsyncPage(page: 1, pageSize: pageSize, ct: _ct);
        });
    }

    [Test]
    public async Task GetAllWithItemByExpiryConditionAsync_WhenExpired_ShouldReturnLotsPastEvaluationDate()
    {
        var item = await SeedItemAsync(expiringSoonDays: 30);
        var evaluationDate = new DateOnly(2030, 6, 1);
        _context.Lots.AddRange(
            CreateLot(item.Id, "LOT-EVAL-EXPIRED", expiryDate: evaluationDate),
            CreateLot(item.Id, "LOT-EVAL-VALID", expiryDate: evaluationDate.AddDays(60)));
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemByExpiryConditionAsync(
            ExpiryCondition.Expired,
            evaluationDate,
            _ct);

        var expired = result.Single();
        Assert.Multiple(() =>
        {
            Assert.That(expired.LotNumber, Is.EqualTo("LOT-EVAL-EXPIRED"));
            Assert.That(expired.Item, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetAllWithItemByExpiryConditionAsync_WhenExpiringSoon_ShouldRespectItemWarningDays()
    {
        var item = await SeedItemAsync(expiringSoonDays: 30);
        var evaluationDate = new DateOnly(2030, 6, 1);
        _context.Lots.AddRange(
            CreateLot(item.Id, "LOT-EVAL-SOON", expiryDate: evaluationDate.AddDays(10)),
            CreateLot(item.Id, "LOT-EVAL-BEYOND", expiryDate: evaluationDate.AddDays(90)));
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemByExpiryConditionAsync(
            ExpiryCondition.ExpiringSoon,
            evaluationDate,
            _ct);

        Assert.That(result.Single().LotNumber, Is.EqualTo("LOT-EVAL-SOON"));
    }

    [Test]
    public async Task GetAllWithItemByExpiryConditionAsync_WhenValid_ShouldReturnLotsBeyondWarningWindow()
    {
        var item = await SeedItemAsync(expiringSoonDays: 30);
        var evaluationDate = new DateOnly(2030, 6, 1);
        _context.Lots.AddRange(
            CreateLot(item.Id, "LOT-EVAL-SOON2", expiryDate: evaluationDate.AddDays(10)),
            CreateLot(item.Id, "LOT-EVAL-VALID2", expiryDate: evaluationDate.AddDays(90)));
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllWithItemByExpiryConditionAsync(
            ExpiryCondition.Valid,
            evaluationDate,
            _ct);

        Assert.That(result.Single().LotNumber, Is.EqualTo("LOT-EVAL-VALID2"));
    }

    [Test]
    public void GetAllWithItemByExpiryConditionAsync_WhenConditionIsUndefined_ShouldThrow()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await _repository.GetAllWithItemByExpiryConditionAsync(
                (ExpiryCondition)999,
                new DateOnly(2030, 6, 1),
                _ct);
        });
    }

    private async Task<Guid> CreateItemAsync()
    {
        var item = await SeedItemAsync();
        return item.Id;
    }

    private async Task<Item> SeedItemAsync(int expiringSoonDays = 30)
    {
        return await SeedItemAsync(
            code: $"TEST-{Guid.NewGuid():N}",
            name: $"Test Item {Guid.NewGuid():N}",
            expiringSoonDays: expiringSoonDays);
    }

    private async Task<Item> SeedItemAsync(string code, string name, int expiringSoonDays = 30)
    {
        var item = new Item(
            Guid.NewGuid(),
            code,
            name,
            UnitOfMeasure.Unit,
            10m,
            expiringSoonDays);
        _context.Items.Add(item);
        await _context.SaveChangesAsync(_ct);
        return item;
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
            receivedAt ?? new DateTimeOffset(2030, 1, 1, 8, 0, 0, TimeSpan.Zero),
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
                x.LotNumber.StartsWith("ITEM3-") ||
                x.LotNumber.StartsWith("SEQ-"))
            .ToListAsync(_ct);

        if (testLots.Count > 0)
        {
            await DeleteDependentsForLots([.. testLots.Select(x => x.Id)]);

            _context.Lots.RemoveRange(testLots);
            await _context.SaveChangesAsync(_ct);
        }

        var testItems = await _context.Items
            .Where(x => x.Code.StartsWith("TEST-") || x.Code.StartsWith("SRCH-"))
            .ToListAsync(_ct);

        if (testItems.Count > 0)
        {
            _context.Items.RemoveRange(testItems);
            await _context.SaveChangesAsync(_ct);
        }

        _context.ChangeTracker.Clear();
    }

    private async Task DeleteDependentsForLots(IReadOnlyCollection<Guid> lotIds)
    {
        var consumptions = await _context.Consumptions
            .Where(c => lotIds.Contains(c.LotId))
            .ToListAsync(_ct);
        if (consumptions.Count > 0)
        {
            _context.Consumptions.RemoveRange(consumptions);
            await _context.SaveChangesAsync(_ct);
        }

        var disposals = await _context.Disposals
            .Where(d => lotIds.Contains(d.LotId))
            .ToListAsync(_ct);
        if (disposals.Count > 0)
        {
            _context.Disposals.RemoveRange(disposals);
            await _context.SaveChangesAsync(_ct);
        }
    }
}
