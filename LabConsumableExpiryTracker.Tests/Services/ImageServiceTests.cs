using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Services
{
    [TestFixture]
    public class ImageServiceTests
    {
        private Mock<IImageStorageRepository> _imageStorageRepository = null!;
        private Mock<ICurrentUserContext> _currentUserContext = null!;
        private ImageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _imageStorageRepository =
                new Mock<IImageStorageRepository>(MockBehavior.Strict);

            _currentUserContext =
                new Mock<ICurrentUserContext>(MockBehavior.Strict);

            _service = new ImageService(
                _imageStorageRepository.Object,
                _currentUserContext.Object);
        }

        [Test]
        public async Task GetSecureImageAsync_ShouldReturnError_WhenUserIsNotAuthenticated()
        {
            var imageId = Guid.NewGuid();

            _currentUserContext
                .Setup(x => x.IsAuthenticated)
                .Returns(false);

            var result = await _service.GetSecureImageAsync(
                imageId,
                CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Access denied. Please log in first."));

            _currentUserContext.Verify(
                x => x.IsInRole(It.IsAny<string>()),
                Times.Never);

            _imageStorageRepository.Verify(
                x => x.GetImageAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetSecureImageAsync_ShouldReturnError_WhenUserHasNoRequiredRole()
        {
            var imageId = Guid.NewGuid();

            _currentUserContext
                .Setup(x => x.IsAuthenticated)
                .Returns(true);

            _currentUserContext
                .Setup(x => x.IsInRole(nameof(UserRole.Scientist)))
                .Returns(false);

            _currentUserContext
                .Setup(x => x.IsInRole(nameof(UserRole.WarehouseAdmin)))
                .Returns(false);

            var result = await _service.GetSecureImageAsync(
                imageId,
                CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "You do not have permission to view this image."));

            _imageStorageRepository.Verify(
                x => x.GetImageAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetSecureImageAsync_ShouldReturnError_WhenImageIsNotFound()
        {
            var imageId = Guid.NewGuid();

            _currentUserContext
                .Setup(x => x.IsAuthenticated)
                .Returns(true);

            _currentUserContext
                .Setup(x => x.IsInRole(nameof(UserRole.Scientist)))
                .Returns(true);

            _currentUserContext
                .Setup(x => x.IsInRole(nameof(UserRole.WarehouseAdmin)))
                .Returns(false);

            _imageStorageRepository
                .Setup(x => x.GetImageAsync(
                    imageId,
                    CancellationToken.None))
                .ReturnsAsync(
                    (null, "image/jpeg"));

            var result = await _service.GetSecureImageAsync(
                imageId,
                CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Image Not Found"));

            _imageStorageRepository.Verify(
                x => x.GetImageAsync(
                    imageId,
                    CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task GetSecureImageAsync_ShouldReturnImage_WhenUserIsScientist()
        {
            var imageId = Guid.NewGuid();
            var stream = new MemoryStream(
                [.. "test image"u8]);

            _currentUserContext
                .Setup(x => x.IsAuthenticated)
                .Returns(true);

            _currentUserContext
                .Setup(x => x.IsInRole(nameof(UserRole.Scientist)))
                .Returns(true);

            _currentUserContext
                .Setup(x => x.IsInRole(nameof(UserRole.WarehouseAdmin)))
                .Returns(false);

            _imageStorageRepository
                .Setup(x => x.GetImageAsync(
                    imageId,
                    CancellationToken.None))
                .ReturnsAsync(
                    (stream, "image/jpeg"));

            var result = await _service.GetSecureImageAsync(
                imageId,
                CancellationToken.None);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data.Stream, Is.SameAs(stream));
            Assert.That(
                result.Data.ContentType,
                Is.EqualTo("image/jpeg"));

            _imageStorageRepository.Verify(
                x => x.GetImageAsync(
                    imageId,
                    CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task GetSecureImageAsync_ShouldReturnImage_WhenUserIsWarehouseAdmin()
        {
            var imageId = Guid.NewGuid();
            var stream = new MemoryStream(
                buffer: [.. "test image"u8]);

            _currentUserContext
                .Setup(x => x.IsAuthenticated)
                .Returns(true);

            _currentUserContext
                .Setup(x => x.IsInRole(nameof(UserRole.Scientist)))
                .Returns(false);

            _currentUserContext
                .Setup(x => x.IsInRole(nameof(UserRole.WarehouseAdmin)))
                .Returns(true);

            _imageStorageRepository
                .Setup(x => x.GetImageAsync(
                    imageId,
                    CancellationToken.None))
                .ReturnsAsync(
                    (stream, "image/png"));

            var result = await _service.GetSecureImageAsync(
                imageId,
                CancellationToken.None);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data.Stream, Is.SameAs(stream));
            Assert.That(
                result.Data.ContentType,
                Is.EqualTo("image/png"));

            _imageStorageRepository.Verify(
                x => x.GetImageAsync(
                    imageId,
                    CancellationToken.None),
                Times.Once);
        }
    }
}