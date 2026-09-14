
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Results;
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
        private Mock<IValidator<CreateJobDto>> _createValidator = null!;
        private Mock<IValidator<UpdateJobDto>> _updateValidator = null!;
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

            SetupValidCreateValidation(dto);
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
        public async Task Create_WhenValidationFails_ReturnsBadRequestWithMessageAndDoesNotCallService()
        {
            var dto = new CreateJobDto { JobNumber = "" };
            var validationResult = new FluentValidation.Results.ValidationResult(new[]
            {
            new ValidationFailure(nameof(CreateJobDto.JobNumber), "JobNumber wajib diisi.")
        });

            _createValidator.Setup(validator => validator.ValidateAsync(
                    dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

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
                service => service.CreateAsync(
                    It.IsAny<CreateJobDto>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Create_WhenServiceFails_ReturnsConflictWithMessage()
        {
            var dto = new CreateJobDto { JobNumber = "JOB-001" };
            var expected = ServiceResult<JobDto>.ErrorResult("Job number already exists.");

            SetupValidCreateValidation(dto);
            _jobService.Setup(service => service.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Create(dto, CancellationToken.None);
            var conflict = result.Result as ConflictObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(conflict, Is.Not.Null);
                Assert.That(conflict!.Value, Is.SameAs(expected));
            });
        }

        [Test]
        public async Task Update_WhenRequestIsValid_ReturnsOkWithMessage()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateJobDto { Status = JobStatus.InProgress };
            var expected = ServiceResult<JobDto>.SuccessResult(
                CreateJobDtoResult(id), "Job updated successfully.");

            SetupValidUpdateValidation(dto);
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
        public async Task Update_WhenValidationFails_ReturnsBadRequestWithMessageAndDoesNotCallService()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateJobDto { Status = JobStatus.Draft };
            var validationResult = new FluentValidation.Results.ValidationResult(new[]
            {
            new ValidationFailure(nameof(UpdateJobDto.Status),
                "Status hanya boleh InProgress atau Completed.")
        });

            _updateValidator.Setup(validator => validator.ValidateAsync(
                    dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var result = await _sut.Update(id, dto, CancellationToken.None);
            var badRequest = result.Result as BadRequestObjectResult;
            var body = badRequest?.Value as ServiceResult<JobDto>;

            Assert.Multiple(() =>
            {
                Assert.That(badRequest, Is.Not.Null);
                Assert.That(body, Is.Not.Null);
                Assert.That(body!.Success, Is.False);
                Assert.That(body.Message, Is.EqualTo("Validation failed."));
                Assert.That(body.Errors, Does.Contain(
                    "Status hanya boleh InProgress atau Completed."));
            });

            _jobService.Verify(
                service => service.UpdateStatusAsync(
                    It.IsAny<Guid>(), It.IsAny<UpdateJobDto>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Update_WhenServiceFails_ReturnsConflictWithMessage()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateJobDto { Status = JobStatus.Completed };
            var expected = ServiceResult<JobDto>.ErrorResult("Invalid job state.");

            SetupValidUpdateValidation(dto);
            _jobService.Setup(service => service.UpdateStatusAsync(
                    id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Update(id, dto, CancellationToken.None);
            var conflict = result.Result as ConflictObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(conflict, Is.Not.Null);
                Assert.That(conflict!.Value, Is.SameAs(expected));
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

        private void SetupValidCreateValidation(CreateJobDto dto)
        {
            _createValidator.Setup(validator => validator.ValidateAsync(
                    dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        }

        private void SetupValidUpdateValidation(UpdateJobDto dto)
        {
            _updateValidator.Setup(validator => validator.ValidateAsync(
                    dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
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