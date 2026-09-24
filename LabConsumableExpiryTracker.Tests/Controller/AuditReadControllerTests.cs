using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class AuditReadControllerTests
{
    private static readonly AuditQuery DefaultQuery = new();
    private static readonly AuditQuery NormalizedQuery = new() { Search = "lot-1" };

    private Mock<IAuditService> _auditServiceMock = null!;
    private AuditReadController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _auditServiceMock = new Mock<IAuditService>(MockBehavior.Strict);
        _controller = new AuditReadController(_auditServiceMock.Object);
    }

    [Test]
    public async Task GetAuditEvents_WhenSuccess_ReturnsOk()
    {
        var ct = CancellationToken.None;
        var response = ServiceResult<PagedResult<AuditEventDto>>
            .SuccessResult(new PagedResult<AuditEventDto>(), "success");
        _auditServiceMock
            .Setup(x => x.GetAuditEventsAsync(DefaultQuery, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetAuditEvents(DefaultQuery, ct);

        var ok = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            Assert.That(ok!.Value, Is.EqualTo(response));
        });
    }

    [Test]
    public async Task GetAuditEvents_WhenFailed_ReturnsBadRequest()
    {
        var ct = CancellationToken.None;
        var response = ServiceResult<PagedResult<AuditEventDto>>
            .ErrorResult("failed");
        _auditServiceMock
            .Setup(x => x.GetAuditEventsAsync(DefaultQuery, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetAuditEvents(DefaultQuery, ct);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetAuditEvents_WhenSearchHasWhitespace_ShouldNormalizeSearchBeforeCallingService()
    {
        var ct = CancellationToken.None;
        var requestWithWhitespace = new AuditQuery { Search = "   lot-1   " };
        var response = ServiceResult<PagedResult<AuditEventDto>>
            .SuccessResult(new PagedResult<AuditEventDto>(), "success");
        _auditServiceMock
            .Setup(x => x.GetAuditEventsAsync(NormalizedQuery, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetAuditEvents(requestWithWhitespace, ct);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        _auditServiceMock.Verify(
            x => x.GetAuditEventsAsync(NormalizedQuery, ct),
            Times.Once);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task GetAuditEvents_WhenSearchIsBlank_ShouldPassNullSearch(string? blankSearch)
    {
        var ct = CancellationToken.None;
        var requestWithBlankSearch = new AuditQuery { Search = blankSearch };
        var expectedQuery = new AuditQuery { Search = null };
        var response = ServiceResult<PagedResult<AuditEventDto>>
            .SuccessResult(new PagedResult<AuditEventDto>(), "success");
        _auditServiceMock
            .Setup(x => x.GetAuditEventsAsync(expectedQuery, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetAuditEvents(requestWithBlankSearch, ct);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetConsumptions_WhenSuccess_ReturnsOk()
    {
        var ct = CancellationToken.None;
        var response = ServiceResult<PagedResult<ConsumptionAuditDto>>
            .SuccessResult(new PagedResult<ConsumptionAuditDto>(), "success");
        _auditServiceMock
            .Setup(x => x.GetConsumptionsAsync(DefaultQuery, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetConsumptions(DefaultQuery, ct);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetConsumptions_WhenFailed_ReturnsBadRequest()
    {
        var ct = CancellationToken.None;
        var response = ServiceResult<PagedResult<ConsumptionAuditDto>>
            .ErrorResult("failed");
        _auditServiceMock
            .Setup(x => x.GetConsumptionsAsync(DefaultQuery, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetConsumptions(DefaultQuery, ct);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetConsumptions_WhenSearchHasWhitespace_ShouldNormalizeSearchBeforeCallingService()
    {
        var ct = CancellationToken.None;
        var requestWithWhitespace = new AuditQuery { Search = "  lot-1  " };
        var response = ServiceResult<PagedResult<ConsumptionAuditDto>>
            .SuccessResult(new PagedResult<ConsumptionAuditDto>(), "success");
        _auditServiceMock
            .Setup(x => x.GetConsumptionsAsync(NormalizedQuery, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetConsumptions(requestWithWhitespace, ct);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetConsumption_WhenFound_ReturnsOk()
    {
        var ct = CancellationToken.None;
        var id = Guid.NewGuid();
        var response = ServiceResult<ConsumptionAuditDto>
            .SuccessResult(new ConsumptionAuditDto { Id = id }, "found");
        _auditServiceMock
            .Setup(x => x.GetConsumptionByIdAsync(id, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetConsumption(id, ct);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetConsumption_WhenNotFound_ReturnsNotFound()
    {
        var ct = CancellationToken.None;
        var id = Guid.NewGuid();
        var response = ServiceResult<ConsumptionAuditDto>
            .ErrorResult("not found");
        _auditServiceMock
            .Setup(x => x.GetConsumptionByIdAsync(id, ct))
            .ReturnsAsync(response);

        var result = await _controller.GetConsumption(id, ct);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public void AllowedRoles_ShouldContainScientistAndWarehouseAdmin()
    {
        Assert.That(
            AuditReadController.AllowedRoles,
            Is.EqualTo($"{nameof(UserRole.Scientist)},{nameof(UserRole.WarehouseAdmin)}"));
    }
}
