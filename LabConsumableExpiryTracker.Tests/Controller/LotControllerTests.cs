using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class LotControllerTests
{
    private Mock<ILotService> _lotServiceMock = null!;
    private LotController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _lotServiceMock = new Mock<ILotService>(MockBehavior.Strict);
        _controller = new LotController(_lotServiceMock.Object);
    }

    // ============================================================
    // GET ALL
    // ============================================================

    [Test]
    public async Task GetAll_WhenServiceReturnsSuccess_ReturnsOk()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var lots = new List<LotDto>();

        var serviceResult =
            ServiceResult<IEnumerable<LotDto>>.SuccessResult(
                lots,
                "Data lot berhasil diambil"
            );

        _lotServiceMock
            .Setup(x => x.GetAllLot(cancellationToken))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetAll(cancellationToken);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());

        var okResult = (OkObjectResult)result;

        Assert.That(okResult.Value, Is.EqualTo(serviceResult));

        _lotServiceMock.Verify(
            x => x.GetAllLot(cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GetAll_WhenServiceReturnsError_ReturnsBadRequest()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var serviceResult =
            ServiceResult<IEnumerable<LotDto>>.ErrorResult(
                "Gagal mengambil data lot"
            );

        _lotServiceMock
            .Setup(x => x.GetAllLot(cancellationToken))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetAll(cancellationToken);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());

        var badRequestResult = (BadRequestObjectResult)result;

        Assert.That(
            badRequestResult.Value,
            Is.EqualTo(serviceResult)
        );

        _lotServiceMock.Verify(
            x => x.GetAllLot(cancellationToken),
            Times.Once
        );
    }

    // ============================================================
    // GET BY ID
    // ============================================================

    [Test]
    public async Task GetById_WhenServiceReturnsSuccess_ReturnsOk()
    {
        // Arrange
        var lotId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var lot = new LotDto
        {
            Id = lotId
        };

        var serviceResult =
            ServiceResult<LotDto>.SuccessResult(
                lot,
                "Lot berhasil ditemukan"
            );

        _lotServiceMock
            .Setup(x => x.GetByIdLot(lotId, cancellationToken))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetById(
            lotId,
            cancellationToken
        );

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());

        var okResult = (OkObjectResult)result;

        Assert.That(
            okResult.Value,
            Is.EqualTo(serviceResult)
        );

        _lotServiceMock.Verify(
            x => x.GetByIdLot(lotId, cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GetById_WhenServiceReturnsError_ReturnsNotFound()
    {
        // Arrange
        var lotId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var serviceResult =
            ServiceResult<LotDto>.ErrorResult(
                "Lot tidak ditemukan"
            );

        _lotServiceMock
            .Setup(x => x.GetByIdLot(lotId, cancellationToken))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetById(
            lotId,
            cancellationToken
        );

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

        var notFoundResult = (NotFoundObjectResult)result;

        Assert.That(
            notFoundResult.Value,
            Is.EqualTo(serviceResult)
        );

        _lotServiceMock.Verify(
            x => x.GetByIdLot(lotId, cancellationToken),
            Times.Once
        );
    }

    // ============================================================
    // CREATE
    // ============================================================

    [Test]
    public async Task Create_WhenServiceReturnsSuccess_ReturnsCreatedAtAction()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var request = new CreateLotDto();

        var lotId = Guid.NewGuid();

        var createdLot = new LotDto
        {
            Id = lotId
        };

        var serviceResult =
            ServiceResult<LotDto>.SuccessResult(
                createdLot,
                "Lot berhasil dibuat"
            );

        _lotServiceMock
            .Setup(x => x.CreateLot(request, cancellationToken))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Create(
            request,
            cancellationToken
        );

        // Assert
        Assert.That(
            result,
            Is.TypeOf<CreatedAtActionResult>()
        );

        var createdResult =
            (CreatedAtActionResult)result;

        Assert.That(
            createdResult.ActionName,
            Is.EqualTo(nameof(LotController.GetById))
        );

        Assert.That(
            createdResult.Value,
            Is.EqualTo(serviceResult)
        );

        Assert.That(
            createdResult.RouteValues,
            Is.Not.Null
        );

        Assert.That(
            createdResult.RouteValues!["id"],
            Is.EqualTo(lotId)
        );

        _lotServiceMock.Verify(
            x => x.CreateLot(request, cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task Create_WhenServiceReturnsError_ReturnsBadRequest()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var request = new CreateLotDto();

        var serviceResult =
            ServiceResult<LotDto>.ErrorResult(
                "Data lot tidak valid"
            );

        _lotServiceMock
            .Setup(x => x.CreateLot(request, cancellationToken))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Create(
            request,
            cancellationToken
        );

        // Assert
        Assert.That(
            result,
            Is.TypeOf<BadRequestObjectResult>()
        );

        var badRequestResult =
            (BadRequestObjectResult)result;

        Assert.That(
            badRequestResult.Value,
            Is.EqualTo(serviceResult)
        );

        _lotServiceMock.Verify(
            x => x.CreateLot(request, cancellationToken),
            Times.Once
        );
    }

    // ============================================================
    // UPDATE
    // ============================================================

    [Test]
    public async Task Update_WhenServiceReturnsSuccess_ReturnsNoContent()
    {
        // Arrange
        var lotId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var request = new UpdateLotDto();

        var updatedLot = new LotDto
        {
            Id = lotId
        };

        var serviceResult =
            ServiceResult<LotDto>.SuccessResult(
                updatedLot,
                "Lot berhasil diperbarui"
            );

        _lotServiceMock
            .Setup(x => x.UpdateLot(
                lotId,
                request,
                cancellationToken))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Update(
            lotId,
            request,
            cancellationToken
        );

        // Assert
        Assert.That(
            result,
            Is.TypeOf<NoContentResult>()
        );

        _lotServiceMock.Verify(
            x => x.UpdateLot(
                lotId,
                request,
                cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task Update_WhenServiceReturnsError_ReturnsNotFound()
    {
        // Arrange
        var lotId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var request = new UpdateLotDto();

        var serviceResult =
            ServiceResult<LotDto>.ErrorResult(
                "Lot tidak ditemukan"
            );

        _lotServiceMock
            .Setup(x => x.UpdateLot(
                lotId,
                request,
                cancellationToken))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Update(
            lotId,
            request,
            cancellationToken
        );

        // Assert
        Assert.That(
            result,
            Is.TypeOf<NotFoundObjectResult>()
        );

        var notFoundResult =
            (NotFoundObjectResult)result;

        Assert.That(
            notFoundResult.Value,
            Is.EqualTo(serviceResult)
        );

        _lotServiceMock.Verify(
            x => x.UpdateLot(
                lotId,
                request,
                cancellationToken),
            Times.Once
        );
    }

    // ============================================================
    // DELETE
    // ============================================================

    [Test]
    public async Task Delete_WhenServiceReturnsSuccess_ReturnsNoContent()
    {
        // Arrange
        var lotId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var serviceResult =
            ServiceResult<bool>.SuccessResult(
                true,
                "Lot berhasil dihapus"
            );

        _lotServiceMock
            .Setup(x => x.DeleteLot(
                lotId,
                cancellationToken))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Delete(
            lotId,
            cancellationToken
        );

        // Assert
        Assert.That(
            result,
            Is.TypeOf<NoContentResult>()
        );

        _lotServiceMock.Verify(
            x => x.DeleteLot(
                lotId,
                cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task Delete_WhenServiceReturnsError_ReturnsNotFound()
    {
        // Arrange
        var lotId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var serviceResult =
            ServiceResult<bool>.ErrorResult(
                "Lot tidak ditemukan"
            );

        _lotServiceMock
            .Setup(x => x.DeleteLot(
                lotId,
                cancellationToken))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Delete(
            lotId,
            cancellationToken
        );

        // Assert
        Assert.That(
            result,
            Is.TypeOf<NotFoundObjectResult>()
        );

        var notFoundResult =
            (NotFoundObjectResult)result;

        Assert.That(
            notFoundResult.Value,
            Is.EqualTo(serviceResult)
        );

        _lotServiceMock.Verify(
            x => x.DeleteLot(
                lotId,
                cancellationToken),
            Times.Once
        );
    }

    // ============================================================
    // GET ALL SUMMARY
    // ============================================================

    [Test]
    public async Task GetAllSummary_WhenServiceReturnsSuccess_ReturnsOk()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var summaries = new List<LotSummaryDto>();

        var serviceResult =
            ServiceResult<IEnumerable<LotSummaryDto>>.SuccessResult(
                summaries,
                "Summary berhasil diambil"
            );

        _lotServiceMock
            .Setup(x => x.GetAllSummary(cancellationToken))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetAllSummary(
            cancellationToken
        );

        // Assert
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>()
        );

        var okResult =
            (OkObjectResult)result.Result!;

        Assert.That(
            okResult.Value,
            Is.EqualTo(serviceResult)
        );

        _lotServiceMock.Verify(
            x => x.GetAllSummary(cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GetAllSummary_WhenServiceReturnsError_ReturnsOk()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var serviceResult =
            ServiceResult<IEnumerable<LotSummaryDto>>.ErrorResult(
                "Gagal mengambil summary"
            );

        _lotServiceMock
            .Setup(x => x.GetAllSummary(cancellationToken))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetAllSummary(
            cancellationToken
        );

        // Assert
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>()
        );

        var okResult =
            (OkObjectResult)result.Result!;

        Assert.That(
            okResult.Value,
            Is.EqualTo(serviceResult)
        );

        _lotServiceMock.Verify(
            x => x.GetAllSummary(cancellationToken),
            Times.Once
        );
    }
}