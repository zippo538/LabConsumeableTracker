using LabConsumableExpiryTracker.Application.Common.Result;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces
{
    public interface IImageService
    {
        Task<ServiceResult<(Stream Stream, string ContentType)>> GetSecureImageAsync(Guid imageId, CancellationToken cancellationToken);
    }
}