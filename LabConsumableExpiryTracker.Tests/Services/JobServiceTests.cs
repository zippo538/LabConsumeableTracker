using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
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
        public void SetUp()
        {
            _jobRepository = new Mock<IJobRepository>(MockBehavior.Strict);
            _mapper = new Mock<IMapper>(MockBehavior.Loose);
            _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            _timeProvider = new FixedTimeProvider(FixedNow);

            _sut = new JobService(
                _jobRepository.Object,
                _timeProvider,
                _mapper.Object,
                _unitOfWork.Object);
        }

        [Test]
        public async Task CreateAsync_WhenDtoIsNull_ReturnsErrorAndDoesNotAccessDependencies()
        {
            ServiceResult<JobDto> result = await _sut.CreateAsync(null!);

            Assert.That(result, Is.Not.Null);
            _jobRepository.VerifyNoOtherCalls();
            _unitOfWork.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\r\n")]
        public async Task CreateAsync_WhenJobNumberIsBlank_ReturnsErrorWithoutCheckingDuplicate(
            string? jobNumber)
        {
            var dto = new CreateJobDto { JobNumber = jobNumber! };

            ServiceResult<JobDto> result = await _sut.CreateAsync(dto);

            Assert.That(result, Is.Not.Null);
            _jobRepository.Verify(
                repository => repository.ExistsByJobNumberAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
            _jobRepository.VerifyNoOtherCalls();
            _unitOfWork.VerifyNoOtherCalls();
        }

        [Test]
        public async Task CreateAsync_WhenJobNumberIsUnique_TrimsPersistsAndMapsJob()
        {
            const string inputJobNumber = "  JOB-001  ";
            const string normalizedJobNumber = "JOB-001";
            var dto = new CreateJobDto { JobNumber = inputJobNumber };
            var cancellationToken = new CancellationTokenSource().Token;
            Job? capturedJob = null;

            _jobRepository
                .Setup(repository => repository.ExistsByJobNumberAsync(
                    normalizedJobNumber,
                    cancellationToken))
                .ReturnsAsync(false);
            _jobRepository
                .Setup(repository => repository.AddAsync(
                    It.IsAny<Job>(),
                    cancellationToken))
                .Callback<Job, CancellationToken>((job, _) => capturedJob = job)
                .ReturnsAsync((Job job, CancellationToken _) => job);
            _unitOfWork
                .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            ServiceResult<JobDto> result = await _sut.CreateAsync(dto, cancellationToken);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(capturedJob, Is.Not.Null);
                Assert.That(capturedJob!.JobNumber, Is.EqualTo(normalizedJobNumber));
                Assert.That(capturedJob.Status, Is.EqualTo(JobStatus.Draft));
                Assert.That(capturedJob.StartedAt, Is.Null);
                Assert.That(capturedJob.CompletedAt, Is.Null);
            });

            _jobRepository.Verify(repository => repository.ExistsByJobNumberAsync(
                normalizedJobNumber,
                cancellationToken), Times.Once);
            _jobRepository.Verify(repository => repository.AddAsync(
                It.Is<Job>(job =>
                    job.JobNumber == normalizedJobNumber &&
                    job.Status == JobStatus.Draft),
                cancellationToken), Times.Once);
            _unitOfWork.Verify(
                unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
                Times.Once);
            _mapper.Verify(mapper => mapper.Map<JobDto>(It.IsAny<Job>()), Times.Once);
        }

        [Test]
        public async Task CreateAsync_WhenJobNumberAlreadyExists_ReturnsErrorWithoutAddingOrSaving()
        {
            const string jobNumber = "JOB-001";
            var dto = new CreateJobDto { JobNumber = jobNumber };

            _jobRepository
                .Setup(repository => repository.ExistsByJobNumberAsync(
                    jobNumber,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            ServiceResult<JobDto> result = await _sut.CreateAsync(dto);

            Assert.That(result, Is.Not.Null);
            _jobRepository.Verify(repository => repository.AddAsync(
                It.IsAny<Job>(),
                It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(
                unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
            _mapper.Verify(mapper => mapper.Map<JobDto>(It.IsAny<Job>()), Times.Never);
        }

        [Test]
        public async Task GetAllAsync_ReturnsMappedJobs()
        {
            var jobs = new List<Job>
        {
            new(Guid.NewGuid(), "JOB-001"),
            new(Guid.NewGuid(), "JOB-002")
        };

            _jobRepository
                .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(jobs);

            ServiceResult<IReadOnlyList<JobDto>> result = await _sut.GetAllAsync();

            Assert.That(result, Is.Not.Null);
            _jobRepository.Verify(
                repository => repository.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            _mapper.Verify(
                mapper => mapper.Map<IReadOnlyList<JobDto>>(jobs),
                Times.Once);
        }

        [Test]
        public async Task GetByIdAsync_WhenIdIsEmpty_ReturnsErrorWithoutReadingRepository()
        {
            ServiceResult<JobDto> result = await _sut.GetByIdAsync(Guid.Empty);

            Assert.That(result, Is.Not.Null);
            _jobRepository.Verify(
                repository => repository.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetByIdAsync_WhenJobExists_ReadsAndMapsJob()
        {
            var id = Guid.NewGuid();
            var job = new Job(id, "JOB-001");

            _jobRepository
                .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(job);

            ServiceResult<JobDto> result = await _sut.GetByIdAsync(id);

            Assert.That(result, Is.Not.Null);
            _mapper.Verify(mapper => mapper.Map<JobDto>(job), Times.Once);
        }

        [Test]
        public async Task GetByIdAsync_WhenJobDoesNotExist_ReturnsError()
        {
            var id = Guid.NewGuid();

            _jobRepository
                .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Job?)null);

            ServiceResult<JobDto> result = await _sut.GetByIdAsync(id);

            Assert.That(result, Is.Not.Null);
            _mapper.Verify(mapper => mapper.Map<JobDto>(It.IsAny<Job>()), Times.Never);
        }

        [Test]
        public async Task StartAsync_WhenIdIsEmpty_ReturnsErrorWithoutReadingRepository()
        {
            ServiceResult<JobDto> result = await _sut.StartAsync(Guid.Empty);

            Assert.That(result, Is.Not.Null);
            _jobRepository.Verify(
                repository => repository.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task StartAsync_WhenJobDoesNotExist_ReturnsErrorWithoutUpdating()
        {
            var id = Guid.NewGuid();

            _jobRepository
                .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Job?)null);

            ServiceResult<JobDto> result = await _sut.StartAsync(id);

            Assert.That(result, Is.Not.Null);
            _jobRepository.Verify(
                repository => repository.UpdateAsync(
                    It.IsAny<Job>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
            _unitOfWork.Verify(
                unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task StartAsync_WhenJobIsDraft_ChangesStatusAndUsesServerTime()
        {
            var id = Guid.NewGuid();
            var job = new Job(id, "JOB-001");
            var cancellationToken = new CancellationTokenSource().Token;

            _jobRepository
                .Setup(repository => repository.GetByIdAsync(id, cancellationToken))
                .ReturnsAsync(job);
            _jobRepository
                .Setup(repository => repository.UpdateAsync(job, cancellationToken))
                .ReturnsAsync(job);
            _unitOfWork
                .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            ServiceResult<JobDto> result = await _sut.StartAsync(id, cancellationToken);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(job.Status, Is.EqualTo(JobStatus.InProgress));
                Assert.That(job.StartedAt, Is.EqualTo(FixedNow));
                Assert.That(job.CompletedAt, Is.Null);
            });

            _jobRepository.Verify(repository => repository.UpdateAsync(job, cancellationToken), Times.Once);
            _unitOfWork.Verify(
                unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task CompleteAsync_WhenJobIsInProgress_ChangesStatusAndUsesServerTime()
        {
            var id = Guid.NewGuid();
            var job = new Job(id, "JOB-001");
            job.Start(FixedNow.AddMinutes(-30));
            var cancellationToken = new CancellationTokenSource().Token;

            _jobRepository
                .Setup(repository => repository.GetByIdAsync(id, cancellationToken))
                .ReturnsAsync(job);
            _jobRepository
                .Setup(repository => repository.UpdateAsync(job, cancellationToken))
                .ReturnsAsync(job);
            _unitOfWork
                .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            ServiceResult<JobDto> result = await _sut.CompleteAsync(id, cancellationToken);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(job.Status, Is.EqualTo(JobStatus.Completed));
                Assert.That(job.StartedAt, Is.EqualTo(FixedNow.AddMinutes(-30)));
                Assert.That(job.CompletedAt, Is.EqualTo(FixedNow));
            });

            _jobRepository.Verify(repository => repository.UpdateAsync(job, cancellationToken), Times.Once);
            _unitOfWork.Verify(
                unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task StartAsync_WhenJobIsNotDraft_PropagatesDomainLifecycleError()
        {
            var id = Guid.NewGuid();
            var job = new Job(id, "JOB-001");
            job.Start(FixedNow.AddMinutes(-30));

            _jobRepository
                .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(job);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.StartAsync(id));
            _jobRepository.Verify(
                repository => repository.UpdateAsync(
                    It.IsAny<Job>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task CompleteAsync_WhenJobIsDraft_PropagatesDomainLifecycleError()
        {
            var id = Guid.NewGuid();
            var job = new Job(id, "JOB-001");

            _jobRepository
                .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(job);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.CompleteAsync(id));
            _jobRepository.Verify(
                repository => repository.UpdateAsync(
                    It.IsAny<Job>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public void UpdateStatusAsync_WhenDtoIsNull_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _sut.UpdateStatusAsync(Guid.NewGuid(), null!));
        }

        [Test]
        public async Task UpdateStatusAsync_WhenStatusIsInProgress_DelegatesToStart()
        {
            var id = Guid.NewGuid();
            var job = new Job(id, "JOB-001");

            _jobRepository
                .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(job);
            _jobRepository
                .Setup(repository => repository.UpdateAsync(job, It.IsAny<CancellationToken>()))
                .ReturnsAsync(job);

            var dto = new UpdateJobDto { Status = JobStatus.InProgress };
            _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
            ServiceResult<JobDto> result = await _sut.UpdateStatusAsync(id, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(job.Status, Is.EqualTo(JobStatus.InProgress));
            _jobRepository.Verify(
                repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);
            _jobRepository.Verify(
                repository => repository.UpdateAsync(job, It.IsAny<CancellationToken>()),
                Times.Once);
            _unitOfWork.Verify(
                unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            
        }

        [Test]
        public async Task UpdateStatusAsync_WhenStatusIsCompleted_DelegatesToComplete()
        {
            var id = Guid.NewGuid();
            var job = new Job(id, "JOB-001");
            job.Start(FixedNow.AddMinutes(-30));

            _jobRepository
                .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(job);
            _jobRepository
                .Setup(repository => repository.UpdateAsync(job, It.IsAny<CancellationToken>()))
                .ReturnsAsync(job);

            var dto = new UpdateJobDto { Status = JobStatus.Completed };
            _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
            ServiceResult<JobDto> result = await _sut.UpdateStatusAsync(id, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(job.Status, Is.EqualTo(JobStatus.Completed));
            Assert.That(job.CompletedAt, Is.EqualTo(FixedNow));
            _jobRepository.Verify(
                repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);
            _jobRepository.Verify(
                repository => repository.UpdateAsync(job, It.IsAny<CancellationToken>()),
                Times.Once);
            _unitOfWork.Verify(
                unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestCase(JobStatus.Draft)]
        [TestCase(JobStatus.Cancelled)]
        public void UpdateStatusAsync_WhenStatusIsNotSupported_ThrowsInvalidOperationException(JobStatus status)
        {
            var dto = new UpdateJobDto { Status = status };

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.UpdateStatusAsync(Guid.NewGuid(), dto));

            _jobRepository.VerifyNoOtherCalls();
        }

        private sealed class FixedTimeProvider : TimeProvider
        {
            private readonly DateTimeOffset _utcNow;

            public FixedTimeProvider(DateTimeOffset utcNow)
            {
                _utcNow = utcNow;
            }

            public override DateTimeOffset GetUtcNow() => _utcNow;
        }
    }
}