using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using Microsoft.Extensions.Logging.Abstractions;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Services
{
    [TestFixture]
    public class ItemServiceTests
    {
        private static readonly DateTimeOffset FixedNow =
         new(2026, 9, 13, 10, 30, 0, TimeSpan.Zero);

        private Mock<IItemRepository> _itemRepository = null!;
        private Mock<ILotRepository> _lotRepository = null!;
        private Mock<IMapper> _mapper = null!;
        private Mock<IUnitOfWork> _unitOfWork = null!;
        private ItemService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _itemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            _lotRepository = new Mock<ILotRepository>(MockBehavior.Strict);
            _mapper = new Mock<IMapper>(MockBehavior.Strict);
            _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

            _sut = new ItemService(
                _itemRepository.Object,
                _mapper.Object,
                new FixedTimeProvider(FixedNow),
                _lotRepository.Object,
                _unitOfWork.Object,
                NullLogger<ItemService>.Instance);
        }

        [Test]
        public async Task GetAllItem_WhenItemsExist_ReturnsItemsWithUsableQuantityAndStockStatus()
        {
            var item = CreateItem(minimumStock: 10);
            var expectedDto = CreateItemDtoResult(item.Id);
            var today = DateOnly.FromDateTime(FixedNow.UtcDateTime);
            var items = new[] { item };

            _itemRepository.Setup(repository => repository.GetAllWithLotsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);
            _lotRepository.Setup(repository => repository.GetUsableQuantityByItemIdsAsync(
                    It.Is<IEnumerable<Guid>>(ids => ids.Single() == item.Id),
                    today,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, decimal> { [item.Id] = 5m });
            _mapper.Setup(mapper => mapper.Map<ItemDto>(item)).Returns(expectedDto);

            var result = await _sut.GetAllItem(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo("Items retrieved successfully."));
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data!.Single(), Is.SameAs(expectedDto));
                Assert.That(expectedDto.TotalRemainingQuantity, Is.EqualTo(5m));
                Assert.That(expectedDto.StockStatus, Is.EqualTo(StockLevelStatus.LowStock));
            });

            _itemRepository.Verify(repository => repository.GetAllWithLotsAsync(
                It.IsAny<CancellationToken>()), Times.Once);
            _lotRepository.Verify(repository => repository.GetUsableQuantityByItemIdsAsync(
                It.IsAny<IEnumerable<Guid>>(), today, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetAllItem_WhenRepositoryThrows_PropagatesException()
        {
            var exception = new InvalidOperationException("Database unavailable.");

            _itemRepository.Setup(repository => repository.GetAllWithLotsAsync(
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetAllItem(CancellationToken.None));

            Assert.That(actual, Is.SameAs(exception));
            _lotRepository.Verify(repository => repository.GetUsableQuantityByItemIdsAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetByIdItem_WhenItemDoesNotExist_ReturnsError()
        {
            var id = Guid.NewGuid();

            _itemRepository.Setup(repository => repository.GetByIdWithLotsAsync(
                    id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Item?)null);

            var result = await _sut.GetByIdItem(id, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Item not found."));
                Assert.That(result.Data, Is.Null);
            });

            _lotRepository.Verify(repository => repository.GetTotalUsableQuantityAsync(
                It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
            _mapper.Verify(mapper => mapper.Map<ItemDto>(It.IsAny<Item>()), Times.Never);
        }

        [Test]
        public async Task GetByIdItem_WhenItemExists_ReturnsMappedItemWithStockInformation()
        {
            var item = CreateItem(minimumStock: 10);
            var expectedDto = CreateItemDtoResult(item.Id);
            var today = DateOnly.FromDateTime(FixedNow.UtcDateTime);

            _itemRepository.Setup(repository => repository.GetByIdWithLotsAsync(
                    item.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);
            _lotRepository.Setup(repository => repository.GetTotalUsableQuantityAsync(
                    item.Id, today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(20m);
            _mapper.Setup(mapper => mapper.Map<ItemDto>(item)).Returns(expectedDto);

            var result = await _sut.GetByIdItem(item.Id, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo("Item retrieved successfully."));
                Assert.That(result.Data, Is.SameAs(expectedDto));
                Assert.That(expectedDto.TotalRemainingQuantity, Is.EqualTo(20m));
                Assert.That(expectedDto.StockStatus, Is.EqualTo(StockLevelStatus.Sufficient));
            });
        }

        [Test]
        public async Task GetByIdItem_WhenLotRepositoryThrows_PropagatesException()
        {
            var item = CreateItem();
            var exception = new TimeoutException("Lot query timed out.");

            _itemRepository.Setup(repository => repository.GetByIdWithLotsAsync(
                    item.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);
            _lotRepository.Setup(repository => repository.GetTotalUsableQuantityAsync(
                    item.Id, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<TimeoutException>(
                () => _sut.GetByIdItem(item.Id, CancellationToken.None));

            Assert.That(actual, Is.SameAs(exception));
            _mapper.Verify(mapper => mapper.Map<ItemDto>(It.IsAny<Item>()), Times.Never);
        }

        [Test]
        public async Task CreateItem_WhenCodeAlreadyExists_ReturnsErrorAndDoesNotPersist()
        {
            var dto = CreateRequest();
            var existing = CreateItem();

            _itemRepository.Setup(repository => repository.GetByCodeAsync(
                    dto.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var result = await _sut.CreateItem(dto, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Item code already exists."));
            });

            _mapper.Verify(mapper => mapper.Map<Item>(It.IsAny<CreateItemDto>()), Times.Never);
            _itemRepository.Verify(repository => repository.AddAsync(
                It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CreateItem_WhenCodeIsAvailable_CreatesItemAndCommits()
        {
            var dto = CreateRequest();
            var mappedItem = CreateItem();
            var createdItem = CreateItem(mappedItem.Id);
            var expectedDto = CreateItemDtoResult(createdItem.Id);

            _itemRepository.Setup(repository => repository.GetByCodeAsync(
                    dto.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Item?)null);
            _mapper.Setup(mapper => mapper.Map<Item>(dto)).Returns(mappedItem);
            _itemRepository.Setup(repository => repository.AddAsync(
                    mappedItem, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdItem);
            _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _mapper.Setup(mapper => mapper.Map<ItemDto>(createdItem)).Returns(expectedDto);

            var result = await _sut.CreateItem(dto, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo("Item created successfully."));
                Assert.That(result.Data, Is.SameAs(expectedDto));
            });

            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task CreateItem_WhenAddThrows_PropagatesExceptionAndDoesNotCommit()
        {
            var dto = CreateRequest();
            var mappedItem = CreateItem();
            var exception = new InvalidOperationException("Insert failed.");

            _itemRepository.Setup(repository => repository.GetByCodeAsync(
                    dto.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Item?)null);
            _mapper.Setup(mapper => mapper.Map<Item>(dto)).Returns(mappedItem);
            _itemRepository.Setup(repository => repository.AddAsync(
                    mappedItem, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateItem(dto, CancellationToken.None));

            Assert.That(actual, Is.SameAs(exception));
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CreateItem_WhenSaveChangesThrows_PropagatesException()
        {
            var dto = CreateRequest();
            var mappedItem = CreateItem();
            var exception = new InvalidOperationException("Commit failed.");

            _itemRepository.Setup(repository => repository.GetByCodeAsync(
                    dto.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Item?)null);
            _mapper.Setup(mapper => mapper.Map<Item>(dto)).Returns(mappedItem);
            _itemRepository.Setup(repository => repository.AddAsync(
                    mappedItem, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedItem);
            _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateItem(dto, CancellationToken.None));

            Assert.That(actual, Is.SameAs(exception));
            _mapper.Verify(mapper => mapper.Map<ItemDto>(It.IsAny<Item>()), Times.Never);
        }

        [Test]
        public async Task UpdateItem_WhenItemDoesNotExist_ReturnsErrorAndDoesNotPersist()
        {
            var id = Guid.NewGuid();
            var request = UpdateRequest();

            _itemRepository.Setup(repository => repository.GetByIdAsync(
                    id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Item?)null);

            var result = await _sut.UpdateItem(id, request, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Item not found."));
            });

            _itemRepository.Verify(repository => repository.UpdateAsync(
                It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task UpdateItem_WhenRequestIsValid_UpdatesDetailsAndCommits()
        {
            var item = CreateItem(minimumStock: 10);
            var request = UpdateRequest();
            var expectedDto = CreateItemDtoResult(item.Id);

            _itemRepository.Setup(repository => repository.GetByIdAsync(
                    item.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);
            _itemRepository.Setup(repository => repository.UpdateAsync(
                    item, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);
            _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _mapper.Setup(mapper => mapper.Map<ItemDto>(item)).Returns(expectedDto);

            var result = await _sut.UpdateItem(item.Id, request, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo("Item updated successfully."));
                Assert.That(result.Data, Is.SameAs(expectedDto));
                Assert.That(item.Name, Is.EqualTo(request.Name.Trim()));
                Assert.That(item.MinimumStock, Is.EqualTo(request.MinimumStock));
                Assert.That(item.ExpiringSoonDays, Is.EqualTo(request.ExpiringSoonDays));
            });

            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task UpdateItem_WhenDomainValidationFails_PropagatesExceptionAndDoesNotPersist()
        {
            var item = CreateItem();
            var request = UpdateRequest();
            request.MinimumStock = -1;

            _itemRepository.Setup(repository => repository.GetByIdAsync(
                    item.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _sut.UpdateItem(item.Id, request, CancellationToken.None));

            _itemRepository.Verify(repository => repository.UpdateAsync(
                It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task DeleteItem_WhenRepositoryReturnsFalse_ReturnsNotFoundAndCommits()
        {
            var id = Guid.NewGuid();

            _itemRepository.Setup(repository => repository.DeleteAsync(
                    id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await _sut.DeleteItem(id, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Item not found."));
            });

            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteItem_WhenRepositoryReturnsTrue_ReturnsSuccessAndCommits()
        {
            var id = Guid.NewGuid();

            _itemRepository.Setup(repository => repository.DeleteAsync(
                    id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _sut.DeleteItem(id, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Data, Is.True);
                Assert.That(result.Message, Is.EqualTo("Item deleted successfully."));
            });
        }

        [Test]
        public async Task DeleteItem_WhenRepositoryThrows_PropagatesExceptionAndDoesNotCommit()
        {
            var id = Guid.NewGuid();
            var exception = new InvalidOperationException("Delete failed.");

            _itemRepository.Setup(repository => repository.DeleteAsync(
                    id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.DeleteItem(id, CancellationToken.None));

            Assert.That(actual, Is.SameAs(exception));
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Never);
        }

        private static Item CreateItem(Guid? id = null, decimal minimumStock = 5)
        {
            return new Item(
                id ?? Guid.NewGuid(),
                "ITEM-001",
                "Test item",
                Enum.GetValues<UnitOfMeasure>().First(),
                minimumStock,
                30);
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

        private static ItemDto CreateItemDtoResult(Guid id)
        {
            return new ItemDto
            {
                Id = id,
                Code = "ITEM-001",
                Name = "Test item"
            };
        }

        private sealed class FixedTimeProvider : TimeProvider
        {
            private readonly DateTimeOffset _now;

            public FixedTimeProvider(DateTimeOffset now)
            {
                _now = now;
            }

            public override DateTimeOffset GetUtcNow() => _now;
        }
    }
}