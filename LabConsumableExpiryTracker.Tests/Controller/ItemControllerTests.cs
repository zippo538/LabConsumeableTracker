
using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Controller
{
    [TestFixture]
    public class ItemControllerTests
    {
        private Mock<IItemService> _itemService = null!;
        private Mock<ILotService> _lotService = null!;
        private ItemController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _itemService = new Mock<IItemService>(MockBehavior.Strict);
            _lotService = new Mock<ILotService>(MockBehavior.Strict);
            _sut = new ItemController(
                _itemService.Object,
                _lotService.Object);
        }

        [Test]
        public async Task GetAll_WhenServiceSucceeds_ReturnsOkWithMessage()
        {
            var expected = ServiceResult<IEnumerable<ItemDto>>.SuccessResult(
                Array.Empty<ItemDto>(), "Items retrieved successfully.");

            _itemService.Setup(service => service.GetAllItem(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetAll(CancellationToken.None);
            var ok = result.Result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.Not.Null);
                Assert.That(ok!.Value, Is.SameAs(expected));
                Assert.That(expected.Success, Is.True);
                Assert.That(expected.Message, Is.EqualTo("Items retrieved successfully."));
            });

            _itemService.Verify(
                service => service.GetAllItem(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetAll_WhenServiceFails_ReturnsBadRequestWithMessage()
        {
            var expected = ServiceResult<IEnumerable<ItemDto>>.ErrorResult("Items could not be retrieved.");

            _itemService.Setup(service => service.GetAllItem(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetAll(CancellationToken.None);
            var badRequest = result.Result as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(badRequest, Is.Not.Null);
                Assert.That(badRequest!.Value, Is.SameAs(expected));
                Assert.That(expected.Success, Is.False);
                Assert.That(expected.Message, Is.EqualTo("Items could not be retrieved."));
            });
        }

        [Test]
        public async Task GetById_WhenServiceSucceeds_ReturnsOkWithMessage()
        {
            var id = Guid.NewGuid();
            var expected = ServiceResult<ItemDto>.SuccessResult(
                CreateItemResult(id), "Item retrieved successfully.");

            _itemService.Setup(service => service.GetByIdItem(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetById(id, CancellationToken.None);
            var ok = result.Result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.Not.Null);
                Assert.That(ok!.Value, Is.SameAs(expected));
                Assert.That(expected.Message, Is.EqualTo("Item retrieved successfully."));
            });

            _itemService.Verify(
                service => service.GetByIdItem(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetById_WhenServiceFails_ReturnsNotFoundWithMessage()
        {
            var id = Guid.NewGuid();
            var expected = ServiceResult<ItemDto>.ErrorResult("Item not found.");

            _itemService.Setup(service => service.GetByIdItem(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetById(id, CancellationToken.None);
            var notFound = result.Result as NotFoundObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(notFound, Is.Not.Null);
                Assert.That(notFound!.Value, Is.SameAs(expected));
                Assert.That(expected.Success, Is.False);
                Assert.That(expected.Message, Is.EqualTo("Item not found."));
            });
        }

        [Test]
        public async Task Create_WhenServiceSucceeds_ReturnsCreatedAtActionWithMessage()
        {
            var dto = CreateRequest();
            var item = CreateItemResult();
            var expected = ServiceResult<ItemDto>.SuccessResult(
                item, "Item created successfully.");

            _itemService.Setup(service => service.CreateItem(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Create(dto, CancellationToken.None);
            var created = result.Result as CreatedAtActionResult;

            Assert.Multiple(() =>
            {
                Assert.That(created, Is.Not.Null);
                Assert.That(created!.ActionName, Is.EqualTo(nameof(ItemController.GetById)));
                Assert.That(created.RouteValues!["id"], Is.EqualTo(item.Id));
                Assert.That(created.Value, Is.SameAs(expected));
                Assert.That(expected.Message, Is.EqualTo("Item created successfully."));
            });

            _itemService.Verify(
                service => service.CreateItem(dto, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Create_WhenServiceReturnsValidationError_ReturnsBadRequestWithMessage()
        {
            var dto = CreateRequest();
            var expected = ServiceResult<ItemDto>.ErrorResult(
                "Validation failed.",
                new List<string> { "Item code is required." });

            _itemService.Setup(service => service.CreateItem(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Create(dto, CancellationToken.None);
            var badRequest = result.Result as BadRequestObjectResult;
            var body = badRequest?.Value as ServiceResult<ItemDto>;

            Assert.Multiple(() =>
            {
                Assert.That(badRequest, Is.Not.Null);
                Assert.That(body, Is.Not.Null);
                Assert.That(body!.Success, Is.False);
                Assert.That(body.Message, Is.EqualTo("Validation failed."));
                Assert.That(body.Errors, Does.Contain("Item code is required."));
            });

            _itemService.Verify(
                service => service.CreateItem(dto, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Create_WhenServiceFails_ReturnsBadRequestWithMessage()
        {
            var dto = CreateRequest();
            var expected = ServiceResult<ItemDto>.ErrorResult("Item code already exists.");

            _itemService.Setup(service => service.CreateItem(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Create(dto, CancellationToken.None);
            var badRequest = result.Result as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(badRequest, Is.Not.Null);
                Assert.That(badRequest!.Value, Is.SameAs(expected));
                Assert.That(expected.Message, Is.EqualTo("Item code already exists."));
            });
        }

        [Test]
        public async Task Update_WhenServiceSucceeds_ReturnsOkWithMessage()
        {
            var id = Guid.NewGuid();
            var dto = UpdateRequest();
            var expected = ServiceResult<ItemDto>.SuccessResult(
                CreateItemResult(id), "Item updated successfully.");

            _itemService.Setup(service => service.UpdateItem(id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Update(id, dto, CancellationToken.None);
            var ok = result.Result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.Not.Null);
                Assert.That(ok!.Value, Is.SameAs(expected));
                Assert.That(expected.Message, Is.EqualTo("Item updated successfully."));
            });

            _itemService.Verify(
                service => service.UpdateItem(id, dto, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Update_WhenServiceReturnsValidationError_ReturnsConflictWithMessage()
        {
            var id = Guid.NewGuid();
            var dto = UpdateRequest();

            var expected = ServiceResult<ItemDto>.ErrorResult(
                "Validation failed.",
                new List<string> { "Item name is required." });

            _itemService.Setup(service => service.UpdateItem(id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Update(id, dto, CancellationToken.None);

            var conflict = result.Result as ConflictObjectResult;
            var body = conflict?.Value as ServiceResult<ItemDto>;

            Assert.Multiple(() =>
            {
                Assert.That(conflict, Is.Not.Null);
                Assert.That(body, Is.Not.Null);
                Assert.That(body!.Success, Is.False);
                Assert.That(body.Message, Is.EqualTo("Validation failed."));
                Assert.That(body.Errors, Does.Contain("Item name is required."));
            });

            _itemService.Verify(
                service => service.UpdateItem(id, dto, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Update_WhenServiceFails_ReturnsConflictWithMessage()
        {
            var id = Guid.NewGuid();
            var dto = UpdateRequest();

            var expected = ServiceResult<ItemDto>.ErrorResult("Item not found.");

            _itemService.Setup(service => service.UpdateItem(id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Update(id, dto, CancellationToken.None);

            var conflict = result.Result as ConflictObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(conflict, Is.Not.Null);
                Assert.That(conflict!.Value, Is.SameAs(expected));
                Assert.That(expected.Message, Is.EqualTo("Item not found."));
            });
        }

        [Test]
        public async Task Delete_WhenServiceSucceeds_ReturnsOkWithMessage()
        {
            var id = Guid.NewGuid();
            var expected = ServiceResult<bool>.SuccessResult(true, "Item deleted successfully.");

            _itemService.Setup(service => service.DeleteItem(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Delete(id, CancellationToken.None);
            var ok = result.Result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.Not.Null);
                Assert.That(ok!.Value, Is.SameAs(expected));
                Assert.That(expected.Message, Is.EqualTo("Item deleted successfully."));
            });
        }

        [Test]
        public async Task Delete_WhenServiceFails_ReturnsNotFoundWithMessage()
        {
            var id = Guid.NewGuid();
            var expected = ServiceResult<bool>.ErrorResult("Item not found.");

            _itemService.Setup(service => service.DeleteItem(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Delete(id, CancellationToken.None);
            var notFound = result.Result as NotFoundObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(notFound, Is.Not.Null);
                Assert.That(notFound!.Value, Is.SameAs(expected));
                Assert.That(expected.Message, Is.EqualTo("Item not found."));
            });
        }

        [Test]
        public async Task GetSummaryByItemId_WhenServiceSucceeds_ReturnsOkWithMessage()
        {
            var itemId = Guid.NewGuid();
            var expected = ServiceResult<IEnumerable<LotSummaryDto>>.SuccessResult(
                Array.Empty<LotSummaryDto>(), "Lot summary retrieved successfully.");

            _lotService.Setup(service => service.GetSummaryByItemId(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetSummaryByItemId(itemId, CancellationToken.None);
            var ok = result.Result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.Not.Null);
                Assert.That(ok!.Value, Is.SameAs(expected));
                Assert.That(expected.Message, Is.EqualTo("Lot summary retrieved successfully."));
            });
        }

        [Test]
        public async Task GetSummaryByItemId_WhenServiceFails_ReturnsBadRequestWithMessage()
        {
            var itemId = Guid.NewGuid();
            var expected = ServiceResult<IEnumerable<LotSummaryDto>>.ErrorResult(
                "Lot summary could not be retrieved.");

            _lotService.Setup(service => service.GetSummaryByItemId(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetSummaryByItemId(itemId, CancellationToken.None);
            var badRequest = result.Result as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(badRequest, Is.Not.Null);
                Assert.That(badRequest!.Value, Is.SameAs(expected));
                Assert.That(expected.Message, Is.EqualTo("Lot summary could not be retrieved."));
            });
        }

        private static ItemDto CreateItemResult(Guid? id = null)
        {
            return new ItemDto
            {
                Id = id ?? Guid.NewGuid(),
                Code = "ITEM-001",
                Name = "Test item"
            };
        }

        private static CreateItemDto CreateRequest()
        {
            return new CreateItemDto
            {
                Code = "ITEM-001",
                Name = "Test item",
                BaseUnit = Enum.GetValues<UnitOfMeasure>().First(),
                MinimumStock = 10,
                ExpiringSoonDays = 30
            };
        }

        private static UpdateItemDto UpdateRequest()
        {
            return new UpdateItemDto
            {
                Name = "Updated item",
                BaseUnit = Enum.GetValues<UnitOfMeasure>().First(),
                MinimumStock = 10,
                ExpiringSoonDays = 30
            };
        }
    }
}
