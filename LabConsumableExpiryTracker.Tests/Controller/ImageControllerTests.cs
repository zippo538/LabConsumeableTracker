using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class ImageControllerTests
{
    private Mock<IImageService> _imageServiceMock = null!;
    private ImageController _controller = null!;
    [SetUp]
    public void SetUp()
    {
        _imageServiceMock = new Mock<IImageService>();
        _controller = new ImageController(_imageServiceMock.Object);
    }
    [Test]
    public async Task GetImage_Success_ReturnsFileStreamResult()
    {
        var imageId = Guid.NewGuid();
        var stream = new MemoryStream([1, 2, 3]);
        const string contentType = "image/jpeg";
        var serviceResult = ServiceResult<(Stream, string)>.SuccessResult((stream, contentType));
        _imageServiceMock.Setup(s => s.GetSecureImageAsync(imageId))
            .ReturnsAsync(serviceResult);
        var result = await _controller.GetImage(imageId);
        Assert.That(result, Is.InstanceOf<FileStreamResult>());
        var fileResult = result as FileStreamResult;
        Assert.Multiple(() =>
        {
            Assert.That(fileResult!.ContentType, Is.EqualTo(contentType));
            Assert.That(fileResult.FileStream, Is.EqualTo(stream));
            Assert.That(fileResult.EnableRangeProcessing, Is.True);
        });
        _imageServiceMock.Verify(s => s.GetSecureImageAsync(imageId), Times.Once);
    }
    [Test]
    public async Task GetImage_Failure_ReturnsBadRequest()
    {
        var imageId = Guid.NewGuid();
        var serviceResult = ServiceResult<(Stream, string)>.ErrorResult("Image not found or access denied.");
        _imageServiceMock.Setup(s => s.GetSecureImageAsync(imageId))
            .ReturnsAsync(serviceResult);
        var result = await _controller.GetImage(imageId);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult!.Value, Is.EqualTo(serviceResult));
    }
}