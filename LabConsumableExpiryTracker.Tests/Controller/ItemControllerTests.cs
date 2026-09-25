using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class ItemControllerTests
{
    private Mock<IItemService> _itemServiceMock = null!;
    private Mock<ILotService> _lotServiceMock = null!;
    private ItemController _controller = null!;
    [SetUp]
    public void Setup()
    {
        _itemServiceMock =
            new Mock<IItemService>(
                MockBehavior.Strict);
        _lotServiceMock =
            new Mock<ILotService>(
                MockBehavior.Strict);
        _controller =
            new ItemController(
                _itemServiceMock.Object,
                _lotServiceMock.Object);
    }
    [Test]
    public async Task GetAll_WhenSuccess_ReturnsOk()
    {
        var response =
            ServiceResult<IEnumerable<ItemDto>>
            .SuccessResult(
                new List<ItemDto>(),
                "success");
        _itemServiceMock
            .Setup(x =>
                x.GetAllItem(
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.GetAll(
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
        var ok =
            (OkObjectResult)result.Result!;
        Assert.That(
            ok.Value,
            Is.EqualTo(response));
    }
    [Test]
    public async Task GetAll_WhenFailed_ReturnsBadRequest()
    {
        var response =
            ServiceResult<IEnumerable<ItemDto>>
            .ErrorResult(
                "failed");
        _itemServiceMock
            .Setup(x =>
                x.GetAllItem(
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.GetAll(
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<BadRequestObjectResult>());
    }
    [Test]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<ItemDto>
            .SuccessResult(
                new ItemDto
                {
                    Id = id,
                    Code = "ITEM001"
                },
                "found");
        _itemServiceMock
            .Setup(x =>
                x.GetByIdItem(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.GetById(
                id,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<ItemDto>
            .ErrorResult(
                "Item not found.");
        _itemServiceMock
            .Setup(x =>
                x.GetByIdItem(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.GetById(
                id,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<NotFoundObjectResult>());
    }
    [Test]
    public async Task Create_WhenSuccess_ReturnsOk()
    {
        var request =
            CreateRequest();
        var response =
            ServiceResult<ItemDto>
            .SuccessResult(
                new ItemDto
                {
                    Id = Guid.NewGuid(),
                    Code = "ITEM001"
                },
                "created");
        _itemServiceMock
            .Setup(x =>
                x.CreateItem(
                    request,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Create(
                request,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<CreatedAtActionResult>());
        var ok =
            (CreatedAtActionResult)result.Result!;
        Assert.That(
            ok.Value,
            Is.EqualTo(response));
    }
    [Test]
    public async Task Create_WhenFailed_ReturnsOk()
    {
        var request =
            CreateRequest();
        var response =
            ServiceResult<ItemDto>
            .ErrorResult(
                "Validation failed.");
        _itemServiceMock
            .Setup(x =>
                x.CreateItem(
                    request,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Create(
                request,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<ConflictObjectResult>());
    }
    [Test]
    public async Task Update_WhenSuccess_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var request =
            UpdateRequest();
        var response =
            ServiceResult<ItemDto>
            .SuccessResult(
                new ItemDto
                {
                    Id = id
                },
                "updated");
        _itemServiceMock
            .Setup(x =>
                x.UpdateItem(
                    id,
                    request,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Update(
                id,
                request,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task Update_WhenFailed_ReturnsConflict()
    {
        var id = Guid.NewGuid();
        var request =
            UpdateRequest();
        var response =
            ServiceResult<ItemDto>
            .ErrorResult(
                "Item not found.");
        _itemServiceMock
            .Setup(x =>
                x.UpdateItem(
                    id,
                    request,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Update(
                id,
                request,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<ConflictObjectResult>());
    }
    [Test]
    public async Task Delete_WhenSuccess_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<bool>
            .SuccessResult(
                true,
                "deleted");
        _itemServiceMock
            .Setup(x =>
                x.DeleteItem(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Delete(
                id,
                CancellationToken.None);
        Assert.That(
            result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task Delete_WhenItemNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<bool>
            .ErrorResult(
                "Item not found.");
        _itemServiceMock
            .Setup(x =>
                x.DeleteItem(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Delete(
                id,
                CancellationToken.None);
        Assert.That(
            result,
            Is.TypeOf<NotFoundObjectResult>());
    }
    [Test]
    public async Task Delete_WhenOtherError_ReturnsConflict()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<bool>
            .ErrorResult(
                "Cannot delete item.");
        _itemServiceMock
            .Setup(x =>
                x.DeleteItem(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.Delete(
                id,
                CancellationToken.None);
        Assert.That(
            result,
            Is.TypeOf<ConflictObjectResult>());
    }
    [Test]
    public async Task GetSummaryByItemId_WhenSuccess_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var response =
            ServiceResult<IEnumerable<LotSummaryDto>>
            .SuccessResult(
                new List<LotSummaryDto>(),
                "success");
        _lotServiceMock
            .Setup(x =>
                x.GetSummaryByItemId(
                    id,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.GetSummaryByItemId(
                id,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    [Test]
    public async Task GetAllPage_WhenSuccess_ReturnsOk()
    {
        var response =
            ServiceResult<PagedResult<ItemDto>>
            .SuccessResult(
                new PagedResult<ItemDto>(),
                "success");
        _itemServiceMock
            .Setup(x =>
                x.GetAllItemPage(
                    1,
                    20,
                    CancellationToken.None))
            .ReturnsAsync(response);
        var result =
            await _controller.GetAllPage(
                1,
                20,
                CancellationToken.None);
        Assert.That(
            result.Result,
            Is.TypeOf<OkObjectResult>());
    }
    private static CreateItemDto CreateRequest()
    {
        return new CreateItemDto
        {
            Code = "ITEM001",
            Name = "Test Item",
            BaseUnit = nameof(UnitOfMeasure.Unit),
            MinimumStock = 10,
            ExpiringSoonDays = 30
        };
    }
    private static UpdateItemDto UpdateRequest()
    {
        return new UpdateItemDto
        {
            Name = "Updated Item",
            BaseUnit = UnitOfMeasure.Unit,
            MinimumStock = 10,
            ExpiringSoonDays = 30
        };
    }
}