namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IImageStorageRepository
{
    Task<Guid> SaveImageAsync(Stream stream, string extension, CancellationToken cancellationToken);
    Task<(Stream? Stream, string ContentType)> GetImageAsync(Guid imageId);
    Task DeleteImageAsync(Guid imageId);
}