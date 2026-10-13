using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Tests.Repository;

[TestFixture]
public class JobRepositoryTests
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
    public async Task ExistsByJobNumberAsync_ReturnsTrue_WhenExists()
    {
        var jobNumber = $"JOB-EXISTS-{Guid.NewGuid():N}";
        await SeedJobAsync(jobNumber);

        var result = await _repository.ExistsByJobNumberAsync(jobNumber);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExistsByJobNumberAsync_ReturnsFalse_WhenNotExists()
    {
        var result = await _repository.ExistsByJobNumberAsync($"JOB-NOPE-{Guid.NewGuid():N}");
        Assert.That(result, Is.False);
    }

    [Test]
    public void ExistsByJobNumberAsync_Throws_WhenNullOrWhitespace()
    {
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _repository.ExistsByJobNumberAsync("   "));
    }

    [Test]
    public async Task GetByJobNumberAsync_ReturnsJob_WhenExists()
    {
        var jobNumber = $"JOB-GET-{Guid.NewGuid():N}";
        var seeded = await SeedJobAsync(jobNumber);

        var result = await _repository.GetByJobNumberAsync(jobNumber);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Id, Is.EqualTo(seeded.Id));
            Assert.That(result.JobNumber, Is.EqualTo(jobNumber));
        });
    }

    [Test]
    public async Task GetByJobNumberAsync_ReturnsNull_WhenNotExists()
    {
        var result = await _repository.GetByJobNumberAsync($"JOB-NOPE-{Guid.NewGuid():N}");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetByJobNumberAsync_Throws_WhenNullOrWhitespace()
    {
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _repository.GetByJobNumberAsync(string.Empty));
    }
}