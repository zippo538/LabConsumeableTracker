using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class DisposalServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(
            2026,
            9,
            25,
            10,
            0,
            0,
            TimeSpan.Zero);

    private Mock<ILotRepository> _lotRepository = null!;
    private Mock<IDisposalRepository> _disposalRepository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private Mock<IMapper> _mapper = null!;
    private Mock<TimeProvider> _timeProvider = null!;
    private Mock<ILogger<DisposalService>> _logger = null!;
    private DisposalService _service = null!;

    [SetUp]
    public void Setup()
    {
        _lotRepository = new Mock<ILotRepository>(MockBehavior.Strict);
        _disposalRepository = new Mock<IDisposalRepository>(MockBehavior.Strict);
        _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _mapper = new Mock<IMapper>(MockBehavior.Strict);
        _timeProvider = new Mock<TimeProvider>(MockBehavior.Strict);
        _logger = new Mock<ILogger<DisposalService>>();

        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);

        _service = new DisposalService(
            _lotRepository.Object,
            _disposalRepository.Object,
            _unitOfWork.Object,
            _mapper.Object,
            _timeProvider.Object,
            _logger.Object);
    }

    [Test]
    public async Task CreateDisposalAsync_ShouldReturnError_WhenQuantityIsZero()
    {
        var lotId = Guid.NewGuid();

        var request = new CreateDisposalDto
        {
            Quantity = 0,
            Reason = "Expired",
            ExpectedRowVersion = Convert.ToBase64String([1, 2, 3])
        };

        var result = await _service.CreateDisposalAsync(lotId, request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Quantity harus lebih dari nol."));

        _lotRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task CreateDisposalAsync_ShouldReturnError_WhenQuantityIsNegative()
    {
        var lotId = Guid.NewGuid();

        var request = new CreateDisposalDto
        {
            Quantity = -10,
            Reason = "Expired",
            ExpectedRowVersion = Convert.ToBase64String([1, 2, 3])
        };

        var result = await _service.CreateDisposalAsync(lotId, request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Quantity harus lebih dari nol."));
    }

    [Test]
    public async Task CreateDisposalAsync_ShouldReturnError_WhenReasonIsNull()
    {
        var lotId = Guid.NewGuid();

        var request = new CreateDisposalDto
        {
            Quantity = 10,
            Reason = null!,
            ExpectedRowVersion = Convert.ToBase64String([1, 2, 3])
        };

        var result = await _service.CreateDisposalAsync(lotId, request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Reason wajib diisi."));
    }

    [Test]
    public async Task CreateDisposalAsync_ShouldReturnError_WhenReasonIsWhitespace()
    {
        var lotId = Guid.NewGuid();

        var request = new CreateDisposalDto
        {
            Quantity = 10,
            Reason = "   ",
            ExpectedRowVersion = Convert.ToBase64String([1, 2, 3])
        };

        var result = await _service.CreateDisposalAsync(lotId, request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Reason wajib diisi."));
    }

    [Test]
    public async Task CreateDisposalAsync_ShouldReturnError_WhenLotNotFound()
    {
        var lotId = Guid.NewGuid();

        var request = new CreateDisposalDto
        {
            Quantity = 10,
            Reason = "Expired",
            ExpectedRowVersion = Convert.ToBase64String([1, 2, 3])
        };

        _lotRepository
            .Setup(x => x.GetByIdAsync(lotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lot?)null);

        var result = await _service.CreateDisposalAsync(lotId, request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Lot not found."));

        _lotRepository.Verify(
            x => x.GetByIdAsync(lotId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateDisposalAsync_ShouldReturnError_WhenLotAlreadyDisposed()
    {
        var lotId = Guid.NewGuid();

        var lot = CreateLot(
            lotId,
            remainingQuantity: 100m,
            administrativeStatus: LotAdministrativeStatus.Disposed);

        var request = new CreateDisposalDto
        {
            Quantity = 10,
            Reason = "Expired",
            ExpectedRowVersion = Convert.ToBase64String([1, 2, 3])
        };

        _lotRepository
            .Setup(x => x.GetByIdAsync(lotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var result = await _service.CreateDisposalAsync(lotId, request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Lot is already disposed."));
    }

    [Test]
    public async Task CreateDisposalAsync_ShouldReturnError_WhenQuantityExceedsRemaining()
    {
        var lotId = Guid.NewGuid();

        var lot = CreateLot(
            lotId,
            remainingQuantity: 50m,
            administrativeStatus: LotAdministrativeStatus.Active);

        var request = new CreateDisposalDto
        {
            Quantity = 51,
            Reason = "Expired",
            ExpectedRowVersion = Convert.ToBase64String([1, 2, 3])
        };

        _lotRepository
            .Setup(x => x.GetByIdAsync(lotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var result = await _service.CreateDisposalAsync(lotId, request);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Disposal quantity exceeds remaining quantity."));
    }

    [Test]
    public async Task CreateDisposalAsync_ShouldReturnError_WhenRowVersionIsInvalid()
    {
        var lotId = Guid.NewGuid();

        var lot = CreateLot(
            lotId,
            remainingQuantity: 100m,
            administrativeStatus: LotAdministrativeStatus.Active);

        var request = new CreateDisposalDto
        {
            Quantity = 10,
            Reason = "Expired",
            ExpectedRowVersion = "invalid-base64"
        };

        _lotRepository
            .Setup(x => x.GetByIdAsync(lotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var result = await _service.CreateDisposalAsync(lotId, request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid RowVersion format."));
    }

    [Test]
    public async Task CreateDisposalAsync_ShouldCreateDisposal_WhenRemainingQuantityIsGreaterThanZero()
    {
        var lotId = Guid.NewGuid();
        var expectedRowVersion = new byte[] { 1, 2, 3 };

        var lot = CreateLot(
            lotId,
            remainingQuantity: 100m,
            administrativeStatus: LotAdministrativeStatus.Active);

        var request = new CreateDisposalDto
        {
            Quantity = 30,
            Reason = "Expired",
            ExpectedRowVersion = Convert.ToBase64String(expectedRowVersion)
        };

        var disposalDto = new DisposalDto();
        var lotSnapshotDto = new LotSnapshotDto();

        _lotRepository
            .Setup(x => x.GetByIdAsync(lotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        _lotRepository
            .Setup(x => x.UpdateAsync(lot, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        _disposalRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Disposal>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Disposal disposal, CancellationToken _) => disposal);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapper
            .Setup(x => x.Map<DisposalDto>(It.IsAny<Disposal>()))
            .Returns(disposalDto);

        _mapper
            .Setup(x => x.Map<LotSnapshotDto>(lot))
            .Returns(lotSnapshotDto);

        var result = await _service.CreateDisposalAsync(lotId, request);

        Assert.That(result.Success, Is.True);
        Assert.That(
            result.Message,
            Is.EqualTo("Lot disposed successfully."));
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Disposal, Is.EqualTo(disposalDto));
        Assert.That(result.Data.Lot, Is.EqualTo(lotSnapshotDto));
        Assert.That(lot.RemainingQuantity, Is.EqualTo(70m));
        Assert.That(lot.AdministrativeStatus, Is.EqualTo(LotAdministrativeStatus.Active));

        _lotRepository.Verify(
            x => x.UpdateAsync(lot, It.IsAny<CancellationToken>()),
            Times.Once);

        _disposalRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Disposal>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateDisposalAsync_ShouldDisposeLot_WhenRemainingQuantityBecomesZero()
    {
        var lotId = Guid.NewGuid();
        var expectedRowVersion = new byte[] { 1, 2, 3 };

        var lot = CreateLot(
            lotId,
            remainingQuantity: 100m,
            administrativeStatus: LotAdministrativeStatus.Active);

        var request = new CreateDisposalDto
        {
            Quantity = 100,
            Reason = "Expired",
            ExpectedRowVersion = Convert.ToBase64String(expectedRowVersion)
        };

        _lotRepository
            .Setup(x => x.GetByIdAsync(lotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        _lotRepository
            .Setup(x => x.UpdateAsync(lot, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        _disposalRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Disposal>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Disposal disposal, CancellationToken _) => disposal);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapper
            .Setup(x => x.Map<DisposalDto>(It.IsAny<Disposal>()))
            .Returns(new DisposalDto());

        _mapper
            .Setup(x => x.Map<LotSnapshotDto>(lot))
            .Returns(new LotSnapshotDto());

        var result = await _service.CreateDisposalAsync(lotId, request);

        Assert.That(result.Success, Is.True);
        Assert.That(lot.RemainingQuantity, Is.EqualTo(0m));
        Assert.That(
            lot.AdministrativeStatus,
            Is.EqualTo(LotAdministrativeStatus.Disposed));
    }

    [Test]
    public async Task CreateDisposalAsync_ShouldReturnConcurrencyError_WhenConcurrencyExceptionOccurs()
    {
        var lotId = Guid.NewGuid();

        var lot = CreateLot(
            lotId,
            remainingQuantity: 100m,
            administrativeStatus: LotAdministrativeStatus.Active);

        var request = new CreateDisposalDto
        {
            Quantity = 10,
            Reason = "Expired",
            ExpectedRowVersion = Convert.ToBase64String([1, 2, 3])
        };

        _lotRepository
            .Setup(x => x.GetByIdAsync(lotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        _lotRepository
            .Setup(x => x.UpdateAsync(lot, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var result = await _service.CreateDisposalAsync(lotId, request);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "Concurrency conflict. The lot was modified by another user."));
        Assert.That(
            result.Errors,
            Does.Contain("CONCURRENCY_CONFLICT"));

        _lotRepository.Verify(
            x => x.UpdateAsync(lot, It.IsAny<CancellationToken>()),
            Times.Once);

        _disposalRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Disposal>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task CreateDisposalAsync_ShouldReturnError_WhenInvalidOperationExceptionOccurs()
    {
        var lotId = Guid.NewGuid();

        var lot = CreateLot(
            lotId,
            remainingQuantity: 100m,
            administrativeStatus: LotAdministrativeStatus.Active);

        var request = new CreateDisposalDto
        {
            Quantity = 10,
            Reason = "Expired",
            ExpectedRowVersion = Convert.ToBase64String([1, 2, 3])
        };

        _lotRepository
            .Setup(x => x.GetByIdAsync(lotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        _lotRepository
            .Setup(x => x.UpdateAsync(lot, It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException("Invalid disposal operation."));

        var result = await _service.CreateDisposalAsync(lotId, request);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Invalid disposal operation."));

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Lot CreateLot(
        Guid lotId,
        decimal remainingQuantity,
        LotAdministrativeStatus administrativeStatus)
    {
        return new Lot(
            lotId,
            Guid.NewGuid(),
            "LOT-001",
            "SUP-001",
            FixedNow,
            "Test Supplier",
            100m,
            remainingQuantity,
            DateOnly.FromDateTime(FixedNow.UtcDateTime.AddDays(30)),
            "Storage A",
            administrativeStatus);
    }
}