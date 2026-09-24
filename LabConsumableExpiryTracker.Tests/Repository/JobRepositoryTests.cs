using LabConsumableExpiryTracker.Domain.Enums;
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
            var jobs = await _context.Jobs
                .Where(j => _seededJobIds.Contains(j.Id))
                .ToListAsync();
            _context.Jobs.RemoveRange(jobs);
            await _context.SaveChangesAsync();
            _seededJobIds.Clear();
        }
    }
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _context.DisposeAsync();
    }
    private async Task<Job> SeedJobAsync(string jobNumber, JobStatus status = JobStatus.Draft)
{
    var job = new Job(Guid.NewGuid(), jobNumber, "Test Description");
    if (status == JobStatus.InProgress || status == JobStatus.Completed)
    {
        job.Start(DateTimeOffset.UtcNow); 
    }
    if (status == JobStatus.Completed)
    {
        job.Complete(DateTimeOffset.UtcNow); 
    }
    if (status == JobStatus.Cancelled)
    {
        job.Cancel(DateTimeOffset.UtcNow); 
    }
    await _repository.AddAsync(job);
    await _context.SaveChangesAsync();
    _seededJobIds.Add(job.Id);
    return job;
}
    [Test]
    public async Task ExistsByJobNumberAsync_ReturnsTrue_WhenJobExists()
    {
        var uniqueNumber = $"JOB-EXISTS-{Guid.NewGuid():N}";
        await SeedJobAsync(uniqueNumber);
        var result = await _repository.ExistsByJobNumberAsync(uniqueNumber);
        Assert.That(result, Is.True);
    }
    [Test]
    public async Task ExistsByJobNumberAsync_ReturnsFalse_WhenJobDoesNotExist()
    {
        var result = await _repository.ExistsByJobNumberAsync($"JOB-NOPE-{Guid.NewGuid():N}");
        Assert.That(result, Is.False);
    }
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ExistsByJobNumberAsync_ThrowsOnNullOrWhitespace(string? input)
    {
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _repository.ExistsByJobNumberAsync(input!));
    }
    [Test]
    public async Task AddAsync_PersistsJob_And_GetByIdAsync_ReturnsIt()
    {
        var jobNumber = $"JOB-ADD-{Guid.NewGuid():N}";
        var job = new Job(Guid.NewGuid(), jobNumber);
        await _repository.AddAsync(job);
        await _context.SaveChangesAsync();
        _seededJobIds.Add(job.Id);
        var fetched = await _repository.GetByIdAsync(job.Id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.JobNumber, Is.EqualTo(jobNumber));
        Assert.That(fetched.Status, Is.EqualTo(JobStatus.Draft));
        Assert.That(fetched.StartedAt, Is.Null);
    }
    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }
    [Test]
    public async Task SeedJob_Draft_HasNullTimestamps()
    {
        var job = await SeedJobAsync($"JOB-DRAFT-{Guid.NewGuid():N}");
        var fetched = await _repository.GetByIdAsync(job.Id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Status, Is.EqualTo(JobStatus.Draft));
        Assert.That(fetched.StartedAt, Is.Null);
        Assert.That(fetched.CompletedAt, Is.Null);
    }
    [Test]
    public async Task SeedJob_Completed_HasBothTimestamps()
    {
        var job = await SeedJobAsync($"JOB-COMP-{Guid.NewGuid():N}", JobStatus.Completed);
        var fetched = await _repository.GetByIdAsync(job.Id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Status, Is.EqualTo(JobStatus.Completed));
        Assert.That(fetched.StartedAt, Is.Not.Null);
        Assert.That(fetched.CompletedAt, Is.Not.Null);
    }
}