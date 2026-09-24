using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class LotControllerTests
{
    private Mock<ILotService> _lotServiceMock = null!;
    private Mock<IDisposalService> _disposalServiceMock = null!;
    private LotController _controller = null!;
    [SetUp]
    public void Setup()
    {
        _lotServiceMock =
            new Mock<ILotService>(
                MockBehavior.Strict);
        _disposalServiceMock =
            new Mock<IDisposalService>(
                MockBehavior.Strict);
        _controller =
            new LotController(
                _lotServiceMock.Object,
                _disposalServiceMock.Object
            );
    }
    [Test]
    public async Task GetAll_WhenSuccess_ReturnsOk()
    {
        var ct = CancellationToken.None;
        var response =
            ServiceResult<IEnumerable<LotDto>>
            .SuccessResult(
                new List<LotDto>(),
                "success"
            );
        _lotServiceMock
            .Setup(x => x.GetAllLot(ct))
            .ReturnsAsync(response);
        var result =
            await _controller.GetAll(ct);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
        var ok =
            (OkObjectResult)result.Result!;
        Assert.That(
            ok.Value,
            Is.EqualTo(response));
    }
    [Test]
    public async Task GetAll_WhenFailed_ReturnsBadRequest()
    {
        var ct = CancellationToken.None;
        var response =
            ServiceResult<IEnumerable<LotDto>>
            .ErrorResult(
                "failed"
            );
        _lotServiceMock
            .Setup(x => x.GetAllLot(ct))
            .ReturnsAsync(response);
        var result =
            await _controller.GetAll(ct);
        Assert.That(
            result.Result,
            Is.TypeOf<BadRequestObjectResult>());
    }
    [Test]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<LotDto>
            .SuccessResult(
                new LotDto
                {
                    Id = id
                },
                "found"
            );
        _lotServiceMock
            .Setup(x =>
                x.GetByIdLot(
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
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<LotDto>
            .ErrorResult(
                "not found"
            );
        _lotServiceMock
            .Setup(x =>
                x.GetByIdLot(
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
            new CreateLotDto();
        var lotId =
            Guid.NewGuid();
        var response =
            ServiceResult<LotDto>
            .SuccessResult(
                new LotDto
                {
                    Id = lotId
                },
                "created"
            );
        _lotServiceMock
            .Setup(x =>
                x.CreateLot(
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
        var created =
            (CreatedAtActionResult)result.Result!;
        Assert.That(
            created.ActionName,
            Is.EqualTo(
                nameof(LotController.GetById)
            ));
    }
    [Test]
    public async Task Create_WhenFailed_ReturnsBadRequest()
    {
        var request =
            new CreateLotDto();
        var response =
            ServiceResult<LotDto>
            .ErrorResult(
                "failed"
            );
        _lotServiceMock
            .Setup(x =>
                x.CreateLot(
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
            new UpdateLotDto();
        var response =
            ServiceResult<LotDto>
            .SuccessResult(
                new LotDto
                {
                    Id = id
                },
                "updated"
            );
        _lotServiceMock
            .Setup(x =>
                x.UpdateLot(
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
            new UpdateLotDto();
        var response =
            ServiceResult<LotDto>
            .ErrorResult(
                "not found"
            );
        _lotServiceMock
            .Setup(x =>
                x.UpdateLot(
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
    public async Task Delete_WhenSuccess_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<bool>
            .SuccessResult(
                true,
                "deleted"
            );
        _lotServiceMock
            .Setup(x =>
                x.DeleteLot(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Delete(
                id,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task Delete_WhenFailed_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<bool>
            .ErrorResult(
                "not found"
            );
        _lotServiceMock
            .Setup(x =>
                x.DeleteLot(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Delete(
                id,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<NotFoundObjectResult>());
    }
    [Test]
    public async Task DisposeLot_WhenSuccess_ReturnsCreated()
    {
        var lotId = Guid.NewGuid();

        var request = new CreateDisposalDto();

        var disposalId = Guid.NewGuid();

        var responseData = new CreateDisposalResponseDto
        {
            Disposal = new DisposalDto
            {
                Id = disposalId
            },
            Lot = new LotSnapshotDto()
        };

        var response =
            ServiceResult<CreateDisposalResponseDto>
            .SuccessResult(
                responseData,
                "success"
            );

        _disposalServiceMock
            .Setup(x =>
                x.CreateDisposalAsync(
                    lotId,
                    request,
                    CancellationToken.None))
            .ReturnsAsync(response);

        var result =
            await _controller.DisposeLot(
                lotId,
                request,
                CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(
                result.Result,
                Is.TypeOf<CreatedAtActionResult>());

            var created =
                (CreatedAtActionResult)result.Result!;

            Assert.That(
                created.ActionName,
                Is.EqualTo(nameof(LotController.GetById)));

            Assert.That(
                created.RouteValues!["id"],
                Is.EqualTo(disposalId));
        });
    }
    [Test]
    public async Task GetAllSummary_WhenSuccess_ReturnsOk()
    {
        var response =
            ServiceResult<IEnumerable<LotSummaryDto>>
            .SuccessResult(
                new List<LotSummaryDto>(),
                "success"
            );
        _lotServiceMock
            .Setup(x =>
                x.GetAllSummary(
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.GetAllSummary(
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task GetAllPage_WhenSuccess_ReturnsOk()
    {
        var response =
            ServiceResult<PagedResult<LotDto>>
            .SuccessResult(
                new PagedResult<LotDto>(),
                "success"
            );
        _lotServiceMock
            .Setup(x =>
                x.GetAllLotPage(
                    CancellationToken.None,
                    1,
                    20,
                    null,
                    null,
                    null,
                    null))
            .ReturnsAsync(response);
        var result =
            await _controller.GetAllPage(
                ct: CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
}