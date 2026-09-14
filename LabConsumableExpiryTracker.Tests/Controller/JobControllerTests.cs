
using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.JobDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Controller
{
    [TestFixture]
    public class JobControllerTests
    {
        private Mock<IJobService> _jobService = null!;
        private JobController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _jobService = new Mock<IJobService>(MockBehavior.Strict);

            _sut = new JobController(
                _jobService.Object);
        }

        [Test]
        public async Task GetAll_WhenServiceSucceeds_ReturnsOkWithServiceResult()
        {
            var expected = ServiceResult<IReadOnlyList<JobDto>>.SuccessResult(
                Array.Empty<JobDto>(), "Jobs retrieved successfully.");

            _jobService.Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetAll(CancellationToken.None);
            var ok = result.Result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.Not.Null);
                Assert.That(ok!.Value, Is.SameAs(expected));
            });

            _jobService.Verify(
                service => service.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetAll_WhenServiceFails_ReturnsNotFoundWithMessage()
        {
            var expected = ServiceResult<IReadOnlyList<JobDto>>.ErrorResult(
                "Jobs could not be found.");

            _jobService.Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetAll(CancellationToken.None);
            var notFound = result.Result as NotFoundObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(notFound, Is.Not.Null);
                Assert.That(notFound!.Value, Is.SameAs(expected));
            });
        }

        [Test]
        public async Task GetById_WhenServiceSucceeds_ReturnsOkWithMessage()
        {
            var id = Guid.NewGuid();
            var expected = ServiceResult<JobDto>.SuccessResult(
                CreateJobDtoResult(id), "Job retrieved successfully.");

            _jobService.Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetById(id, CancellationToken.None);
            var ok = result.Result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.Not.Null);
                Assert.That(ok!.Value, Is.SameAs(expected));
            });

            _jobService.Verify(
                service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetById_WhenServiceFails_ReturnsNotFoundWithMessage()
        {
            var id = Guid.NewGuid();
            var expected = ServiceResult<JobDto>.ErrorResult("Job not found.");

            _jobService.Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetById(id, CancellationToken.None);
            var notFound = result.Result as NotFoundObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(notFound, Is.Not.Null);
                Assert.That(notFound!.Value, Is.SameAs(expected));
            });
        }

        [Test]
        public async Task Create_WhenRequestIsValid_ReturnsCreatedAtActionWithMessage()
        {
            var dto = new CreateJobDto { JobNumber = "JOB-001" };
            var expected = ServiceResult<JobDto>.SuccessResult(
                CreateJobDtoResult(), "Job created successfully.");

            _jobService.Setup(service => service.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Create(dto, CancellationToken.None);
            var created = result.Result as CreatedAtActionResult;

            Assert.Multiple(() =>
            {
                Assert.That(created, Is.Not.Null);
                Assert.That(created!.ActionName, Is.EqualTo(nameof(JobController.GetById)));
                Assert.That(created.RouteValues!["id"], Is.EqualTo(expected.Data!.Id));
                Assert.That(created.Value, Is.SameAs(expected));
            });

            _jobService.Verify(
                service => service.CreateAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Create_WhenServiceReturnsError_ReturnsBadRequestWithMessage()
        {
            var dto = new CreateJobDto { JobNumber = "" };

            var serviceResult = ServiceResult<JobDto>.ErrorResult(
                "Validation failed.",
                new List<string> { "JobNumber wajib diisi." });

            _jobService
                .Setup(service => service.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceResult);

            var result = await _sut.Create(dto, CancellationToken.None);

            var badRequest = result.Result as BadRequestObjectResult;
            var body = badRequest?.Value as ServiceResult<JobDto>;

            Assert.Multiple(() =>
            {
                Assert.That(badRequest, Is.Not.Null);
                Assert.That(body, Is.Not.Null);
                Assert.That(body!.Success, Is.False);
                Assert.That(body.Message, Is.EqualTo("Validation failed."));
                Assert.That(body.Errors, Does.Contain("JobNumber wajib diisi."));
            });

            _jobService.Verify(
                service => service.CreateAsync(dto, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Create_WhenServiceFails_ReturnsBadRequestWithMessage()
        {
            var dto = new CreateJobDto { JobNumber = "JOB-001" };
            var expected = ServiceResult<JobDto>.ErrorResult("Job number already exists.");

            _jobService.Setup(service => service.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Create(dto, CancellationToken.None);

            var badRequest = result.Result as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(badRequest, Is.Not.Null);
                Assert.That(badRequest!.Value, Is.SameAs(expected));
            });
        }

        [Test]
        public async Task Update_WhenRequestIsValid_ReturnsOkWithMessage()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateJobDto { Status = JobStatus.InProgress };
            var expected = ServiceResult<JobDto>.SuccessResult(
                CreateJobDtoResult(id), "Job updated successfully.");

            _jobService.Setup(service => service.UpdateStatusAsync(
                    id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Update(id, dto, CancellationToken.None);
            var ok = result.Result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.Not.Null);
                Assert.That(ok!.Value, Is.SameAs(expected));
            });

            _jobService.Verify(
                service => service.UpdateStatusAsync(id, dto, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Update_WhenServiceReturnsValidationError_ReturnsNotFoundWithMessage()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateJobDto { Status = JobStatus.Draft };

            var expected = ServiceResult<JobDto>.ErrorResult(
                "Validation failed.",
                new List<string> { "Status hanya boleh InProgress atau Completed." });

            _jobService.Setup(service => service.UpdateStatusAsync(
                    id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Update(id, dto, CancellationToken.None);

            var notFound = result.Result as NotFoundObjectResult;
            var body = notFound?.Value as ServiceResult<JobDto>;

            Assert.Multiple(() =>
            {
                Assert.That(notFound, Is.Not.Null);
                Assert.That(body, Is.Not.Null);
                Assert.That(body!.Success, Is.False);
                Assert.That(body.Message, Is.EqualTo("Validation failed."));
                Assert.That(body.Errors, Does.Contain(
                    "Status hanya boleh InProgress atau Completed."));
            });

            _jobService.Verify(
                service => service.UpdateStatusAsync(id, dto, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Update_WhenServiceFails_ReturnsNotFoundWithMessage()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateJobDto { Status = JobStatus.Completed };
            var expected = ServiceResult<JobDto>.ErrorResult("Invalid job state.");

            _jobService.Setup(service => service.UpdateStatusAsync(
                    id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Update(id, dto, CancellationToken.None);

            var notFound = result.Result as NotFoundObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(notFound, Is.Not.Null);
                Assert.That(notFound!.Value, Is.SameAs(expected));
            });
        }

        [Test]
        public async Task Start_WhenServiceSucceeds_ReturnsOkWithMessage()
        {
            var id = Guid.NewGuid();
            var expected = ServiceResult<JobDto>.SuccessResult(
                CreateJobDtoResult(id), "Job started successfully.");

            _jobService.Setup(service => service.StartAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Start(id, CancellationToken.None);
            var ok = result.Result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.Not.Null);
                Assert.That(ok!.Value, Is.SameAs(expected));
            });
        }

        [Test]
        public async Task Start_WhenServiceFails_ReturnsBadRequestWithMessage()
        {
            var id = Guid.NewGuid();
            var expected = ServiceResult<JobDto>.ErrorResult("Job cannot be started.");

            _jobService.Setup(service => service.StartAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Start(id, CancellationToken.None);
            var badRequest = result.Result as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(badRequest, Is.Not.Null);
                Assert.That(badRequest!.Value, Is.SameAs(expected));
            });
        }

        [Test]
        public async Task Complete_WhenServiceSucceeds_ReturnsOkWithMessage()
        {
            var id = Guid.NewGuid();
            var expected = ServiceResult<JobDto>.SuccessResult(
                CreateJobDtoResult(id), "Job completed successfully.");

            _jobService.Setup(service => service.CompleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Complete(id, CancellationToken.None);
            var ok = result.Result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.Not.Null);
                Assert.That(ok!.Value, Is.SameAs(expected));
            });
        }

        [Test]
        public async Task Complete_WhenServiceFails_ReturnsBadRequestWithMessage()
        {
            var id = Guid.NewGuid();
            var expected = ServiceResult<JobDto>.ErrorResult("Job cannot be completed.");

            _jobService.Setup(service => service.CompleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Complete(id, CancellationToken.None);
            var badRequest = result.Result as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(badRequest, Is.Not.Null);
                Assert.That(badRequest!.Value, Is.SameAs(expected));
            });
        }

        private static JobDto CreateJobDtoResult(Guid? id = null)
        {
            return new JobDto
            {
                Id = id ?? Guid.NewGuid()
            };
        }
    }
}
