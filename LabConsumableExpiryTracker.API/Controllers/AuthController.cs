using FluentValidation;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.Login;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/login")]
public class AuthController : ControllerBase
{
    private readonly ILoginService _authService;

    public AuthController(ILoginService authService)
    {
        _authService = authService;
    }
    [HttpPost]
    public async Task<ActionResult<ServiceResult<LoginResponseDto>>> Login(
        LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);

        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }
}