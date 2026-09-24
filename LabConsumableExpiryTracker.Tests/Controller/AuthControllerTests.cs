using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.LoginDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _authServiceMock = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _authServiceMock = new Mock<IAuthService>(MockBehavior.Strict);
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Test]
    public async Task Login_WhenSuccess_ReturnsOk()
    {
        var request = new LoginRequestDto
        {
            Username = "admin",
            Password = "secret"
        };
        var response = ServiceResult<LoginResponseDto>
            .SuccessResult(new LoginResponseDto { Token = "token" }, "success");
        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.Login(request);

        var ok = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            Assert.That(ok!.Value, Is.EqualTo(response));
        });
    }

    [Test]
    public async Task Login_WhenFailed_ReturnsUnauthorized()
    {
        var request = new LoginRequestDto
        {
            Username = "admin",
            Password = "wrong"
        };
        var response = ServiceResult<LoginResponseDto>
            .ErrorResult("Invalid credentials");
        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.Login(request);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Refresh_WhenSuccess_ReturnsOk()
    {
        var request = new RefreshTokenRequestDto { RefreshToken = "refresh-token" };
        var response = ServiceResult<LoginResponseDto>
            .SuccessResult(new LoginResponseDto { Token = "new-token" }, "success");
        _authServiceMock
            .Setup(x => x.RefreshTokenAsync("refresh-token"))
            .ReturnsAsync(response);

        var result = await _controller.Refresh(request);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task Refresh_WhenFailed_ReturnsUnauthorized()
    {
        var request = new RefreshTokenRequestDto { RefreshToken = "expired-token" };
        var response = ServiceResult<LoginResponseDto>
            .ErrorResult("Invalid refresh token");
        _authServiceMock
            .Setup(x => x.RefreshTokenAsync("expired-token"))
            .ReturnsAsync(response);

        var result = await _controller.Refresh(request);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Revoke_WhenSuccess_ReturnsOk()
    {
        var request = new RefreshTokenRequestDto { RefreshToken = "refresh-token" };
        var response = ServiceResult<bool>
            .SuccessResult(true, "revoked");
        _authServiceMock
            .Setup(x => x.RevokeTokenAsync("refresh-token"))
            .ReturnsAsync(response);

        var result = await _controller.Revoke(request);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task Revoke_WhenFailed_ReturnsBadRequest()
    {
        var request = new RefreshTokenRequestDto { RefreshToken = "unknown-token" };
        var response = ServiceResult<bool>
            .ErrorResult("Token not found");
        _authServiceMock
            .Setup(x => x.RevokeTokenAsync("unknown-token"))
            .ReturnsAsync(response);

        var result = await _controller.Revoke(request);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }
}
