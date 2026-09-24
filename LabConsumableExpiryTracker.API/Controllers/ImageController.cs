using System.ComponentModel.Design.Serialization;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LabConsumableExpiryTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        public ImageController(IImageService imageService)
    {
        _imageService = imageService;
    }
   [HttpGet("{imageId:guid}")]
        public async Task<IActionResult> GetImage(Guid imageId, CancellationToken cancellationToken)
        {
            var response = await _imageService.GetSecureImageAsync(imageId, cancellationToken);
            if (!response.Success)
            {
                return BadRequest(response); 
            }
            var (stream, contentType) = response.Data!;
            return File(stream, contentType, enableRangeProcessing: true);
        }
    }
}