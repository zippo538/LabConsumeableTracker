namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IImageStorageRepository
{
    Task<Guid> SaveImageAsync(Stream stream, string extension, CancellationToken cancellationToken);
    Task<(Stream? Stream, string ContentType)> GetImageAsync(Guid imageId, CancellationToken cancellationToken);
    Task DeleteImageAsync(Guid imageId, CancellationToken cancellationToken);
}