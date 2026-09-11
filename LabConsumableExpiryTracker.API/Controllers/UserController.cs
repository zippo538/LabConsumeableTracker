using System.Security.Claims;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ScientistDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IScientistService _iUserService;

    public UserController(IScientistService userService)
    {
        _iUserService = userService;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ServiceResult<UserProfileResponseDto>> GetMe(
        CancellationToken ct)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        return await _iUserService.GetUserProfileAsync(userId);
    }
}