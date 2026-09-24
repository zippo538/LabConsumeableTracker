using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class ItemServiceTests
{
    private Mock<IItemRepository> _itemRepository = null!;
    private Mock<ILotRepository> _lotRepository = null!;
    private Mock<IFileStorageService> _fileStorageService = null!;
    private Mock<IMapper> _mapper = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private ItemService _sut = null!;
    [SetUp]
    public void Setup()
    {
        var imageStorageRepositoryMock = new Mock<IImageStorageRepository>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<ItemService>>();
        _itemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
        _lotRepository = new Mock<ILotRepository>(MockBehavior.Strict);
        _fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
        _mapper = new Mock<IMapper>(MockBehavior.Strict);
        _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _sut = new ItemService(
            _itemRepository.Object,
            imageStorageRepositoryMock.Object, 
            _mapper.Object,                    
            TimeProvider.System,               
            _lotRepository.Object,             
            _unitOfWork.Object,                
            loggerMock.Object);                  
    }
    [Test]
    public async Task GetByIdItem_WhenNotFound_ReturnsError()
    {
        var id = Guid.NewGuid();
        _itemRepository
            .Setup(x => x.GetByIdWithLotsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);
        var result = await _sut.GetByIdItem(id, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Item not found."));
    }
    [Test]
    public async Task GetByIdItem_WhenExists_ReturnsSuccess()
    {
        var item = CreateItem();
        var dto = new ItemDto();
        _itemRepository
            .Setup(x => x.GetByIdWithLotsAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _lotRepository
            .Setup(x => x.GetTotalUsableQuantityAsync(
                item.Id,
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        _mapper
            .Setup(x => x.Map<ItemDto>(item))
            .Returns(dto);
        var result = await _sut.GetByIdItem(item.Id, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.SameAs(dto));
        Assert.That(dto.TotalRemainingQuantity, Is.EqualTo(10));
    }
    [Test]
    public async Task CreateItem_WhenCodeExists_ReturnsError()
    {
        var dto = new CreateItemDto
        {
            Code = "ITEM-001",
            Name = "Test",
            BaseUnit = "Box"
        };
        _itemRepository
            .Setup(x => x.GetByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateItem());
        var result = await _sut.CreateItem(dto, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Item code already exists."));
    }
    [Test]
    public async Task CreateItem_WhenValid_CreatesItem()
    {
        var dto = new CreateItemDto
        {
            Code = "ITEM-001",
            Name = "Test",
            BaseUnit = "Unit",
            MinimumStock = 5,
            ExpiringSoonDays = 30
        };
        var item = CreateItem();
        _itemRepository
            .Setup(x => x.GetByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);
        _itemRepository
            .Setup(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mapper
            .Setup(x => x.Map<ItemDto>(item))
            .Returns(new ItemDto());
        var result = await _sut.CreateItem(dto, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        _unitOfWork.Verify(x =>
            x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Test]
    public async Task UpdateItem_WhenNotFound_ReturnsError()
    {
        var id = Guid.NewGuid();
        _itemRepository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);
        var result = await _sut.UpdateItem(
            id,
            new UpdateItemDto(),
            CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Item not found."));
    }
    [Test]
    public async Task UpdateItem_WhenValid_UpdatesAndSaves()
    {
        var item = CreateItem();
        _itemRepository
            .Setup(x => x.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _itemRepository
            .Setup(x => x.UpdateAsync(item, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mapper
            .Setup(x => x.Map<ItemDto>(item))
            .Returns(new ItemDto());
        var result = await _sut.UpdateItem(
            item.Id,
            new UpdateItemDto
            {
                Name = "Updated",
                BaseUnit = UnitOfMeasure.Unit,
                MinimumStock = 10,
                ExpiringSoonDays = 30
            },
            CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(item.Name, Is.EqualTo("Updated"));
    }
    [Test]
    public async Task DeleteItem_WhenSuccess_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _itemRepository
            .Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var result = await _sut.DeleteItem(id, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.True);
    }
    [Test]
    public async Task CreateItem_WhenInvalidBaseUnit_ReturnsValidationError()
    {
        var dto = new CreateItemDto
        {
            Code = "ITEM-001",
            Name = "Test",
            BaseUnit = "INVALID",
            MinimumStock = 5,
            ExpiringSoonDays = 30
        };
        _itemRepository
            .Setup(x => x.GetByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);
        var result = await _sut.CreateItem(dto, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Validation failed."));
        _itemRepository.Verify(
            x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task GetAllItem_WhenItemsExist_ReturnsStockInformation()
    {
        var item = CreateItem();
        _itemRepository
            .Setup(x => x.GetAllWithLotsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { item });
        _lotRepository
            .Setup(x => x.GetUsableQuantityByItemIdsAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, decimal>
            {
                [item.Id] = 20
            });
        _mapper
            .Setup(x => x.Map<ItemDto>(item))
            .Returns(new ItemDto());
        var result = await _sut.GetAllItem(CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
    }
    [Test]
    public async Task DeleteItem_WhenNotFound_ReturnsError()
    {
        var id = Guid.NewGuid();
        _itemRepository
            .Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        var result = await _sut.DeleteItem(id, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Item not found."));
    }
    private static Item CreateItem()
    {
        return new Item(
            Guid.NewGuid(),
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            5,
            30);
    }
}