using System.Security.Claims;
using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;
using LabConsumableExpiryTracker.Application.DTOs.FefoDtos;
using LabConsumableExpiryTracker.Application.DTOs.JobDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class JobControllerTests
{
    private Mock<IJobService> _jobServiceMock = null!;
    private Mock<ILotService> _lotServiceMock = null!;
    private JobController _controller = null!;
    [SetUp]
    public void Setup()
    {
        _jobServiceMock =
            new Mock<IJobService>(
                MockBehavior.Strict);
        _lotServiceMock =
            new Mock<ILotService>(
                MockBehavior.Strict);
        _controller =
            new JobController(
                _jobServiceMock.Object,
                _lotServiceMock.Object);
    }
    [Test]
    public async Task GetAll_WhenSuccess_ReturnsOk()
    {
        var response =
            ServiceResult<IReadOnlyList<JobDto>>
            .SuccessResult(
                [],
                "success");
        _jobServiceMock
            .Setup(x =>
                x.GetAllAsync(
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.GetAll(
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task GetAll_WhenFailed_ReturnsNotFound()
    {
        var response =
            ServiceResult<IReadOnlyList<JobDto>>
            .ErrorResult(
                "not found");
        _jobServiceMock
            .Setup(x =>
                x.GetAllAsync(
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.GetAll(
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<NotFoundObjectResult>());
    }
    [Test]
    public async Task GetById_WhenSuccess_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<JobDto>
            .SuccessResult(
                new JobDto
                {
                    Id = id
                },
                "success");
        _jobServiceMock
            .Setup(x =>
                x.GetByIdAsync(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.GetById(
                id,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task GetById_WhenFailed_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<JobDto>
            .ErrorResult(
                "not found");
        _jobServiceMock
            .Setup(x =>
                x.GetByIdAsync(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.GetById(
                id,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<NotFoundObjectResult>());
    }
    [Test]
    public async Task Create_WhenSuccess_ReturnsCreatedAtAction()
    {
        var request =
            new CreateJobDto
            {
                JobNumber = "JOB001"
            };
        var response =
            ServiceResult<JobDto>
            .SuccessResult(
                new JobDto
                {
                    Id = Guid.NewGuid()
                },
                "created");
        _jobServiceMock
            .Setup(x =>
                x.CreateAsync(
                    request,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Create(
                request,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<CreatedAtActionResult>());
    }
    [Test]
    public async Task Create_WhenFailed_ReturnsBadRequest()
    {
        var request =
            new CreateJobDto();
        var response =
            ServiceResult<JobDto>
            .ErrorResult(
                "validation failed");
        _jobServiceMock
            .Setup(x =>
                x.CreateAsync(
                    request,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Create(
                request,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<BadRequestObjectResult>());
    }
    [Test]
    public async Task Update_WhenSuccess_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var request =
            new UpdateJobDto();
        var response =
            ServiceResult<JobDto>
            .SuccessResult(
                new JobDto
                {
                    Id = id
                },
                "updated");
        _jobServiceMock
            .Setup(x =>
                x.UpdateAsync(
                    id,
                    request,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Update(
                id,
                request,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task Update_WhenFailed_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var request =
            new UpdateJobDto();
        var response =
            ServiceResult<JobDto>
            .ErrorResult(
                "failed");
        _jobServiceMock
            .Setup(x =>
                x.UpdateAsync(
                    id,
                    request,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Update(
                id,
                request,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<NotFoundObjectResult>());
    }
    [Test]
    public async Task Start_WhenSuccess_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<JobDto>
            .SuccessResult(
                new JobDto(),
                "started");
        _jobServiceMock
            .Setup(x =>
                x.StartAsync(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Start(
                id,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task Start_WhenFailed_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<JobDto>
            .ErrorResult(
                "cannot start");
        _jobServiceMock
            .Setup(x =>
                x.StartAsync(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Start(
                id,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<BadRequestObjectResult>());
    }
    [Test]
    public async Task Cancel_WhenSuccess_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<JobDto>
            .SuccessResult(
                new JobDto(),
                "cancelled");
        _jobServiceMock
            .Setup(x =>
                x.CancelAsync(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Cancel(
                id,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task Complete_WhenSuccess_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<JobDto>
            .SuccessResult(
                new JobDto(),
                "completed");
        _jobServiceMock
            .Setup(x =>
                x.CompleteAsync(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Complete(
                id,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task PreviewAllocation_WhenSuccess_ReturnsOk()
    {
        var jobId =
            Guid.NewGuid();
        var request =
            new RequestFefoAllocationDto();
        var response =
            ServiceResult<FefoAllocationPreviewDto>
            .SuccessResult(
                new FefoAllocationPreviewDto(),
                "success");
        _lotServiceMock
            .Setup(x =>
                x.RequestFefoAllocation(
                    jobId,
                    request.ItemId,
                    request.RequestedQuantity,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.PreviewAllocation(
                jobId,
                request,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task ConfirmConsumption_WithValidUser_ReturnsCreated()
    {
        var userId =
            Guid.NewGuid();
        var jobId =
            Guid.NewGuid();
        var request =
            new ConfirmConsumptionRequestDto();
        var response =
            ServiceResult<ConsumptionResponseDto>
            .SuccessResult(
                new ConsumptionResponseDto(),
                "success");
        _lotServiceMock
            .Setup(x =>
                x.ConfirmConsumption(
                    jobId,
                    request,
                    userId,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var claims =
            new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.ToString())
            };
        _controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext
                    {
                        User =
                            new ClaimsPrincipal(
                                new ClaimsIdentity(
                                    claims))
                    }
            };
        var result =
            await _controller.ConfirmConsumption(
                jobId,
                request,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<CreatedResult>());
    }
}