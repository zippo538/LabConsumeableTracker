using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class ImageController(IImageService imageService) : ControllerBase
{
    [HttpGet("{imageId:guid}")]
    public async Task<IActionResult> GetImage(Guid imageId)
    {
        var response = await imageService.GetSecureImageAsync(imageId);
        if (!response.Success)
        {
            return BadRequest(response); 
        }
        var (stream, contentType) = response.Data;
        return File(stream, contentType, enableRangeProcessing: true);
    }
}