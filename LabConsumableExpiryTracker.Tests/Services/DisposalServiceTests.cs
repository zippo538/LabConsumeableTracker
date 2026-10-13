using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

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

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Quantity harus lebih dari nol."));
        });

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

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Quantity harus lebih dari nol."));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Reason wajib diisi."));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Reason wajib diisi."));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Lot not found."));
        });

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

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Lot is already disposed."));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Disposal quantity exceeds remaining quantity."));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Invalid RowVersion format."));
        });
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