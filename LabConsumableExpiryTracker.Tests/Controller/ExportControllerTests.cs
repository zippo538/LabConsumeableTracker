using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class ExportsControllerTests
{
    private Mock<IExportService> _exportServiceMock;
    private ExportsController _controller;

    [SetUp]
    public void Setup()
    {
        _exportServiceMock = new Mock<IExportService>();
        _controller = new ExportsController(_exportServiceMock.Object);
    }

    // ========== STOCK ==========

    [Test]
    public async Task ExportStock_WhenSuccess_ReturnsFile()
    {
        // Gunakan DTO asli dari proyek, bukan TestExportDto
        var data = new List<StockExportRowDto> { new() };
        var serviceResult = ServiceResult<IEnumerable<StockExportRowDto>>.SuccessResult(data);

        _exportServiceMock
            .Setup(s => s.GetStockExport(It.IsAny<StockExportFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.ExportStock(new StockExportFilterDto(), CancellationToken.None);

        var fileResult = result as FileContentResult;
        Assert.That(fileResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(fileResult!.ContentType, Is.EqualTo("text/csv"));
            Assert.That(fileResult.FileDownloadName, Does.StartWith("stock-export-"));
        });
    }

    [Test]
    public async Task ExportStock_WhenFail_ReturnsBadRequest()
    {
        var serviceResult = ServiceResult<IEnumerable<StockExportRowDto>>.ErrorResult("Error");

        _exportServiceMock
            .Setup(s => s.GetStockExport(It.IsAny<StockExportFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.ExportStock(new StockExportFilterDto(), CancellationToken.None);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    // ========== CONSUMPTIONS ==========

    [Test]
    public async Task ExportConsumptions_WhenSuccess_ReturnsFile()
    {
        var data = new List<ConsumptionExportRowDto> { new() };
        var serviceResult = ServiceResult<IEnumerable<ConsumptionExportRowDto>>.SuccessResult(data);

        _exportServiceMock
            .Setup(s => s.GetConsumptionExport(It.IsAny<ConsumptionExportFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.ExportConsumptions(new ConsumptionExportFilterDto(), CancellationToken.None);

        var fileResult = result as FileContentResult;
        Assert.That(fileResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(fileResult!.ContentType, Is.EqualTo("text/csv"));
            Assert.That(fileResult.FileDownloadName, Does.StartWith("consumptions-export-"));
        });
    }

    [Test]
    public async Task ExportConsumptions_WhenFail_ReturnsBadRequest()
    {
        var serviceResult = ServiceResult<IEnumerable<ConsumptionExportRowDto>>.ErrorResult("Error");

        _exportServiceMock
            .Setup(s => s.GetConsumptionExport(It.IsAny<ConsumptionExportFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.ExportConsumptions(new ConsumptionExportFilterDto(), CancellationToken.None);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    // ========== DISPOSALS ==========

    [Test]
    public async Task ExportDisposals_WhenSuccess_ReturnsFile()
    {
        var data = new List<DisposalExportRowDto> { new() };
        var serviceResult = ServiceResult<IEnumerable<DisposalExportRowDto>>.SuccessResult(data);

        _exportServiceMock
            .Setup(s => s.GetDisposalExport(It.IsAny<DisposalExportFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.ExportDisposals(new DisposalExportFilterDto(), CancellationToken.None);

        var fileResult = result as FileContentResult;
        Assert.That(fileResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(fileResult!.ContentType, Is.EqualTo("text/csv"));
            Assert.That(fileResult.FileDownloadName, Does.StartWith("disposals-export-"));
        });
    }

    [Test]
    public async Task ExportDisposals_WhenFail_ReturnsBadRequest()
    {
        var serviceResult = ServiceResult<IEnumerable<DisposalExportRowDto>>.ErrorResult("Error");

        _exportServiceMock
            .Setup(s => s.GetDisposalExport(It.IsAny<DisposalExportFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.ExportDisposals(new DisposalExportFilterDto(), CancellationToken.None);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}