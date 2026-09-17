namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        Stream stream,
        string fileName,
        string folder,
        CancellationToken ct = default);

    Task DeleteAsync(
        string filePath,
        CancellationToken ct = default);
}