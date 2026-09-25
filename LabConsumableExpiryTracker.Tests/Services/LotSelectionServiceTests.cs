using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class LotSelectionServiceTests
{
    private Mock<ILotSelectionRepository> _lotSelectionRepository = null!;
    private LotSelectionService _service = null!;

    private static readonly DateTimeOffset FixedNow =
        new(
            2026,
            9,
            23,
            10,
            0,
            0,
            TimeSpan.Zero);

    [SetUp]
    public void Setup()
    {
        _lotSelectionRepository =
            new Mock<ILotSelectionRepository>(MockBehavior.Strict);

        _service = new LotSelectionService(
            _lotSelectionRepository.Object);
    }

    [Test]
    public async Task AllocateAsync_ShouldReturnError_WhenRequestedQuantityIsZero()
    {
        var itemId = Guid.NewGuid();

        var result = await _service.AllocateAsync(
            itemId,
            0,
            FixedNow,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Requested quantity must be greater than zero."));

        _lotSelectionRepository.Verify(
            x => x.GetAvailableLotsAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task AllocateAsync_ShouldReturnError_WhenRequestedQuantityIsNegative()
    {
        var itemId = Guid.NewGuid();

        var result = await _service.AllocateAsync(
            itemId,
            -10m,
            FixedNow,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Requested quantity must be greater than zero."));

        _lotSelectionRepository.Verify(
            x => x.GetAvailableLotsAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task AllocateAsync_ShouldReturnError_WhenEligibleStockIsInsufficient()
    {
        var itemId = Guid.NewGuid();

        var lot1 = CreateLot(itemId, 30m);
        var lot2 = CreateLot(itemId, 20m);

        _lotSelectionRepository
            .Setup(x => x.GetAvailableLotsAsync(
                itemId,
                FixedNow,
                CancellationToken.None))
            .ReturnsAsync([lot1, lot2]);

        var result = await _service.AllocateAsync(
            itemId,
            60m,
            FixedNow,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "Insufficient eligible stock to fulfill the requested quantity."));

        _lotSelectionRepository.Verify(
            x => x.GetAvailableLotsAsync(
                itemId,
                FixedNow,
                CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task AllocateAsync_ShouldAllocatePartialQuantityFromSingleLot()
    {
        var itemId = Guid.NewGuid();

        var lot = CreateLot(itemId, 100m);

        _lotSelectionRepository
            .Setup(x => x.GetAvailableLotsAsync(
                itemId,
                FixedNow,
                CancellationToken.None))
            .ReturnsAsync([lot]);

        var result = await _service.AllocateAsync(
            itemId,
            40m,
            FixedNow,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(
            result.Message,
            Is.EqualTo("Lots allocated successfully."));

        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.Data![0].LotId, Is.EqualTo(lot.Id));
        Assert.That(result.Data[0].Quantity, Is.EqualTo(40m));

        _lotSelectionRepository.Verify(
            x => x.GetAvailableLotsAsync(
                itemId,
                FixedNow,
                CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task AllocateAsync_ShouldAllocateAcrossMultipleLots()
    {
        var itemId = Guid.NewGuid();

        var lot1 = CreateLot(itemId, 30m);
        var lot2 = CreateLot(itemId, 40m);
        var lot3 = CreateLot(itemId, 50m);

        _lotSelectionRepository
            .Setup(x => x.GetAvailableLotsAsync(
                itemId,
                FixedNow,
                CancellationToken.None))
            .ReturnsAsync([lot1, lot2, lot3]);

        var result = await _service.AllocateAsync(
            itemId,
            100m,
            FixedNow,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Has.Count.EqualTo(3));

        Assert.That(result.Data![0].LotId, Is.EqualTo(lot1.Id));
        Assert.That(result.Data[0].Quantity, Is.EqualTo(30m));

        Assert.That(result.Data[1].LotId, Is.EqualTo(lot2.Id));
        Assert.That(result.Data[1].Quantity, Is.EqualTo(40m));

        Assert.That(result.Data[2].LotId, Is.EqualTo(lot3.Id));
        Assert.That(result.Data[2].Quantity, Is.EqualTo(30m));

        Assert.That(
            result.Data.Sum(x => x.Quantity),
            Is.EqualTo(100m));

        _lotSelectionRepository.Verify(
            x => x.GetAvailableLotsAsync(
                itemId,
                FixedNow,
                CancellationToken.None),
            Times.Once);
    }

    private static Lot CreateLot(
        Guid itemId,
        decimal remainingQuantity)
    {
        return new Lot(
            Guid.NewGuid(),
            itemId,
            $"LOT-{Guid.NewGuid():N}",
            "SUP-001",
            FixedNow,
            "Test Supplier",
            100m,
            remainingQuantity,
            DateOnly.FromDateTime(
                FixedNow.UtcDateTime.AddDays(60)),
            "Storage A",
            LotAdministrativeStatus.Active);
    }
}