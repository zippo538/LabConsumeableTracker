using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.LoginDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/auth")] 
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")] 
    public async Task<ActionResult<ServiceResult<LoginResponseDto>>> Login(
        [FromBody] LoginRequestDto request)
    {
        var response = await authService.LoginAsync(request);
        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }
    
    [HttpPost("refresh")] 
    public async Task<ActionResult<ServiceResult<LoginResponseDto>>> Refresh(
        [FromBody] RefreshTokenRequestDto request)
    {
        var response = await authService.RefreshTokenAsync(request.RefreshToken);
        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }
   
    [HttpPost("revoke")] 
    public async Task<ActionResult<ServiceResult<bool>>> Revoke(
        [FromBody] RefreshTokenRequestDto request)
    {
        var response = await authService.RevokeTokenAsync(request.RefreshToken);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}