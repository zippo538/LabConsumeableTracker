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
        public Task<(Stream? Stream, string ContentType)> GetImageAsync(Guid imageId, CancellationToken cancellationToken)
        {
            try
            {
                var files = Directory.GetFiles(_storagePath, $"{imageId}.*");
                if (files.Length == 0)
                {
                    return Task.FromResult<(Stream? Stream, string ContentType)>((null, string.Empty));
                }

                var filePath = files[0];
                var contentType = Path.GetExtension(filePath).ToLowerInvariant() switch
                {
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    _ => "image/jpeg"
                };

                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return Task.FromResult<(Stream? Stream, string ContentType)>((stream, contentType));
            }
            catch (Exception exception)
            {
                return Task.FromException<(Stream? Stream, string ContentType)>(exception);
            }
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

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
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