using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class ItemServiceTests
{
    private Mock<IItemRepository> _itemRepository = null!;
    private Mock<ILotRepository> _lotRepository = null!;
    private Mock<IMapper> _mapper = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private ItemService _sut = null!;
    private Mock<IImageStorageRepository> _imageStorageRepository = null!;

    [SetUp]
    public void Setup()
    {
        var loggerMock = new Mock<ILogger<ItemService>>();

        _itemRepository =
            new Mock<IItemRepository>(MockBehavior.Strict);

        _lotRepository =
            new Mock<ILotRepository>(MockBehavior.Strict);

        _imageStorageRepository =
            new Mock<IImageStorageRepository>(MockBehavior.Strict);

        _mapper =
            new Mock<IMapper>(MockBehavior.Strict);

        _unitOfWork =
            new Mock<IUnitOfWork>(MockBehavior.Strict);

        _sut = new ItemService(
            _itemRepository.Object,
            _imageStorageRepository.Object,
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
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Item not found."));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.SameAs(dto));
            Assert.That(dto.TotalRemainingQuantity, Is.EqualTo(10));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Item code already exists."));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(item.Name, Is.EqualTo("Updated"));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.True);
        });
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
            .ReturnsAsync([item]);
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
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Item not found."));
        });
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

    [Test]
    public async Task GetAllItemPage_ShouldReturnPagedItemsSuccessfully()
    {
        // Arrange
        const int page = 1;
        const int pageSize = 10;
        var ct = CancellationToken.None;

        var itemId = Guid.NewGuid();

        var item = new Item(
            itemId,
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Gram,
            10m,
            30);

        var itemDto = new ItemDto
        {
            Id = itemId,
            Code = "ITEM-001",
            Name = "Test Item"
        };

        var pagedItems = new PagedResult<Item>
        {
            Items = [item],
            Page = page,
            PageSize = pageSize,
            TotalCount = 1
        };

        const decimal usableQuantity = 25m;

        _itemRepository
            .Setup(x => x.GetAllWithLotsAsyncPaged(page, pageSize, ct))
            .ReturnsAsync(pagedItems);

        _lotRepository
            .Setup(x => x.GetUsableQuantityByItemIdsAsync(
                It.Is<Guid[]>(ids =>
                    ids.Length == 1 &&
                    ids[0] == itemId),
                It.IsAny<DateOnly>(),
                ct))
            .ReturnsAsync(new Dictionary<Guid, decimal>
            {
                [itemId] = usableQuantity
            });

        _mapper
            .Setup(x => x.Map<ItemDto>(item))
            .Returns(itemDto);

        // Act
        var result = await _sut.GetAllItemPage(
            page,
            pageSize,
            ct);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Items, Has.Count.EqualTo(1));
            Assert.That(result.Data.Page, Is.EqualTo(page));
            Assert.That(result.Data.PageSize, Is.EqualTo(pageSize));
            Assert.That(result.Data.TotalCount, Is.EqualTo(1));
        });

        var dto = result.Data.Items.Single();

        Assert.That(dto.Id, Is.EqualTo(itemId));
        Assert.That(dto.TotalRemainingQuantity, Is.EqualTo(usableQuantity));
        Assert.That(
            dto.StockStatus,
            Is.EqualTo(item.GetStockStatus(usableQuantity)));
    }

    [Test]
    [TestCase("image/jpeg", ".jpg")]
    [TestCase("image/png", ".png")]
    [TestCase("image/webp", ".webp")]
    public async Task CreateItem_ShouldCreateItem_WhenImageIsValid(
     string contentType,
     string extension)
    {
        // Arrange
        var dto = new CreateItemDto
        {
            Code = "ITEM-001",
            Name = "Test Item",
            BaseUnit = nameof(UnitOfMeasure.Unit),
            MinimumStock = 10m,
            ExpiringSoonDays = 30,
            Image = CreateMockImage(
                length: 1024,
                contentType: contentType,
                fileName: $"test{extension}")
        };

        var createdItem = new Item(
            Guid.NewGuid(),
            dto.Code,
            dto.Name,
            UnitOfMeasure.Unit,
            dto.MinimumStock,
            dto.ExpiringSoonDays);

        var imageId = Guid.NewGuid();

        var itemDto = new ItemDto
        {
            Id = createdItem.Id,
            Code = createdItem.Code,
            Name = createdItem.Name
        };

        _itemRepository
            .Setup(x => x.GetByCodeAsync(
                dto.Code,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);

        _itemRepository
            .Setup(x => x.AddAsync(
                It.Is<Item>(item =>
                    item.Code == dto.Code &&
                    item.Name == dto.Name &&
                    item.BaseUnit == UnitOfMeasure.Unit &&
                    item.MinimumStock == dto.MinimumStock &&
                    item.ExpiringSoonDays == dto.ExpiringSoonDays),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdItem);

        _imageStorageRepository
            .Setup(x => x.SaveImageAsync(
                It.IsAny<Stream>(),
                extension,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageId);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapper
            .Setup(x => x.Map<ItemDto>(createdItem))
            .Returns(itemDto);

        // Act
        var result = await _sut.CreateItem(
            dto,
            CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(createdItem.Id));
        Assert.That(
            result.Message,
            Is.EqualTo("Item created successfully."));

        _imageStorageRepository.Verify(
            x => x.SaveImageAsync(
                It.IsAny<Stream>(),
                extension,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static IFormFile CreateMockImage(
     long length,
     string contentType,
     string fileName = "test.jpg")
    {
        var image = new Mock<IFormFile>();

        image.Setup(x => x.Length)
            .Returns(length);

        image.Setup(x => x.ContentType)
            .Returns(contentType);

        image.Setup(x => x.FileName)
            .Returns(fileName);

        image.Setup(x => x.OpenReadStream())
            .Returns(new MemoryStream([1, 2, 3]));

        return image.Object;
    }

    [Test]
    public async Task UpdateItem_ShouldReturnError_WhenItemNotFound()
    {
        var id = Guid.NewGuid();

        var request = new UpdateItemDto
        {
            Name = "Updated Item",
            BaseUnit = UnitOfMeasure.Unit,
            MinimumStock = 20m,
            ExpiringSoonDays = 15
        };

        _itemRepository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);

        var result = await _sut.UpdateItem(
            id,
            request,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Item not found."));

        _itemRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<Item>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateItem_ShouldReturnError_WhenImageIsEmpty()
    {
        var id = Guid.NewGuid();

        var item = new Item(
            id,
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);

        var request = new UpdateItemDto
        {
            Name = "Updated Item",
            BaseUnit = UnitOfMeasure.Unit,
            MinimumStock = 20m,
            ExpiringSoonDays = 15,
            Image = CreateMockImage(0, "image/jpeg")
        };

        _itemRepository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _sut.UpdateItem(
            id,
            request,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Validation failed."));
        Assert.That(result.Errors, Does.Contain("Image cannot be empty."));

        _itemRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<Item>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateItem_ShouldReturnError_WhenImageExceeds5MB()
    {
        var id = Guid.NewGuid();

        var item = new Item(
            id,
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);

        var request = new UpdateItemDto
        {
            Name = "Updated Item",
            BaseUnit = UnitOfMeasure.Unit,
            MinimumStock = 20m,
            ExpiringSoonDays = 15,
            Image = CreateMockImage(
                5 * 1024 * 1024 + 1,
                "image/jpeg")
        };

        _itemRepository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _sut.UpdateItem(
            id,
            request,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Validation failed."));
        Assert.That(result.Errors, Does.Contain("Image cannot exceed 5 MB."));

        _itemRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<Item>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    [TestCase("application/pdf")]
    [TestCase("text/plain")]
    [TestCase("application/octet-stream")]
    public async Task UpdateItem_ShouldReturnError_WhenImageTypeIsInvalid(
        string contentType)
    {
        var id = Guid.NewGuid();

        var item = new Item(
            id,
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);

        var request = new UpdateItemDto
        {
            Name = "Updated Item",
            BaseUnit = UnitOfMeasure.Unit,
            MinimumStock = 20m,
            ExpiringSoonDays = 15,
            Image = CreateMockImage(1024, contentType)
        };

        _itemRepository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _sut.UpdateItem(
            id,
            request,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Validation failed."));
        Assert.That(result.Errors, Does.Contain("Invalid image type."));

        _itemRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<Item>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateItem_ShouldUpdateItem_WhenImageIsNotProvided()
    {
        var id = Guid.NewGuid();

        var item = new Item(
            id,
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);

        var request = new UpdateItemDto
        {
            Name = "Updated Item",
            BaseUnit = UnitOfMeasure.Unit,
            MinimumStock = 20m,
            ExpiringSoonDays = 15
        };

        var updatedItem = item;

        var itemDto = new ItemDto
        {
            Id = id,
            Code = "ITEM-001",
            Name = "Updated Item"
        };

        _itemRepository
            .Setup(x => x.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _itemRepository
            .Setup(x => x.UpdateAsync(
                It.Is<Item>(item1 =>
                    item1.Id == id &&
                    item1.Name == request.Name),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedItem);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapper
            .Setup(x => x.Map<ItemDto>(updatedItem))
            .Returns(itemDto);

        var result = await _sut.UpdateItem(
            id,
            request,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(id));
        Assert.That(result.Data.Name, Is.EqualTo("Updated Item"));
        Assert.That(
            result.Message,
            Is.EqualTo("Item updated successfully."));

        _itemRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<Item>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _imageStorageRepository.Verify(
            x => x.SaveImageAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _imageStorageRepository.Verify(
            x => x.DeleteImageAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateItem_ShouldReplaceImage_WhenNewImageIsProvided()
    {
        var id = Guid.NewGuid();
        var oldImageId = Guid.NewGuid();
        var newImageId = Guid.NewGuid();

        var item = new Item(
            id,
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);

        item.SetImageId(oldImageId);

        var request = new UpdateItemDto
        {
            Name = "Updated Item",
            BaseUnit = UnitOfMeasure.Unit,
            MinimumStock = 20m,
            ExpiringSoonDays = 15,
            Image = CreateMockImage(
                1024,
                "image/png",
                "updated.png")
        };

        var itemDto = new ItemDto
        {
            Id = id,
            Code = "ITEM-001",
            Name = "Updated Item"
        };

        _itemRepository
            .Setup(x => x.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _imageStorageRepository
            .Setup(x => x.SaveImageAsync(
                It.IsAny<Stream>(),
                ".png",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(newImageId);

        _imageStorageRepository
            .Setup(x => x.DeleteImageAsync(
                oldImageId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _itemRepository
            .Setup(x => x.UpdateAsync(
                It.IsAny<Item>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapper
            .Setup(x => x.Map<ItemDto>(item))
            .Returns(itemDto);

        var result = await _sut.UpdateItem(
            id,
            request,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Id, Is.EqualTo(id));
        Assert.That(result.Message, Is.EqualTo("Item updated successfully."));

        Assert.That(item.ImageId, Is.EqualTo(newImageId));

        _imageStorageRepository.Verify(
            x => x.SaveImageAsync(
                It.IsAny<Stream>(),
                ".png",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _imageStorageRepository.Verify(
            x => x.DeleteImageAsync(
                oldImageId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _itemRepository.Verify(
            x => x.UpdateAsync(
                It.Is<Item>(item1 =>
                    item1.Id == id &&
                    item1.ImageId == newImageId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task UpdateItem_ShouldNotDeleteOldImage_WhenNewImageIsProvidedAndOldImageDoesNotExist()
    {
        var id = Guid.NewGuid();
        var newImageId = Guid.NewGuid();

        var item = new Item(
            id,
            "ITEM-001",
            "Test Item",
            UnitOfMeasure.Unit,
            10m,
            30);

        var request = new UpdateItemDto
        {
            Name = "Updated Item",
            BaseUnit = UnitOfMeasure.Unit,
            MinimumStock = 20m,
            ExpiringSoonDays = 15,
            Image = CreateMockImage(
                1024,
                "image/jpeg",
                "updated.jpg")
        };

        var itemDto = new ItemDto
        {
            Id = id,
            Code = "ITEM-001",
            Name = "Updated Item"
        };

        _itemRepository
            .Setup(x => x.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _imageStorageRepository
            .Setup(x => x.SaveImageAsync(
                It.IsAny<Stream>(),
                ".jpg",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(newImageId);

        _itemRepository
            .Setup(x => x.UpdateAsync(
                It.IsAny<Item>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapper
            .Setup(x => x.Map<ItemDto>(item))
            .Returns(itemDto);

        var result = await _sut.UpdateItem(
            id,
            request,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(item.ImageId, Is.EqualTo(newImageId));

        _imageStorageRepository.Verify(
            x => x.SaveImageAsync(
                It.IsAny<Stream>(),
                ".jpg",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _imageStorageRepository.Verify(
            x => x.DeleteImageAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task DeleteItem_ShouldReturnError_WhenItemIsReferencedByLots()
    {
        var id = Guid.NewGuid();

        _itemRepository
            .Setup(x => x.DeleteAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException());

        var result = await _sut.DeleteItem(
            id,
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "This item cannot be deleted because it is already used by one or more lots."));

        _itemRepository.Verify(
            x => x.DeleteAsync(
                id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

}
