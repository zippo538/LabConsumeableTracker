using System.Security.Claims;
using LabConsumableExpiryTracker.API.Controllers;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.UserDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace LabConsumableExpiryTracker.Tests.Controller;

[TestFixture]
public class UserControllerTests
{
    private Mock<IUserService> _userServiceMock;
    private UserController _controller;
    [SetUp]
    public void Setup()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new UserController(_userServiceMock.Object);
    }
    private void SetupUserClaims(string? userIdClaim)
    {
        var claims = new List<Claim>();
        if (userIdClaim != null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userIdClaim));
        }
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
    [Test]
    public async Task GetMe_WhenInvalidToken_ReturnsUnauthorized()
    {
        SetupUserClaims("invalid-guid");
        var result = await _controller.GetMe();
        Assert.That(result.Result, Is.InstanceOf<UnauthorizedObjectResult>());
    }
    [Test]
    public async Task GetMe_WhenServiceFails_ReturnsNotFound()
    {
        SetupUserClaims(Guid.NewGuid().ToString());
        var serviceResult = ServiceResult<UserResponseDto>.ErrorResult("Not found");
        _userServiceMock.Setup(s => s.GetUserProfileAsync(It.IsAny<Guid>())).ReturnsAsync(serviceResult);
        var result = await _controller.GetMe();
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
    [Test]
    public async Task GetMe_WhenSuccess_ReturnsOk()
    {
        SetupUserClaims(Guid.NewGuid().ToString());
        var serviceResult = ServiceResult<UserResponseDto>.SuccessResult(new UserResponseDto());
        _userServiceMock.Setup(s => s.GetUserProfileAsync(It.IsAny<Guid>())).ReturnsAsync(serviceResult);
        var result = await _controller.GetMe();
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }
    [Test]
    public async Task UpdateMe_WhenInvalidToken_ReturnsUnauthorized()
    {
        SetupUserClaims(null);
        var result = await _controller.UpdateMe(new UpdateFullNameRequestDto());
        Assert.That(result.Result, Is.InstanceOf<UnauthorizedObjectResult>());
    }
    [Test]
    public async Task UpdateMe_WhenServiceFails_ReturnsBadRequest()
    {
        SetupUserClaims(Guid.NewGuid().ToString());
        var serviceResult = ServiceResult<UserResponseDto>.ErrorResult("Error");
        _userServiceMock.Setup(s => s.UpdateMyProfileAsync(It.IsAny<Guid>(), It.IsAny<UpdateFullNameRequestDto>())).ReturnsAsync(serviceResult);
        var result = await _controller.UpdateMe(new UpdateFullNameRequestDto());
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    [Test]
    public async Task UpdateMe_WhenSuccess_ReturnsOk()
    {
        SetupUserClaims(Guid.NewGuid().ToString());
        var serviceResult = ServiceResult<UserResponseDto>.SuccessResult(new UserResponseDto());
        _userServiceMock.Setup(s => s.UpdateMyProfileAsync(It.IsAny<Guid>(), It.IsAny<UpdateFullNameRequestDto>())).ReturnsAsync(serviceResult);
        var result = await _controller.UpdateMe(new UpdateFullNameRequestDto());
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }
    [Test]
    public async Task CreateUser_WhenSuccess_ReturnsCreated()
    {
        var serviceResult = ServiceResult<UserResponseDto>.SuccessResult(new UserResponseDto());
        _userServiceMock.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserRequestDto>())).ReturnsAsync(serviceResult);
        var result = await _controller.CreateUser(new CreateUserRequestDto());
        var createdResult = result.Result as ObjectResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
    }
    [Test]
    public async Task CreateUser_WhenFail_ReturnsBadRequest()
    {
        var serviceResult = ServiceResult<UserResponseDto>.ErrorResult("Error");
        _userServiceMock.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserRequestDto>())).ReturnsAsync(serviceResult);
        var result = await _controller.CreateUser(new CreateUserRequestDto());
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    [Test]
    public async Task GetAllUsers_WhenInvalidToken_ReturnsUnauthorized()
    {
        SetupUserClaims("invalid-guid");
        var result = await _controller.GetAllUsers();
        Assert.That(result.Result, Is.InstanceOf<UnauthorizedResult>());
    }
    [Test]
    public async Task GetAllUsers_WhenSuccess_ReturnsOk()
    {
        SetupUserClaims(Guid.NewGuid().ToString());
        var serviceResult = ServiceResult<PagedUsersResponseDto>.SuccessResult(new PagedUsersResponseDto());
        _userServiceMock.Setup(s => s.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>())).ReturnsAsync(serviceResult);
        var result = await _controller.GetAllUsers();
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }
    [Test]
    public async Task UpdateUser_WhenSuccess_ReturnsOk()
    {
        var serviceResult = ServiceResult<UserResponseDto>.SuccessResult(new UserResponseDto());
        _userServiceMock.Setup(s => s.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserRequestDto>())).ReturnsAsync(serviceResult);
        var result = await _controller.UpdateUser(Guid.NewGuid(), new UpdateUserRequestDto());
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }
    [Test]
    public async Task UpdateUser_WhenFail_ReturnsBadRequest()
    {
        var serviceResult = ServiceResult<UserResponseDto>.ErrorResult("Error");
        _userServiceMock.Setup(s => s.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserRequestDto>())).ReturnsAsync(serviceResult);
        var result = await _controller.UpdateUser(Guid.NewGuid(), new UpdateUserRequestDto());
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    [Test]
    public async Task ActivateUser_WhenSuccess_ReturnsOk()
    {
        SetupUserClaims(Guid.NewGuid().ToString());
        // Gunakan ServiceResult<bool>
        var serviceResult = ServiceResult<bool>.SuccessResult(true);
        _userServiceMock.Setup(s => s.SetUserActiveStatusAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(serviceResult);

        var result = await _controller.ActivateUser(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task ActivateUser_WhenFail_ReturnsBadRequest()
    {
        SetupUserClaims(Guid.NewGuid().ToString());
        var serviceResult = ServiceResult<bool>.ErrorResult("Error");
        _userServiceMock.Setup(s => s.SetUserActiveStatusAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(serviceResult);

        var result = await _controller.ActivateUser(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeactivateUser_WhenSuccess_ReturnsOk()
    {
        SetupUserClaims(Guid.NewGuid().ToString());
        var serviceResult = ServiceResult<bool>.SuccessResult(true);
        _userServiceMock.Setup(s => s.SetUserActiveStatusAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).ReturnsAsync(serviceResult);

        var result = await _controller.DeactivateUser(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task DeactivateUser_WhenFail_ReturnsBadRequest()
    {
        SetupUserClaims(Guid.NewGuid().ToString());
        var serviceResult = ServiceResult<bool>.ErrorResult("Error");
        _userServiceMock.Setup(s => s.SetUserActiveStatusAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).ReturnsAsync(serviceResult);

        var result = await _controller.DeactivateUser(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    [Test]
    public async Task GetUserStatistics_WhenSuccess_ReturnsOk()
    {
        var serviceResult = ServiceResult<UserStatisticsResponseDto>.SuccessResult(new UserStatisticsResponseDto());
        _userServiceMock.Setup(s => s.GetUserStatisticsAsync()).ReturnsAsync(serviceResult);
        var result = await _controller.GetUserStatistics();
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }
    [Test]
    public async Task GetUserById_WhenSuccess_ReturnsOk()
    {
        var serviceResult = ServiceResult<UserResponseDto>.SuccessResult(new UserResponseDto());
        _userServiceMock.Setup(s => s.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync(serviceResult);
        var result = await _controller.GetUserById("user-id");
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }
    [Test]
    public async Task ChangePassword_WhenSuccess_ReturnsOk()
    {
        var serviceResult = ServiceResult<bool>.SuccessResult(true);
        _userServiceMock.Setup(s => s.ChangeUserPasswordAsync(It.IsAny<Guid>(), It.IsAny<ChangeUserPasswordRequestDto>())).ReturnsAsync(serviceResult);
        var result = await _controller.ChangePassword(Guid.NewGuid(), new ChangeUserPasswordRequestDto());
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }
    [Test]
    public async Task ChangePassword_WhenFail_ReturnsBadRequest()
    {
        var serviceResult = ServiceResult<bool>.ErrorResult("Error");
        _userServiceMock.Setup(s => s.ChangeUserPasswordAsync(It.IsAny<Guid>(), It.IsAny<ChangeUserPasswordRequestDto>())).ReturnsAsync(serviceResult);
        var result = await _controller.ChangePassword(Guid.NewGuid(), new ChangeUserPasswordRequestDto());
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
}