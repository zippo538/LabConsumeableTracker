using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
namespace LabConsumableExpiryTracker.Tests.Repository;
[TestFixture]
public class AuditRepositoryTests
{
    private static string _connectionString = null!;
    private AppDbContext _context = null!;
    private AuditRepository _repository = null!;
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededItemIds = [];
    private readonly List<Guid> _seededLotIds = [];
    private readonly List<Guid> _seededJobIds = [];
    private readonly List<Guid> _seededConsumptionIds = [];
    private readonly List<Guid> _seededDisposalIds = [];
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _connectionString = Connection.GetConnection("UnitTestConnection");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;
        _context = new AppDbContext(options);
        _repository = new AuditRepository(_context);
        await _context.Database.CanConnectAsync();
    }
    [TearDown]
    public async Task TearDown()
    {
        async Task Remove<T>(List<Guid> ids) where T : class
        {
            if (ids.Count > 0)
            {
                if (typeof(T) == typeof(Consumption)) _context.Consumptions.RemoveRange(await _context.Consumptions.Where(c => _seededConsumptionIds.Contains(c.Id)).ToListAsync());
                else if (typeof(T) == typeof(Disposal)) _context.Disposals.RemoveRange(await _context.Disposals.Where(d => _seededDisposalIds.Contains(d.Id)).ToListAsync());
                else if (typeof(T) == typeof(Job)) _context.Jobs.RemoveRange(await _context.Jobs.Where(j => _seededJobIds.Contains(j.Id)).ToListAsync());
                else if (typeof(T) == typeof(Lot)) _context.Lots.RemoveRange(await _context.Lots.Where(l => _seededLotIds.Contains(l.Id)).ToListAsync());
                else if (typeof(T) == typeof(Item)) _context.Items.RemoveRange(await _context.Items.Where(i => _seededItemIds.Contains(i.Id)).ToListAsync());
                else if (typeof(T) == typeof(ApplicationUser)) _context.Users.RemoveRange(await _context.Users.Where(u => _seededUserIds.Contains(u.Id)).ToListAsync());
                await _context.SaveChangesAsync();
                ids.Clear();
            }
        }
        await Remove<Consumption>(_seededConsumptionIds);
        await Remove<Disposal>(_seededDisposalIds);
        await Remove<Job>(_seededJobIds);
        await Remove<Lot>(_seededLotIds);
        await Remove<Item>(_seededItemIds);
        await Remove<ApplicationUser>(_seededUserIds);
    }
    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _context.DisposeAsync();
    private async Task<ApplicationUser> SeedUserAsync(string fullName = "Test Scientist")
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = $"u_{Guid.NewGuid():N}", Email = $"u_{Guid.NewGuid():N}@test.com", FullName = fullName, IsActive = true };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _seededUserIds.Add(user.Id);
        return user;
    }
    private async Task<Item> SeedItemAsync(string code = "ITM-AUDIT")
    {
        var item = new Item(Guid.NewGuid(), code, $"Item {code}", UnitOfMeasure.Milliliter, 10m, 30);
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        _seededItemIds.Add(item.Id);
        return item;
    }
    private async Task<Lot> SeedLotAsync(Guid itemId, string lotNumber = "LOT-AUDIT")
    {
        var lot = Lot.Create(itemId, lotNumber, null, DateTimeOffset.UtcNow, null, 100m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)), "Shelf-A");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync();
        _seededLotIds.Add(lot.Id);
        return lot;
    }
    private async Task<Job> SeedJobAsync(string jobNumber = "JOB-AUDIT")
    {
        var job = new Job(Guid.NewGuid(), jobNumber, "Audit Test Job");
        job.Start(DateTimeOffset.UtcNow);
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
        _seededJobIds.Add(job.Id);
        return job;
    }
    private async Task<Consumption> SeedConsumptionAsync(Guid jobId, Guid lotId, Guid userId, decimal qty, DateTimeOffset at)
    {
        var c = new Consumption(Guid.NewGuid(), jobId, lotId, qty, at, userId);
        _context.Consumptions.Add(c);
        await _context.SaveChangesAsync();
        _seededConsumptionIds.Add(c.Id);
        return c;
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
    public async Task GetAuditEventsAsync_CombinesConsumptionsAndDisposals()
    {
        var user = await SeedUserAsync();
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var job = await SeedJobAsync();
        var now = DateTimeOffset.UtcNow;
        await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 5m, now);
        await SeedDisposalAsync(lot.Id, 10, now);
        var query = new AuditQuery { Page = 1, PageSize = 10 };
        var result = await _repository.GetAuditEventsAsync(query);
        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Items.Any(e => e.EventType == AuditEventType.Consumption), Is.True);
        Assert.That(result.Items.Any(e => e.EventType == AuditEventType.Disposal), Is.True);
    }
    [Test]
    public async Task GetAuditEventsAsync_FiltersByEventType()
    {
        var user = await SeedUserAsync();
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var job = await SeedJobAsync();
        var now = DateTimeOffset.UtcNow;
        await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 5m, now);
        await SeedDisposalAsync(lot.Id, 10, now);
        var query = new AuditQuery { Page = 1, PageSize = 10, EventType = AuditEventType.Consumption };
        var result = await _repository.GetAuditEventsAsync(query);
        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items[0].EventType, Is.EqualTo(AuditEventType.Consumption));
    }
    [Test]
    public async Task GetAuditEventsAsync_FiltersBySearch_ILike_CaseInsensitive()
    {
        var user = await SeedUserAsync();
        var item = await SeedItemAsync("ITM-XYZ");
        var lot = await SeedLotAsync(item.Id, "LOT-ABC-123");
        var job = await SeedJobAsync();
        await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 5m, DateTimeOffset.UtcNow);
        var query = new AuditQuery { Page = 1, PageSize = 10, Search = "lot-abc" }; 
        var result = await _repository.GetAuditEventsAsync(query);
        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items[0].LotNumber, Is.EqualTo("LOT-ABC-123"));
    }
    [Test]
    public async Task GetAuditEventsAsync_PaginatesCorrectly()
    {
        var user = await SeedUserAsync();
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var job = await SeedJobAsync();
        var now = DateTimeOffset.UtcNow;
        for (int i = 0; i < 5; i++)
            await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 1m, now.AddMinutes(i));
        var query = new AuditQuery { Page = 1, PageSize = 2 };
        var result = await _repository.GetAuditEventsAsync(query);
        Assert.That(result.TotalCount, Is.EqualTo(5));
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Page, Is.EqualTo(1));
    }
    [Test]
    public async Task GetConsumptionByIdAsync_ReturnsDetail_WhenExists()
    {
        var user = await SeedUserAsync("Dr. Smith");
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var job = await SeedJobAsync();
        var c = await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 7m, DateTimeOffset.UtcNow);
        var detail = await _repository.GetConsumptionByIdAsync(c.Id);
        Assert.That(detail, Is.Not.Null);
        Assert.That(detail!.Quantity, Is.EqualTo(7m));
        Assert.That(detail.FullName, Is.EqualTo("Dr. Smith"));
        Assert.That(detail.itemCode, Is.EqualTo(item.Code));
    }
    [Test]
    public async Task GetDisposalByIdAsync_ReturnsNull_WhenNotExists()
    {
        var detail = await _repository.GetDisposalByIdAsync(Guid.NewGuid());
        Assert.That(detail, Is.Null);
    }
}