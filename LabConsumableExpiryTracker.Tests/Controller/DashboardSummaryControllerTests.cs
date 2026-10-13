using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class DashboardSummaryControllerTests
{
    private Mock<IDashboardSummaryService> _serviceMock = null!;
    private DashboardSummaryController _controller = null!;
    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IDashboardSummaryService>();
        _controller = new DashboardSummaryController(_serviceMock.Object);
    }
    [Test]
    public async Task GetSummary_Success_ReturnsOkWithServiceResult()
    {
        var itemId = Guid.NewGuid();
        var dto = new DashboardSummaryDto();
        var serviceResult = ServiceResult<DashboardSummaryDto>.SuccessResult(dto);
        _serviceMock.Setup(s => s.GetSummaryAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);
        var result = await _controller.GetSummary(itemId, CancellationToken.None);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(okResult!.Value, Is.EqualTo(serviceResult));
            Assert.That(okResult.Value, Is.Not.Null);
        });
    }
    [Test]
    public async Task GetSummary_Failure_ReturnsBadRequest()
    {
        var itemId = Guid.NewGuid();
        var serviceResult = ServiceResult<DashboardSummaryDto>.ErrorResult("Summary generation failed.");
        _serviceMock.Setup(s => s.GetSummaryAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);
        var result = await _controller.GetSummary(itemId, CancellationToken.None);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult!.Value, Is.EqualTo(serviceResult));
    }
    [Test]
    public async Task GetSummary_WithNullItemId_PassesNullToService()
    {
        var dto = new DashboardSummaryDto();
        var serviceResult = ServiceResult<DashboardSummaryDto>.SuccessResult(dto);
        _serviceMock.Setup(s => s.GetSummaryAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);
        var result = await _controller.GetSummary(null, CancellationToken.None);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        _serviceMock.Verify(s => s.GetSummaryAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }
}