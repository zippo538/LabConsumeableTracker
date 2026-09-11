using LabConsumableExpiryTracker.Application.Common.Interfaces;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.LoginDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace LabConsumableExpiryTracker.Application.Services;

public class LoginService : ILoginService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequestDto requestDto)
    {
        var user = await _userManager.FindByNameAsync(requestDto.Username);
        if (user is null || !user.IsActive)
        {
            return ServiceResult<LoginResponseDto>.ErrorResult("User not found or inactive.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, requestDto.Password);
        if (!isPasswordValid)
        {
            return ServiceResult<LoginResponseDto>.ErrorResult("Invalid password.");
        }

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