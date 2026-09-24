using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.JobDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Moq;
namespace LabConsumableExpiryTracker.Tests.Services
{
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
            Assert.That(result.Success, Is.True);
            Assert.That(captured!.JobNumber, Is.EqualTo("JOB-001"));
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
            Assert.That(result.Success, Is.True);
            Assert.That(job.Status, Is.EqualTo(JobStatus.InProgress));
            Assert.That(job.StartedAt, Is.EqualTo(FixedNow));
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
            Assert.That(result.Success, Is.True);
            Assert.That(job.Status, Is.EqualTo(JobStatus.Completed));
            Assert.That(job.CompletedAt, Is.EqualTo(FixedNow));
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
            Assert.That(result.Success, Is.True);
            Assert.That(job.Status, Is.EqualTo(JobStatus.Cancelled));
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
    }
}