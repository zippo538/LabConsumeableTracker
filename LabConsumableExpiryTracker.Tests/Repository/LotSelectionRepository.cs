using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
namespace LabConsumableExpiryTracker.Tests.Repository;
[TestFixture]
public class LotSelectionRepositoryTests
{
    private static string _connectionString = null!;
    private AppDbContext _context = null!;
    private LotSelectionRepository _repository = null!;
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
        _repository = new LotSelectionRepository(_context);
        await _context.Database.CanConnectAsync();
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
    private async Task<Item> SeedItemAsync()
    {
        var item = new Item(Guid.NewGuid(), $"ITM-{Guid.NewGuid():N}"[..15], $"Item {Guid.NewGuid():N}", UnitOfMeasure.Milliliter, 10m, 30);
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        _seededItemIds.Add(item.Id);
        return item;
    }
    private async Task<Lot> SeedLotAsync(Guid itemId, DateOnly expiryDate, LotAdministrativeStatus status, decimal remainingQty, DateTimeOffset receivedAt)
    {
        var lot = new Lot(
            Guid.NewGuid(), itemId, $"LOT-{Guid.NewGuid():N}"[..20], null, receivedAt, null,
            100m, remainingQty, expiryDate, "Shelf-A", status);
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync();
        _seededLotIds.Add(lot.Id);
        return lot;
    }
    [Test]
    public async Task GetAvailableLotsAsync_ReturnsOnlyEligibleLots()
    {
        var item = await SeedItemAsync();
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var eligibleLot = await SeedLotAsync(item.Id, today.AddDays(10), LotAdministrativeStatus.Active, 50m, now.AddDays(-5));
        await SeedLotAsync(item.Id, today.AddDays(-1), LotAdministrativeStatus.Active, 50m, now.AddDays(-5));
        await SeedLotAsync(item.Id, today.AddDays(10), LotAdministrativeStatus.ManuallyBlocked, 50m, now.AddDays(-5));
        await SeedLotAsync(item.Id, today.AddDays(10), LotAdministrativeStatus.Active, 0m, now.AddDays(-5));
        var result = await _repository.GetAvailableLotsAsync(item.Id, now, CancellationToken.None);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(eligibleLot.Id));
    }
    [Test]
    public async Task GetAvailableLotsAsync_OrdersByExpiry_Received_Id()
    {
        var item = await SeedItemAsync();
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var lot1 = await SeedLotAsync(item.Id, today.AddDays(5), LotAdministrativeStatus.Active, 50m, now.AddDays(-10));
        var lot2 = await SeedLotAsync(item.Id, today.AddDays(10), LotAdministrativeStatus.Active, 50m, now.AddDays(-5));
        var lot3 = await SeedLotAsync(item.Id, today.AddDays(5), LotAdministrativeStatus.Active, 50m, now.AddDays(-2));
        var result = await _repository.GetAvailableLotsAsync(item.Id, now, CancellationToken.None);
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].Id, Is.EqualTo(lot1.Id)); 
        Assert.That(result[1].Id, Is.EqualTo(lot3.Id)); 
        Assert.That(result[2].Id, Is.EqualTo(lot2.Id)); 
    }
    [Test]
    public async Task GetAvailableLotsAsync_ReturnsEmpty_WhenNoEligibleLots()
    {
        var item = await SeedItemAsync();
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        await SeedLotAsync(item.Id, today.AddDays(-1), LotAdministrativeStatus.Active, 50m, now);
        var result = await _repository.GetAvailableLotsAsync(item.Id, now, CancellationToken.None);
        Assert.That(result, Is.Empty);
    }
}