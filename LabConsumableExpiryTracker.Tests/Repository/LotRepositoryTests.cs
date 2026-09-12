using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Tests.Repository;

[TestFixture]
public class LotRepositoryTests
{
    private AppDbContext _context = null!;
    private LotRepository _repository = null!;

    private CancellationToken _ct;

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("CONNECTION_STRING");

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        DotEnv.Load(
            FindDotEnvFile());

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .EnableSensitiveDataLogging()
            .Options;

        _context = new AppDbContext(options);
        _repository = new LotRepository(_context, NullLogger<LotRepository>.Instance);

        await _context.Database.CanConnectAsync();

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

    // =========================================================
    // GetAllAsync
    // =========================================================

    [Test]
    public async Task GetAllAsync_WhenLotsExist_ShouldReturnAllLots()
    {
        // Arrange
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

        // Act
        var result = await _repository.GetAllAsync(_ct);

        // Assert
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
        // Act
        var result = await _repository.GetAllAsync(_ct);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    // =========================================================
    // GetByIdAsync
    // =========================================================

    [Test]
    public async Task GetByIdAsync_WhenLotExists_ShouldReturnLot()
    {
        // Arrange
        var itemId = await CreateItemAsync();

        var lot = CreateLot(
            itemId,
            "LOT-GET-BY-ID");

        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);

        // Clear tracking so repository query is tested
        // against the database rather than an already tracked entity.
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(
            lot.Id,
            _ct);

        // Assert
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
        // Arrange
        var lotId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(
            lotId,
            _ct);

        // Assert
        Assert.That(result, Is.Null);
    }

    // =========================================================
    // AddAsync
    // =========================================================

    [Test]
    public async Task AddAsync_ShouldAddLotToDbSet()
    {
        // Arrange
        var itemId = await CreateItemAsync();

        var lot = CreateLot(
            itemId,
            "LOT-ADD-001");

        // Act
        var result = await _repository.AddAsync(
            lot,
            _ct);

        await _context.SaveChangesAsync(_ct);

        // Assert
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

    // =========================================================
    // UpdateAsync
    // =========================================================

    [Test]
    public async Task UpdateAsync_ShouldPersistUpdatedLot()
    {
        // Arrange
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

        // Act
        var result = await _repository.UpdateAsync(
            existing,
            _ct);

        await _context.SaveChangesAsync(_ct);

        // Assert
        Assert.That(result, Is.SameAs(existing));

        _context.ChangeTracker.Clear();

        var updated = await _context.Lots
            .AsNoTracking()
            .SingleAsync(x => x.Id == lot.Id, _ct);

        Assert.That(
            updated.StorageLocation,
            Is.EqualTo("RACK-B-02"));
    }

    // =========================================================
    // DeleteAsync
    // =========================================================

    [Test]
    public async Task DeleteAsync_WhenLotExists_ShouldRemoveLot()
    {
        // Arrange
        var itemId = await CreateItemAsync();

        var lot = CreateLot(
            itemId,
            "LOT-DELETE-001");

        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);

        // Act
        var result = await _repository.DeleteAsync(
            lot.Id,
            _ct);

        await _context.SaveChangesAsync(_ct);

        // Assert
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
        // Arrange
        var lotId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(
            lotId,
            _ct);

        // Assert
        Assert.That(result, Is.False);
    }

    // =========================================================
    // ExistsAsync
    // =========================================================

    [Test]
    public async Task ExistsAsync_WhenMatchingLotExists_ShouldReturnTrue()
    {
        // Arrange
        var itemId = await CreateItemAsync();

        var expiryDate = new DateOnly(2030, 12, 31);

        var lot = CreateLot(
            itemId,
            "LOT-EXISTS-001",
            expiryDate: expiryDate);

        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);

        // Act
        var result = await _repository.ExistsAsync(
            itemId,
            "LOT-EXISTS-001",
            expiryDate,
            _ct);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExistsAsync_WhenLotNumberHasWhitespace_ShouldTrimLotNumber()
    {
        // Arrange
        var itemId = await CreateItemAsync();

        var expiryDate = new DateOnly(2030, 12, 31);

        var lot = CreateLot(
            itemId,
            "LOT-TRIM-001",
            expiryDate: expiryDate);

        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);

        // Act
        var result = await _repository.ExistsAsync(
            itemId,
            "   LOT-TRIM-001   ",
            expiryDate,
            _ct);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExistsAsync_WhenItemIsDifferent_ShouldReturnFalse()
    {
        // Arrange
        var itemId = await CreateItemAsync();
        var differentItemId = await CreateItemAsync();

        var expiryDate = new DateOnly(2030, 12, 31);

        var lot = CreateLot(
            itemId,
            "LOT-EXISTS-002",
            expiryDate: expiryDate);

        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);

