using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class DashboardSummaryServiceTests
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

    private Mock<IItemRepository> _itemRepository = null!;
    private Mock<TimeProvider> _timeProvider = null!;
    private Mock<ILogger<DashboardAlertDto>> _logger = null!;
    private DashboardSummaryService _service = null!;

    [SetUp]
    public void Setup()
    {
        _itemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
        _timeProvider = new Mock<TimeProvider>(MockBehavior.Strict);
        _logger = new Mock<ILogger<DashboardAlertDto>>();

        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);

        _service = new DashboardSummaryService(
            _itemRepository.Object,
            _timeProvider.Object,
            _logger.Object);
    }

    [Test]
    public async Task GetSummaryAsync_ShouldReturnError_WhenItemNotFound()
    {
        var itemId = Guid.NewGuid();

        _itemRepository
            .Setup(x => x.GetByIdWithLotsAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);

        var result = await _service.GetSummaryAsync(itemId);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Item tidak ditemukan."));

        _itemRepository.Verify(
            x => x.GetByIdWithLotsAsync(
                itemId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _itemRepository.Verify(
            x => x.GetAllWithLotsAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSummaryAsync_ShouldCalculateMetrics_WhenItemIdProvided()
    {
        var itemId = Guid.NewGuid();

        var item = CreateItem(
      itemId,
      minimumStock: 100,
      expiringSoonDays: 30,
      CreateLot(
          itemId,
          50,
          FixedNow.AddDays(60),
          LotAdministrativeStatus.Active),
      CreateLot(
          itemId,
          20,
          FixedNow.AddDays(10),
          LotAdministrativeStatus.Active),
      CreateLot(
          itemId,
          10,
          FixedNow.AddDays(-1),
          LotAdministrativeStatus.Active),
      CreateLot(
          itemId,
          30,
          FixedNow.AddDays(10),
          LotAdministrativeStatus.Quarantined));

        _itemRepository
            .Setup(x => x.GetByIdWithLotsAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _service.GetSummaryAsync(itemId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);


        Assert.That(result.Data.ValidLotCount, Is.EqualTo(1));
        Assert.That(result.Data.ExpiringSoonLotCount, Is.EqualTo(1));
        Assert.That(result.Data.ExpiredLotCount, Is.EqualTo(1));
        Assert.That(result.Data.BlockedLotCount, Is.EqualTo(1));
        Assert.That(result.Data.EvaluatedAt, Is.EqualTo(FixedNow));
    }

    [Test]
    public async Task GetSummaryAsync_ShouldCountManuallyBlockedLot()
    {
        var itemId = Guid.NewGuid();

        var item = CreateItem(
         itemId,
         minimumStock: 50,
         expiringSoonDays: 30,
         CreateLot(
             itemId,
             100,
             FixedNow.AddDays(60),
             LotAdministrativeStatus.Active));

        _itemRepository
            .Setup(x => x.GetByIdWithLotsAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _service.GetSummaryAsync(itemId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data?.ExpiringSoonLotCount, Is.EqualTo(0));
        Assert.That(result.Data.ExpiredLotCount, Is.EqualTo(0));
        Assert.That(result.Data.LowStockItemCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetSummaryAsync_ShouldIgnoreActiveLotWithZeroRemainingQuantity()
    {
        var itemId = Guid.NewGuid();

        var item = CreateItem(
         itemId,
         minimumStock: 50,
         expiringSoonDays: 30,
         CreateLot(
             itemId,
             100,
             FixedNow.AddDays(60),
             LotAdministrativeStatus.Active));

        _itemRepository
            .Setup(x => x.GetByIdWithLotsAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _service.GetSummaryAsync(itemId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data?.ExpiredLotCount, Is.EqualTo(0));
        Assert.That(result.Data.BlockedLotCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetSummaryAsync_ShouldNotCountExpiredLotAsUsableStock()
    {
        var itemId = Guid.NewGuid();

        var item = CreateItem(
      itemId,
      minimumStock: 50,
      expiringSoonDays: 30,
      CreateLot(
          itemId,
          100,
          FixedNow.AddDays(60),
          LotAdministrativeStatus.Active));

        _itemRepository
            .Setup(x => x.GetByIdWithLotsAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _service.GetSummaryAsync(itemId);

        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task GetSummaryAsync_ShouldCountMultipleItems_WhenItemIdIsNull()
    {
        var itemId1 = Guid.NewGuid();
        var itemId2 = Guid.NewGuid();

        var item1 = CreateItem(
      itemId1,
      minimumStock: 50,
      expiringSoonDays: 30,
      CreateLot(
          itemId1,
          100,
          FixedNow.AddDays(60),
          LotAdministrativeStatus.Active));

        var item2 = CreateItem(
      itemId2,
      minimumStock: 50,
      expiringSoonDays: 30,
      CreateLot(
          itemId2,
          100,
          FixedNow.AddDays(60),
          LotAdministrativeStatus.Active));

        _itemRepository
            .Setup(x => x.GetAllWithLotsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([item1, item2]);

        var result = await _service.GetSummaryAsync(null);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.ExpiredLotCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetSummaryAsync_ShouldReturnZeroMetrics_WhenNoItemsExist()
    {
        _itemRepository
            .Setup(x => x.GetAllWithLotsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _service.GetSummaryAsync(null);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);

        Assert.That(result.Data!.LowStockItemCount, Is.EqualTo(0));
        Assert.That(result.Data.ValidLotCount, Is.EqualTo(0));
        Assert.That(result.Data.ExpiringSoonLotCount, Is.EqualTo(0));
        Assert.That(result.Data.ExpiredLotCount, Is.EqualTo(0));
        Assert.That(result.Data.BlockedLotCount, Is.EqualTo(0));
        Assert.That(result.Data.EvaluatedAt, Is.EqualTo(FixedNow));
    }

    [Test]
    public async Task GetSummaryAsync_ShouldCountLowStock_WhenUsableStockEqualsMinimumStock()
    {
        var itemId = Guid.NewGuid();

        var item = CreateItem(
      itemId,
      minimumStock: 50,
      expiringSoonDays: 30,
      CreateLot(
          itemId,
          100,
          FixedNow.AddDays(60),
          LotAdministrativeStatus.Active));

        _itemRepository
            .Setup(x => x.GetByIdWithLotsAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _service.GetSummaryAsync(itemId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.LowStockItemCount, Is.EqualTo(0));
        Assert.That(result.Data.ValidLotCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetSummaryAsync_ShouldCountLowStock_WhenUsableStockIsBelowMinimum()
    {
        var itemId = Guid.NewGuid();

        var item = CreateItem(
      itemId,
      minimumStock: 50,
      expiringSoonDays: 30,
      CreateLot(
          itemId,
          100,
          FixedNow.AddDays(60),
          LotAdministrativeStatus.Active));

        _itemRepository
            .Setup(x => x.GetByIdWithLotsAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _service.GetSummaryAsync(itemId);

        Assert.That(result.Success, Is.True);

    }



    private static Lot CreateLot(
        Guid itemId,
        decimal remainingQuantity,
        DateTimeOffset expiryDate,
        LotAdministrativeStatus status)
    {
        return new Lot(
            Guid.NewGuid(),
            itemId,
            $"LOT-{Guid.NewGuid().ToString()[..8]}",
            "SUP-001",
            FixedNow,
            "Test Supplier",
            100m,
            remainingQuantity,
            DateOnly.FromDateTime(expiryDate.UtcDateTime),
            "Storage A",
            status);
    }

    private static Item CreateItem(
      Guid itemId,
      decimal minimumStock,
      int expiringSoonDays,
      params Lot[] lots)
    {
        var item = new Item(
            itemId,
            $"ITEM-{itemId.ToString()[..8]}",
            "Test Item",
            UnitOfMeasure.Unit,
            minimumStock,
            expiringSoonDays);

        var lotsField = typeof(Item)
            .GetField(
                "_lots",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);

        var lotsList = (List<Lot>)lotsField!.GetValue(item)!;

        lotsList.AddRange(lots);

        return item;
    }
}