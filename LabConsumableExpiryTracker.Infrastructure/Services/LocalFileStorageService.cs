using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace LabConsumableExpiryTracker.Infrastructure.Services;

public class LocalFileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    private readonly string _webRootPath = environment.WebRootPath
                                           ?? throw new InvalidOperationException("WebRootPath is not configured.");

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