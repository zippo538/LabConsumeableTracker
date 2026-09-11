using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.LoginDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Application.Services;

public class LoginService : ILoginService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ILogger<LoginService> _logger;

    public LoginService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator tokenGenerator,
        ILogger<LoginService> logger)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequestDto requestDto)
    {
        var user = await _userManager.FindByNameAsync(requestDto.Username);
        if (user is null || !user.IsActive)
        {
            _logger.LogWarning(
                "User not found for username {Username}.",
                requestDto.Username);
            return ServiceResult<LoginResponseDto>.ErrorResult("User not found or inactive.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, requestDto.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning(
                "Login failed for username {Username}: invalid password.",
                requestDto.Username);
            return ServiceResult<LoginResponseDto>.ErrorResult("Invalid password.");
        }
        _logger.LogInformation(
            "User {Username} logged in successfully.",
            user.UserName);
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        var token = _tokenGenerator.GenerateToken(user, role);

        var response = new LoginResponseDto
        {
            Token = token,
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            FullName = user.FullName,
            Role = role
        };

        return ServiceResult<LoginResponseDto>.SuccessResult(response);
    }
}