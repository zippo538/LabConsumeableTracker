using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Reflection;

namespace LabConsumableExpiryTracker.Tests.Services
{
    [TestFixture]
    public class LotServiceTests
    {
        private static readonly DateTimeOffset FixedNow =
            new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);

        private Mock<ILotRepository> _lotRepository = null!;
        private Mock<IItemRepository> _itemRepository = null!;
        private Mock<IMapper> _mapper = null!;
        private Mock<IUnitOfWork> _unitOfWork = null!;
        private Mock<TimeProvider> _timeProvider = null!;
        private LotService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _lotRepository = new Mock<ILotRepository>(MockBehavior.Strict);
            _itemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            _mapper = new Mock<IMapper>(MockBehavior.Strict);
            _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            _timeProvider = new Mock<TimeProvider>(MockBehavior.Strict);

            _timeProvider.Setup(timeProvider => timeProvider.GetUtcNow())
                .Returns(FixedNow);

            _sut = new LotService(
                _lotRepository.Object,
                _itemRepository.Object,
                _mapper.Object,
                _unitOfWork.Object,
                NullLogger<LotService>.Instance,
                _timeProvider.Object);
        }

        [Test]
        public async Task GetAllLot_WhenRepositoryReturnsLots_ReturnsSuccessWithMappedData()
        {
            var lot = CreateLotWithItem(expiringSoonDays: 30);
            var lots = new List<Lot> { lot };
            var expectedData = CreateLotDto(lot.Id);

            _lotRepository.Setup(repository => repository.GetAllWithItemAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(lots);
            _mapper.Setup(mapper => mapper.Map<LotDto>(lot)).Returns(expectedData);

            var result = await _sut.GetAllLot(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo("Lots retrieved successfully."));
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data!.Single(), Is.SameAs(expectedData));
                Assert.That(expectedData.ExpiryCondition, Is.EqualTo(ExpiryCondition.Valid));
                Assert.That(expectedData.IsEligible, Is.True);
                Assert.That(expectedData.EvaluatedAt, Is.EqualTo(FixedNow));
            });
        }

        [Test]
        public void GetAllLot_WhenRepositoryThrows_PropagatesException()
        {
            var exception = new InvalidOperationException("Database unavailable.");

            _lotRepository.Setup(repository => repository.GetAllWithItemAsync(
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetAllLot(CancellationToken.None));

            Assert.That(actual, Is.SameAs(exception));
            _mapper.Verify(mapper => mapper.Map<LotDto>(It.IsAny<Lot>()), Times.Never);
        }

        [Test]
        public async Task GetByIdLot_WhenLotDoesNotExist_ReturnsNotFoundError()
        {
            var id = Guid.NewGuid();

            _lotRepository.Setup(repository => repository.GetByIdWithItemAsync(
                    id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Lot?)null);

            var result = await _sut.GetByIdLot(id, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("NotFound"));
                Assert.That(result.Data, Is.Null);
            });

            _mapper.Verify(mapper => mapper.Map<LotDto>(It.IsAny<Lot>()), Times.Never);
        }

        [Test]
        public async Task GetByIdLot_WhenLotExists_ReturnsSuccessWithMappedData()
        {
            var lot = CreateLotWithItem(expiringSoonDays: 30);
            var expectedData = CreateLotDto(lot.Id);

            _lotRepository.Setup(repository => repository.GetByIdWithItemAsync(
                    lot.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lot);
            _mapper.Setup(mapper => mapper.Map<LotDto>(lot)).Returns(expectedData);

            var result = await _sut.GetByIdLot(lot.Id, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo("Lot retrieved successfully."));
                Assert.That(result.Data, Is.SameAs(expectedData));
                Assert.That(expectedData.EvaluatedAt, Is.EqualTo(FixedNow));
            });
        }

        [Test]
        public void GetByIdLot_WhenRepositoryThrows_PropagatesException()
        {
            var id = Guid.NewGuid();
            var exception = new TimeoutException("Query timed out.");

            _lotRepository.Setup(repository => repository.GetByIdWithItemAsync(
                    id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<TimeoutException>(
                () => _sut.GetByIdLot(id, CancellationToken.None));

            Assert.That(actual, Is.SameAs(exception));
        }

        [Test]
        public async Task CreateLot_WhenItemIdIsEmpty_ReturnsErrorWithoutCallingRepository()
        {
            var dto = CreateRequest();
            dto.ItemId = Guid.Empty;

            var result = await _sut.CreateLot(dto, CancellationToken.None);

            AssertError(result, "ItemId is required.");
            VerifyCreateWasNotContinued();
        }

        [Test]
        public async Task CreateLot_WhenLotNumberIsBlank_ReturnsErrorWithoutCallingRepository()
        {
            var dto = CreateRequest();
            dto.LotNumber = "  ";

            var result = await _sut.CreateLot(dto, CancellationToken.None);

            AssertError(result, "Lot number is required.");
            VerifyCreateWasNotContinued();
        }

        [Test]
        public async Task CreateLot_WhenInitialQuantityIsNotPositive_ReturnsErrorWithoutCallingRepository()
        {
            var dto = CreateRequest();
            dto.InitialQuantity = 0;

            var result = await _sut.CreateLot(dto, CancellationToken.None);

            AssertError(result, "Initial quantity must be greater than zero.");
            VerifyCreateWasNotContinued();
        }

        [Test]
        public async Task CreateLot_WhenDuplicateExists_ReturnsErrorWithoutPersisting()
        {
            var dto = CreateRequest();
            var trimmedLotNumber = dto.LotNumber.Trim();

            _lotRepository.Setup(repository => repository.ExistsAsync(
                    dto.ItemId,
                    trimmedLotNumber,
                    dto.ExpiryDate,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _sut.CreateLot(dto, CancellationToken.None);

            AssertError(result,
                "A lot with the same item, lot number, and expiry date already exists.");
            _mapper.Verify(mapper => mapper.Map<Lot>(It.IsAny<CreateLotDto>()), Times.Never);
            _lotRepository.Verify(repository => repository.AddAsync(
                It.IsAny<Lot>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Never);
            _itemRepository.Verify(repository => repository.GetByIdAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CreateLot_WhenRequestIsValid_MapsAddsCommitsAndReturnsSuccess()
        {
            var dto = CreateRequest();
            var mappedLot = CreateLot();
            var createdLot = CreateLot(id: mappedLot.Id, itemId: dto.ItemId);
            var item = new Item(
                dto.ItemId,
                "ITM-001",
                "Item Name",
                UnitOfMeasure.Unit,
                10m,
                30);
            var expectedData = CreateLotDto(createdLot.Id);

            _lotRepository.Setup(repository => repository.ExistsAsync(
                    dto.ItemId,
                    dto.LotNumber.Trim(),
                    dto.ExpiryDate,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mapper.Setup(mapper => mapper.Map<Lot>(dto)).Returns(mappedLot);
            _lotRepository.Setup(repository => repository.AddAsync(
                    mappedLot, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdLot);
            _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _itemRepository.Setup(repository => repository.GetByIdAsync(
                    dto.ItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);
            _mapper.Setup(mapper => mapper.Map<LotDto>(createdLot)).Returns(expectedData);

            var result = await _sut.CreateLot(dto, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo("Lot created successfully."));
                Assert.That(result.Data, Is.SameAs(expectedData));
                Assert.That(expectedData.EvaluatedAt, Is.EqualTo(FixedNow));
            });

            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Once);
            _itemRepository.Verify(repository => repository.GetByIdAsync(
                dto.ItemId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void CreateLot_WhenRepositoryThrows_PropagatesException()
        {
            var dto = CreateRequest();
            var exception = new InvalidOperationException("Duplicate check failed.");

            _lotRepository.Setup(repository => repository.ExistsAsync(
                    dto.ItemId,
                    dto.LotNumber.Trim(),
                    dto.ExpiryDate,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateLot(dto, CancellationToken.None));

            Assert.That(actual, Is.SameAs(exception));
            _lotRepository.Verify(repository => repository.AddAsync(
                It.IsAny<Lot>(), It.IsAny<CancellationToken>()), Times.Never);
            _itemRepository.Verify(repository => repository.GetByIdAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void CreateLot_WhenSaveChangesThrows_PropagatesException()
        {
            var dto = CreateRequest();
            var mappedLot = CreateLot();
            var exception = new InvalidOperationException("Commit failed.");

            _lotRepository.Setup(repository => repository.ExistsAsync(
                    dto.ItemId,
                    dto.LotNumber.Trim(),
                    dto.ExpiryDate,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mapper.Setup(mapper => mapper.Map<Lot>(dto)).Returns(mappedLot);
            _lotRepository.Setup(repository => repository.AddAsync(
                    mappedLot, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedLot);
            _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateLot(dto, CancellationToken.None));

            Assert.That(actual, Is.SameAs(exception));
            _mapper.Verify(mapper => mapper.Map<LotDto>(It.IsAny<Lot>()), Times.Never);
            _itemRepository.Verify(repository => repository.GetByIdAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task UpdateLot_WhenLotDoesNotExist_ReturnsErrorWithoutPersisting()
        {
            var id = Guid.NewGuid();
            var dto = UpdateRequest();

            _lotRepository.Setup(repository => repository.GetByIdWithItemAsync(
                    id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Lot?)null);

            var result = await _sut.UpdateLot(id, dto, CancellationToken.None);

            AssertError(result, "Lot not found.");
            _lotRepository.Verify(repository => repository.UpdateAsync(
                It.IsAny<Lot>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task UpdateLot_WhenRequestIsValid_UpdatesStorageCommitsAndReturnsSuccess()
        {
            var lot = CreateLotWithItem(expiringSoonDays: 30);
            var dto = UpdateRequest();
            var expectedData = CreateLotDto(lot.Id);

            _lotRepository.Setup(repository => repository.GetByIdWithItemAsync(
                    lot.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lot);
            _lotRepository.Setup(repository => repository.UpdateAsync(
                    lot, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lot);
            _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _mapper.Setup(mapper => mapper.Map<LotDto>(lot)).Returns(expectedData);

            var result = await _sut.UpdateLot(lot.Id, dto, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo("Lot updated successfully."));
                Assert.That(result.Data, Is.SameAs(expectedData));
                Assert.That(lot.StorageLocation, Is.EqualTo(dto.StorageLocation.Trim()));
                Assert.That(expectedData.EvaluatedAt, Is.EqualTo(FixedNow));
            });

            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void UpdateLot_WhenStorageLocationIsInvalid_PropagatesDomainException()
        {
            var lot = CreateLotWithItem(expiringSoonDays: 30);
            var dto = UpdateRequest();
            dto.StorageLocation = " ";

            _lotRepository.Setup(repository => repository.GetByIdWithItemAsync(
                    lot.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lot);

            Assert.ThrowsAsync<ArgumentException>(
                () => _sut.UpdateLot(lot.Id, dto, CancellationToken.None));

            _lotRepository.Verify(repository => repository.UpdateAsync(
                It.IsAny<Lot>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task DeleteLot_WhenDeleteReturnsFalse_ReturnsErrorAndCommits()
        {
            var id = Guid.NewGuid();

            _lotRepository.Setup(repository => repository.DeleteAsync(
                    id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await _sut.DeleteLot(id, CancellationToken.None);

            AssertError(result, "Lot not found.");
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteLot_WhenDeleteReturnsTrue_ReturnsSuccessAndCommits()
        {
            var id = Guid.NewGuid();

            _lotRepository.Setup(repository => repository.DeleteAsync(
                    id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _sut.DeleteLot(id, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Data, Is.True);
                Assert.That(result.Message, Is.EqualTo("Lot deleted successfully."));
            });
        }

        [Test]
        public void DeleteLot_WhenRepositoryThrows_PropagatesExceptionAndDoesNotCommit()
        {
            var id = Guid.NewGuid();
            var exception = new InvalidOperationException("Delete failed.");

            _lotRepository.Setup(repository => repository.DeleteAsync(
                    id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.DeleteLot(id, CancellationToken.None));

            Assert.That(actual, Is.SameAs(exception));
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetSummaryByItemId_WhenRepositoryReturnsLots_ReturnsMappedSuccess()
        {
            var itemId = Guid.NewGuid();
            var lots = new List<Lot> { CreateLot(itemId: itemId) };
            var expectedData = new List<LotSummaryDto>();

            _lotRepository.Setup(repository => repository.GetByItemIdAsync(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lots);
            _mapper.Setup(mapper => mapper.Map<IEnumerable<LotSummaryDto>>(It.IsAny<object>()))
                .Returns(expectedData);

            var result = await _sut.GetSummaryByItemId(itemId, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo("Lots retrieved successfully."));
                Assert.That(result.Data, Is.SameAs(expectedData));
            });
        }

        [Test]
        public void GetSummaryByItemId_WhenRepositoryThrows_PropagatesException()
        {
            var itemId = Guid.NewGuid();
            var exception = new InvalidOperationException("Summary query failed.");

            _lotRepository.Setup(repository => repository.GetByItemIdAsync(
                    itemId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetSummaryByItemId(itemId, CancellationToken.None));

            Assert.That(actual, Is.SameAs(exception));
        }

        [Test]
        public async Task GetAllSummary_WhenRepositoryReturnsLots_ReturnsMappedSuccess()
        {
            var lots = new List<Lot> { CreateLot() };
            var expectedData = new List<LotSummaryDto>();

            _lotRepository.Setup(repository => repository.GetAllAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(lots);
            _mapper.Setup(mapper => mapper.Map<IEnumerable<LotSummaryDto>>(It.IsAny<object>()))
                .Returns(expectedData);

            var result = await _sut.GetAllSummary(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo("Lot summaries retrieved successfully."));
                Assert.That(result.Data, Is.SameAs(expectedData));
            });
        }

        [Test]
        public void GetAllSummary_WhenMapperThrows_PropagatesException()
        {
            var lots = new List<Lot> { CreateLot() };
            var exception = new AutoMapperMappingException("Mapping failed.");

            _lotRepository.Setup(repository => repository.GetAllAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(lots);
            _mapper.Setup(mapper => mapper.Map<IEnumerable<LotSummaryDto>>(It.IsAny<object>()))
                .Throws(exception);

            var actual = Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _sut.GetAllSummary(CancellationToken.None));

            Assert.That(actual, Is.SameAs(exception));
        }

        private static Lot CreateLot(Guid? id = null, Guid? itemId = null)
        {
            return new Lot(
                id ?? Guid.NewGuid(),
                itemId ?? Guid.NewGuid(),
                "LOT-001",
                "SUP-LOT-001",
                new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero),
                "Supplier",
                100m,
                100m,
                new DateOnly(2026, 12, 1),
                "Rack-A1",
                LotAdministrativeStatus.Active);
        }

        // Lot.Item has a private setter with no public API to attach it, so tests
        // that rely on lot.Item (GetAllLot/GetByIdLot/UpdateLot mapping) build the
        // lot via reflection to simulate what EF Core would populate via navigation.
        private static Lot CreateLotWithItem(
            Guid? id = null,
            Guid? itemId = null,
            int expiringSoonDays = 30,
            DateOnly? expiryDate = null)
        {
            var resolvedItemId = itemId ?? Guid.NewGuid();

            var lot = new Lot(
                id ?? Guid.NewGuid(),
                resolvedItemId,
                "LOT-001",
                "SUP-LOT-001",
                new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero),
                "Supplier",
                100m,
                100m,
                expiryDate ?? new DateOnly(2026, 12, 1),
                "Rack-A1",
                LotAdministrativeStatus.Active);

            var item = new Item(
                resolvedItemId,
                "ITM-001",
                "Item Name",
                UnitOfMeasure.Unit,
                10m,
                expiringSoonDays);

            typeof(Lot)
                .GetProperty(nameof(Lot.Item), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(lot, item);

            return lot;
        }

        private static CreateLotDto CreateRequest()
        {
            return new CreateLotDto
            {
                ItemId = Guid.NewGuid(),
                LotNumber = " LOT-001 ",
                SupplierLotNumber = "SUP-LOT-001",
                SupplierName = "Supplier",
                InitialQuantity = 100m,
                ReceivedAt = new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero),
                ExpiryDate = new DateOnly(2026, 12, 1),
                StorageLocation = "Rack-A1"
            };
        }

        private static UpdateLotDto UpdateRequest()
        {
            return new UpdateLotDto
            {
                LotNumber = "LOT-001",
                SupplierLotNumber = "SUP-LOT-001",
                SupplierName = "Supplier",
                ExpiryDate = new DateOnly(2026, 12, 1),
                StorageLocation = " Rack-B2 ",
                RowVersion = new byte[] { 1 }
            };
        }

        private static LotDto CreateLotDto(Guid id)
        {
            return new LotDto
            {
                Id = id,
                ItemId = Guid.NewGuid(),
                LotNumber = "LOT-001",
                ReceivedAt = new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero),
                InitialQuantity = 100m,
                RemainingQuantity = 100m,
                ExpiryDate = new DateOnly(2026, 12, 1),
                StorageLocation = "Rack-A1"
            };
        }

        private static void AssertError<T>(ServiceResult<T> result, string message)
        {
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo(message));
            });
        }

        private void VerifyCreateWasNotContinued()
        {
            _lotRepository.Verify(repository => repository.ExistsAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()), Times.Never);
            _lotRepository.Verify(repository => repository.AddAsync(
                It.IsAny<Lot>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Never);
            _itemRepository.Verify(repository => repository.GetByIdAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}