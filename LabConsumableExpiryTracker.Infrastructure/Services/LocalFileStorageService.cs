using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace LabConsumableExpiryTracker.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _webRootPath;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _webRootPath = environment.WebRootPath
                       ?? throw new InvalidOperationException("WebRootPath is not configured.");
    }

    public async Task<string> SaveAsync(
        Stream stream,
        string fileName,
        string folder,
        CancellationToken ct = default)
    {
        var relativePath = Path.Combine(folder, fileName)
            .Replace('\\', '/');

        var fullPath = Path.Combine(
            _webRootPath,
            relativePath);

        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.Create);

        await stream.CopyToAsync(fileStream, ct);

        return relativePath;
    }

    public Task DeleteAsync(
        string filePath,
        CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_webRootPath, filePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}