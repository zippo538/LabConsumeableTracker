using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Application.Common.Result;
namespace LabConsumableExpiryTracker.Application.Services;

public class ImageService(
    IImageStorageRepository imageStorageRepository,
    ICurrentUserContext currentUserContext) : IImageService
{
    private const string ScientistRole = nameof(UserRole.Scientist);
    private const string WarehouseAdminRole = nameof(UserRole.WarehouseAdmin);

    public async Task<ServiceResult<(Stream Stream, string ContentType)>> GetSecureImageAsync(Guid imageId)
    {
        if (!currentUserContext.IsAuthenticated)
        {
            return ServiceResult<(Stream, string)>.ErrorResult("Access denied. Please log in first.");
        }
        var isScientis = currentUserContext.IsInRole(ScientistRole);
        var isAdmin = currentUserContext.IsInRole(WarehouseAdminRole);
        if (!isScientis && !isAdmin)
        {
            return ServiceResult<(Stream, string)>.ErrorResult("You do not have permission to view this image.");
        }
        var (stream, contentType) = await imageStorageRepository.GetImageAsync(imageId);
        return stream == null ? ServiceResult<(Stream, string)>.ErrorResult("Image Not Found") : ServiceResult<(Stream, string)>.SuccessResult((stream, contentType));
    }
}