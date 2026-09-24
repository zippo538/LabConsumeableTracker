using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class DashboardAlertsControllerTests
{
    private Mock<IDashboardAlertService> _serviceMock = null!;
    private DashboardAlertsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IDashboardAlertService>();
        _controller = new DashboardAlertsController(_serviceMock.Object);
    }

    [Test]
    public async Task GetAlerts_Success_ReturnsOkWithPagedResult()
    {
        // Arrange
        var query = new DashboardAlertQuery { Page = 1, PageSize = 10 };
        var pagedData = new PagedResult<DashboardAlertDto>
        {
            Items = new List<DashboardAlertDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };
        var serviceResult = ServiceResult<PagedResult<DashboardAlertDto>>.SuccessResult(pagedData);

        _serviceMock.Setup(s => s.GetAlertsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetAlerts(query, CancellationToken.None);

        // Assert
        // PERBAIKAN: Gunakan result.Result untuk mengakses IActionResult yang sebenarnya
        Assert.IsInstanceOf<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(serviceResult));
    }

    [Test]
    public async Task GetAlerts_Failure_ReturnsBadRequest()
    {
        // Arrange
        var query = new DashboardAlertQuery { Page = 1, PageSize = 10 };
        var serviceResult = ServiceResult<PagedResult<DashboardAlertDto>>.ErrorResult("Invalid query parameters.");

        _serviceMock.Setup(s => s.GetAlertsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetAlerts(query, CancellationToken.None);

        // Assert
        // PERBAIKAN: Gunakan result.Result untuk mengakses IActionResult yang sebenarnya
        Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        var badRequestResult = result.Result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.Value, Is.EqualTo(serviceResult));
    }
}