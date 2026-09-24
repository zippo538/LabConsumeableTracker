using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using LabConsumableExpiryTracker.Tests.Helpers;
namespace LabConsumableExpiryTracker.Tests.Repository;
    [TestFixture]
    public class ItemRepositoryTests
    {
        private AppDbContext _context = null!;
        private ItemRepository _repository = null!;
        private readonly List<Guid> _createdItemIds = [];
        private static string _connectionString = string.Empty;
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _connectionString = Connection.GetConnection("UnitTestConnection");
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_connectionString)
                .EnableSensitiveDataLogging()
                .Options;
            _context = new AppDbContext(options);
            _repository = new ItemRepository(
                _context,
                NullLogger<ItemRepository>.Instance);
            Assert.That(
                await _context.Database.CanConnectAsync(),
                Is.True,
                "The PostgreSQL database cannot be reached.");
        }
        [SetUp]
        public void SetUp()
        {
            _context.ChangeTracker.Clear();
            _createdItemIds.Clear();
        }
        [TearDown]
        public async Task TearDown()
        {
            if (_createdItemIds.Count == 0)
            {
                return;
            }
            var lots = await _context.Set<Lot>()
                .Where(lot => _createdItemIds.Contains(lot.ItemId))
                .ToListAsync();
            _context.Set<Lot>().RemoveRange(lots);
            var items = await _context.Items
                .Where(item => _createdItemIds.Contains(item.Id))
                .ToListAsync();
            _context.Items.RemoveRange(items);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }
        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _context.DisposeAsync();
        }
        [Test]
        public async Task GetAllWithLotsAsync_WhenItemsExist_ReturnsItemsAndTheirLots()
        {
            var item = CreateItem();
            var lot = CreateLot(item.Id);
            _context.Items.Add(item);
            _context.Lots.Add(lot);
            await _context.SaveChangesAsync();
            _createdItemIds.Add(item.Id);
            _context.ChangeTracker.Clear();
            var result = await _repository.GetAllWithLotsAsync();
            var loadedItem = result.Single(found => found.Id == item.Id);
            Assert.Multiple(() =>
            {
                Assert.That(loadedItem.Code, Is.EqualTo(item.Code));
                Assert.That(loadedItem.Lots, Has.Count.EqualTo(1));
                Assert.That(loadedItem.Lots.Single().Id, Is.EqualTo(lot.Id));
            });
        }
        [Test]
        public async Task GetAllWithLotsAsync_WhenNoItemsMatch_ReturnsEmptyList()
        {
            var result = await _repository.GetAllWithLotsAsync();
            Assert.That(result, Is.Not.Null);
        }
        [Test]
        public async Task GetByCodeAsync_WhenCodeExists_ReturnsMatchingItem()
        {
            var item = CreateItem();
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            _createdItemIds.Add(item.Id);
            _context.ChangeTracker.Clear();
            var result = await _repository.GetByCodeAsync(item.Code);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(item.Id));
            Assert.That(result.Code, Is.EqualTo(item.Code));
        }
        [Test]
        public async Task GetByCodeAsync_WhenCodeDoesNotExist_ReturnsNull()
        {
            var result = await _repository.GetByCodeAsync(
                $"NOT-FOUND-{Guid.NewGuid():N}");
            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task GetByIdWithLotsAsync_WhenIdExists_ReturnsItemWithLots()
        {
            var item = CreateItem();
            var lot = CreateLot(item.Id);
            _context.Items.Add(item);
            _context.Lots.Add(lot);
            await _context.SaveChangesAsync();
            _createdItemIds.Add(item.Id);
            _context.ChangeTracker.Clear();
            var result = await _repository.GetByIdWithLotsAsync(item.Id);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.Id, Is.EqualTo(item.Id));
                Assert.That(result.Lots, Has.Count.EqualTo(1));
                Assert.That(result.Lots.Single().ItemId, Is.EqualTo(item.Id));
            });
        }
        [Test]
        public async Task GetByIdWithLotsAsync_WhenIdDoesNotExist_ReturnsNull()
        {
            var result = await _repository.GetByIdWithLotsAsync(Guid.NewGuid());
            Assert.That(result, Is.Null);
        }
        [Test]
        public void GetAllWithLotsAsync_WhenCancelled_ThrowsOperationCanceledException()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.CatchAsync<OperationCanceledException>(
                () => _repository.GetAllWithLotsAsync(cancellation.Token));
        }
        [Test]
        public void GetByCodeAsync_WhenCancelled_ThrowsOperationCanceledException()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.ThrowsAsync<OperationCanceledException>(
                () => _repository.GetByCodeAsync("ITEM-001", cancellation.Token));
        }
        [Test]
        public void GetByIdWithLotsAsync_WhenCancelled_ThrowsOperationCanceledException()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.ThrowsAsync<OperationCanceledException>(
                () => _repository.GetByIdWithLotsAsync(Guid.NewGuid(), cancellation.Token));
        }
        private static Item CreateItem()
        {
            return new Item(
                Guid.NewGuid(),
                $"ITEM-{Guid.NewGuid():N}",
                "Repository test item",
                Enum.GetValues<UnitOfMeasure>().First(),
                10m,
                30);
        }
        private static Lot CreateLot(Guid itemId)
        {
            return new Lot(
                Guid.NewGuid(),
                itemId,
                $"LOT-{Guid.NewGuid():N}",
                "SUP-001",
                new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero),
                "Test supplier",
                100m,
                100m,
                new DateOnly(2026, 12, 1),
                "Rack-A1",
                LotAdministrativeStatus.Active);
        }
    }
