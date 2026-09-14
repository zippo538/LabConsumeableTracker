using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.Configurations;
using LabConsumableExpiryTracker.Application.DTOs.LoginDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LabConsumableExpiryTracker.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ILogger<AuthService> _logger;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly IMapper _mapper;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator tokenGenerator,
        ILogger<AuthService> logger,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<JwtSettings> jwtSettings,
        IMapper mapper)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtSettings = jwtSettings.Value;
        _mapper = mapper;
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequestDto requestDto)
    {
        var user = await _userManager.FindByNameAsync(requestDto.Username);
        if (user is null || !user.IsActive)
        {
            _logger.LogWarning(
                "User not found for username {Username}.",
                requestDto.Username);
            return ServiceResult<LoginResponseDto>.ErrorResult("Invalid username or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, requestDto.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning(
                "Login failed for username {Username}: invalid password.",
                requestDto.Username);
            return ServiceResult<LoginResponseDto>.ErrorResult("Invalid username or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        var token = _tokenGenerator.GenerateToken(user, role);
        var refreshTokenString = _tokenGenerator.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = user.Id,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        _logger.LogInformation("User {Username} logged in successfully.", user.UserName);

        var response = _mapper.Map<LoginResponseDto>(user);
        response.Token = token;
        response.RefreshToken = refreshTokenString;
        response.Role = role;

        return ServiceResult<LoginResponseDto>.SuccessResult(response);
    }

    public async Task<ServiceResult<LoginResponseDto>> RefreshTokenAsync(string token)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenWithUserAsync(token);
        if (existingToken is null || !existingToken.IsActive || !existingToken.User.IsActive)
        {
            return ServiceResult<LoginResponseDto>.ErrorResult("Refresh token not valid or has been expired.");
        }
        
        existingToken.IsRevoked = true;
        existingToken.RevokedOnUtc = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(existingToken);
        
        var roles = await _userManager.GetRolesAsync(existingToken.User);
        var role = roles.FirstOrDefault() ?? string.Empty;
        var newAccessToken = _tokenGenerator.GenerateToken(existingToken.User, role);
        var newRefreshTokenString = _tokenGenerator.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenString,
            UserId = existingToken.UserId,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
        };
        
        await _refreshTokenRepository.AddAsync(newRefreshToken);

        var response = _mapper.Map<LoginResponseDto>(existingToken.User);
        response.Token = newAccessToken;
        response.RefreshToken = newRefreshTokenString;
        response.Role = role;

        return ServiceResult<LoginResponseDto>.SuccessResult(response);
    }

    public async Task<ServiceResult<bool>> RevokeTokenAsync(string token)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenWithUserAsync(token);
        if (existingToken is null || existingToken.IsRevoked)
        {
            return ServiceResult<bool>.ErrorResult("Token not found or has been revoked.");
        }

        existingToken.IsRevoked = true;
        existingToken.RevokedOnUtc = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(existingToken);

        return ServiceResult<bool>.SuccessResult(true, "Token has been revoked..");
    }
}