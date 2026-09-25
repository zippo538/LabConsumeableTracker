using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
namespace LabConsumableExpiryTracker.Tests.Repository;

[TestFixture]
public class UnitOfWorkTests
{
    private static string _connectionString = null!;
    private AppDbContext _context = null!;
    private UnitOfWork _uow = null!;
    private readonly List<Guid> _seededJobIds = [];
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _connectionString = Connection.GetConnection("UnitTestConnection");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;
        _context = new AppDbContext(options);
        _uow = new UnitOfWork(_context);
        await _context.Database.CanConnectAsync();
    }
    [TearDown]
    public async Task TearDown()
    {
        if (_seededJobIds.Count > 0)
        {
            var jobs = await _context.Jobs.Where(j => _seededJobIds.Contains(j.Id)).ToListAsync();
            _context.Jobs.RemoveRange(jobs);
            await _context.SaveChangesAsync();
            _seededJobIds.Clear();
        }
    }
    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _context.DisposeAsync();
    [Test]
    public async Task SaveChangesAsync_PersistsChangesToDatabase()
    {
        var job = new Job(Guid.NewGuid(), $"JOB-UOW-{Guid.NewGuid():N}", "UoW Test");
        _context.Jobs.Add(job);
        var affectedRows = await _uow.SaveChangesAsync();
        _seededJobIds.Add(job.Id);
        Assert.That(affectedRows, Is.GreaterThan(0));
        var fetched = await _context.Jobs.FindAsync(job.Id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.JobNumber, Is.EqualTo(job.JobNumber));
    }
}