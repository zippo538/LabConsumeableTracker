using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Application.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
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
        Assert.Multiple(() =>
        {
            Assert.That(
                    result.Success,
                    Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Validation failed."));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(
                    result.Success,
                    Is.False);
            Assert.That(
                result.Message,
                Does.Contain(
                    "already exists"));
        });
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
            [
                1,
                2,
                3
            ]
        };
    }
    [Test]
    public async Task GetAllLot_ShouldReturnSuccess_WhenLotsExist()
    {
        var item = new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = new Lot(
            Guid.NewGuid(),
            item.Id,
            "LOT-001",
            "SUP-LOT-001",
            FixedNow,
            "Test Supplier",
            100m,
            100m,
            DateOnly.FromDateTime(FixedNow.UtcDateTime.AddDays(60)),
            "Storage A",
            LotAdministrativeStatus.Active);
        typeof(Lot)
            .GetField(
                "<Item>k__BackingField",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)!
            .SetValue(lot, item);
        var mappedDto = new LotDto();
        _lotRepository
            .Setup(x => x.GetAllWithItemAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Lot> { lot });
        _mapper
            .Setup(x => x.Map<LotDto>(lot))
            .Returns(mappedDto);
        var result = await _service.GetAllLot(CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Count(), Is.EqualTo(1));
            Assert.That(mappedDto.EvaluatedAt, Is.EqualTo(FixedNow));
            Assert.That(mappedDto.IsEligible, Is.True);
        });
        _lotRepository.Verify(
            x => x.GetAllWithItemAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _mapper.Verify(
            x => x.Map<LotDto>(lot),
            Times.Once);
    }
    [Test]
    public async Task GetAllLot_ShouldReturnEmptyResult_WhenNoLotsExist()
    {
        var lots = new List<Lot>();
        if (lots == null) throw new ArgumentNullException(nameof(lots));
        _lotRepository
            .Setup(x => x.GetAllWithItemAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lots);
        var result = await _service.GetAllLot(
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(
                result.Message,
                Is.EqualTo("Lots retrieved successfully."));
        });
        _lotRepository.Verify(
            x => x.GetAllWithItemAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
        _mapper.Verify(
            x => x.Map<LotDto>(It.IsAny<Lot>()),
            Times.Never);
    }
    [Test]
    public async Task CreateLot_ShouldReturnSuccess_WhenRequestIsValid()
    {
        var itemId = Guid.NewGuid();
        var createLotDto = new CreateLotDto
        {
            ItemId = itemId.ToString(),
            LotNumber = "LOT-001",
            SupplierLotNumber = "SUP-001",
            ReceivedAt = FixedNow,
            SupplierName = "Test Supplier",
            InitialQuantity = 100m,
            ExpiryDate = DateOnly.FromDateTime(
                FixedNow.UtcDateTime.AddDays(60)),
            StorageLocation = "Storage A"
        };
        _lotRepository
     .Setup(x => x.ExistsAsync(
         itemId,
         "LOT-001",
         createLotDto.ExpiryDate,
         It.IsAny<CancellationToken>()))
     .ReturnsAsync(false);
        var item = new Item(
            itemId,
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = Lot.Create(
            itemId,
            createLotDto.LotNumber,
            createLotDto.SupplierLotNumber,
            createLotDto.ReceivedAt.ToUniversalTime(),
            createLotDto.SupplierName,
            createLotDto.InitialQuantity,
            createLotDto.ExpiryDate,
            createLotDto.StorageLocation);
        var mappedDto = new LotDto();
        _itemRepository
            .Setup(x => x.GetByIdAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _lotRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Lot>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _mapper
            .Setup(x => x.Map<LotDto>(lot))
            .Returns(mappedDto);
        var result = await _service.CreateLot(
            createLotDto,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(
                result.Message,
                Is.EqualTo("Lot created successfully."));
        });
        _lotRepository.Verify(
            x => x.AddAsync(
                It.Is<Lot>(l =>
                    l.ItemId == itemId &&
                    l.LotNumber == "LOT-001" &&
                    l.InitialQuantity == 100m),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
        _itemRepository.Verify(
            x => x.GetByIdAsync(
                itemId,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _mapper.Verify(
            x => x.Map<LotDto>(lot),
            Times.Once);
    }
    [Test]
    public async Task UpdateLot_ShouldReturnError_WhenLotNotFound()
    {
        var id = Guid.NewGuid();
        _lotRepository
            .Setup(x => x.GetByIdWithItemAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lot?)null);
        var dto = new UpdateLotDto
        {
            RowVersion = [1, 2, 3]
        };
        var result = await _service.UpdateLot(
            id,
            dto,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Lot not found."));
        });
        _lotRepository.Verify(
            x => x.GetByIdWithItemAsync(
                id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Test]
    public async Task UpdateLot_ShouldReturnError_WhenRowVersionIsNull()
    {
        var item = new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = CreateTestLot(item);
        _lotRepository
            .Setup(x => x.GetByIdWithItemAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        var dto = new UpdateLotDto
        {
            RowVersion = null!
        };
        var result = await _service.UpdateLot(
            lot.Id,
            dto,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("RowVersion is required for update."));
        });
    }
    [Test]
    public async Task UpdateLot_ShouldReturnError_WhenRowVersionIsEmpty()
    {
        var item = new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = CreateTestLot(item);
        _lotRepository
            .Setup(x => x.GetByIdWithItemAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        var dto = new UpdateLotDto
        {
            RowVersion = []
        };
        var result = await _service.UpdateLot(
            lot.Id,
            dto,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("RowVersion is required for update."));
        });
    }
    private static Lot CreateTestLot(Item item)
    {
        var lot = new Lot(
            Guid.NewGuid(),
            item.Id,
            "LOT-001",
            "SUP-001",
            FixedNow,
            "Test Supplier",
            100m,
            100m,
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
    [Test]
    public async Task UpdateLot_ShouldReturnSuccess_WhenNoChangesDetected()
    {
        var item = new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = CreateTestLot(item);
        var dto = new UpdateLotDto
        {
            LotNumber = lot.LotNumber,
            SupplierLotNumber = lot.SupplierLotNumber,
            SupplierName = lot.SupplierName,
            StorageLocation = lot.StorageLocation,
            ExpiryDate = lot.ExpiryDate,
            AdministrativeStatus = lot.AdministrativeStatus,
            RowVersion = [1, 2, 3]
        };
        var mappedDto = new LotDto
        {
            Id = lot.Id,
            LotNumber = lot.LotNumber
        };
        _lotRepository
            .Setup(x => x.GetByIdWithItemAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _mapper
            .Setup(x => x.Map<LotDto>(lot))
            .Returns(mappedDto);
        var result = await _service.UpdateLot(
            lot.Id,
            dto,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo("No changes detected."));
            Assert.That(result.Data, Is.SameAs(mappedDto));
        });
        _lotRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<Lot>()),
            Times.Never);
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task UpdateLot_ShouldUpdateLotNumber()
    {
        var item = new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = CreateTestLot(item);
        var dto = new UpdateLotDto
        {
            RowVersion = [1, 2, 3],
            LotNumber = "LOT-002"
        };
        var mappedDto = new LotDto();
        _lotRepository
            .Setup(x => x.GetByIdWithItemAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        _lotRepository
            .Setup(x => x.UpdateAsync(
                lot))
            .ReturnsAsync(lot);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _mapper
            .Setup(x => x.Map<LotDto>(lot))
            .Returns(mappedDto);
        var result = await _service.UpdateLot(
            lot.Id,
            dto,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo("Lot updated successfully."));
            Assert.That(lot.LotNumber, Is.EqualTo("LOT-002"));
        });
    }
    [Test]
    public async Task UpdateLot_ShouldUpdateSupplierLotNumber()
    {
        var item = new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = CreateTestLot(item);
        var dto = new UpdateLotDto
        {
            RowVersion = [1, 2, 3],
            SupplierLotNumber = "SUP-002"
        };
        var mappedDto = new LotDto();
        _lotRepository
            .Setup(x => x.GetByIdWithItemAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        _lotRepository
            .Setup(x => x.UpdateAsync(
                lot))
            .ReturnsAsync(lot);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _mapper
            .Setup(x => x.Map<LotDto>(lot))
            .Returns(mappedDto);
        var result = await _service.UpdateLot(
            lot.Id,
            dto,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(lot.SupplierLotNumber, Is.EqualTo("SUP-002"));
        });
    }
    [Test]
    public async Task UpdateLot_ShouldUpdateSupplierName()
    {
        var item = new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = CreateTestLot(item);
        var dto = new UpdateLotDto
        {
            RowVersion = [1, 2, 3],
            SupplierName = "New Supplier"
        };
        var mappedDto = new LotDto();
        _lotRepository
            .Setup(x => x.GetByIdWithItemAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        _lotRepository
            .Setup(x => x.UpdateAsync(
                lot))
            .ReturnsAsync(lot);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _mapper
            .Setup(x => x.Map<LotDto>(lot))
            .Returns(mappedDto);
        var result = await _service.UpdateLot(
            lot.Id,
            dto,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(lot.SupplierName, Is.EqualTo("New Supplier"));
        });
    }
    [Test]
    public async Task UpdateLot_ShouldUpdateStorageLocation()
    {
        var item = new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = CreateTestLot(item);
        var dto = new UpdateLotDto
        {
            RowVersion = [1, 2, 3],
            StorageLocation = "Storage B"
        };
        var mappedDto = new LotDto();
        _lotRepository
            .Setup(x => x.GetByIdWithItemAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        _lotRepository
            .Setup(x => x.UpdateAsync(
                lot))
            .ReturnsAsync(lot);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _mapper
            .Setup(x => x.Map<LotDto>(lot))
            .Returns(mappedDto);
        var result = await _service.UpdateLot(
            lot.Id,
            dto,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(lot.StorageLocation, Is.EqualTo("Storage B"));
        });
    }
    [Test]
    public async Task UpdateLot_ShouldUpdateExpiryDate()
    {
        var item = new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = CreateTestLot(item);
        var newExpiryDate = DateOnly.FromDateTime(
            FixedNow.UtcDateTime.AddDays(90));
        var dto = new UpdateLotDto
        {
            RowVersion = [1, 2, 3],
            ExpiryDate = newExpiryDate
        };
        var mappedDto = new LotDto();
        _lotRepository
            .Setup(x => x.GetByIdWithItemAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        _lotRepository
            .Setup(x => x.UpdateAsync(
                lot))
            .ReturnsAsync(lot);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _mapper
            .Setup(x => x.Map<LotDto>(lot))
            .Returns(mappedDto);
        var result = await _service.UpdateLot(
            lot.Id,
            dto,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(lot.ExpiryDate, Is.EqualTo(newExpiryDate));
        });
    }
    [Test]
    public async Task UpdateLot_ShouldUpdateAdministrativeStatus()
    {
        var item = new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = CreateTestLot(item);
        var dto = new UpdateLotDto
        {
            RowVersion = [1, 2, 3],
            AdministrativeStatus =
                LotAdministrativeStatus.Quarantined
        };
        var mappedDto = new LotDto();
        _lotRepository
            .Setup(x => x.GetByIdWithItemAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        _lotRepository
            .Setup(x => x.UpdateAsync(
                lot))
            .ReturnsAsync(lot);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _mapper
            .Setup(x => x.Map<LotDto>(lot))
            .Returns(mappedDto);
        var result = await _service.UpdateLot(
            lot.Id,
            dto,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(
                lot.AdministrativeStatus,
                Is.EqualTo(LotAdministrativeStatus.Quarantined));
        });
    }
    [Test]
    public async Task UpdateLot_ShouldUpdateAllFields_WhenAllChangesProvided()
    {
        var item = new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = CreateTestLot(item);
        var newExpiryDate = DateOnly.FromDateTime(
            FixedNow.UtcDateTime.AddDays(90));
        var dto = new UpdateLotDto
        {
            RowVersion = [1, 2, 3],
            LotNumber = "LOT-002",
            SupplierLotNumber = "SUP-002",
            SupplierName = "New Supplier",
            StorageLocation = "Storage B",
            ExpiryDate = newExpiryDate,
            AdministrativeStatus =
                LotAdministrativeStatus.Quarantined
        };
        var mappedDto = new LotDto();
        _lotRepository
            .Setup(x => x.GetByIdWithItemAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        _lotRepository
            .Setup(x => x.UpdateAsync(
                lot))
            .ReturnsAsync(lot);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _mapper
            .Setup(x => x.Map<LotDto>(lot))
            .Returns(mappedDto);
        var result = await _service.UpdateLot(
            lot.Id,
            dto,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo("Lot updated successfully."));
            Assert.That(lot.LotNumber, Is.EqualTo("LOT-002"));
            Assert.That(
                lot.SupplierLotNumber,
                Is.EqualTo("SUP-002"));
            Assert.That(
                lot.SupplierName,
                Is.EqualTo("New Supplier"));
            Assert.That(
                lot.StorageLocation,
                Is.EqualTo("Storage B"));
            Assert.That(
                lot.ExpiryDate,
                Is.EqualTo(newExpiryDate));
            Assert.That(
                lot.AdministrativeStatus,
                Is.EqualTo(
                    LotAdministrativeStatus.Quarantined));
        });
        _lotRepository.Verify(
            x => x.UpdateAsync(
                lot),
            Times.Once);
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
        _mapper.Verify(
            x => x.Map<LotDto>(lot),
            Times.Once);
    }
    [Test]
    public async Task GetSummaryByItemId_ShouldReturnSuccess_WhenLotsExist()
    {
        var itemId = Guid.NewGuid();
        var lots = new List<Lot>
    {
        new(
            Guid.NewGuid(),
            itemId,
            "LOT-001",
            "SUP-001",
            FixedNow,
            "Test Supplier",
            100m,
            80m,
            DateOnly.FromDateTime(
                FixedNow.UtcDateTime.AddDays(60)),
            "Storage A",
            LotAdministrativeStatus.Active)
    };
        var summaries = new List<LotSummaryDto>
{
    new()
    {
        LotNumber = "LOT-001",
        Status = "Active",
        RemainingQuantity = 80m,
        ExpiryDate = DateOnly.FromDateTime(
            FixedNow.UtcDateTime.AddDays(60))
    }
};
        _lotRepository
            .Setup(x => x.GetByItemIdAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lots);
        _mapper
            .Setup(x => x.Map<IEnumerable<LotSummaryDto>>(lots))
            .Returns(summaries);
        var result = await _service.GetSummaryByItemId(
            itemId,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.SameAs(summaries));
            Assert.That(
                result.Message,
                Is.EqualTo("Lots retrieved successfully."));
        });
        _lotRepository.Verify(
            x => x.GetByItemIdAsync(
                itemId,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _mapper.Verify(
            x => x.Map<IEnumerable<LotSummaryDto>>(lots),
            Times.Once);
    }
    [Test]
    public async Task GetSummaryByItemId_ShouldReturnSuccess_WhenNoLotsExist()
    {
        var itemId = Guid.NewGuid();
        var lots = new List<Lot>();
        var summaries = new List<LotSummaryDto>();
        if (summaries == null) throw new ArgumentNullException(nameof(summaries));
        _lotRepository
            .Setup(x => x.GetByItemIdAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lots);
        _mapper
            .Setup(x => x.Map<IEnumerable<LotSummaryDto>>(lots))
            .Returns(summaries);
        var result = await _service.GetSummaryByItemId(
            itemId,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(
                result.Message,
                Is.EqualTo("Lots retrieved successfully."));
        });
    }
    [Test]
    public async Task GetAllSummary_ShouldReturnSuccess_WhenLotsExist()
    {
        var lots = new List<Lot>
    {
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "LOT-001",
            "SUP-001",
            FixedNow,
            "Test Supplier",
            100m,
            80m,
            DateOnly.FromDateTime(
                FixedNow.UtcDateTime.AddDays(60)),
            "Storage A",
            LotAdministrativeStatus.Active)
    };
        var summaries = new List<LotSummaryDto>
{
    new()
    {
        LotNumber = "LOT-001",
        Status = "Active",
        RemainingQuantity = 80m,
        ExpiryDate = DateOnly.FromDateTime(
            FixedNow.UtcDateTime.AddDays(60))
    }
};
        _lotRepository
            .Setup(x => x.GetAllAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lots);
        _mapper
            .Setup(x => x.Map<IEnumerable<LotSummaryDto>>(lots))
            .Returns(summaries);
        var result = await _service.GetAllSummary(
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.SameAs(summaries));
            Assert.That(
                result.Message,
                Is.EqualTo("Lot summaries retrieved successfully."));
        });
        _lotRepository.Verify(
            x => x.GetAllAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
        _mapper.Verify(
            x => x.Map<IEnumerable<LotSummaryDto>>(lots),
            Times.Once);
    }
    [Test]
    public async Task GetAllSummary_ShouldReturnSuccess_WhenNoLotsExist()
    {
        var lots = new List<Lot>();
        var summaries = new List<LotSummaryDto>();
        if (summaries == null) throw new ArgumentNullException(nameof(summaries));
        _lotRepository
            .Setup(x => x.GetAllAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lots);
        _mapper
            .Setup(x => x.Map<IEnumerable<LotSummaryDto>>(lots))
            .Returns(summaries);
        var result = await _service.GetAllSummary(
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(
                result.Message,
                Is.EqualTo("Lot summaries retrieved successfully."));
        });
    }
    [Test]
    public async Task GetAllLotPage_ShouldReturnPagedLots_WhenLotsExist()
    {
        var item = new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = CreateTestLot(item);
        var pagedLots = new PagedResult<Lot>
        {
            Items = new List<Lot> { lot },
            Page = 1,
            PageSize = 20,
            TotalCount = 1
        };
        var mappedDto = new LotDto();
        _lotRepository
            .Setup(x => x.GetAllWithItemAsyncPage(
                1,
                20,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedLots);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _mapper
            .Setup(x => x.Map<LotDto>(lot))
            .Returns(mappedDto);
        var result = await _service.GetAllLotPage(
            CancellationToken.None,
            1,
            20);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Items, Has.Count.EqualTo(1));
            Assert.That(result.Data.Items[0], Is.SameAs(mappedDto));
            Assert.That(result.Data.Page, Is.EqualTo(1));
            Assert.That(result.Data.PageSize, Is.EqualTo(20));
            Assert.That(result.Data.TotalCount, Is.EqualTo(1));
            Assert.That(result.Message, Is.EqualTo("Paged lots retrieved successfully."));
        });
        _lotRepository.Verify(
            x => x.GetAllWithItemAsyncPage(
                1,
                20,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _mapper.Verify(
            x => x.Map<LotDto>(lot),
            Times.Once);
    }
    [Test]
    public async Task GetAllLotPage_ShouldUseZeroWarningDays_WhenItemIsNull()
    {
        var itemId = Guid.NewGuid();
        var lot = new Lot(
            Guid.NewGuid(),
            itemId,
            "LOT-001",
            "SUP-001",
            FixedNow,
            "Test Supplier",
            100m,
            100m,
            DateOnly.FromDateTime(FixedNow.UtcDateTime.AddDays(60)),
            "Storage A",
            LotAdministrativeStatus.Active);
        var pagedLots = new PagedResult<Lot>
        {
            Items = new List<Lot> { lot },
            Page = 1,
            PageSize = 20,
            TotalCount = 1
        };
        var mappedDto = new LotDto();
        _lotRepository
            .Setup(x => x.GetAllWithItemAsyncPage(
                1,
                20,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedLots);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _mapper
            .Setup(x => x.Map<LotDto>(lot))
            .Returns(mappedDto);
        var result = await _service.GetAllLotPage(
            CancellationToken.None,
            1,
            20);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Items, Has.Count.EqualTo(1));
            Assert.That(result.Data.Items[0], Is.SameAs(mappedDto));
        });
    }
    [Test]
    public async Task GetAllLotPage_ShouldReturnEmptyItems_WhenNoLotsExist()
    {
        var pagedLots = new PagedResult<Lot>
        {
            Items = new List<Lot>(),
            Page = 2,
            PageSize = 20,
            TotalCount = 0
        };
        _lotRepository
            .Setup(x => x.GetAllWithItemAsyncPage(
                2,
                20,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedLots);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        var result = await _service.GetAllLotPage(
            CancellationToken.None,
            2,
            20);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Items, Is.Empty);
            Assert.That(result.Data.Page, Is.EqualTo(2));
            Assert.That(result.Data.PageSize, Is.EqualTo(20));
            Assert.That(result.Data.TotalCount, Is.EqualTo(0));
            Assert.That(result.Message, Is.EqualTo("Paged lots retrieved successfully."));
        });
        _mapper.Verify(
            x => x.Map<LotDto>(It.IsAny<Lot>()),
            Times.Never);
    }
    [Test]
    public async Task RequestFefoAllocation_ShouldReturnError_WhenRequestedQuantityIsNotPositive()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var job = new Job(
            jobId,
            "JOB-001",
            "Test Job");
        job.Start(FixedNow);
        _jobRepository
            .Setup(x => x.GetByIdAsync(
                jobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        var result = await _service.RequestFefoAllocation(
            jobId,
            itemId,
            0m,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Requested quantity must be greater than zero."));
        });
        _lotRepository.Verify(
            x => x.GetByItemIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _lotSelectionService.Verify(
            x => x.AllocateAsync(
                It.IsAny<Guid>(),
                It.IsAny<decimal>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task RequestFefoAllocation_ShouldReturnError_WhenRequestedQuantityIsNegative()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var job = new Job(
            jobId,
            "JOB-001",
            "Test Job");
        job.Start(FixedNow);
        _jobRepository
            .Setup(x => x.GetByIdAsync(
                jobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        var result = await _service.RequestFefoAllocation(
            jobId,
            itemId,
            -10m,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Requested quantity must be greater than zero."));
        });
    }
    [Test]
    public async Task RequestFefoAllocation_ShouldReturnError_WhenFefoAllocationFails()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var job = new Job(
            jobId,
            "JOB-001",
            "Test Job");
        job.Start(FixedNow);
        _jobRepository
            .Setup(x => x.GetByIdAsync(
                jobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _lotRepository
            .Setup(x => x.GetByItemIdAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Lot>());
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _lotSelectionService
            .Setup(x => x.AllocateAsync(
                itemId,
                50m,
                FixedNow,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                ServiceResult<IReadOnlyList<LotAllocation>>.ErrorResult(
                    "Insufficient stock."));
        var result = await _service.RequestFefoAllocation(
            jobId,
            itemId,
            50m,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Insufficient stock."));
        });
        _lotSelectionService.Verify(
            x => x.AllocateAsync(
                itemId,
                50m,
                FixedNow,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Test]
    public async Task RequestFefoAllocation_ShouldReturnPreview_WhenAllocationSucceeds()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var item = new Item(
            itemId,
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var lot = CreateTestLot(item);
        var allocation = new LotAllocation(
         lot.Id,
         40m);
        var job = new Job(
            jobId,
            "JOB-001",
            "Test Job");
        job.Start(FixedNow);
        var allocationResult =
            ServiceResult<IReadOnlyList<LotAllocation>>.SuccessResult(
                new List<LotAllocation>
                {
                allocation
                });
        _jobRepository
            .Setup(x => x.GetByIdAsync(
                jobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _lotRepository
            .Setup(x => x.GetByItemIdAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Lot>
            {
            lot
            });
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _lotSelectionService
            .Setup(x => x.AllocateAsync(
                itemId,
                40m,
                FixedNow,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(allocationResult);
        var result = await _service.RequestFefoAllocation(
            jobId,
            itemId,
            40m,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Lots allocated successfully."));
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.JobId, Is.EqualTo(jobId));
            Assert.That(result.Data.ItemId, Is.EqualTo(itemId));
            Assert.That(result.Data.BaseUnit, Is.EqualTo(UnitOfMeasure.Unit));
            Assert.That(result.Data.RequestedQuantity, Is.EqualTo(40m));
            Assert.That(result.Data.AllocatedQuantity, Is.EqualTo(40m));
            Assert.That(result.Data.IsReservation, Is.False);
            Assert.That(result.Data.PreviewedAt, Is.EqualTo(FixedNow));
            Assert.That(result.Data.Allocations, Has.Count.EqualTo(1));
        });
        var line = result.Data.Allocations[0];
        Assert.Multiple(() =>
        {
            Assert.That(line.LotId, Is.EqualTo(lot.Id));
            Assert.That(line.LotNumber, Is.EqualTo(lot.LotNumber));
            Assert.That(line.ExpiryDate, Is.EqualTo(lot.ExpiryDate));
            Assert.That(line.RemainingQuantity, Is.EqualTo(lot.RemainingQuantity));
            Assert.That(line.Quantity, Is.EqualTo(40m));
            Assert.That(line.RowVersion, Is.EqualTo(lot.RowVersion));
        });
    }
    [Test]
    public async Task ConfirmConsumption_ShouldReturnError_WhenAllocationQuantityIsNotPositive()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var request = new ConfirmConsumptionRequestDto
        {
            ItemId = itemId,
            RequestedQuantity = 10m,
            Allocations =
            [
                new ConsumptionAllocationRequestDto
                {
                LotId = Guid.NewGuid(),
                Quantity = 0m,
                ExpectedRowVersion = [1, 2, 3]
            }
            ]
        };
        var result = await _service.ConfirmConsumption(
            jobId,
            request,
            Guid.NewGuid(),
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Every allocation quantity must be greater than zero."));
        });
        _jobRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task ConfirmConsumption_ShouldReturnError_WhenLotIdIsDuplicated()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var lotId = Guid.NewGuid();
        var request = new ConfirmConsumptionRequestDto
        {
            ItemId = itemId,
            RequestedQuantity = 20m,
            Allocations =
            [
                new ConsumptionAllocationRequestDto
                {
                LotId = lotId,
                Quantity = 10m,
                ExpectedRowVersion = [1, 2, 3]
            },
            new ConsumptionAllocationRequestDto
                {
                LotId = lotId,
                Quantity = 10m,
                ExpectedRowVersion = [1, 2, 3]
            }
            ]
        };
        var result = await _service.ConfirmConsumption(
            jobId,
            request,
            Guid.NewGuid(),
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Duplicate LotId is not allowed within a single consumption request."));
        });
        _jobRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task ConfirmConsumption_ShouldReturnError_WhenAllocationTotalDoesNotMatchRequestedQuantity()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var request = new ConfirmConsumptionRequestDto
        {
            ItemId = itemId,
            RequestedQuantity = 20m,
            Allocations =
            [
                new ConsumptionAllocationRequestDto
                {
                LotId = Guid.NewGuid(),
                Quantity = 10m,
                ExpectedRowVersion = [1, 2, 3]
            }
            ]
        };
        var result = await _service.ConfirmConsumption(
            jobId,
            request,
            Guid.NewGuid(),
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Sum of allocation quantities (10) does not match RequestedQuantity (20)."));
        });
        _jobRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task ConfirmConsumption_ShouldReturnError_WhenJobNotFound()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var request = new ConfirmConsumptionRequestDto
        {
            ItemId = itemId,
            RequestedQuantity = 10m,
            Allocations =
            [
                new ConsumptionAllocationRequestDto
                {
                LotId = Guid.NewGuid(),
                Quantity = 10m,
                ExpectedRowVersion = [1, 2, 3]
            }
            ]
        };
        _jobRepository
            .Setup(x => x.GetByIdAsync(
                jobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);
        var result = await _service.ConfirmConsumption(
            jobId,
            request,
            Guid.NewGuid(),
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Job not found."));
        });
    }
    [Test]
    public async Task ConfirmConsumption_ShouldReturnError_WhenJobIsNotInProgress()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var job = new Job(
            jobId,
            "JOB-001",
            "Test Job");
        var request = new ConfirmConsumptionRequestDto
        {
            ItemId = itemId,
            RequestedQuantity = 10m,
            Allocations =
            [
                new ConsumptionAllocationRequestDto
                {
                LotId = Guid.NewGuid(),
                Quantity = 10m,
                ExpectedRowVersion = [1, 2, 3]
            }
            ]
        };
        _jobRepository
            .Setup(x => x.GetByIdAsync(
                jobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        var result = await _service.ConfirmConsumption(
            jobId,
            request,
            Guid.NewGuid(),
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Job must be InProgress to confirm consumption. Current status: Draft."));
        });
    }
    [Test]
    public async Task ConfirmConsumption_ShouldReturnError_WhenLotNotFound()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var lotId = Guid.NewGuid();
        var job = new Job(
            jobId,
            "JOB-001",
            "Test Job");
        job.Start(FixedNow);
        var request = new ConfirmConsumptionRequestDto
        {
            ItemId = itemId,
            RequestedQuantity = 10m,
            Allocations =
            [
                new ConsumptionAllocationRequestDto
                {
                LotId = lotId,
                Quantity = 10m,
                ExpectedRowVersion = [1, 2, 3]
            }
            ]
        };
        _jobRepository
            .Setup(x => x.GetByIdAsync(
                jobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _lotRepository
            .Setup(x => x.GetByIdAsync(
                lotId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lot?)null);
        var result = await _service.ConfirmConsumption(
            jobId,
            request,
            Guid.NewGuid(),
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo($"Lot {lotId} not found."));
        });
    }
    [Test]
    public async Task ConfirmConsumption_ShouldReturnError_WhenLotDoesNotBelongToItem()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var differentItemId = Guid.NewGuid();
        var job = new Job(
            jobId,
            "JOB-001",
            "Test Job");
        job.Start(FixedNow);
        var lot = new Lot(
            Guid.NewGuid(),
            differentItemId,
            "LOT-001",
            "SUP-001",
            FixedNow,
            "Test Supplier",
            100m,
            100m,
            DateOnly.FromDateTime(FixedNow.UtcDateTime.AddDays(60)),
            "Storage A",
            LotAdministrativeStatus.Active);
        var request = new ConfirmConsumptionRequestDto
        {
            ItemId = itemId,
            RequestedQuantity = 10m,
            Allocations =
            [
                new ConsumptionAllocationRequestDto
                {
                LotId = lot.Id,
                Quantity = 10m,
                ExpectedRowVersion = []
            }
            ]
        };
        _jobRepository
            .Setup(x => x.GetByIdAsync(
                jobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _lotRepository
            .Setup(x => x.GetByIdAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        var result = await _service.ConfirmConsumption(
            jobId,
            request,
            Guid.NewGuid(),
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    $"Lot {lot.Id} does not belong to item {itemId}."));
        });
    }
    [Test]
    public async Task ConfirmConsumption_ShouldReturnError_WhenRowVersionDoesNotMatch()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var job = new Job(
            jobId,
            "JOB-001",
            "Test Job");
        job.Start(FixedNow);
        var lot = new Lot(
            Guid.NewGuid(),
            itemId,
            "LOT-001",
            "SUP-001",
            FixedNow,
            "Test Supplier",
            100m,
            100m,
            DateOnly.FromDateTime(FixedNow.UtcDateTime.AddDays(60)),
            "Storage A",
            LotAdministrativeStatus.Active);
        var request = new ConfirmConsumptionRequestDto
        {
            ItemId = itemId,
            RequestedQuantity = 10m,
            Allocations =
            [
                new ConsumptionAllocationRequestDto
                {
                LotId = lot.Id,
                Quantity = 10m,
                ExpectedRowVersion = [.. "\t\t\t"u8]
            }
            ]
        };
        _jobRepository
            .Setup(x => x.GetByIdAsync(
                jobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _lotRepository
            .Setup(x => x.GetByIdAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        var result = await _service.ConfirmConsumption(
            jobId,
            request,
            Guid.NewGuid(),
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    $"Lot {lot.Id} has changed since it was previewed (stale RowVersion)."));
        });
    }
    [Test]
    public async Task ConfirmConsumption_ShouldReturnError_WhenLotConsumeThrowsInvalidOperationException()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var consumedBy = Guid.NewGuid();
        var job = new Job(
            jobId,
            "JOB-001",
            "Test Job");
        job.Start(FixedNow);
        var lot = new Lot(
            Guid.NewGuid(),
            itemId,
            "LOT-001",
            "SUP-001",
            FixedNow,
            "Test Supplier",
            10m,
            5m,
            DateOnly.FromDateTime(FixedNow.UtcDateTime.AddDays(60)),
            "Storage A",
            LotAdministrativeStatus.Active);
        var request = new ConfirmConsumptionRequestDto
        {
            ItemId = itemId,
            RequestedQuantity = 10m,
            Allocations =
            [
                new ConsumptionAllocationRequestDto
                {
                LotId = lot.Id,
                Quantity = 10m,
                ExpectedRowVersion = []
            }
            ]
        };
        _jobRepository
            .Setup(x => x.GetByIdAsync(
                jobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _lotRepository
            .Setup(x => x.GetByIdAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        var result = await _service.ConfirmConsumption(
            jobId,
            request,
            consumedBy,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Does.StartWith($"Lot {lot.Id}:"));
        });
    }
    [Test]
    public async Task ConfirmConsumption_ShouldReturnSuccess_WhenConsumptionIsValid()
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var consumedBy = Guid.NewGuid();
        var job = new Job(
            jobId,
            "JOB-001",
            "Test Job");
        job.Start(FixedNow);
        var lot = new Lot(
            Guid.NewGuid(),
            itemId,
            "LOT-001",
            "SUP-001",
            FixedNow,
            "Test Supplier",
            100m,
            100m,
            DateOnly.FromDateTime(FixedNow.UtcDateTime.AddDays(60)),
            "Storage A",
            LotAdministrativeStatus.Active);
        var item = new Item(
            itemId,
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);
        var request = new ConfirmConsumptionRequestDto
        {
            ItemId = itemId,
            RequestedQuantity = 40m,
            Allocations =
            [
                new ConsumptionAllocationRequestDto
                {
                LotId = lot.Id,
                Quantity = 40m,
                ExpectedRowVersion = []
            }
            ]
        };
        _jobRepository
            .Setup(x => x.GetByIdAsync(
                jobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);
        _lotRepository
            .Setup(x => x.GetByIdAsync(
                lot.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        _consumptionRepository
         .Setup(x => x.AddAsync(
             It.IsAny<Consumption>(),
             It.IsAny<CancellationToken>()))
         .ReturnsAsync((Consumption consumption, CancellationToken _) => consumption);
        _lotRepository
            .Setup(x => x.UpdateAsync(
                It.IsAny<Lot>()))
            .ReturnsAsync((Lot updatedLot, CancellationToken _) => updatedLot);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _itemRepository
            .Setup(x => x.GetByIdAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        var result = await _service.ConfirmConsumption(
            jobId,
            request,
            consumedBy,
            CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo("Consumption confirmed successfully."));
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.JobId, Is.EqualTo(jobId));
            Assert.That(result.Data.ItemId, Is.EqualTo(itemId));
            Assert.That(result.Data.BaseUnit, Is.EqualTo(UnitOfMeasure.Unit));
            Assert.That(result.Data.TotalConsumedQuantity, Is.EqualTo(40m));
            Assert.That(result.Data.Consumptions, Has.Count.EqualTo(1));
            Assert.That(result.Data.UpdatedLots, Has.Count.EqualTo(1));
        });
        Assert.Multiple(() =>
        {
            Assert.That(
                    result.Data.Consumptions[0].JobId,
                    Is.EqualTo(jobId));
            Assert.That(
                result.Data.Consumptions[0].LotId,
                Is.EqualTo(lot.Id));
            Assert.That(
                result.Data.Consumptions[0].Quantity,
                Is.EqualTo(40m));
            Assert.That(
                result.Data.Consumptions[0].ConsumedBy,
                Is.EqualTo(consumedBy));
            Assert.That(
                result.Data.UpdatedLots[0].LotId,
                Is.EqualTo(lot.Id));
            Assert.That(
                result.Data.UpdatedLots[0].RemainingQuantity,
                Is.EqualTo(60m));
        });
        _consumptionRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Consumption>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _lotRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<Lot>()),
            Times.Once);
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
        _itemRepository.Verify(
            x => x.GetByIdAsync(
                itemId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