        // Act
        var result = await _repository.ExistsAsync(
            differentItemId,
            "LOT-EXISTS-002",
            expiryDate,
            _ct);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ExistsAsync_WhenExpiryDateIsDifferent_ShouldReturnFalse()
    {
        // Arrange
        var itemId = await CreateItemAsync();

        var actualExpiryDate = new DateOnly(2030, 12, 31);
        var differentExpiryDate = new DateOnly(2031, 12, 31);

        var lot = CreateLot(
            itemId,
            "LOT-EXISTS-003",
            expiryDate: actualExpiryDate);

        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);

        // Act
        var result = await _repository.ExistsAsync(
            itemId,
            "LOT-EXISTS-003",
            differentExpiryDate,
            _ct);

        // Assert
        Assert.That(result, Is.False);
    }

    // =========================================================
    // GetByItemIdAsync
    // =========================================================

    [Test]
    public async Task GetByItemIdAsync_ShouldReturnOnlyLotsForRequestedItem()
    {
        // Arrange
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

        // Act
        var result = (
            await _repository.GetByItemIdAsync(
                itemId,
                _ct))
            .ToList();

        // Assert
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
        // Arrange
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

        // Act
        var result = (
            await _repository.GetByItemIdAsync(
                itemId,
                _ct))
            .ToList();

        // Assert
        Assert.That(
            result.Select(x => x.LotNumber),
            Is.EqualTo(new[]
            {
                "LOT-C",
                "LOT-B",
                "LOT-A"
            }));
    }

    // =========================================================
    // GetTotalUsableQuantityAsync
    // =========================================================

    [Test]
    public async Task GetTotalUsableQuantityAsync_ShouldSumEligibleActiveLots()
    {
        // Arrange
        var itemId = await CreateItemAsync();

        var today = new DateOnly(2030, 1, 10);

        var activeUsableLot1 = CreateLot(
            itemId,
            "LOT-USABLE-001",
            initialQuantity: 100m,
            remainingQuantity: 40m,
            expiryDate: new DateOnly(2030, 1, 20),
            status: LotStatus.Active);

        var activeUsableLot2 = CreateLot(
            itemId,
            "LOT-USABLE-002",
            initialQuantity: 100m,
            remainingQuantity: 30m,
            expiryDate: today,
            status: LotStatus.Active);

        var expiredLot = CreateLot(
            itemId,
            "LOT-EXPIRED",
            initialQuantity: 100m,
            remainingQuantity: 50m,
            expiryDate: new DateOnly(2030, 1, 9),
            status: LotStatus.Active);

        var zeroQuantityLot = CreateLot(
            itemId,
            "LOT-ZERO",
            initialQuantity: 100m,
            remainingQuantity: 0m,
            expiryDate: new DateOnly(2030, 1, 20),
            status: LotStatus.Active);

        var inactiveLot = CreateLot(
            itemId,
            "LOT-INACTIVE",
            initialQuantity: 100m,
            remainingQuantity: 80m,
            expiryDate: new DateOnly(2030, 1, 20),
            status: LotStatus.Disposed);

        _context.Lots.AddRange(
            activeUsableLot1,
            activeUsableLot2,
            expiredLot,
            zeroQuantityLot,
            inactiveLot);

        await _context.SaveChangesAsync(_ct);

        // Act
        var result =
            await _repository.GetTotalUsableQuantityAsync(
                itemId,
                today,
                _ct);

        // Assert
        Assert.That(result, Is.EqualTo(70m));
    }

    [Test]
    public async Task GetTotalUsableQuantityAsync_WhenNoUsableLotExists_ShouldReturnZero()
    {
        // Arrange
        var itemId = await CreateItemAsync();

        var today = new DateOnly(2030, 1, 10);

        var expiredLot = CreateLot(
            itemId,
            "LOT-NO-USABLE",
            initialQuantity: 100m,
            remainingQuantity: 50m,
            expiryDate: new DateOnly(2030, 1, 9),
            status: LotStatus.Active);

        _context.Lots.Add(expiredLot);

        await _context.SaveChangesAsync(_ct);

        // Act
        var result =
            await _repository.GetTotalUsableQuantityAsync(
                itemId,
                today,
                _ct);

        // Assert
        Assert.That(result, Is.EqualTo(0m));
    }

    // =========================================================
    // GetUsableQuantityByItemIdsAsync
    // =========================================================

    [Test]
    public async Task GetUsableQuantityByItemIdsAsync_ShouldGroupUsableQuantityByItem()
    {
        // Arrange
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
                status: LotStatus.Active),

            CreateLot(
                item1,
                "ITEM1-LOT2",
                initialQuantity: 100m,
                remainingQuantity: 30m,
                expiryDate: new DateOnly(2030, 2, 1),
                status: LotStatus.Active),

            CreateLot(
                item1,
                "ITEM1-EXPIRED",
                initialQuantity: 100m,
                remainingQuantity: 50m,
                expiryDate: new DateOnly(2030, 1, 9),
                status: LotStatus.Active),

            CreateLot(
                item2,
                "ITEM2-LOT1",
                initialQuantity: 100m,
                remainingQuantity: 40m,
                expiryDate: new DateOnly(2030, 1, 20),
                status: LotStatus.Active),

            CreateLot(
                item2,
                "ITEM2-DISPOSED",
                initialQuantity: 100m,
                remainingQuantity: 60m,
                expiryDate: new DateOnly(2030, 1, 20),
                status: LotStatus.Disposed),

            CreateLot(
                item3,
                "ITEM3-ZERO",
                initialQuantity: 100m,
                remainingQuantity: 0m,
                expiryDate: new DateOnly(2030, 1, 20),
                status: LotStatus.Active)
        );

        await _context.SaveChangesAsync(_ct);

        // Act
        var result =
            await _repository.GetUsableQuantityByItemIdsAsync(
                new[]
                {
                    item1,
                    item2,
                    item3
                },
                today,
                _ct);

        // Assert
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
        // Arrange
        var itemId = await CreateItemAsync();

        var today = new DateOnly(2030, 1, 10);

        var lot = CreateLot(
            itemId,
            "LOT-DISTINCT",
            initialQuantity: 100m,
            remainingQuantity: 25m,
            expiryDate: new DateOnly(2030, 1, 20),
            status: LotStatus.Active);

        _context.Lots.Add(lot);
        await _context.SaveChangesAsync(_ct);

        // Act
        var result =
            await _repository.GetUsableQuantityByItemIdsAsync(
                new[]
                {
                    Guid.Empty,
                    itemId,
                    itemId,
                    Guid.Empty
                },
                today,
                _ct);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(
            result[itemId],
            Is.EqualTo(25m));
    }

    [Test]
    public async Task GetUsableQuantityByItemIdsAsync_WhenIdsAreEmpty_ShouldReturnEmptyDictionary()
    {
        // Act
        var result =
            await _repository.GetUsableQuantityByItemIdsAsync(
                new[]
                {
                    Guid.Empty,
                    Guid.Empty
                },
                new DateOnly(2030, 1, 10),
                _ct);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    // =========================================================
    // Helpers
    // =========================================================

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
        LotStatus status = LotStatus.Active)
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
        // Hanya menghapus data dengan pola TEST yang dibuat oleh test.
        // Lot dihapus lebih dahulu karena memiliki relasi ke Item.
        var testLots = await _context.Lots
            .Where(x =>
                x.LotNumber.StartsWith("LOT-") ||
                x.LotNumber.StartsWith("ITEM1-") ||
                x.LotNumber.StartsWith("ITEM2-") ||
                x.LotNumber.StartsWith("ITEM3-"))
            .ToListAsync();

        if (testLots.Count > 0)
        {
            _context.Lots.RemoveRange(testLots);
            await _context.SaveChangesAsync(_ct);
        }

        var itemIds = await _context.Items
            .Where(x => x.Code.StartsWith("TEST-"))
            .Select(x => x.Id)
            .ToListAsync();

        if (itemIds.Count > 0)
        {
            var items = await _context.Items
                .Where(x => itemIds.Contains(x.Id))
                .ToListAsync();

            _context.Items.RemoveRange(items);
            await _context.SaveChangesAsync(_ct);
        }

        _context.ChangeTracker.Clear();
    }

    private static string FindDotEnvFile()
    {
        var directory = new DirectoryInfo(
            TestContext.CurrentContext.TestDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                ".env");

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            "File .env tidak ditemukan dari TestDirectory sampai root project.");
    }
}


// =============================================================
// Simple .env loader
// Tidak membutuhkan package tambahan.
// =============================================================

internal static class DotEnv
{
    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "File .env tidak ditemukan.",
                filePath);
        }

        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("#"))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');

            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 &&
                value.StartsWith('"') &&
                value.EndsWith('"'))
            {
                value = value[1..^1];
            }

            if (value.Length >= 2 &&
                value.StartsWith('\'') &&
                value.EndsWith('\''))
            {
                value = value[1..^1];
            }

            Environment.SetEnvironmentVariable(
                key,
                value);
        }
    }
}

