using LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
namespace LabConsumableExpiryTracker.Tests.Services;
[TestFixture]
public class ExportServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(
            2026,
            9,
            23,
            10,
            0,
            0,
            TimeSpan.Zero);
    private Mock<IItemRepository> _itemRepository = null!;
    private Mock<ILotRepository> _lotRepository = null!;
    private Mock<IConsumptionRepository> _consumptionRepository = null!;
    private Mock<IDisposalRepository> _disposalRepository = null!;
    private Mock<IUserLookupService> _userLookupService = null!;
    private Mock<TimeProvider> _timeProvider = null!;
    private Mock<ILogger<ExportService>> _logger = null!;
    private ExportService _service = null!;
    [SetUp]
    public void Setup()
    {
        _itemRepository =
            new Mock<IItemRepository>(MockBehavior.Strict);
        _lotRepository =
            new Mock<ILotRepository>(MockBehavior.Strict);
        _consumptionRepository =
            new Mock<IConsumptionRepository>(MockBehavior.Strict);
        _disposalRepository =
            new Mock<IDisposalRepository>(MockBehavior.Strict);
        _userLookupService =
            new Mock<IUserLookupService>(MockBehavior.Strict);
        _timeProvider =
            new Mock<TimeProvider>(MockBehavior.Strict);
        _logger = new Mock<ILogger<ExportService>>();
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _service = new ExportService(
            _itemRepository.Object,
            _lotRepository.Object,
            _consumptionRepository.Object,
            _disposalRepository.Object,
            _userLookupService.Object,
            _timeProvider.Object,
            _logger.Object);
    }
    [Test]
    public async Task GetStockExport_ShouldReturnError_WhenFormatIsInvalid()
    {
        var filter = new StockExportFilterDto
        {
            Format = "xlsx"
        };
        var result = await _service.GetStockExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "Invalid format. Only 'csv' is currently supported."));
        _itemRepository.Verify(
            x => x.GetAllWithLotsAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _lotRepository.Verify(
            x => x.GetUsableQuantityByItemIdsAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task GetStockExport_ShouldReturnRows_WhenItemIdIsSpecified()
    {
        var item = CreateItem();
        var filter = new StockExportFilterDto
        {
            Format = "csv",
            ItemId = item.Id
        };
        _itemRepository
            .Setup(x => x.GetByIdWithLotsAsync(
                item.Id,
                CancellationToken.None))
            .ReturnsAsync(item);
        _lotRepository
            .Setup(x => x.GetUsableQuantityByItemIdsAsync(
                It.Is<IEnumerable<Guid>>(ids =>
                    ids.Single() == item.Id),
                DateOnly.FromDateTime(FixedNow.UtcDateTime),
                CancellationToken.None))
            .ReturnsAsync(
                new Dictionary<Guid, decimal>
                {
                    [item.Id] = 25m
                });
        var result = await _service.GetStockExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
        var row = result.Data!.Single();
        Assert.That(row.ItemId, Is.EqualTo(item.Id));
        Assert.That(row.Code, Is.EqualTo(item.Code));
        Assert.That(row.Name, Is.EqualTo(item.Name));
        Assert.That(row.BaseUnit, Is.EqualTo(item.BaseUnit));
        Assert.That(row.UsableStock, Is.EqualTo(25m));
        Assert.That(row.MinimumStock, Is.EqualTo(item.MinimumStock));
        Assert.That(row.EvaluatedAt, Is.EqualTo(FixedNow));
    }
    [Test]
    public async Task GetStockExport_ShouldReturnEmpty_WhenSpecifiedItemDoesNotExist()
    {
        var itemId = Guid.NewGuid();
        var filter = new StockExportFilterDto
        {
            Format = "csv",
            ItemId = itemId
        };
        _itemRepository
            .Setup(x => x.GetByIdWithLotsAsync(
                itemId,
                CancellationToken.None))
            .ReturnsAsync((Item?)null);
        _lotRepository
            .Setup(x => x.GetUsableQuantityByItemIdsAsync(
                It.Is<IEnumerable<Guid>>(ids => !ids.Any()),
                DateOnly.FromDateTime(FixedNow.UtcDateTime),
                CancellationToken.None))
            .ReturnsAsync(
                new Dictionary<Guid, decimal>());
        var result = await _service.GetStockExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Empty);
    }
    [Test]
    public async Task GetStockExport_ShouldGetAllItems_WhenItemIdIsNotSpecified()
    {
        var item1 = CreateItem("ITEM-001", "Item One");
        var item2 = CreateItem("ITEM-002", "Item Two");
        var filter = new StockExportFilterDto
        {
            Format = "CSV"
        };
        _itemRepository
            .Setup(x => x.GetAllWithLotsAsync(CancellationToken.None))
            .ReturnsAsync([item1, item2]);
        _lotRepository
            .Setup(x => x.GetUsableQuantityByItemIdsAsync(
                It.Is<IEnumerable<Guid>>(ids =>
                    ids.Count() == 2 &&
                    ids.Contains(item1.Id) &&
                    ids.Contains(item2.Id)),
                DateOnly.FromDateTime(FixedNow.UtcDateTime),
                CancellationToken.None))
            .ReturnsAsync(
                new Dictionary<Guid, decimal>
                {
                    [item1.Id] = 50m,
                    [item2.Id] = 20m
                });
        var result = await _service.GetStockExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
    }
    [Test]
    public async Task GetStockExport_ShouldUseZero_WhenUsableQuantityDoesNotExist()
    {
        var item = CreateItem();
        var filter = new StockExportFilterDto
        {
            Format = "csv",
            ItemId = item.Id
        };
        _itemRepository
            .Setup(x => x.GetByIdWithLotsAsync(
                item.Id,
                CancellationToken.None))
            .ReturnsAsync(item);
        _lotRepository
            .Setup(x => x.GetUsableQuantityByItemIdsAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                CancellationToken.None))
            .ReturnsAsync(
                new Dictionary<Guid, decimal>());
        var result = await _service.GetStockExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.Single().UsableStock, Is.EqualTo(0m));
    }
    [Test]
    public async Task GetStockExport_ShouldReturnOnlyLowStockRows_WhenFilterIsTrue()
    {
        var lowStockItem = CreateItem(
            "ITEM-LOW",
            "Low Stock Item",
            100m);
        var sufficientItem = CreateItem(
            "ITEM-OK",
            "Sufficient Item");
        var filter = new StockExportFilterDto
        {
            Format = "csv",
            IsLowStock = true
        };
        _itemRepository
            .Setup(x => x.GetAllWithLotsAsync(CancellationToken.None))
            .ReturnsAsync([lowStockItem, sufficientItem]);
        _lotRepository
            .Setup(x => x.GetUsableQuantityByItemIdsAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                CancellationToken.None))
            .ReturnsAsync(
                new Dictionary<Guid, decimal>
                {
                    [lowStockItem.Id] = 20m,
                    [sufficientItem.Id] = 50m
                });
        var result = await _service.GetStockExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(
            result.Data!.Single().ItemId,
            Is.EqualTo(lowStockItem.Id));
        Assert.That(
            result.Data?.Single().IsLowStock,
            Is.True);
    }
    [Test]
    public async Task GetStockExport_ShouldReturnOnlyNonLowStockRows_WhenFilterIsFalse()
    {
        var lowStockItem = CreateItem(
            "ITEM-LOW",
            "Low Stock Item",
            100m);
        var sufficientItem = CreateItem(
            "ITEM-OK",
            "Sufficient Item");
        var filter = new StockExportFilterDto
        {
            Format = "csv",
            IsLowStock = false
        };
        _itemRepository
            .Setup(x => x.GetAllWithLotsAsync(CancellationToken.None))
            .ReturnsAsync([lowStockItem, sufficientItem]);
        _lotRepository
            .Setup(x => x.GetUsableQuantityByItemIdsAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                CancellationToken.None))
            .ReturnsAsync(
                new Dictionary<Guid, decimal>
                {
                    [lowStockItem.Id] = 20m,
                    [sufficientItem.Id] = 50m
                });
        var result = await _service.GetStockExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(
            result.Data!.Single().ItemId,
            Is.EqualTo(sufficientItem.Id));
        Assert.That(
            result?.Data?.Single().IsLowStock,
            Is.False);
    }
    [Test]
    public async Task GetConsumptionExport_ShouldReturnError_WhenFormatIsInvalid()
    {
        var filter = new ConsumptionExportFilterDto
        {
            Format = "xlsx"
        };
        var result = await _service.GetConsumptionExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "Invalid format. Only 'csv' is currently supported."));
        _consumptionRepository.Verify(
            x => x.SearchAsync(
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task GetConsumptionExport_ShouldReturnError_WhenDateRangeIsInvalid()
    {
        var filter = new ConsumptionExportFilterDto
        {
            Format = "csv",
            From = FixedNow,
            To = FixedNow.AddDays(-1)
        };
        var result = await _service.GetConsumptionExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Invalid date range: From must be <= To."));
        _consumptionRepository.Verify(
            x => x.SearchAsync(
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task GetConsumptionExport_ShouldReturnEmpty_WhenNoConsumptionsExist()
    {
        var filter = new ConsumptionExportFilterDto
        {
            Format = "csv"
        };
        _consumptionRepository
            .Setup(x => x.SearchAsync(
                null,
                null,
                null,
                null,
                null,
                CancellationToken.None))
            .ReturnsAsync([]);
        var result = await _service.GetConsumptionExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Empty);
        Assert.That(
            result.Message,
            Is.EqualTo("Consumption export retrieved."));
        _lotRepository.Verify(
            x => x.GetByIdsAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _userLookupService.Verify(
            x => x.GetDisplayNamesAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task GetConsumptionExport_ShouldReturnRows_WhenConsumptionsExist()
    {
        var item = CreateItem();
        var lot = CreateLot(item, 100m);
        var jobId = Guid.NewGuid();
        var consumedBy = Guid.NewGuid();
        var consumption = new Consumption(
            Guid.NewGuid(),
            jobId,
            lot.Id,
            25m,
            FixedNow,
            consumedBy);
        var filter = new ConsumptionExportFilterDto
        {
            Format = "csv"
        };
        _consumptionRepository
            .Setup(x => x.SearchAsync(
                null,
                null,
                null,
                null,
                null,
                CancellationToken.None))
            .ReturnsAsync([consumption]);
        _lotRepository
            .Setup(x => x.GetByIdsAsync(
                It.Is<IEnumerable<Guid>>(ids =>
                    ids.Single() == lot.Id),
                CancellationToken.None))
            .ReturnsAsync([lot]);
        _userLookupService
            .Setup(x => x.GetDisplayNamesAsync(
                It.Is<IEnumerable<Guid>>(ids =>
                    ids.Single() == consumedBy),
                CancellationToken.None))
            .ReturnsAsync(
                new Dictionary<Guid, string>
                {
                    [consumedBy] = "Scientist One"
                });
        var result = await _service.GetConsumptionExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
        var row = result.Data!.Single();
        Assert.That(row.Id, Is.EqualTo(consumption.Id));
        Assert.That(row.JobId, Is.EqualTo(jobId));
        Assert.That(row.LotId, Is.EqualTo(lot.Id));
        Assert.That(row.ItemId, Is.EqualTo(item.Id));
        Assert.That(row.ItemName, Is.EqualTo(item.Name));
        Assert.That(row.Quantity, Is.EqualTo(25m));
        Assert.That(row.BaseUnit, Is.EqualTo(item.BaseUnit));
        Assert.That(row.ConsumedAt, Is.EqualTo(FixedNow));
        Assert.That(row.ConsumedBy, Is.EqualTo("Scientist One"));
    }
    [Test]
    public async Task GetConsumptionExport_ShouldUseUserId_WhenDisplayNameIsNotFound()
    {
        var item = CreateItem();
        var lot = CreateLot(item, 100m);
        var consumedBy = Guid.NewGuid();
        var consumption = new Consumption(
            Guid.NewGuid(),
            Guid.NewGuid(),
            lot.Id,
            10m,
            FixedNow,
            consumedBy);
        var filter = new ConsumptionExportFilterDto
        {
            Format = "csv"
        };
        _consumptionRepository
            .Setup(x => x.SearchAsync(
                null,
                null,
                null,
                null,
                null,
                CancellationToken.None))
            .ReturnsAsync([consumption]);
        _lotRepository
            .Setup(x => x.GetByIdsAsync(
                It.IsAny<IEnumerable<Guid>>(),
                CancellationToken.None))
            .ReturnsAsync([lot]);
        _userLookupService
            .Setup(x => x.GetDisplayNamesAsync(
                It.IsAny<IEnumerable<Guid>>(),
                CancellationToken.None))
            .ReturnsAsync(
                new Dictionary<Guid, string>());
        var result = await _service.GetConsumptionExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(
            result.Data!.Single().ConsumedBy,
            Is.EqualTo(consumedBy.ToString()));
    }
    [Test]
    public async Task GetDisposalExport_ShouldReturnError_WhenFormatIsInvalid()
    {
        var filter = new DisposalExportFilterDto
        {
            Format = "xlsx"
        };
        var result = await _service.GetDisposalExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "Invalid format. Only 'csv' is currently supported."));
        _disposalRepository.Verify(
            x => x.SearchAsync(
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task GetDisposalExport_ShouldReturnError_WhenDateRangeIsInvalid()
    {
        var filter = new DisposalExportFilterDto
        {
            Format = "csv",
            From = FixedNow,
            To = FixedNow.AddDays(-1)
        };
        var result = await _service.GetDisposalExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Invalid date range: From must be <= To."));
        _disposalRepository.Verify(
            x => x.SearchAsync(
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task GetDisposalExport_ShouldReturnEmpty_WhenNoDisposalsExist()
    {
        var filter = new DisposalExportFilterDto
        {
            Format = "csv"
        };
        _disposalRepository
            .Setup(x => x.SearchAsync(
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Disposal>());
        _userLookupService
            .Setup(x => x.GetDisplayNamesAsync(
                It.Is<IEnumerable<Guid>>(ids => !ids.Any()),
                CancellationToken.None))
            .ReturnsAsync(
                new Dictionary<Guid, string>());
        var result = await _service.GetDisposalExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Empty);
        Assert.That(
            result.Message,
            Is.EqualTo("Disposal export retrieved."));
    }
    [Test]
    public async Task GetDisposalExport_ShouldReturnRows_WhenDisposalsExist()
    {
        var item = CreateItem();
        var lot = CreateLot(item, 100m);
        var disposedBy = Guid.NewGuid();
        var disposal = new Disposal(
            Guid.NewGuid(),
            lot.Id,
            20m,
            "Expired",
            FixedNow,
            disposedBy);
        SetDisposalLot(disposal, lot);
        var filter = new DisposalExportFilterDto
        {
            Format = "csv"
        };
        _disposalRepository
            .Setup(x => x.SearchAsync(
                null,
                null,
                null,
                null,
                CancellationToken.None))
            .ReturnsAsync([disposal]);
        _userLookupService
            .Setup(x => x.GetDisplayNamesAsync(
                It.Is<IEnumerable<Guid>>(ids =>
                    ids.Single() == disposedBy),
                CancellationToken.None))
            .ReturnsAsync(
                new Dictionary<Guid, string>
                {
                    [disposedBy] = "Warehouse Admin"
                });
        var result = await _service.GetDisposalExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
        var row = result.Data!.Single();
        Assert.That(row.Id, Is.EqualTo(disposal.Id));
        Assert.That(row.LotId, Is.EqualTo(lot.Id));
        Assert.That(row.LotNumber, Is.EqualTo(lot.LotNumber));
        Assert.That(row.ItemId, Is.EqualTo(item.Id));
        Assert.That(row.ItemName, Is.EqualTo(item.Name));
        Assert.That(row.Quantity, Is.EqualTo(20m));
        Assert.That(row.BaseUnit, Is.EqualTo(item.BaseUnit));
        Assert.That(row.Reason, Is.EqualTo("Expired"));
        Assert.That(row.DisposedAt, Is.EqualTo(FixedNow));
        Assert.That(row.DisposedBy, Is.EqualTo("Warehouse Admin"));
    }
    [Test]
    public async Task GetDisposalExport_ShouldUseUserId_WhenDisplayNameIsNotFound()
    {
        var item = CreateItem();
        var lot = CreateLot(item, 100m);
        var disposedBy = Guid.NewGuid();
        var disposal = new Disposal(
            Guid.NewGuid(),
            lot.Id,
            10m,
            "Expired",
            FixedNow,
            disposedBy);
        SetDisposalLot(disposal, lot);
        var filter = new DisposalExportFilterDto
        {
            Format = "csv"
        };
        _disposalRepository
            .Setup(x => x.SearchAsync(
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Disposal> { disposal });
        _userLookupService
            .Setup(x => x.GetDisplayNamesAsync(
                It.IsAny<IEnumerable<Guid>>(),
                CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, string>());
        var result = await _service.GetDisposalExport(
            filter,
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(
            result.Data!.Single().DisposedBy,
            Is.EqualTo(disposedBy.ToString()));
    }
    private static Item CreateItem(
        string code = "ITEM-001",
        string name = "Test Item",
        decimal minimumStock = 10m)
    {
        return new Item(
            Guid.NewGuid(),
            code,
            name,
            UnitOfMeasure.Unit,
            minimumStock,
            30);
    }
    private static Lot CreateLot(
        Item item,
        decimal remainingQuantity)
    {
        var lot = new Lot(
            Guid.NewGuid(),
            item.Id,
            "LOT-001",
            "SUP-001",
            FixedNow,
            "Test Supplier",
            100m,
            remainingQuantity,
            DateOnly.FromDateTime(
                FixedNow.UtcDateTime.AddDays(60)),
            "Storage A",
            LotAdministrativeStatus.Active);
        typeof(Lot)
            .GetField(
                "<Item>k__BackingField",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)!
            .SetValue(lot, item);
        return lot;
    }
    private static void SetDisposalLot(
        Disposal disposal,
        Lot lot)
    {
        typeof(Disposal)
            .GetProperty(
                nameof(Disposal.Lot))!
            .SetValue(
                disposal,
                lot);
    }
}