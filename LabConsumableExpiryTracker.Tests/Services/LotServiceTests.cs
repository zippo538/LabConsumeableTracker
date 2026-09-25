using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Application.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using System.Reflection;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;
using LabConsumableExpiryTracker.Application.Common.Result;
namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class LotServiceTests
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
    private Mock<ILotRepository> _lotRepository = null!;
    private Mock<IItemRepository> _itemRepository = null!;
    private Mock<IJobRepository> _jobRepository = null!;
    private Mock<IConsumptionRepository> _consumptionRepository = null!;
    private Mock<IMapper> _mapper = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private Mock<ILotSelectionService> _lotSelectionService = null!;
    private Mock<TimeProvider> _timeProvider = null!;
    private LotService _service = null!;
    [SetUp]
    public void Setup()
    {
        _lotRepository =
            new Mock<ILotRepository>(
                MockBehavior.Strict);
        _itemRepository =
            new Mock<IItemRepository>(
                MockBehavior.Strict);
        _jobRepository =
            new Mock<IJobRepository>(
                MockBehavior.Strict);
        _consumptionRepository =
            new Mock<IConsumptionRepository>(
                MockBehavior.Strict);
        _mapper =
            new Mock<IMapper>(
                MockBehavior.Strict);
        _unitOfWork =
            new Mock<IUnitOfWork>(
                MockBehavior.Strict);
        _lotSelectionService =
            new Mock<ILotSelectionService>(
                MockBehavior.Strict);
        _timeProvider =
            new Mock<TimeProvider>(
                MockBehavior.Strict);
        _timeProvider
            .Setup(x =>
                x.GetUtcNow())
            .Returns(FixedNow);
        _service =
            new LotService(
                _lotRepository.Object,
                _itemRepository.Object,
                _jobRepository.Object,
                _consumptionRepository.Object,
                _mapper.Object,
                _unitOfWork.Object,
                NullLogger<LotService>.Instance,
                _lotSelectionService.Object,
                _timeProvider.Object
            );
    }
    [Test]
    public async Task GetAllLot_WhenDataExists_ReturnsSuccess()
    {
        var lot =
            CreateLot();
        var lots =
            new List<Lot>
            {
                lot
            };
        var dto =
            CreateLotDto(
                lot.Id);
        _lotRepository
            .Setup(x =>
                x.GetAllWithItemAsync(
                    CancellationToken.None))
            .ReturnsAsync(lots);
        _mapper
            .Setup(x =>
                x.Map<LotDto>(
                    lot))
            .Returns(dto);
        var result =
            await _service.GetAllLot(
                CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(
                result.Success,
                Is.True);
            Assert.That(
                result.Data!.Count(),
                Is.EqualTo(1));
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Lots retrieved successfully."));
        });
    }
    [Test]
    public Task GetAllLot_WhenRepositoryThrows_ExceptionPropagated()
    {
        _lotRepository
            .Setup(x =>
                x.GetAllWithItemAsync(
                    CancellationToken.None))
            .ThrowsAsync(
                new Exception(
                    "Database error"));
        Assert.ThrowsAsync<Exception>(
            async () =>
            {
                await _service.GetAllLot(
                    CancellationToken.None);
            });
        return null!;
    }
    [Test]
    public async Task GetByIdLot_WhenLotNotFound_ReturnsError()
    {
        var id =
            Guid.NewGuid();
        _lotRepository
            .Setup(x =>
                x.GetByIdWithItemAsync(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(
                (Lot?)null);
        var result =
            await _service.GetByIdLot(
                id,
                CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(
                result.Success,
                Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "NotFound"));
            Assert.That(
                result.Data,
                Is.Null);
        });
    }
    [Test]
    public async Task GetByIdLot_WhenFound_ReturnsSuccess()
    {
        var lot =
            CreateLot();
        var dto =
            CreateLotDto(
                lot.Id);
        _lotRepository
            .Setup(x =>
                x.GetByIdWithItemAsync(
                    lot.Id,
                    CancellationToken.None))
            .ReturnsAsync(lot);
        _mapper
            .Setup(x =>
                x.Map<LotDto>(
                    lot))
            .Returns(dto);
        var result =
            await _service.GetByIdLot(
                lot.Id,
                CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(
                result.Success,
                Is.True);
            Assert.That(
                result.Data,
                Is.EqualTo(dto));
        });
    }
    [Test]
    public async Task CreateLot_WhenItemIdInvalid_ReturnsValidationError()
    {
        var request =
            CreateLotRequest();
        request.ItemId =
            "abc";
        var result =
            await _service.CreateLot(
                request,
                CancellationToken.None);
        Assert.That(
            result.Success,
            Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "Validation failed."));
    }
    [Test]
    public async Task CreateLot_WhenLotNumberEmpty_ReturnsError()
    {
        var request =
            CreateLotRequest();
        request.LotNumber =
            "";
        var result =
            await _service.CreateLot(
                request,
                CancellationToken.None);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "Lot number is required."));
    }
    [Test]
    public async Task CreateLot_WhenQuantityInvalid_ReturnsError()
    {
        var request =
            CreateLotRequest();
        request.InitialQuantity =
            0;
        var result =
            await _service.CreateLot(
                request,
                CancellationToken.None);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "Initial quantity must be greater than zero."));
    }
    [Test]
    public async Task CreateLot_WhenDuplicate_ReturnsError()
    {
        var request =
            CreateLotRequest();
        _lotRepository
            .Setup(x =>
                x.ExistsAsync(
                    Guid.Parse(
                        request.ItemId),
                    request.LotNumber.Trim(),
                    request.ExpiryDate,
                    CancellationToken.None))
            .ReturnsAsync(true);
        var result =
            await _service.CreateLot(
                request,
                CancellationToken.None);
        Assert.That(
            result.Success,
            Is.False);
        Assert.That(
            result.Message,
            Does.Contain(
                "already exists"));
    }
    [Test]
    public async Task UpdateLot_WhenLotNotFound_ReturnsError()
    {
        var lotId =
            Guid.NewGuid();
        _lotRepository
            .Setup(x =>
                x.GetByIdWithItemAsync(
                    lotId,
                    CancellationToken.None))
            .ReturnsAsync(
                (Lot?)null);
        var result =
            await _service.UpdateLot(
                lotId,
                CreateUpdateRequest(),
                CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(
                result.Success,
                Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Lot not found."));
        });
    }
    [Test]
    public async Task UpdateLot_WhenRowVersionEmpty_ReturnsError()
    {
        var lot =
            CreateLot();
        var request =
            CreateUpdateRequest();
        request.RowVersion =
            Array.Empty<byte>();
        _lotRepository
            .Setup(x =>
                x.GetByIdWithItemAsync(
                    lot.Id,
                    CancellationToken.None))
            .ReturnsAsync(lot);
        var result =
            await _service.UpdateLot(
                lot.Id,
                request,
                CancellationToken.None);
        Assert.That(
            result.Success,
            Is.False);
    }
    [Test]
    public async Task UpdateLot_WhenValid_ReturnsSuccess()
    {
        var lot =
            CreateLot();
        var request =
            CreateUpdateRequest();
        var dto =
            CreateLotDto(
                lot.Id);
        _lotRepository
            .Setup(x =>
                x.GetByIdWithItemAsync(
                    lot.Id,
                    CancellationToken.None))
            .ReturnsAsync(lot);
        _lotRepository
            .Setup(x =>
                x.UpdateAsync(
                    lot,
                    CancellationToken.None))
            .ReturnsAsync(lot);
        _unitOfWork
            .Setup(x =>
                x.SaveChangesAsync(
                    CancellationToken.None))
            .ReturnsAsync(1);
        _mapper
            .Setup(x =>
                x.Map<LotDto>(
                    lot))
            .Returns(dto);
        var result =
            await _service.UpdateLot(
                lot.Id,
                request,
                CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(
                result.Success,
                Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Lot updated successfully."));
        });
    }
    [Test]
    public async Task DeleteLot_WhenLotNotFound_ReturnsError()
    {
        var id =
            Guid.NewGuid();
        _lotRepository
            .Setup(x =>
                x.DeleteAsync(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(false);
        _unitOfWork
            .Setup(x =>
                x.SaveChangesAsync(
                    CancellationToken.None))
            .ReturnsAsync(0);
        var result =
            await _service.DeleteLot(
                id,
                CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(
                result.Success,
                Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Lot not found."));
        });
    }
    [Test]
    public async Task DeleteLot_WhenSuccess_ReturnsTrue()
    {
        var id =
            Guid.NewGuid();
        _lotRepository
            .Setup(x =>
                x.DeleteAsync(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(true);
        _unitOfWork
            .Setup(x =>
                x.SaveChangesAsync(
                    CancellationToken.None))
            .ReturnsAsync(1);
        var result =
            await _service.DeleteLot(
                id,
                CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(
                result.Success,
                Is.True);
            Assert.That(
                result.Data,
                Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Lot deleted successfully."));
        });
    }
    [Test]
    public async Task GetSummaryByItemId_WhenSuccess_ReturnsData()
    {
        var itemId =
            Guid.NewGuid();
        var lots =
            new List<Lot>
            {
            CreateLot(itemId)
            };
        var summaries =
            new List<LotSummaryDto>
            {
            CreateSummaryDto()
            };
        _lotRepository
            .Setup(x =>
                x.GetByItemIdAsync(
                    itemId,
                    CancellationToken.None))
            .ReturnsAsync(lots);
        _mapper
            .Setup(x =>
                x.Map<IEnumerable<LotSummaryDto>>(
                    lots))
            .Returns(summaries);
        var result =
            await _service.GetSummaryByItemId(
                itemId,
                CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(
                result.Success,
                Is.True);
            Assert.That(
                result.Data,
                Is.EqualTo(
                    summaries));
        });
    }
    [Test]
    public async Task GetAllSummary_WhenSuccess_ReturnsData()
    {
        var lots =
            new List<Lot>
            {
            CreateLot()
            };
        var summaries =
            new List<LotSummaryDto>
            {
            CreateSummaryDto()
            };
        _lotRepository
            .Setup(x =>
                x.GetAllAsync(
                    CancellationToken.None))
            .ReturnsAsync(lots);
        _mapper
            .Setup(x =>
                x.Map<IEnumerable<LotSummaryDto>>(
                    lots))
            .Returns(summaries);
        var result =
            await _service.GetAllSummary(
                CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(
                result.Success,
                Is.True);
            Assert.That(
                result.Data,
                Is.EqualTo(
                    summaries));
        });
    }
    [Test]
    public async Task GetAllLotPage_WhenSuccess_ReturnsPagedData()
    {
        var parameters =
            new LotQueryParameters
            {
                Page = 1,
                PageSize = 20
            };
        var lot =
            CreateLot();
        var repositoryResult =
            new PagedResult<Lot>
            {
                Items =
                    new List<Lot>
                    {
                    lot
                    },
                Page =
                    1,
                PageSize =
                    20,
                TotalCount =
                    1
            };
        var dto =
            CreateLotDto(
                lot.Id);
        _lotRepository
         .Setup(x =>
        x.GetAllWithItemAsyncPage(
            1,
            20,
            null,
            null,
            null,
            null,
            CancellationToken.None))
    .ReturnsAsync(repositoryResult);
        _mapper
            .Setup(x =>
                x.Map<LotDto>(
                    lot))
            .Returns(dto);
        var result =
            await _service.GetAllLotPage(
                CancellationToken.None,
                1,
                20,
                null,
                null,
                null,
                null);
        Assert.Multiple(() =>
        {
            Assert.That(
                result.Success,
                Is.True);
            Assert.That(
                result.Data,
                Is.Not.Null);
            Assert.That(
                result.Data!.Items.Count(),
                Is.EqualTo(1));
        });
    }
    [Test]
    public async Task RequestFefoAllocation_WhenJobNotFound_ReturnsError()
    {
        var jobId =
            Guid.NewGuid();
        _jobRepository
            .Setup(x =>
                x.GetByIdAsync(
                    jobId,
                    CancellationToken.None))
            .ReturnsAsync(
                (Job?)null);
        var result =
            await _service.RequestFefoAllocation(
                jobId,
                Guid.NewGuid(),
                10,
                CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(
                result.Success,
                Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Job not found."));
        });
    }
    [Test]
    public async Task RequestFefoAllocation_WhenQuantityInvalid_ReturnsError()
    {
        var job =
            new Job(
                Guid.NewGuid(),
                "JOB-001");
        _jobRepository
            .Setup(x =>
                x.GetByIdAsync(
                    job.Id,
                    CancellationToken.None))
            .ReturnsAsync(job);
        var result =
            await _service.RequestFefoAllocation(
                job.Id,
                Guid.NewGuid(),
                0,
                CancellationToken.None);
        Assert.That(
            result.Success,
            Is.False);
    }
    [Test]
    public async Task RequestFefoAllocation_WhenJobNotInProgress_ReturnsError()
    {
        var job =
            new Job(
                Guid.NewGuid(),
                "JOB-001");
        _jobRepository
            .Setup(x =>
                x.GetByIdAsync(
                    job.Id,
                    CancellationToken.None))
            .ReturnsAsync(job);
        var result =
            await _service.RequestFefoAllocation(
                job.Id,
                Guid.NewGuid(),
                10,
                CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(
                result.Success,
                Is.False);
            Assert.That(
                result.Message,
                Does.Contain(
                    "must be InProgress"));
        });
    }
    [Test]
    public async Task RequestFefoAllocation_WhenValid_ReturnsSuccess()
    {
        var job =
            new Job(
                Guid.NewGuid(),
                "JOB-001");
        job.Start(FixedNow);
        var itemId = Guid.NewGuid();
        var item = CreateItem(itemId);
        var lot = CreateLot(itemId);
        var allocationResult = ServiceResult<IReadOnlyList<LotAllocation>>
            .SuccessResult(
                new List<LotAllocation>(),
                "Allocation success");
        _jobRepository
            .Setup(x =>
                x.GetByIdAsync(
                    job.Id,
                    CancellationToken.None))
            .ReturnsAsync(job);

        _lotRepository
    .Setup(x =>
        x.GetByItemIdAsync(
            itemId,
            CancellationToken.None))
    .ReturnsAsync(
        new List<Lot>
        {
            lot
        });
        _lotSelectionService
            .Setup(x =>
                x.AllocateAsync(
                    itemId,
                    10,
                    FixedNow,
                    CancellationToken.None))
            .ReturnsAsync(allocationResult);
        var result =
            await _service.RequestFefoAllocation(
                job.Id,
                itemId,
                10,
                CancellationToken.None);
        Assert.That(
            result.Success,
            Is.True);
    }
    [Test]
    public async Task ConfirmConsumption_WhenJobNotFound_ReturnsError()
    {
        var jobId =
            Guid.NewGuid();
        _jobRepository
            .Setup(x =>
                x.GetByIdAsync(
                    jobId,
                    CancellationToken.None))
            .ReturnsAsync(
                (Job?)null);
        var request =
            new ConfirmConsumptionRequestDto();
        var result =
            await _service.ConfirmConsumption(
                jobId,
                request,
                Guid.NewGuid(),
                CancellationToken.None);
        Assert.That(
            result.Success,
            Is.False);
    }
    private static CreateLotDto CreateLotRequest()
    {
        return new CreateLotDto
        {
            ItemId =
                Guid.NewGuid()
                .ToString(),
            LotNumber =
                "LOT-001",
            SupplierLotNumber =
                "SUP-001",
            SupplierName =
                "Supplier Test",
            InitialQuantity =
                100,
            ReceivedAt =
                FixedNow,
            ExpiryDate =
                new DateOnly(
                    2027,
                    1,
                    1),
            StorageLocation =
                "Rack-A"
        };
    }
    private static UpdateLotDto CreateUpdateRequest()
    {
        return new UpdateLotDto
        {
            LotNumber =
                "LOT-UPDATED",
            AdministrativeStatus =
                LotAdministrativeStatus.Active,
            SupplierLotNumber =
                "SUP-UPDATED",
            SupplierName =
                "Supplier Updated",
            ExpiryDate =
                new DateOnly(
                    2027,
                    2,
                    1),
            StorageLocation =
                "Rack-B",
            RowVersion =
                new byte[]
                {
                1,
                2,
                3
                }
        };
    }
    private static LotDto CreateLotDto(Guid id)
    {
        return new LotDto
        {
            Id =
                id,
            ItemId =
                Guid.NewGuid(),
            ItemName =
                "Test Item",
            ItemCode =
                "ITEM-001",
            BaseUnit =
                "Unit",
            Suppliername =
                "Supplier",
            SupplierLotNumber =
                "SUP-001",
            LotNumber =
                "LOT-001",
            ReceivedAt =
                FixedNow,
            InitialQuantity =
                100,
            RemainingQuantity =
                100,
            ExpiryDate =
                new DateOnly(
                    2027,
                    1,
                    1),
            StorageLocation =
                "Rack-A",
            AdministrativeStatus =
                LotAdministrativeStatus.Active,
            ExpiryCondition =
                ExpiryCondition.Valid,
            IsEligible =
                true,
            EvaluatedAt =
                FixedNow,
            RowVersion =
                new byte[]
                {
                1
                }
        };
    }
    private static LotSummaryDto CreateSummaryDto()
    {
        return new LotSummaryDto
        {
            LotId =
                Guid.NewGuid(),
            LotNumber =
                "LOT-001",
            RemainingQuantity =
                100,
            ExpiryDate =
                new DateOnly(
                    2027,
                    1,
                    1),
            Status =
                "Active"
        };
    }
    private static Lot CreateLot(
        Guid? itemId = null)
    {
        var id = itemId ?? Guid.NewGuid();
        var lot = new Lot(
            Guid.NewGuid(),
            id,
            "LOT-001",
            "SUP-001",
            FixedNow,
            "Supplier Test",
            100,
            100,
            new DateOnly(
                2027,
                1,
                1),
            "Rack-A",
            LotAdministrativeStatus.Active
        );


        var item = CreateItem(id);
        typeof(Lot)
            .GetProperty(nameof(Lot.Item),
                BindingFlags.Instance | BindingFlags.Public)
            ?.SetValue(lot, item);
        return lot;
    }
    private static Item CreateItem(
        Guid id)
    {
        return new Item(
            id,
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10,
            30
        );
    }
}