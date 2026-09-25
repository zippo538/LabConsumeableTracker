using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Tests.Domain;
[TestFixture]
public class JobTests
{
    [Test]
    public void Constructor_ShouldCreateJob_WithDraftStatus()
    {
        var id = Guid.NewGuid();
        var job = new Job(
            id,
            "JOB-001",
            "Sample analysis"
        );
        Assert.Multiple(() =>
        {
            Assert.That(job.Id, Is.EqualTo(id));
            Assert.That(job.JobNumber, Is.EqualTo("JOB-001"));
            Assert.That(job.Status,
                Is.EqualTo(JobStatus.Draft));
        });
    }
    [Test]
    public void Constructor_WithEmptyId_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new Job(
                Guid.Empty,
                "JOB-001"
            );
        });
    }
    [Test]
    public void Start_FromDraft_ShouldChangeStatusToInProgress()
    {
        var job = CreateJob();
        var now = DateTimeOffset.UtcNow;
        job.Start(now);
        Assert.Multiple(() =>
        {
            Assert.That(job.Status,
                Is.EqualTo(JobStatus.InProgress));
            Assert.That(job.StartedAt,
                Is.EqualTo(now));
        });
    }
    [Test]
    public void Complete_FromInProgress_ShouldChangeStatusCompleted()
    {
        var job = CreateJob();
        var now = DateTimeOffset.UtcNow;
        job.Start(now);
        job.Complete(now);
        Assert.That(
            job.Status,
            Is.EqualTo(JobStatus.Completed)
        );
    }
    [Test]
    public void Complete_FromDraft_ShouldThrowException()
    {
        var job = CreateJob();
        Assert.Throws<InvalidOperationException>(() =>
        {
            job.Complete(DateTimeOffset.UtcNow);
        });
    }
    [Test]
    public void Cancel_FromDraft_ShouldChangeStatusCancelled()
    {
        var job = CreateJob();
        job.Cancel(DateTimeOffset.UtcNow);
        Assert.That(
            job.Status,
            Is.EqualTo(JobStatus.Cancelled)
        );
    }
    private Job CreateJob()
    {
        return new Job(
            Guid.NewGuid(),
            "JOB-TEST"
        );
    }
}