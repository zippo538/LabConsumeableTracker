
using AutoMapper;
using LabConsumableExpiryTracker.Application.Configurations;
using LabConsumableExpiryTracker.Application.DTOs.LoginDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LabConsumableExpiryTracker.Infrastructure.Persistence;

namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<UserManager<ApplicationUser>> _userManager = null!;
    private Mock<IJwtTokenGenerator> _tokenGenerator = null!;
    private Mock<ILogger<AuthService>> _logger = null!;
    private Mock<IRefreshTokenRepository> _refreshTokenRepository = null!;
    private Mock<IMapper> _mapper = null!;
    private JwtSettings _jwtSettings = null!;
    private AuthService _service = null!;

    private readonly Guid _userId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);

        var userStore =
            new UserStore<
                ApplicationUser,
                IdentityRole<Guid>,
                AppDbContext,
                Guid>(dbContext);

        _userManager = new Mock<UserManager<ApplicationUser>>(
            userStore,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!)
        {
            CallBase = true
        };

        _tokenGenerator =
            new Mock<IJwtTokenGenerator>(MockBehavior.Strict);

        _logger =
            new Mock<ILogger<AuthService>>();

        _refreshTokenRepository =
            new Mock<IRefreshTokenRepository>(MockBehavior.Strict);

        _mapper =
            new Mock<IMapper>(MockBehavior.Strict);

        _jwtSettings = new JwtSettings
        {
            AccessTokenExpiryMinutes = 60,
            RefreshTokenExpiryDays = 7
        };

        _service = new AuthService(
            _userManager.Object,
            _tokenGenerator.Object,
            _logger.Object,
            _refreshTokenRepository.Object,
            Options.Create(_jwtSettings),
            _mapper.Object);
    }

    [Test]
    public async Task LoginAsync_ShouldReturnError_WhenUserNotFound()
    {
        var request = new LoginRequestDto
        {
            Username = "unknown",
            Password = "password"
        };

        _userManager
            .Setup(x => x.FindByNameAsync(request.Username))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _service.LoginAsync(request);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Invalid username or password."));
        });

        _userManager.Verify(
            x => x.CheckPasswordAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task LoginAsync_ShouldReturnError_WhenUserInactive()
    {
        var user = CreateUser();
        user.IsActive = false;

        var request = new LoginRequestDto
        {
            Username = user.UserName!,
            Password = "password"
        };

        _userManager
            .Setup(x => x.FindByNameAsync(request.Username))
            .ReturnsAsync(user);

        var result = await _service.LoginAsync(request);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Invalid username or password."));
        });

        _userManager.Verify(
            x => x.CheckPasswordAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task LoginAsync_ShouldReturnError_WhenPasswordInvalid()
    {
        var user = CreateUser();

        var request = new LoginRequestDto
        {
            Username = user.UserName!,
            Password = "wrong-password"
        };

        _userManager
            .Setup(x => x.FindByNameAsync(request.Username))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.CheckPasswordAsync(
                user,
                request.Password))
            .ReturnsAsync(false);

        var result = await _service.LoginAsync(request);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Invalid username or password."));
        });
    }

    [Test]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        var user = CreateUser();

        var request = new LoginRequestDto
        {
            Username = user.UserName!,
            Password = "correct-password"
        };

        var mappedResponse = new LoginResponseDto();

        _userManager
            .Setup(x => x.FindByNameAsync(request.Username))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.CheckPasswordAsync(
                user,
                request.Password))
            .ReturnsAsync(true);

        _userManager
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>
            {
                "Scientist"
            });

        _tokenGenerator
            .Setup(x => x.GenerateToken(
                user,
                "Scientist"))
            .Returns("access-token");

        _tokenGenerator
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        _refreshTokenRepository
            .Setup(x => x.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mapper
            .Setup(x => x.Map<LoginResponseDto>(user))
            .Returns(mappedResponse);

        var result = await _service.LoginAsync(request);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Token, Is.EqualTo("access-token"));
            Assert.That(
                result.Data.RefreshToken,
                Is.EqualTo("refresh-token"));
            Assert.That(
                result.Data.TokenType,
                Is.EqualTo("Bearer"));
            Assert.That(
                result.Data.Role,
                Is.EqualTo("Scientist"));
            Assert.That(
                result.Data.ExpiresAt,
                Is.GreaterThan(DateTime.UtcNow));
        });

        _refreshTokenRepository.Verify(
            x => x.AddAsync(
                It.Is<RefreshToken>(token =>
                    token.UserId == user.Id &&
                    token.Token == "refresh-token"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task RefreshTokenAsync_ShouldReturnError_WhenTokenNotFound()
    {
        _refreshTokenRepository
            .Setup(x => x.GetByTokenWithUserAsync(
                "invalid-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var result =
            await _service.RefreshTokenAsync("invalid-token");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Refresh token not valid or has been expired."));
        });

        _refreshTokenRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task RefreshTokenAsync_ShouldReturnError_WhenTokenIsInactive()
    {
        var user = CreateUser();

        var refreshToken = new RefreshToken
        {
            Token = "refresh-token",
            UserId = user.Id,
            User = user,
            IsRevoked = true,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(1)
        };

        _refreshTokenRepository
            .Setup(x => x.GetByTokenWithUserAsync(
                "refresh-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var result =
            await _service.RefreshTokenAsync("refresh-token");

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "Refresh token not valid or has been expired."));

        _refreshTokenRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task RefreshTokenAsync_ShouldReturnError_WhenUserInactive()
    {
        var user = CreateUser();
        user.IsActive = false;

        var refreshToken = new RefreshToken
        {
            Token = "refresh-token",
            UserId = user.Id,
            User = user,
            IsRevoked = false,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(1)
        };

        _refreshTokenRepository
            .Setup(x => x.GetByTokenWithUserAsync(
                "refresh-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var result =
            await _service.RefreshTokenAsync("refresh-token");

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "Refresh token not valid or has been expired."));

        _refreshTokenRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task RefreshTokenAsync_ShouldReturnSuccess_WhenTokenIsValid()
    {
        var user = CreateUser();

        var refreshToken = new RefreshToken
        {
            Token = "old-refresh-token",
            UserId = user.Id,
            User = user,
            IsRevoked = false,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(1)
        };

        var mappedResponse = new LoginResponseDto();

        _refreshTokenRepository
            .Setup(x => x.GetByTokenWithUserAsync(
                "old-refresh-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _refreshTokenRepository
            .Setup(x => x.UpdateAsync(
                refreshToken,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _refreshTokenRepository
            .Setup(x => x.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userManager
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>
            {
                "Scientist"
            });

        _tokenGenerator
            .Setup(x => x.GenerateToken(
                user,
                "Scientist"))
            .Returns("new-access-token");

        _tokenGenerator
            .Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        _mapper
            .Setup(x => x.Map<LoginResponseDto>(user))
            .Returns(mappedResponse);

        var result =
            await _service.RefreshTokenAsync(
                "old-refresh-token");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(
            result.Data!.Token,
            Is.EqualTo("new-access-token"));
        Assert.That(
            result.Data.RefreshToken,
            Is.EqualTo("new-refresh-token"));
        Assert.That(
            result.Data.TokenType,
            Is.EqualTo("Bearer"));
        Assert.That(
            result.Data.Role,
            Is.EqualTo("Scientist"));

        Assert.That(
            refreshToken.IsRevoked,
            Is.True);

        Assert.That(
            refreshToken.RevokedOnUtc,
            Is.Not.Null);

        _refreshTokenRepository.Verify(
            x => x.UpdateAsync(
                refreshToken,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _refreshTokenRepository.Verify(
            x => x.AddAsync(
                It.Is<RefreshToken>(token =>
                    token.UserId == user.Id &&
                    token.Token == "new-refresh-token"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task RevokeTokenAsync_ShouldReturnError_WhenTokenNotFound()
    {
        _refreshTokenRepository
            .Setup(x => x.GetByTokenWithUserAsync(
                "invalid-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var result =
            await _service.RevokeTokenAsync("invalid-token");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Token not found or has been revoked."));
        });

        _refreshTokenRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task RevokeTokenAsync_ShouldReturnError_WhenTokenAlreadyRevoked()
    {
        var user = CreateUser();

        var refreshToken = new RefreshToken
        {
            Token = "revoked-token",
            UserId = user.Id,
            User = user,
            IsRevoked = true,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(1)
        };

        _refreshTokenRepository
            .Setup(x => x.GetByTokenWithUserAsync(
                "revoked-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var result =
            await _service.RevokeTokenAsync("revoked-token");

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "Token not found or has been revoked."));

        _refreshTokenRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task RevokeTokenAsync_ShouldReturnSuccess_WhenTokenIsValid()
    {
        var user = CreateUser();

        var refreshToken = new RefreshToken
        {
            Token = "valid-token",
            UserId = user.Id,
            User = user,
            IsRevoked = false,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(1)
        };

        _refreshTokenRepository
            .Setup(x => x.GetByTokenWithUserAsync(
                "valid-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _refreshTokenRepository
            .Setup(x => x.UpdateAsync(
                refreshToken,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result =
            await _service.RevokeTokenAsync("valid-token");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That(
            result.Message,
            Is.EqualTo("Token has been revoked.."));

        Assert.That(
            refreshToken.IsRevoked,
            Is.True);

        Assert.That(
            refreshToken.RevokedOnUtc,
            Is.Not.Null);

        _refreshTokenRepository.Verify(
            x => x.UpdateAsync(
                refreshToken,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private ApplicationUser CreateUser()
    {
        return new ApplicationUser
        {
            Id = _userId,
            UserName = "scientist1",
            Email = "scientist1@formulatrix.com",
            FullName = "Scientist One",
            IsActive = true
        };
    }
}
