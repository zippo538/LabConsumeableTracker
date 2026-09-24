using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class DisposalControllerTests
{
    private Mock<IAuditService> _auditServiceMock;
    private DisposalController _controller;

    [SetUp]
    public void Setup()
    {
        _auditServiceMock = new Mock<IAuditService>();
        _controller = new DisposalController(_auditServiceMock.Object);
    }

    [Test]
    public async Task GetDisposals_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var query = new AuditQuery { Search = "  test  " }; // Menguji NormalizeSearch (trim)
        var pagedResult = new PagedResult<DisposalAuditDto>();
        var serviceResult = ServiceResult<PagedResult<DisposalAuditDto>>.SuccessResult(pagedResult);
        _auditServiceMock.Setup(s => s.GetDisposalsAsync(It.IsAny<AuditQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetDisposals(query, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(serviceResult));
    }

    [Test]
    public async Task GetDisposals_WhenFail_ReturnsBadRequest()
    {
        // Arrange
        var query = new AuditQuery { Search = null }; // Menguji NormalizeSearch (null)
        var serviceResult = ServiceResult<PagedResult<DisposalAuditDto>>.ErrorResult("Error");
        _auditServiceMock.Setup(s => s.GetDisposalsAsync(It.IsAny<AuditQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetDisposals(query, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.Value, Is.EqualTo(serviceResult));
    }

    [Test]
    public async Task GetDisposalById_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new DisposalAuditDto();
        var serviceResult = ServiceResult<DisposalAuditDto>.SuccessResult(dto);
        _auditServiceMock.Setup(s => s.GetDisposalByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetDisposalbyId(id, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(serviceResult));
    }

    [Test]
    public async Task GetDisposalById_WhenFail_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var serviceResult = ServiceResult<DisposalAuditDto>.ErrorResult("Not found");
        _auditServiceMock.Setup(s => s.GetDisposalByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetDisposalbyId(id, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult!.Value, Is.EqualTo(serviceResult));
    }
}