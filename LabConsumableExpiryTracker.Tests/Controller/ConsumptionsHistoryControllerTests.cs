using System.Security.Claims;
using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class ConsumptionsHistoryControllerTests
{
    private static readonly ConsumptionListQueryDto DefaultQuery = new();

    private Mock<IConsumptionHistoryService> _historyServiceMock = null!;
    private ConsumptionsHistoryController _controller = null!;
    private Guid _userId;

    [SetUp]
    public void Setup()
    {
        _userId = Guid.NewGuid();
        _historyServiceMock = new Mock<IConsumptionHistoryService>(MockBehavior.Strict);
        _controller = new ConsumptionsHistoryController(_historyServiceMock.Object);
    }

    private void SetUserClaim(string? claimValue)
    {
        var claims = claimValue is null
            ? Array.Empty<Claim>()
            : [new Claim(ClaimTypes.NameIdentifier, claimValue)];
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };
    }

    [Test]
    public async Task GetMy_WhenSuccess_ReturnsOk()
    {
        var ct = CancellationToken.None;
        SetUserClaim(_userId.ToString());
        var response = ServiceResult<MyConsumptionsResponseDto>
            .SuccessResult(new MyConsumptionsResponseDto(), "success");
        _historyServiceMock
            .Setup(x => x.GetMyConsumptions(_userId, DefaultQuery, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetMy(DefaultQuery, ct);

        var ok = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            Assert.That(ok!.Value, Is.EqualTo(response));
        });
    }

    [Test]
    public async Task GetMy_WhenFailed_ReturnsBadRequest()
    {
        var ct = CancellationToken.None;
        SetUserClaim(_userId.ToString());
        var response = ServiceResult<MyConsumptionsResponseDto>
            .ErrorResult("failed");
        _historyServiceMock
            .Setup(x => x.GetMyConsumptions(_userId, DefaultQuery, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetMy(DefaultQuery, ct);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetMy_WhenClaimIsMissing_ReturnsUnauthorized()
    {
        var ct = CancellationToken.None;
        SetUserClaim(null);

        var result = await _controller.GetMy(DefaultQuery, ct);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task GetMy_WhenClaimIsNotGuid_ReturnsUnauthorized()
    {
        var ct = CancellationToken.None;
        SetUserClaim("not-a-guid");

        var result = await _controller.GetMy(DefaultQuery, ct);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task GetById_WhenSuccess_ReturnsOk()
    {
        var ct = CancellationToken.None;
        var consumptionId = Guid.NewGuid();
        SetUserClaim(_userId.ToString());
        var response = ServiceResult<ConsumptionHistoryDetailDto>
            .SuccessResult(new ConsumptionHistoryDetailDto { Id = consumptionId }, "success");
        _historyServiceMock
            .Setup(x => x.GetConsumptionDetail(consumptionId, _userId, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetById(consumptionId, ct);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        var ct = CancellationToken.None;
        var consumptionId = Guid.NewGuid();
        SetUserClaim(_userId.ToString());
        var response = ServiceResult<ConsumptionHistoryDetailDto>
            .ErrorResult("not found");
        _historyServiceMock
            .Setup(x => x.GetConsumptionDetail(consumptionId, _userId, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetById(consumptionId, ct);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetById_WhenClaimIsMissing_ReturnsUnauthorized()
    {
        var ct = CancellationToken.None;
        SetUserClaim(null);

        var result = await _controller.GetById(Guid.NewGuid(), ct);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task GetById_WhenClaimIsNotGuid_ReturnsUnauthorized()
    {
        var ct = CancellationToken.None;
        SetUserClaim(string.Empty);

        var result = await _controller.GetById(Guid.NewGuid(), ct);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedObjectResult>());
    }
}
