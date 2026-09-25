using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.JobDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Moq;
namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class JobServiceTests
{
    private Mock<IJobRepository> _jobRepository = null!;
    private Mock<IMapper> _mapper = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private FixedTimeProvider _timeProvider = null!;
    private JobService _sut = null!;
    private static readonly DateTimeOffset FixedNow =
        new(2026, 9, 12, 8, 30, 0, TimeSpan.Zero);
    [SetUp]
    public void Setup()
    {
        _jobRepository = new Mock<IJobRepository>(MockBehavior.Strict);
        _mapper = new Mock<IMapper>();
        _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _timeProvider = new FixedTimeProvider(FixedNow);
        _sut = new JobService(
            _jobRepository.Object,
            _timeProvider,
            _mapper.Object,
            _unitOfWork.Object);
    }
    [Test]
    public async Task CreateAsync_WhenValid_ReturnsSuccess()
    {
        var dto = new CreateJobDto
        {
            JobNumber = " JOB-001 ",
            Description = "Test"
        };
        var token = CancellationToken.None;
        Job? captured = null;
        _jobRepository
            .Setup(x => x.ExistsByJobNumberAsync("JOB-001", token))
            .ReturnsAsync(false);
        _jobRepository
            .Setup(x => x.AddAsync(It.IsAny<Job>(), token))
            .Callback<Job, CancellationToken>((j, _) => captured = j)
            .ReturnsAsync((Job j, CancellationToken _) => j);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(token))
            .ReturnsAsync(1);
        _mapper
            .Setup(x => x.Map<JobDto>(It.IsAny<Job>()))
            .Returns(new JobDto());
        var result = await _sut.CreateAsync(dto, token);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(captured!.JobNumber, Is.EqualTo("JOB-001"));
        });
    }
    [Test]
    public async Task UpdateAsync_WhenJobExists_UpdatesAndReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var job = new Job(id, "JOB-001", "Old");
        var dto = new UpdateJobDto
        {
            JobNumber = "JOB-002",
            Description = "Updated"
        };
        var token = CancellationToken.None;
        _jobRepository
            .Setup(x => x.GetByIdAsync(id, token))
            .ReturnsAsync(job);
        _jobRepository
            .Setup(x => x.UpdateAsync(job, token))
            .ReturnsAsync(job);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(token))
            .ReturnsAsync(1);
        _mapper
            .Setup(x => x.Map<JobDto>(job))
            .Returns(new JobDto());
        var result = await _sut.UpdateAsync(id, dto, token);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(job.JobNumber, Is.EqualTo("JOB-002"));
            Assert.That(job.Description, Is.EqualTo("Updated"));
        });
        _jobRepository.Verify(x => x.UpdateAsync(job, token), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(token), Times.Once);
    }
    [Test]
    public async Task UpdateAsync_WhenJobNotFound_ReturnsError()
    {
        var id = Guid.NewGuid();
        _jobRepository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);
        var result = await _sut.UpdateAsync(
            id,
            new UpdateJobDto(),
            CancellationToken.None);
        Assert.That(result.Success, Is.False);
    }
    [Test]
    public async Task StartAsync_WhenDraft_ChangesStatus()
    {
        var job = new Job(Guid.NewGuid(), "JOB-001");
        var token = CancellationToken.None;
        _jobRepository.Setup(x => x.GetByIdAsync(job.Id, token))
            .ReturnsAsync(job);
        _jobRepository.Setup(x => x.UpdateAsync(job, token))
            .ReturnsAsync(job);
        _unitOfWork.Setup(x => x.SaveChangesAsync(token))
            .ReturnsAsync(1);
        _mapper.Setup(x => x.Map<JobDto>(job))
            .Returns(new JobDto());
        var result = await _sut.StartAsync(job.Id, token);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(job.Status, Is.EqualTo(JobStatus.InProgress));
            Assert.That(job.StartedAt, Is.EqualTo(FixedNow));
        });
    }
    [Test]
    public async Task CompleteAsync_WhenInProgress_ChangesStatus()
    {
        var job = new Job(Guid.NewGuid(), "JOB-001");
        job.Start(FixedNow.AddMinutes(-5));
        var token = CancellationToken.None;
        _jobRepository.Setup(x => x.GetByIdAsync(job.Id, token))
            .ReturnsAsync(job);
        _jobRepository.Setup(x => x.UpdateAsync(job, token))
            .ReturnsAsync(job);
        _unitOfWork.Setup(x => x.SaveChangesAsync(token))
            .ReturnsAsync(1);
        _mapper.Setup(x => x.Map<JobDto>(job))
            .Returns(new JobDto());
        var result = await _sut.CompleteAsync(job.Id, token);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(job.Status, Is.EqualTo(JobStatus.Completed));
            Assert.That(job.CompletedAt, Is.EqualTo(FixedNow));
        });
    }
    [Test]
    public async Task CancelAsync_WhenDraft_ChangesStatus()
    {
        var job = new Job(Guid.NewGuid(), "JOB-001");
        var token = CancellationToken.None;
        _jobRepository.Setup(x => x.GetByIdAsync(job.Id, token))
            .ReturnsAsync(job);
        _jobRepository.Setup(x => x.UpdateAsync(job, token))
            .ReturnsAsync(job);
        _unitOfWork.Setup(x => x.SaveChangesAsync(token))
            .ReturnsAsync(1);
        _mapper.Setup(x => x.Map<JobDto>(job))
            .Returns(new JobDto());
        var result = await _sut.CancelAsync(job.Id, token);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(job.Status, Is.EqualTo(JobStatus.Cancelled));
        });
    }
    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;
        public FixedTimeProvider(DateTimeOffset now)
        {
            _now = now;
        }
        public override DateTimeOffset GetUtcNow() => _now;
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnSuccess_WhenJobsExist()
    {
        var jobs = new List<Job>
    {
        new(
            Guid.NewGuid(),
            "JOB-001",
            "Test Job"),
        new(
            Guid.NewGuid(),
            "JOB-002",
            "Another Job")
    };

        var jobDtos = new List<JobDto>
    {
        new()
        {
            Id = jobs[0].Id,
            JobNumber = "JOB-001",
            Description = "Test Job"
        },
        new()
        {
            Id = jobs[1].Id,
            JobNumber = "JOB-002",
            Description = "Another Job"
        }
    };

        _jobRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobs);

        _mapper
            .Setup(x => x.Map<IReadOnlyList<JobDto>>(jobs))
            .Returns(jobDtos);

        var result = await _sut.GetAllAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Count, Is.EqualTo(2));
            Assert.That(
                result.Message,
                Is.EqualTo("Job retrieved successfully."));
        });

        _jobRepository.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _mapper.Verify(
            x => x.Map<IReadOnlyList<JobDto>>(jobs),
            Times.Once);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnSuccess_WhenNoJobsExist()
    {
        var jobs = new List<Job>();
        var jobDtos = new List<JobDto>();

        _jobRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobs);

        _mapper
            .Setup(x => x.Map<IReadOnlyList<JobDto>>(jobs))
            .Returns(jobDtos);

        var result = await _sut.GetAllAsync(CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data, Is.Empty);
        Assert.That(
            result.Message,
            Is.EqualTo("Job retrieved successfully."));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnError_WhenIdIsEmpty()
    {
        var result = await _sut.GetByIdAsync(
            Guid.Empty,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Id is null"));

        _jobRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mapper.Verify(
            x => x.Map<JobDto>(It.IsAny<Job>()),
            Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnError_WhenJobNotFound()
    {
        var id = Guid.NewGuid();

        _jobRepository
            .Setup(x => x.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);

        var result = await _sut.GetByIdAsync(
            id,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo($"Job with id '{id}' not found."));

        _mapper.Verify(
            x => x.Map<JobDto>(It.IsAny<Job>()),
            Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnSuccess_WhenJobExists()
    {
        var id = Guid.NewGuid();

        var job = new Job(
            id,
            "JOB-001",
            "Test Job");

        var jobDto = new JobDto
        {
            Id = id,
            JobNumber = "JOB-001",
            Description = "Test Job"
        };

        _jobRepository
            .Setup(x => x.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _mapper
            .Setup(x => x.Map<JobDto>(job))
            .Returns(jobDto);

        var result = await _sut.GetByIdAsync(
            id,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(id));
        Assert.That(result.Data.JobNumber, Is.EqualTo("JOB-001"));
        Assert.That(result.Data.Description, Is.EqualTo("Test Job"));

        _jobRepository.Verify(
            x => x.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapper.Verify(
            x => x.Map<JobDto>(job),
            Times.Once);
    }

    [Test]
    public void Start_ShouldSetStatusToInProgress_WhenJobIsDraft()
    {
        var job = new Job(
            Guid.NewGuid(),
            "JOB-001",
            "Test Job");

        var now = DateTimeOffset.UtcNow;

        job.Start(now);

        Assert.That(job.Status, Is.EqualTo(JobStatus.InProgress));
        Assert.That(job.StartedAt, Is.EqualTo(now));
    }

    [Test]
    public void Start_ShouldThrowException_WhenJobIsInProgress()
    {
        var job = new Job(
            Guid.NewGuid(),
            "JOB-001",
            "Test Job");

        var firstStart = DateTimeOffset.UtcNow;
        job.Start(firstStart);

        var secondStart = firstStart.AddMinutes(10);

        var ex = Assert.Throws<InvalidOperationException>(
            () => job.Start(secondStart));

        Assert.That(
            ex!.Message,
            Is.EqualTo(
                $"Job '{job.JobNumber}' can only be started from Draft. Current status: {JobStatus.InProgress}."));
    }

    [Test]
    public void Cancel_ShouldSetStatusToCancelled_WhenJobIsDraft()
    {
        var job = new Job(
            Guid.NewGuid(),
            "JOB-001",
            "Test Job");

        var now = DateTimeOffset.UtcNow;

        job.Cancel(now);

        Assert.That(job.Status, Is.EqualTo(JobStatus.Cancelled));
        Assert.That(job.CompletedAt, Is.EqualTo(now));
    }

    [Test]
    public void Cancel_ShouldSetStatusToCancelled_WhenJobIsInProgress()
    {
        var job = new Job(
            Guid.NewGuid(),
            "JOB-001",
            "Test Job");

        var startTime = DateTimeOffset.UtcNow;
        job.Start(startTime);

        var cancelTime = startTime.AddMinutes(10);

        job.Cancel(cancelTime);

        Assert.That(job.Status, Is.EqualTo(JobStatus.Cancelled));
        Assert.That(job.CompletedAt, Is.EqualTo(cancelTime));
    }

    [Test]
    public void Cancel_ShouldThrowException_WhenJobIsCancelled()
    {
        var job = new Job(
            Guid.NewGuid(),
            "JOB-001",
            "Test Job");

        var now = DateTimeOffset.UtcNow;

        job.Cancel(now);

        var ex = Assert.Throws<InvalidOperationException>(
            () => job.Cancel(now.AddMinutes(10)));

        Assert.That(
            ex!.Message,
            Is.EqualTo(
                $"Job '{job.JobNumber}' can only be canceled from Draft or InProgress. Current status: {JobStatus.Cancelled}."));
    }
    [Test]
    public void Cancel_ShouldThrowException_WhenJobIsCompleted()
    {
        var job = new Job(
            Guid.NewGuid(),
            "JOB-001",
            "Test Job");

        var startTime = DateTimeOffset.UtcNow;
        job.Start(startTime);
        job.Complete(startTime.AddMinutes(10));

        var ex = Assert.Throws<InvalidOperationException>(
            () => job.Cancel(startTime.AddMinutes(20)));

        Assert.That(
            ex!.Message,
            Is.EqualTo(
                $"Job '{job.JobNumber}' can only be canceled from Draft or InProgress. Current status: {JobStatus.Completed}."));
    }

    [Test]
    public async Task CreateAsync_ShouldReturnError_WhenRequestIsNull()
    {
        var result = await _sut.CreateAsync(
            null!,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Request cannot be null."));

        _jobRepository.Verify(
            x => x.ExistsByJobNumberAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _jobRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Job>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("   ")]
    [TestCase("\t")]
    public async Task CreateAsync_ShouldReturnError_WhenJobNumberIsEmpty(
        string? jobNumber)
    {
        var dto = new CreateJobDto
        {
            JobNumber = jobNumber!,
            Description = "Test Job"
        };

        var result = await _sut.CreateAsync(
            dto,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Job number is required."));

        _jobRepository.Verify(
            x => x.ExistsByJobNumberAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _jobRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Job>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task CreateAsync_ShouldReturnError_WhenJobNumberAlreadyExists()
    {
        var dto = new CreateJobDto
        {
            JobNumber = " JOB-001 ",
            Description = "Test Job"
        };

        _jobRepository
            .Setup(x => x.ExistsByJobNumberAsync(
                "JOB-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateAsync(
            dto,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Job number 'JOB-001' already exists."));

        _jobRepository.Verify(
            x => x.ExistsByJobNumberAsync(
                "JOB-001",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _jobRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Job>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task CreateAsync_ShouldCreateJob_WhenJobNumberDoesNotExist()
    {
        var dto = new CreateJobDto
        {
            JobNumber = " JOB-001 ",
            Description = "Test Job"
        };

        var jobId = Guid.NewGuid();

        var createdJob = new Job(
            jobId,
            "JOB-001",
            "Test Job");

        var jobDto = new JobDto
        {
            Id = jobId,
            JobNumber = "JOB-001",
            Description = "Test Job"
        };

        _jobRepository
            .Setup(x => x.ExistsByJobNumberAsync(
                "JOB-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _jobRepository
            .Setup(x => x.AddAsync(
                It.Is<Job>(job =>
                    job.JobNumber == "JOB-001" &&
                    job.Description == "Test Job"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdJob);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapper
            .Setup(x => x.Map<JobDto>(createdJob))
            .Returns(jobDto);

        var result = await _sut.CreateAsync(
            dto,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(jobId));
        Assert.That(result.Data.JobNumber, Is.EqualTo("JOB-001"));
        Assert.That(result.Data.Description, Is.EqualTo("Test Job"));
        Assert.That(
            result.Message,
            Is.EqualTo("Job Create successfully"));

        _jobRepository.Verify(
            x => x.ExistsByJobNumberAsync(
                "JOB-001",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _jobRepository.Verify(
            x => x.AddAsync(
                It.Is<Job>(job =>
                    job.JobNumber == "JOB-001" &&
                    job.Description == "Test Job"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapper.Verify(
            x => x.Map<JobDto>(createdJob),
            Times.Once);
    }

    [Test]
    public async Task StartAsync_ShouldReturnError_WhenIdIsEmpty()
    {
        var result = await _sut.StartAsync(
            Guid.Empty,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Job id is null"));

        _jobRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _jobRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<Job>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task StartAsync_ShouldReturnError_WhenJobNotFound()
    {
        var id = Guid.NewGuid();

        _jobRepository
            .Setup(x => x.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);

        var result = await _sut.StartAsync(
            id,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo($"Job with id '{id}' not found."));

        _jobRepository.Verify(
            x => x.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _jobRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<Job>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task StartAsync_ShouldStartJob_WhenJobExists()
    {
        var id = Guid.NewGuid();

        var job = new Job(
            id,
            "JOB-001",
            "Test Job");

        var jobDto = new JobDto
        {
            Id = id,
            JobNumber = "JOB-001",
            Description = "Test Job",
            Status = JobStatus.InProgress
        };

        _jobRepository
            .Setup(x => x.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _jobRepository
            .Setup(x => x.UpdateAsync(
                It.Is<Job>(job1 =>
                    job1.Id == id &&
                    job1.Status == JobStatus.InProgress),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapper
            .Setup(x => x.Map<JobDto>(job))
            .Returns(jobDto);

        var result = await _sut.StartAsync(
            id,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(id));
        Assert.That(
            result.Data.Status,
            Is.EqualTo(JobStatus.InProgress));

        Assert.That(
            job.Status,
            Is.EqualTo(JobStatus.InProgress));

        Assert.That(job.StartedAt, Is.Not.Null);

        _jobRepository.Verify(
            x => x.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _jobRepository.Verify(
            x => x.UpdateAsync(
                It.Is<Job>(job1 =>
                    job1.Id == id &&
                    job1.Status == JobStatus.InProgress),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapper.Verify(
            x => x.Map<JobDto>(job),
            Times.Once);
    }
}
