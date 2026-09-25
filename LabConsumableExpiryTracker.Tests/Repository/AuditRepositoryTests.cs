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
    private readonly DateTimeOffset _baseTime = new(2023, 10, 27, 10, 0, 0, TimeSpan.Zero);
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
        if (_seededConsumptionIds.Count > 0)
        {
            var entities = await _context.Consumptions.Where(c => _seededConsumptionIds.Contains(c.Id)).ToListAsync();
            _context.Consumptions.RemoveRange(entities);
            await _context.SaveChangesAsync();
            _seededConsumptionIds.Clear();
        }
        if (_seededDisposalIds.Count > 0)
        {
            var entities = await _context.Disposals.Where(d => _seededDisposalIds.Contains(d.Id)).ToListAsync();
            _context.Disposals.RemoveRange(entities);
            await _context.SaveChangesAsync();
            _seededDisposalIds.Clear();
        }
        if (_seededJobIds.Count > 0)
        {
            var entities = await _context.Jobs.Where(j => _seededJobIds.Contains(j.Id)).ToListAsync();
            _context.Jobs.RemoveRange(entities);
            await _context.SaveChangesAsync();
            _seededJobIds.Clear();
        }
        if (_seededLotIds.Count > 0)
        {
            var entities = await _context.Lots.Where(l => _seededLotIds.Contains(l.Id)).ToListAsync();
            _context.Lots.RemoveRange(entities);
            await _context.SaveChangesAsync();
            _seededLotIds.Clear();
        }
        if (_seededItemIds.Count > 0)
        {
            var entities = await _context.Items.Where(i => _seededItemIds.Contains(i.Id)).ToListAsync();
            _context.Items.RemoveRange(entities);
            await _context.SaveChangesAsync();
            _seededItemIds.Clear();
        }
        if (_seededUserIds.Count > 0)
        {
            var entities = await _context.Users.Where(u => _seededUserIds.Contains(u.Id)).ToListAsync();
            _context.Users.RemoveRange(entities);
            await _context.SaveChangesAsync();
            _seededUserIds.Clear();
        }
        _context.ChangeTracker.Clear();
    }
    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _context.DisposeAsync();
    private async Task<ApplicationUser> SeedUserAsync(string fullName = "Test Scientist")
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"u_{Guid.NewGuid():N}",
            Email = $"u_{Guid.NewGuid():N}@test.com",
            FullName = fullName,
            IsActive = true
        };
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
        var lot = Lot.Create(
            itemId, lotNumber, null, _baseTime, null, 100m,
            DateOnly.FromDateTime(_baseTime.UtcDateTime.AddDays(60)), "Shelf-A");
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync();
        _seededLotIds.Add(lot.Id);
        return lot;
    }
    private async Task<Job> SeedJobAsync(string jobNumber = "JOB-AUDIT")
    {
        var job = new Job(Guid.NewGuid(), jobNumber, "Audit Test Job");
        job.Start(_baseTime); 
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
    private async Task<Disposal> SeedDisposalAsync(Guid lotId, Guid userId, decimal qty, DateTimeOffset at, string reason = "Expired")
    {
        var d = new Disposal(Guid.NewGuid(), lotId, qty, reason, at, userId);
        _context.Disposals.Add(d);
        await _context.SaveChangesAsync();
        _seededDisposalIds.Add(d.Id);
        return d;
    }
    [Test]
    public async Task GetAuditEventsAsync_CombinesConsumptionsAndDisposals_WhenNoFilter()
    {
        var user = await SeedUserAsync();
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var job = await SeedJobAsync();
        await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 5m, _baseTime);
        await SeedDisposalAsync(lot.Id, user.Id, 10m, _baseTime);
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
        await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 5m, _baseTime);
        await SeedDisposalAsync(lot.Id, user.Id, 10m, _baseTime);
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
        await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 5m, _baseTime);
        var query = new AuditQuery { Page = 1, PageSize = 10, Search = "lot-abc" }; 
        var result = await _repository.GetAuditEventsAsync(query);
        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items[0].LotNumber, Is.EqualTo("LOT-ABC-123"));
    }
    [Test]
    public async Task GetAuditEventsAsync_FiltersByJobId_ReturnsOnlyConsumptions()
    {
        var user = await SeedUserAsync();
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var job1 = await SeedJobAsync("JOB-1");
        var job2 = await SeedJobAsync("JOB-2");
        await SeedConsumptionAsync(job1.Id, lot.Id, user.Id, 5m, _baseTime);
        await SeedConsumptionAsync(job2.Id, lot.Id, user.Id, 5m, _baseTime);
        await SeedDisposalAsync(lot.Id, user.Id, 10m, _baseTime); 
        var query = new AuditQuery { Page = 1, PageSize = 10, JobId = job1.Id };
        var result = await _repository.GetAuditEventsAsync(query);
        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items[0].JobId, Is.EqualTo(job1.Id));
        Assert.That(result.Items[0].EventType, Is.EqualTo(AuditEventType.Consumption));
    }
    [Test]
    public async Task GetAuditEventsAsync_FiltersByDateRange()
    {
        var user = await SeedUserAsync();
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var job = await SeedJobAsync();
        await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 1m, _baseTime.AddDays(-5));
        await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 2m, _baseTime.AddDays(-2));
        await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 3m, _baseTime.AddDays(1));
        var query = new AuditQuery
        {
            Page = 1,
            PageSize = 10,
            From = _baseTime.AddDays(-3),
            To = _baseTime
        };
        var result = await _repository.GetAuditEventsAsync(query);
        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items[0].Quantity, Is.EqualTo(2m));
    }
    [Test]
    public async Task GetAuditEventsAsync_PaginatesCorrectly()
    {
        var user = await SeedUserAsync();
        var item = await SeedItemAsync();
        var lot = await SeedLotAsync(item.Id);
        var job = await SeedJobAsync();
        for (int i = 0; i < 5; i++)
            await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 1m, _baseTime.AddMinutes(i));
        var query = new AuditQuery { Page = 1, PageSize = 2 };
        var result = await _repository.GetAuditEventsAsync(query);
        Assert.That(result.TotalCount, Is.EqualTo(5));
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Page, Is.EqualTo(1));
    }
    [Test]
    public async Task GetConsumptionsAsync_FiltersByItemIdAndPaginates()
    {
        var user = await SeedUserAsync();
        var item1 = await SeedItemAsync("ITM-C1");
        var item2 = await SeedItemAsync("ITM-C2");
        var lot1 = await SeedLotAsync(item1.Id, "LOT-C1");
        var lot2 = await SeedLotAsync(item2.Id, "LOT-C2");
        var job = await SeedJobAsync();
        await SeedConsumptionAsync(job.Id, lot1.Id, user.Id, 5m, _baseTime);
        await SeedConsumptionAsync(job.Id, lot2.Id, user.Id, 10m, _baseTime);
        var query = new AuditQuery { Page = 1, PageSize = 10, ItemId = item1.Id };
        var result = await _repository.GetConsumptionsAsync(query);
        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items[0].itemCode, Is.EqualTo("ITM-C1"));
    }
    [Test]
    public async Task GetConsumptionByIdAsync_ReturnsDetail_WhenExists()
    {
        var user = await SeedUserAsync("Dr. Smith");
        var item = await SeedItemAsync("ITM-DETAIL");
        var lot = await SeedLotAsync(item.Id, "LOT-DETAIL");
        var job = await SeedJobAsync("JOB-DETAIL");
        var c = await SeedConsumptionAsync(job.Id, lot.Id, user.Id, 7m, _baseTime);
        var detail = await _repository.GetConsumptionByIdAsync(c.Id);
        Assert.That(detail, Is.Not.Null);
        Assert.That(detail!.Quantity, Is.EqualTo(7m));
        Assert.That(detail.FullName, Is.EqualTo("Dr. Smith"));
        Assert.That(detail.itemCode, Is.EqualTo("ITM-DETAIL"));
        Assert.That(detail.JobNumber, Is.EqualTo("JOB-DETAIL"));
    }
    [Test]
    public async Task GetConsumptionByIdAsync_ReturnsNull_WhenNotExists()
    {
        var detail = await _repository.GetConsumptionByIdAsync(Guid.NewGuid());
        Assert.That(detail, Is.Null);
    }
    [Test]
    public async Task GetDisposalsAsync_FiltersByLotIdAndSearch()
    {
        var user = await SeedUserAsync();
        var item = await SeedItemAsync("ITM-D1");
        var lot1 = await SeedLotAsync(item.Id, "LOT-D-SEARCH");
        var lot2 = await SeedLotAsync(item.Id, "LOT-D-OTHER");
        await SeedDisposalAsync(lot1.Id, user.Id, 5m, _baseTime, "Contaminated");
        await SeedDisposalAsync(lot2.Id, user.Id, 10m, _baseTime);
        var query = new AuditQuery { Page = 1, PageSize = 10, LotId = lot1.Id, Search = "search" };
        var result = await _repository.GetDisposalsAsync(query);
        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items[0].Reason, Is.EqualTo("Contaminated"));
    }
    [Test]
    public async Task GetDisposalByIdAsync_ReturnsDetail_WhenExists()
    {
        var user = await SeedUserAsync();
        var item = await SeedItemAsync("ITM-D-DETAIL");
        var lot = await SeedLotAsync(item.Id, "LOT-D-DETAIL");
        var d = await SeedDisposalAsync(lot.Id, user.Id, 15m, _baseTime, "Broken");
        var detail = await _repository.GetDisposalByIdAsync(d.Id);
        Assert.That(detail, Is.Not.Null);
        Assert.That(detail!.Quantity, Is.EqualTo(15m));
        Assert.That(detail.Reason, Is.EqualTo("Broken"));
        Assert.That(detail.LotNumber, Is.EqualTo("LOT-D-DETAIL"));
    }
    [Test]
    public async Task GetDisposalByIdAsync_ReturnsNull_WhenNotExists()
    {
        var detail = await _repository.GetDisposalByIdAsync(Guid.NewGuid());
        Assert.That(detail, Is.Null);
    }
}