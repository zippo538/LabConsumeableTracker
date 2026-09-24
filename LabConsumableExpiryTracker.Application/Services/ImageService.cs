using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Application.Common.Result;
namespace LabConsumableExpiryTracker.Application.Services
{
    public class ImageService : IImageService
    {
    private readonly IImageStorageRepository _imageStorageRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private const string ScientistRole = nameof(UserRole.Scientist);
    private const string WarehouseAdminRole = nameof(UserRole.WarehouseAdmin);
    public ImageService(
        IImageStorageRepository imageStorageRepository, 
        ICurrentUserContext currentUserContext)
    {
        _imageStorageRepository = imageStorageRepository;
        _currentUserContext = currentUserContext;
    }
    public async Task<ServiceResult<(Stream Stream, string ContentType)>> GetSecureImageAsync(Guid imageId, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated)
        {
            return ServiceResult<(Stream, string)>.ErrorResult("Access denied. Please log in first.");
        }
        bool isScientis = _currentUserContext.IsInRole(ScientistRole);
        bool isAdmin = _currentUserContext.IsInRole(WarehouseAdminRole);
        if (!isScientis && !isAdmin)
        {
            return ServiceResult<(Stream, string)>.ErrorResult("You do not have permission to view this image.");
        }
        var (stream, contentType) = await _imageStorageRepository.GetImageAsync(imageId, cancellationToken);
        if (stream == null)
        {
            return ServiceResult<(Stream, string)>.ErrorResult("Image Not Found");
        }
        return ServiceResult<(Stream, string)>.SuccessResult((stream, contentType));
    }
    }
}