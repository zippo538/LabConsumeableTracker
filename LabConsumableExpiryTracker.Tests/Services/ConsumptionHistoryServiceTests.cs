using LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class ConsumptionHistoryServiceTests
{
    private Mock<IConsumptionHistoryRepository> _repository = null!;
    private ConsumptionHistoryService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repository =
            new Mock<IConsumptionHistoryRepository>(MockBehavior.Strict);

        _service = new ConsumptionHistoryService(
            _repository.Object);
    }

    [Test]
    public async Task GetMyConsumptions_ShouldNormalizePageAndPageSize_WhenValuesAreInvalid()
    {
        var consumedBy = Guid.NewGuid();

        var query = new ConsumptionListQueryDto
        {
            Page = 0,
            PageSize = 0
        };

        IReadOnlyList<ConsumptionListItemDto> items = [];

        _repository
            .Setup(x => x.GetPagedAsync(
                consumedBy,
                query.JobId,
                query.ItemId,
                query.LotId,
                query.LotNumber,
                query.From,
                query.To,
                1,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 0));

        var result = await _service.GetMyConsumptions(
            consumedBy,
            query,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Page, Is.EqualTo(1));
            Assert.That(result.Data.PageSize, Is.EqualTo(20));
            Assert.That(result.Data.TotalCount, Is.EqualTo(0));
            Assert.That(result.Data.Items, Is.Empty);
        });
    }

    [Test]
    public async Task GetMyConsumptions_ShouldNormalizePageSize_WhenPageSizeIsGreaterThan100()
    {
        var consumedBy = Guid.NewGuid();

        var query = new ConsumptionListQueryDto
        {
            Page = 2,
            PageSize = 101
        };

        IReadOnlyList<ConsumptionListItemDto> items = [];

        _repository
            .Setup(x => x.GetPagedAsync(
                consumedBy,
                query.JobId,
                query.ItemId,
                query.LotId,
                query.LotNumber,
                query.From,
                query.To,
                2,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 0));

        var result = await _service.GetMyConsumptions(
            consumedBy,
            query,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data!.Page, Is.EqualTo(2));
            Assert.That(result.Data.PageSize, Is.EqualTo(20));
        });
    }

    [Test]
    public async Task GetMyConsumptions_ShouldUseProvidedPageAndPageSize_WhenValuesAreValid()
    {
        var consumedBy = Guid.NewGuid();

        var query = new ConsumptionListQueryDto
        {
            Page = 3,
            PageSize = 50
        };

        var item = new ConsumptionListItemDto();

        IReadOnlyList<ConsumptionListItemDto> items =
            [item];

        _repository
            .Setup(x => x.GetPagedAsync(
                consumedBy,
                query.JobId,
                query.ItemId,
                query.LotId,
                query.LotNumber,
                query.From,
                query.To,
                3,
                50,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 1));

        var result = await _service.GetMyConsumptions(
            consumedBy,
            query,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo("Consumptions retrieved successfully."));

            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Page, Is.EqualTo(3));
            Assert.That(result.Data.PageSize, Is.EqualTo(50));
            Assert.That(result.Data.TotalCount, Is.EqualTo(1));
            Assert.That(result.Data.Items, Has.Count.EqualTo(1));
        });
        Assert.That(result.Data.Items[0], Is.EqualTo(item));
    }

    [Test]
    public async Task GetMyConsumptions_ShouldPassAllFiltersToRepository()
    {
        var consumedBy = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var lotId = Guid.NewGuid();

        var from = new DateTimeOffset(
            2026,
            9,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);

        var to = new DateTimeOffset(
            2026,
            9,
            25,
            23,
            59,
            59,
            TimeSpan.Zero);

        var query = new ConsumptionListQueryDto
        {
            JobId = jobId,
            ItemId = itemId,
            LotId = lotId,
            LotNumber = "LOT-001",
            From = from,
            To = to,
            Page = 1,
            PageSize = 20
        };

        IReadOnlyList<ConsumptionListItemDto> items = [];

        _repository
            .Setup(x => x.GetPagedAsync(
                consumedBy,
                jobId,
                itemId,
                lotId,
                "LOT-001",
                from,
                to,
                1,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 0));

        var result = await _service.GetMyConsumptions(
            consumedBy,
            query,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);

        _repository.Verify(
            x => x.GetPagedAsync(
                consumedBy,
                jobId,
                itemId,
                lotId,
                "LOT-001",
                from,
                to,
                1,
                20,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetConsumptionDetail_ShouldReturnError_WhenConsumptionNotFound()
    {
        var consumptionId = Guid.NewGuid();
        var consumedBy = Guid.NewGuid();

        _repository
            .Setup(x => x.GetDetailAsync(
                consumptionId,
                consumedBy,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsumptionHistoryDetailDto?)null);

        var result = await _service.GetConsumptionDetail(
            consumptionId,
            consumedBy,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Consumption not found."));
        });
    }

    [Test]
    public async Task GetConsumptionDetail_ShouldReturnConsumption_WhenFound()
    {
        var consumptionId = Guid.NewGuid();
        var consumedBy = Guid.NewGuid();

        var detail = new ConsumptionHistoryDetailDto();

        _repository
            .Setup(x => x.GetDetailAsync(
                consumptionId,
                consumedBy,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        var result = await _service.GetConsumptionDetail(
            consumptionId,
            consumedBy,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo("Consumption retrieved successfully."));
            Assert.That(result.Data, Is.EqualTo(detail));
        });

        _repository.Verify(
            x => x.GetDetailAsync(
                consumptionId,
                consumedBy,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}