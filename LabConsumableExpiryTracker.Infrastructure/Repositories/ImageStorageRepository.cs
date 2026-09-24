using LabConsumableExpiryTracker.Application.Interfaces;
using Microsoft.Extensions.Hosting;
namespace LabConsumableExpiryTracker.Infrastructure.Repositories
{
    public class ImageStorageRepository : IImageStorageRepository
    {
        private readonly string _storagePath;
        public ImageStorageRepository(IHostEnvironment environment)
        {
        _storagePath = Path.Combine(environment.ContentRootPath, "Storage", "Images");
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
        }
    public async Task<(Stream? Stream, string ContentType)> GetImageAsync(Guid imageId, CancellationToken cancellationToken)
    {
        var fileName = $"{imageId}.jpg"; 
        var filePath = Path.Combine(_storagePath, fileName);
        if (!File.Exists(filePath))
        {
            return (null, string.Empty);
        }
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = "image/jpeg"; 
        return (stream, contentType);
    }
    public async Task<Guid> SaveImageAsync(Stream stream, string extension, CancellationToken cancellationToken)
    {
    var imageId = Guid.NewGuid();
    var fileName = $"{imageId}{extension}";
    var filePath = Path.Combine(_storagePath, fileName);
    if (!Directory.Exists(_storagePath))
    {
        Directory.CreateDirectory(_storagePath);
    }
    using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
    await stream.CopyToAsync(fileStream, cancellationToken);
    return imageId;
    }
    public async Task DeleteImageAsync(Guid imageId, CancellationToken cancellationToken)
    {
    var files = Directory.GetFiles(_storagePath, $"{imageId}.*");
    foreach (var file in files)
    {
        File.Delete(file);
    }
    await Task.CompletedTask;
    }
    }
}