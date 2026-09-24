using LabConsumableExpiryTracker.Infrastructure.Repositories;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
namespace LabConsumableExpiryTracker.Tests.Repository;
[TestFixture]
public class ImageStorageRepositoryTests
{
    private ImageStorageRepository _repository = null!;
    private string _testRootPath = null!;
    private class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "TestApp";
        public string ContentRootPath { get; set; } = null!;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _testRootPath = Path.Combine(Path.GetTempPath(), $"ImgTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testRootPath);
        var env = new FakeHostEnvironment { ContentRootPath = _testRootPath };
        _repository = new ImageStorageRepository(env);
    }
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (Directory.Exists(_testRootPath))
        {
            Directory.Delete(_testRootPath, true);
        }
    }
    [Test]
    public async Task SaveImageAsync_CreatesFileAndReturnsGuid()
    {
        using var stream = new MemoryStream([1, 2, 3, 4]);
        var imageId = await _repository.SaveImageAsync(stream, ".jpg", CancellationToken.None);
        var expectedPath = Path.Combine(_testRootPath, "Storage", "Images", $"{imageId}.jpg");
        Assert.That(File.Exists(expectedPath), Is.True);
    }
    [Test]
    public async Task GetImageAsync_ReturnsStreamAndContentType_WhenExists()
    {
        using var stream = new MemoryStream([1, 2, 3, 4]);
        var imageId = await _repository.SaveImageAsync(stream, ".jpg", CancellationToken.None);
        var (resultStream, contentType) = await _repository.GetImageAsync(imageId, CancellationToken.None);
        Assert.That(resultStream, Is.Not.Null);
        Assert.That(contentType, Is.EqualTo("image/jpeg"));
        await resultStream!.DisposeAsync();
    }
    [Test]
    public async Task GetImageAsync_ReturnsNull_WhenNotExists()
    {
        var (resultStream, contentType) = await _repository.GetImageAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.That(resultStream, Is.Null);
        Assert.That(contentType, Is.EqualTo(string.Empty));
    }
    [Test]
    public async Task DeleteImageAsync_RemovesFile()
    {
        using var stream = new MemoryStream([1, 2, 3, 4]);
        var imageId = await _repository.SaveImageAsync(stream, ".jpg", CancellationToken.None);
        var expectedPath = Path.Combine(_testRootPath, "Storage", "Images", $"{imageId}.jpg");
        Assert.That(File.Exists(expectedPath), Is.True);
        await _repository.DeleteImageAsync(imageId, CancellationToken.None);
        Assert.That(File.Exists(expectedPath), Is.False);
    }
}