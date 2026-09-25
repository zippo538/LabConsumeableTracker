using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Tests.Repository;

[TestFixture]
public class DashboardAlertRepositoryTests
{
    private static string _connectionString = null!;
    private AppDbContext _context = null!;
    private DashboardAlertRepository _repository = null!;

    private readonly List<Guid> _seededLotIds = [];
    private readonly List<Guid> _seededItemIds = [];

    // Waktu tetap untuk menghindari masalah presisi tick PostgreSQL
    private readonly DateTimeOffset _evaluatedAt = new(2023, 10, 27, 10, 0, 0, TimeSpan.Zero);
    private DateOnly _today;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _connectionString = Connection.GetConnection("UnitTestConnection");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;

        _context = new AppDbContext(options);
        _repository = new DashboardAlertRepository(_context);
        await _context.Database.CanConnectAsync();

        _today = DateOnly.FromDateTime(_evaluatedAt.UtcDateTime);
    }

    [SetUp]
    public async Task SetUp()
    {
        await CleanupTestData();
        _context.ChangeTracker.Clear();
    }

    private async Task CleanupTestData()
    {
        var allLots = await _context.Lots.ToListAsync();
        if (allLots.Count > 0)
        {
            _context.Lots.RemoveRange(allLots);
            await _context.SaveChangesAsync();
        }
        var allItems = await _context.Items.ToListAsync();
        if (allItems.Count > 0)
        {
            _context.Items.RemoveRange(allItems);
            await _context.SaveChangesAsync();
        }
        _seededLotIds.Clear();
        _seededItemIds.Clear();
    }

    [TearDown]
    public async Task TearDown()
    {
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

    // --- HELPERS ---
    private async Task<Item> SeedItemAsync(string code, decimal minStock, int expiringSoonDays)
    {
        var item = new Item(Guid.NewGuid(), code, $"Item {code}", UnitOfMeasure.Milliliter, minStock, expiringSoonDays);
        _context.Items.Add(item);
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
            receivedAt: _evaluatedAt,
            supplierName: null,
            initialQuantity: initialQty,
            remainingQuantity: remainingQty,
            expiryDate: expiryDate,
            storageLocation: "Shelf-A",
            administrativeStatus: status);

        _context.Lots.Add(lot);
        await _context.SaveChangesAsync();
        _seededLotIds.Add(lot.Id);
    }

    // --- TESTS ---
    [Test]
    public async Task GetAlertsAsync_ReturnsLowStock_WhenUsableStockBelowMinimum()
    {
        // MinimumStock = 100. Usable stock will be 50.
        var item = await SeedItemAsync("ITM-LOW", 100m, 30);
        await SeedLotAsync(item.Id, "LOT-LOW-1", 100m, 50m, _today.AddDays(10), LotAdministrativeStatus.Active);

        var query = new DashboardAlertQuery { Type = DashboardAlertType.LowStock, Page = 1, PageSize = 10 };
        var result = await _repository.GetAlertsAsync(query, _evaluatedAt);

        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items[0].AlertType, Is.EqualTo(DashboardAlertType.LowStock));
        Assert.That(result.Items[0].ItemId, Is.EqualTo(item.Id));
    }

    [Test]
    public async Task GetAlertsAsync_ReturnsExpiringSoon_WhenWithinWarningWindow()
    {
        // ExpiringSoonDays = 10. Lot expires in 5 days.
        var item = await SeedItemAsync("ITM-SOON", 10m, 10);
        await SeedLotAsync(item.Id, "LOT-SOON-1", 100m, 100m, _today.AddDays(5), LotAdministrativeStatus.Active);

        var query = new DashboardAlertQuery { Type = DashboardAlertType.ExpiringSoon, Page = 1, PageSize = 10 };
        var result = await _repository.GetAlertsAsync(query, _evaluatedAt);

        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items[0].AlertType, Is.EqualTo(DashboardAlertType.ExpiringSoon));
    }

    [Test]
    public async Task GetAlertsAsync_ReturnsExpired_WhenExpiryDateBeforeToday()
    {
        var item = await SeedItemAsync("ITM-EXP", 10m, 30);
        await SeedLotAsync(item.Id, "LOT-EXP-1", 100m, 100m, _today.AddDays(-1), LotAdministrativeStatus.Active);

        var query = new DashboardAlertQuery { Type = DashboardAlertType.Expired, Page = 1, PageSize = 10 };
        var result = await _repository.GetAlertsAsync(query, _evaluatedAt);

        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items[0].AlertType, Is.EqualTo(DashboardAlertType.Expired));
    }

    [Test]
    public async Task GetAlertsAsync_ReturnsBlocked_WhenStatusIsBlockedOrQuarantined()
    {
        var item = await SeedItemAsync("ITM-BLK", 10m, 30);
        await SeedLotAsync(item.Id, "LOT-BLK-1", 100m, 100m, _today.AddDays(30), LotAdministrativeStatus.ManuallyBlocked);
        await SeedLotAsync(item.Id, "LOT-QR-1", 100m, 100m, _today.AddDays(30), LotAdministrativeStatus.Quarantined);

        var query = new DashboardAlertQuery { Type = DashboardAlertType.Blocked, Page = 1, PageSize = 10 };
        var result = await _repository.GetAlertsAsync(query, _evaluatedAt);

        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Items.All(a => a.AlertType == DashboardAlertType.Blocked), Is.True);
    }

    [Test]
    public async Task GetAlertsAsync_FiltersByItemId()
    {
        var item1 = await SeedItemAsync("ITM-F1", 100m, 30);
        var item2 = await SeedItemAsync("ITM-F2", 100m, 30);

        await SeedLotAsync(item1.Id, "LOT-F1-1", 100m, 50m, _today.AddDays(10), LotAdministrativeStatus.Active); // LowStock
        await SeedLotAsync(item2.Id, "LOT-F2-1", 100m, 50m, _today.AddDays(10), LotAdministrativeStatus.Active); // LowStock

        var query = new DashboardAlertQuery { Type = DashboardAlertType.LowStock, ItemId = item1.Id, Page = 1, PageSize = 10 };
        var result = await _repository.GetAlertsAsync(query, _evaluatedAt);

        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items[0].ItemId, Is.EqualTo(item1.Id));
    }

    [Test]
    public async Task GetAlertsAsync_CombinesAll_WhenTypeIsNull()
    {
        var item = await SeedItemAsync("ITM-ALL", 100m, 10);
        await SeedLotAsync(item.Id, "LOT-ALL-LOW", 100m, 50m, _today.AddDays(20), LotAdministrativeStatus.Active);
        await SeedLotAsync(item.Id, "LOT-ALL-SOON", 100m, 100m, _today.AddDays(5), LotAdministrativeStatus.Active);
        await SeedLotAsync(item.Id, "LOT-ALL-EXP", 100m, 100m, _today.AddDays(-1), LotAdministrativeStatus.Active);
        await SeedLotAsync(item.Id, "LOT-ALL-BLK", 100m, 100m, _today.AddDays(30), LotAdministrativeStatus.ManuallyBlocked);
        var query = new DashboardAlertQuery { Type = null, Page = 1, PageSize = 10 };

        var result = await _repository.GetAlertsAsync(query, _evaluatedAt);

        Console.WriteLine($"TotalCount: {result.TotalCount}");
        foreach (var alert in result.Items)
        {
            Console.WriteLine($"Type: {alert.AlertType}, Resource: {alert.ResourceId}");
        }
        Assert.That(result.TotalCount, Is.EqualTo(3));
        Assert.That(result.Items.Count, Is.EqualTo(3));
    }
}