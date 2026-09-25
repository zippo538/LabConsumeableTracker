using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
namespace LabConsumableExpiryTracker.Tests.Repository;

[TestFixture]
public class GenericRepositoryTests
{
    private static string _connectionString = null!;
    private AppDbContext _context = null!;
    private JobRepository _repository = null!;
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
        _repository = new JobRepository(_context);
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
    private async Task<Job> SeedJobAsync(string jobNumber)
    {
        var job = new Job(Guid.NewGuid(), jobNumber, "Test Description");
        await _repository.AddAsync(job);
        await _context.SaveChangesAsync();
        _seededJobIds.Add(job.Id);
        return job;
    }
    [Test]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        await SeedJobAsync($"JOB-ALL-1-{Guid.NewGuid():N}");
        await SeedJobAsync($"JOB-ALL-2-{Guid.NewGuid():N}");
        var result = await _repository.GetAllAsync();
        Assert.That(result.Count, Is.GreaterThanOrEqualTo(2));
    }
    [Test]
    public async Task GetByIdAsync_ReturnsEntity_WhenExists()
    {
        var job = await SeedJobAsync($"JOB-GET-{Guid.NewGuid():N}");
        var result = await _repository.GetByIdAsync(job.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.JobNumber, Is.EqualTo(job.JobNumber));
    }
    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }
    [Test]
    public async Task UpdateAsync_ModifiesEntity()
    {
        var job = await SeedJobAsync($"JOB-UPD-{Guid.NewGuid():N}");
        var newDesc = "Updated Description";
        job.UpdateDescription(newDesc);
        await _repository.UpdateAsync(job);
        await _context.SaveChangesAsync();
        var fetched = await _repository.GetByIdAsync(job.Id);
        Assert.That(fetched!.Description, Is.EqualTo(newDesc));
    }
    [Test]
    public async Task DeleteAsync_RemovesEntity_ReturnsTrue()
    {
        var job = await SeedJobAsync($"JOB-DEL-{Guid.NewGuid():N}");
        var result = await _repository.DeleteAsync(job.Id);
        Assert.That(result, Is.True);
        await _context.SaveChangesAsync();
        var fetched = await _repository.GetByIdAsync(job.Id);
        Assert.That(fetched, Is.Null);
        _seededJobIds.Remove(job.Id);
    }
    [Test]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());
        Assert.That(result, Is.False);
    }
}