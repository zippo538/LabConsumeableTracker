using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.UserDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private AppDbContext _dbContext = null!;
    private Mock<UserManager<ApplicationUser>> _userManager = null!;
    private Mock<RoleManager<IdentityRole<Guid>>> _roleManager = null!;
    private Mock<IRefreshTokenRepository> _refreshTokenRepository = null!;
    private Mock<IMapper> _mapper = null!;
    private Mock<ILogger<UserService>> _logger = null!;
    private UserService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);

        var userStore =
            new UserStore<ApplicationUser, IdentityRole<Guid>, AppDbContext, Guid>(
                _dbContext);

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

        var roleStore =
            new RoleStore<IdentityRole<Guid>, AppDbContext, Guid>(
                _dbContext);

        _roleManager = new Mock<RoleManager<IdentityRole<Guid>>>(
            roleStore,
            new List<IRoleValidator<IdentityRole<Guid>>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Mock<ILogger<RoleManager<IdentityRole<Guid>>>>().Object);

        _refreshTokenRepository =
            new Mock<IRefreshTokenRepository>(MockBehavior.Strict);

        _mapper = new Mock<IMapper>(MockBehavior.Strict);
        _logger = new Mock<ILogger<UserService>>();

        _service = new UserService(
            _userManager.Object,
            _roleManager.Object,
            _refreshTokenRepository.Object,
            _mapper.Object,
            _logger.Object);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.DisposeAsync();
    }

    private async Task AddUsersAsync(params ApplicationUser[] users)
    {
        _dbContext.Users.AddRange(users);
        await _dbContext.SaveChangesAsync();
    }


    [Test]
    public async Task CreateUserAsync_ShouldReturnSuccess_WhenUserIsCreatedSuccessfully()
    {
        var request = new CreateUserRequestDto
        {
            Username = "scientist1",
            Password = "Password123!",
            Role = UserRole.Scientist
        };

        var user = new ApplicationUser
        {
            UserName = request.Username
        };

        var response = new UserResponseDto();

        _roleManager
            .Setup(x => x.RoleExistsAsync("Scientist"))
            .ReturnsAsync(true);

        _mapper
            .Setup(x => x.Map<ApplicationUser>(request))
            .Returns(user);

        _userManager
            .Setup(x => x.CreateAsync(
                user,
                request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.AddToRoleAsync(
                user,
                "Scientist"))
            .ReturnsAsync(IdentityResult.Success);

        _mapper
            .Setup(x => x.Map<UserResponseDto>(user))
            .Returns(response);

        var result = await _service.CreateUserAsync(request);

        Assert.That(result.Success, Is.True);
        Assert.That(
            result.Message,
            Is.EqualTo("User created successfully."));

        Assert.That(result.Data, Is.SameAs(response));
        Assert.That(result.Data!.Roles, Is.Not.Null);
        Assert.That(result.Data.Roles, Has.Count.EqualTo(1));
        Assert.That(result.Data.Roles[0], Is.EqualTo("Scientist"));

        _roleManager.Verify(
            x => x.RoleExistsAsync("Scientist"),
            Times.Once);

        _mapper.Verify(
            x => x.Map<ApplicationUser>(request),
            Times.Once);

        _userManager.Verify(
            x => x.CreateAsync(
                user,
                request.Password),
            Times.Once);

        _userManager.Verify(
            x => x.AddToRoleAsync(
                user,
                "Scientist"),
            Times.Once);

        _mapper.Verify(
            x => x.Map<UserResponseDto>(user),
            Times.Once);

        _userManager.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Test]
    public async Task CreateUserAsync_ShouldReturnError_WhenRoleDoesNotExist()
    {
        var request = new CreateUserRequestDto
        {
            Username = "scientist1",
            Password = "Password123!",
            Role = UserRole.Scientist
        };

        _roleManager
            .Setup(x => x.RoleExistsAsync("Scientist"))
            .ReturnsAsync(false);

        var result = await _service.CreateUserAsync(request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Role 'Scientist' does not exist."));

        _mapper.Verify(
            x => x.Map<ApplicationUser>(It.IsAny<CreateUserRequestDto>()),
            Times.Never);

        _userManager.Verify(
            x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);
    }
    [Test]
    public async Task CreateUserAsync_ShouldReturnError_WhenUserCreationFails()
    {
        var request = new CreateUserRequestDto
        {
            Username = "scientist1",
            Password = "Password123!",
            Role = UserRole.Scientist
        };

        var user = new ApplicationUser
        {
            UserName = request.Username
        };

        var errors = new[]
        {
        new IdentityError
        {
            Code = "DuplicateUserName",
            Description = "Username already exists."
        }
    };

        _roleManager
            .Setup(x => x.RoleExistsAsync("Scientist"))
            .ReturnsAsync(true);

        _mapper
            .Setup(x => x.Map<ApplicationUser>(request))
            .Returns(user);

        _userManager
            .Setup(x => x.CreateAsync(user, request.Password))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var result = await _service.CreateUserAsync(request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to create user."));
        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors, Does.Contain("Username already exists."));

        _userManager.Verify(
            x => x.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);
    }
    [Test]
    public async Task CreateUserAsync_ShouldDeleteUserAndReturnError_WhenRoleAssignmentFails()
    {
        var request = new CreateUserRequestDto
        {
            Username = "scientist1",
            Password = "Password123!",
            Role = UserRole.Scientist
        };

        var user = new ApplicationUser
        {
            UserName = request.Username
        };

        var errors = new[]
        {
        new IdentityError
        {
            Code = "RoleAssignmentFailed",
            Description = "Failed to assign role."
        }
    };

        _roleManager
            .Setup(x => x.RoleExistsAsync("Scientist"))
            .ReturnsAsync(true);

        _mapper
            .Setup(x => x.Map<ApplicationUser>(request))
            .Returns(user);

        _userManager
            .Setup(x => x.CreateAsync(user, request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.AddToRoleAsync(user, "Scientist"))
            .ReturnsAsync(IdentityResult.Failed(errors));

        _userManager
            .Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.CreateUserAsync(request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to assign user role."));
        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors, Does.Contain("Failed to assign role."));

        _userManager.Verify(
            x => x.DeleteAsync(user),
            Times.Once);

        _mapper.Verify(
            x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Test]
    public async Task GetUserProfileAsync_ShouldReturnError_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        _userManager
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _service.GetUserProfileAsync(userId);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("User not found or inactive."));

        _userManager.Verify(
            x => x.GetRolesAsync(It.IsAny<ApplicationUser>()),
            Times.Never);

        _mapper.Verify(
            x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Test]
    public async Task GetUserProfileAsync_ShouldReturnError_WhenUserIsInactive()
    {
        var userId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "scientist1",
            IsActive = false
        };

        _userManager
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var result = await _service.GetUserProfileAsync(userId);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("User not found or inactive."));

        _userManager.Verify(
            x => x.GetRolesAsync(It.IsAny<ApplicationUser>()),
            Times.Never);

        _mapper.Verify(
            x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()),
            Times.Never);
    }
    [Test]
    public async Task GetUserProfileAsync_ShouldReturnSuccess_WhenUserIsActive()
    {
        var userId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "scientist1",
            Email = "scientist1@formulatrix.com",
            FullName = "Scientist One",
            IsActive = true
        };

        var roles = new List<string>
    {
        "Scientist"
    };

        var response = new UserResponseDto();

        _userManager
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        _mapper
            .Setup(x => x.Map<UserResponseDto>(user))
            .Returns(response);

        var result = await _service.GetUserProfileAsync(userId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("User profile retrieved successfully."));
        Assert.That(result.Data, Is.SameAs(response));
        Assert.That(result.Data!.Roles, Is.Not.Null);
        Assert.That(result.Data.Roles, Has.Count.EqualTo(1));
        Assert.That(result.Data.Roles[0], Is.EqualTo("Scientist"));

        _userManager.Verify(
            x => x.FindByIdAsync(userId.ToString()),
            Times.Once);

        _userManager.Verify(
            x => x.GetRolesAsync(user),
            Times.Once);

        _mapper.Verify(
            x => x.Map<UserResponseDto>(user),
            Times.Once);
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldUseDefaultPagination_WhenValuesAreInvalid()
    {
        var currentUserId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "scientist1",
            Email = "scientist1@formulatrix.com",
            FullName = "Scientist One",
            IsActive = true
        };

        await AddUsersAsync(user);

        _userManager
            .Setup(x => x.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin)))
            .ReturnsAsync([]);

        _userManager
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["Scientist"]);

        _mapper
     .Setup(x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()))
     .Returns(new UserResponseDto());

        var result = await _service.GetAllUsersAsync(
            0,
            101,
            null,
            null,
            null,
            currentUserId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Page, Is.EqualTo(1));
        Assert.That(result.Data.PageSize, Is.EqualTo(20));
        Assert.That(result.Data.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldKeepPagination_WhenValuesAreValid()
    {
        var currentUserId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "scientist1",
            IsActive = true
        };

        await AddUsersAsync(user);

        _userManager
            .Setup(x => x.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin)))
            .ReturnsAsync([]);

        _userManager
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["Scientist"]);

        _mapper
            .Setup(x => x.Map<UserResponseDto>(user))
            .Returns(new UserResponseDto());

        var result = await _service.GetAllUsersAsync(
            2,
            50,
            null,
            null,
            null,
            currentUserId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.Page, Is.EqualTo(2));
        Assert.That(result.Data.PageSize, Is.EqualTo(50));
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldFilterByUsername_WhenSearchIsProvided()
    {
        var currentUserId = Guid.NewGuid();

        var matchedUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "scientist1",
            Email = "scientist1@formulatrix.com",
            FullName = "Scientist One",
            IsActive = true
        };

        var otherUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin1",
            Email = "admin@formulatrix.com",
            FullName = "Admin One",
            IsActive = true
        };

        await AddUsersAsync(matchedUser, otherUser);

        _userManager
            .Setup(x => x.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin)))
            .ReturnsAsync([]);

        _userManager
      .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
      .ReturnsAsync(["Scientist"]);

        _mapper
            .Setup(x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()))
            .Returns(new UserResponseDto());


        var result = await _service.GetAllUsersAsync(
            1,
            20,
            " scientist1 ",
            null,
            null,
            currentUserId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(result.Data.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldFilterByEmail_WhenSearchMatchesEmail()
    {
        var currentUserId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "scientist1",
            Email = "scientist@formulatrix.com",
            FullName = "Scientist One",
            IsActive = true
        };

        await AddUsersAsync(user);

        _userManager
            .Setup(x => x.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin)))
            .ReturnsAsync([]);

        _userManager
     .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
     .ReturnsAsync(["Scientist"]);

        _mapper
            .Setup(x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()))
            .Returns(new UserResponseDto());

        var result = await _service.GetAllUsersAsync(
            1,
            20,
            "formulatrix.com",
            null,
            null,
            currentUserId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldFilterByFullName_WhenSearchMatchesFullName()
    {
        var currentUserId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "scientist1",
            Email = "scientist@formulatrix.com",
            FullName = "John Scientist",
            IsActive = true
        };

        await AddUsersAsync(user);

        _userManager
            .Setup(x => x.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin)))
            .ReturnsAsync([]);

        _userManager
         .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
         .ReturnsAsync(["Scientist"]);

        _mapper
            .Setup(x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()))
            .Returns(new UserResponseDto());

        var result = await _service.GetAllUsersAsync(
            1,
            20,
            "John Scientist",
            null,
            null,
            currentUserId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldFilterActiveUsers_WhenStatusIsActive()
    {
        var currentUserId = Guid.NewGuid();

        var activeUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "active",
            IsActive = true
        };

        var inactiveUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "inactive",
            IsActive = false
        };

        await AddUsersAsync(activeUser, inactiveUser);

        _userManager
            .Setup(x => x.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin)))
            .ReturnsAsync([]);

        _userManager
     .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
     .ReturnsAsync(["Scientist"]);

        _mapper
            .Setup(x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()))
            .Returns(new UserResponseDto());

        var result = await _service.GetAllUsersAsync(
            1,
            20,
            null,
            null,
            "ACTIVE",
            currentUserId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldFilterInactiveUsers_WhenStatusIsInactive()
    {
        var currentUserId = Guid.NewGuid();

        var activeUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "active",
            IsActive = true
        };

        var inactiveUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "inactive",
            IsActive = false
        };

        await AddUsersAsync(activeUser, inactiveUser);

        _userManager
            .Setup(x => x.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin)))
            .ReturnsAsync([]);

        _userManager
     .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
     .ReturnsAsync(["Scientist"]);

        _mapper
            .Setup(x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()))
            .Returns(new UserResponseDto());

        var result = await _service.GetAllUsersAsync(
            1,
            20,
            null,
            null,
            "inactive",
            currentUserId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(result.Data.Items.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldFilterByRole_WhenRoleIsProvided()
    {
        var currentUserId = Guid.NewGuid();

        var scientist = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "scientist1",
            IsActive = true
        };

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin1",
            IsActive = true
        };

        await AddUsersAsync(scientist, admin);

        _userManager
            .Setup(x => x.GetUsersInRoleAsync("Scientist"))
            .ReturnsAsync([scientist]);

        _userManager
            .Setup(x => x.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin)))
            .ReturnsAsync([]);

        _userManager
    .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
    .ReturnsAsync(["Scientist"]);

        _mapper
            .Setup(x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()))
            .Returns(new UserResponseDto());

        var result = await _service.GetAllUsersAsync(
            1,
            20,
            null,
            "Scientist",
            null,
            currentUserId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(result.Data.Items, Has.Count.EqualTo(1));
    }
    [Test]
    public async Task GetAllUsersAsync_ShouldExcludeCurrentUser()
    {
        var currentUserId = Guid.NewGuid();

        var currentUser = new ApplicationUser
        {
            Id = currentUserId,
            UserName = "current",
            IsActive = true
        };

        var otherUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "other",
            IsActive = true
        };

        await AddUsersAsync(currentUser, otherUser);

        _userManager
            .Setup(x => x.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin)))
            .ReturnsAsync([]);

        _userManager
    .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
    .ReturnsAsync(["Scientist"]);

        _mapper
            .Setup(x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()))
            .Returns(new UserResponseDto());
        var result = await _service.GetAllUsersAsync(
            1,
            20,
            null,
            null,
            null,
            currentUserId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.TotalCount, Is.EqualTo(1));
        Assert.That(result.Data.Items, Has.Count.EqualTo(1));
    }
    [Test]

    public async Task GetAllUsersAsync_ShouldPlaceWarehouseAdminFirst()
    {
        var currentUserId = Guid.NewGuid();

        var scientist = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "zscientist",
            IsActive = true
        };

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            IsActive = true
        };

        await AddUsersAsync(scientist, admin);

        _userManager
            .Setup(x => x.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin)))
            .ReturnsAsync([admin]);

        _userManager
            .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(["Scientist"]);

        _mapper
            .Setup(x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()))
            .Returns(new UserResponseDto());

        var result = await _service.GetAllUsersAsync(
            1,
            20,
            null,
            null,
            null,
            currentUserId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.Items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldReturnEmptyItems_WhenNoUsersExist()
    {
        var currentUserId = Guid.NewGuid();

        _userManager
            .Setup(x => x.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin)))
            .ReturnsAsync([]);

        var result = await _service.GetAllUsersAsync(
            1,
            20,
            null,
            null,
            null,
            currentUserId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.Items, Is.Empty);
        Assert.That(result.Data.TotalCount, Is.EqualTo(0));
        Assert.That(result.Data.Page, Is.EqualTo(1));
        Assert.That(result.Data.PageSize, Is.EqualTo(20));

        _mapper.Verify(
            x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()),
            Times.Never);

        _userManager.Verify(
            x => x.GetRolesAsync(It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateUserAsync_ShouldReturnError_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();

        _userManager
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var request = new UpdateUserRequestDto
        {
            Username = "updated",
            Email = "updated@formulatrix.com",
            FullName = "Updated User"
        };

        var result = await _service.UpdateUserAsync(userId, request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("User not found."));

        _userManager.Verify(
            x => x.SetUserNameAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);

        _userManager.Verify(
            x => x.SetEmailAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateUserAsync_ShouldReturnError_WhenUsernameUpdateFails()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "olduser",
            Email = "old@formulatrix.com",
            FullName = "Old User"
        };

        var request = new UpdateUserRequestDto
        {
            Username = "newuser",
            Email = "new@formulatrix.com",
            FullName = "New User"
        };

        var identityError = new IdentityError
        {
            Description = "Username is already taken."
        };

        _userManager
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.SetUserNameAsync(user, request.Username))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var result = await _service.UpdateUserAsync(user.Id, request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to update username."));
        Assert.That(
            result.Errors,
            Does.Contain("Username is already taken."));
    }

    [Test]
    public async Task UpdateUserAsync_ShouldReturnError_WhenEmailUpdateFails()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "olduser",
            Email = "old@formulatrix.com",
            FullName = "Old User"
        };

        var request = new UpdateUserRequestDto
        {
            Username = "newuser",
            Email = "invalid-email",
            FullName = "New User"
        };

        var identityError = new IdentityError
        {
            Description = "Invalid email."
        };

        _userManager
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.SetUserNameAsync(user, request.Username))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.SetEmailAsync(user, request.Email))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var result = await _service.UpdateUserAsync(user.Id, request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to update email."));
        Assert.That(
            result.Errors,
            Does.Contain("Invalid email."));

        _userManager.Verify(
            x => x.UpdateAsync(It.IsAny<ApplicationUser>()),
            Times.Never);
    }
    [Test]
    public async Task UpdateUserAsync_ShouldReturnError_WhenUpdateFails()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "olduser",
            Email = "old@formulatrix.com",
            FullName = "Old User"
        };

        var request = new UpdateUserRequestDto
        {
            Username = "newuser",
            Email = "new@formulatrix.com",
            FullName = "New User"
        };

        var identityError = new IdentityError
        {
            Description = "Failed to update user."
        };

        _userManager
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.SetUserNameAsync(user, request.Username))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.SetEmailAsync(user, request.Email))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var result = await _service.UpdateUserAsync(user.Id, request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to update user."));
        Assert.That(
            result.Errors,
            Does.Contain("Failed to update user."));
    }

    [Test]
    public async Task UpdateUserAsync_ShouldReturnSuccess_WhenUpdateSucceeds()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "olduser",
            Email = "old@formulatrix.com",
            FullName = "Old User"
        };

        var request = new UpdateUserRequestDto
        {
            Username = "newuser",
            Email = "new@formulatrix.com",
            FullName = "New User"
        };

        var response = new UserResponseDto();

        _userManager
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.SetUserNameAsync(user, request.Username))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.SetEmailAsync(user, request.Email))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["Scientist"]);

        _mapper
            .Setup(x => x.Map<UserResponseDto>(user))
            .Returns(response);

        var result = await _service.UpdateUserAsync(user.Id, request);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("User updated successfully."));
        Assert.That(result.Data, Is.EqualTo(response));


        Assert.That(user.FullName, Is.EqualTo("New User"));
    }

    [Test]
    public async Task SetUserActiveStatusAsync_ShouldReturnError_WhenAdminDeactivatesOwnAccount()
    {
        var adminId = Guid.NewGuid();

        var result = await _service.SetUserActiveStatusAsync(
            adminId,
            adminId,
            false);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("You cannot deactivate your own account."));

        _userManager.Verify(
            x => x.FindByIdAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task SetUserActiveStatusAsync_ShouldReturnError_WhenUserNotFound()
    {
        var adminId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        _userManager
            .Setup(x => x.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _service.SetUserActiveStatusAsync(
            adminId,
            targetUserId,
            false);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("User not found."));

        _userManager.Verify(
            x => x.UpdateAsync(It.IsAny<ApplicationUser>()),
            Times.Never);

        _refreshTokenRepository.Verify(
         x => x.RevokeAllByUserIdAsync(
             It.IsAny<Guid>(),
             It.IsAny<CancellationToken>()),
         Times.Never);
    }

    [Test]
    public async Task SetUserActiveStatusAsync_ShouldReturnError_WhenUpdateFails()
    {
        var adminId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = targetUserId,
            UserName = "scientist1",
            IsActive = true
        };

        var identityError = new IdentityError
        {
            Description = "Database update failed."
        };

        _userManager
            .Setup(x => x.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var result = await _service.SetUserActiveStatusAsync(
            adminId,
            targetUserId,
            false);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Failed to update user status."));

        Assert.That(
            result.Errors,
            Does.Contain("Database update failed."));

        _refreshTokenRepository.Verify(
          x => x.RevokeAllByUserIdAsync(
              It.IsAny<Guid>(),
              It.IsAny<CancellationToken>()),
          Times.Never);
    }

 [Test]
public async Task SetUserActiveStatusAsync_ShouldDeactivateUserAndRevokeTokens_WhenSuccessful()
{
    var adminId = Guid.NewGuid();
    var targetUserId = Guid.NewGuid();

    var user = new ApplicationUser
    {
        Id = targetUserId,
        UserName = "scientist1",
        IsActive = true
    };

    _userManager
        .Setup(x => x.FindByIdAsync(targetUserId.ToString()))
        .ReturnsAsync(user);

    _userManager
        .Setup(x => x.UpdateAsync(user))
        .ReturnsAsync(IdentityResult.Success);

    _refreshTokenRepository
        .Setup(x => x.RevokeAllByUserIdAsync(
            targetUserId,
            It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var result = await _service.SetUserActiveStatusAsync(
        adminId,
        targetUserId,
        false);

    Assert.That(result.Success, Is.True);
    Assert.That(result.Data, Is.True);
    Assert.That(
        result.Message,
        Is.EqualTo("User deactivated successfully."));

    Assert.That(user.IsActive, Is.False);

    _refreshTokenRepository.Verify(
        x => x.RevokeAllByUserIdAsync(
            targetUserId,
            It.IsAny<CancellationToken>()),
        Times.Once);
}
[Test]
public async Task SetUserActiveStatusAsync_ShouldActivateUser_WhenSuccessful()
{
    var adminId = Guid.NewGuid();
    var targetUserId = Guid.NewGuid();

    var user = new ApplicationUser
    {
        Id = targetUserId,
        UserName = "scientist1",
        IsActive = false
    };

    _userManager
        .Setup(x => x.FindByIdAsync(targetUserId.ToString()))
        .ReturnsAsync(user);

    _userManager
        .Setup(x => x.UpdateAsync(user))
        .ReturnsAsync(IdentityResult.Success);

    var result = await _service.SetUserActiveStatusAsync(
        adminId,
        targetUserId,
        true);

    Assert.That(result.Success, Is.True);
    Assert.That(result.Data, Is.True);
    Assert.That(
        result.Message,
        Is.EqualTo("User activated successfully."));

    Assert.That(user.IsActive, Is.True);

      _refreshTokenRepository.Verify(
          x => x.RevokeAllByUserIdAsync(
              It.IsAny<Guid>(),
              It.IsAny<CancellationToken>()),
          Times.Never);
}

[Test]
public async Task GetUserStatisticsAsync_ShouldReturnCorrectStatistics()
{
    var activeUser = new ApplicationUser
    {
        Id = Guid.NewGuid(),
        UserName = "active",
        IsActive = true
    };

    var inactiveUser = new ApplicationUser
    {
        Id = Guid.NewGuid(),
        UserName = "inactive",
        IsActive = false
    };

    var activeAdmin = new ApplicationUser
    {
        Id = Guid.NewGuid(),
        UserName = "admin",
        IsActive = true
    };

    var inactiveAdmin = new ApplicationUser
    {
        Id = Guid.NewGuid(),
        UserName = "inactiveadmin",
        IsActive = false
    };

    await AddUsersAsync(
        activeUser,
        inactiveUser,
        activeAdmin,
        inactiveAdmin);

    _userManager
        .Setup(x => x.GetUsersInRoleAsync(
            nameof(UserRole.WarehouseAdmin)))
        .ReturnsAsync([
            activeAdmin,
            inactiveAdmin
        ]);

    var result = await _service.GetUserStatisticsAsync();

    Assert.That(result.Success, Is.True);
    Assert.That(result.Data, Is.Not.Null);

    Assert.That(result.Data!.TotalCount, Is.EqualTo(4));
    Assert.That(result.Data.ActiveCount, Is.EqualTo(2));
    Assert.That(result.Data.DeactivatedCount, Is.EqualTo(2));
    Assert.That(result.Data.ActiveAdminCount, Is.EqualTo(1));
}
[Test]
public async Task GetUserStatisticsAsync_ShouldReturnZero_WhenNoUsersExist()
{
    _userManager
        .Setup(x => x.GetUsersInRoleAsync(
            nameof(UserRole.WarehouseAdmin)))
        .ReturnsAsync([]);

    var result = await _service.GetUserStatisticsAsync();

    Assert.That(result.Success, Is.True);
    Assert.That(result.Data, Is.Not.Null);

    Assert.That(result.Data!.TotalCount, Is.EqualTo(0));
    Assert.That(result.Data.ActiveCount, Is.EqualTo(0));
    Assert.That(result.Data.DeactivatedCount, Is.EqualTo(0));
    Assert.That(result.Data.ActiveAdminCount, Is.EqualTo(0));
}

[Test]
public async Task GetUserByIdAsync_ShouldReturnError_WhenUserNotFound()
{
    var userId = Guid.NewGuid().ToString();

    _userManager
        .Setup(x => x.FindByIdAsync(userId))
        .ReturnsAsync((ApplicationUser?)null);

    var result = await _service.GetUserByIdAsync(userId);

    Assert.That(result.Success, Is.False);
    Assert.That(result.Message, Is.EqualTo("User not found."));

    _userManager.Verify(
        x => x.GetRolesAsync(It.IsAny<ApplicationUser>()),
        Times.Never);

    _mapper.Verify(
        x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()),
        Times.Never);
}
[Test]
public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
{
    var userId = Guid.NewGuid().ToString();

    var user = new ApplicationUser
    {
        Id = Guid.Parse(userId),
        UserName = "scientist1",
        Email = "scientist1@formulatrix.com",
        FullName = "Scientist One",
        IsActive = true
    };

    var response = new UserResponseDto();

    _userManager
        .Setup(x => x.FindByIdAsync(userId))
        .ReturnsAsync(user);

    _userManager
        .Setup(x => x.GetRolesAsync(user))
        .ReturnsAsync(["Scientist"]);

    _mapper
        .Setup(x => x.Map<UserResponseDto>(user))
        .Returns(response);

    var result = await _service.GetUserByIdAsync(userId);

    Assert.That(result.Success, Is.True);
    Assert.That(result.Data, Is.EqualTo(response));

}

[Test]
public async Task ChangeUserPasswordAsync_ShouldReturnError_WhenUserNotFound()
{
    var userId = Guid.NewGuid();

    var request = new ChangeUserPasswordRequestDto
    {
        NewPassword = "NewPassword123!"
    };

    _userManager
        .Setup(x => x.FindByIdAsync(userId.ToString()))
        .ReturnsAsync((ApplicationUser?)null);

    var result = await _service.ChangeUserPasswordAsync(
        userId,
        request);

    Assert.That(result.Success, Is.False);
    Assert.That(result.Message, Is.EqualTo("User not found."));

    _userManager.Verify(
        x => x.GeneratePasswordResetTokenAsync(
            It.IsAny<ApplicationUser>()),
        Times.Never);

    _userManager.Verify(
        x => x.ResetPasswordAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()),
        Times.Never);
}

[Test]
public async Task ChangeUserPasswordAsync_ShouldReturnError_WhenResetPasswordFails()
{
    var userId = Guid.NewGuid();

    var user = new ApplicationUser
    {
        Id = userId,
        UserName = "scientist1",
        Email = "scientist1@formulatrix.com",
        FullName = "Scientist One",
        IsActive = true
    };

    var request = new ChangeUserPasswordRequestDto
    {
        NewPassword = "NewPassword123!"
    };

    var identityError = new IdentityError
    {
        Description = "Password does not meet requirements."
    };

    _userManager
        .Setup(x => x.FindByIdAsync(userId.ToString()))
        .ReturnsAsync(user);

    _userManager
        .Setup(x => x.GeneratePasswordResetTokenAsync(user))
        .ReturnsAsync("reset-token");

    _userManager
        .Setup(x => x.ResetPasswordAsync(
            user,
            "reset-token",
            request.NewPassword))
        .ReturnsAsync(IdentityResult.Failed(identityError));

    var result = await _service.ChangeUserPasswordAsync(
        userId,
        request);

    Assert.That(result.Success, Is.False);
    Assert.That(
        result.Message,
        Is.EqualTo("Failed to change user password."));

    Assert.That(
        result.Errors,
        Does.Contain("Password does not meet requirements."));

    _userManager.Verify(
        x => x.GeneratePasswordResetTokenAsync(user),
        Times.Once);

    _userManager.Verify(
        x => x.ResetPasswordAsync(
            user,
            "reset-token",
            request.NewPassword),
        Times.Once);
}

[Test]
public async Task ChangeUserPasswordAsync_ShouldReturnSuccess_WhenPasswordChanged()
{
    var userId = Guid.NewGuid();

    var user = new ApplicationUser
    {
        Id = userId,
        UserName = "scientist1",
        Email = "scientist1@formulatrix.com",
        FullName = "Scientist One",
        IsActive = true
    };

    var request = new ChangeUserPasswordRequestDto
    {
        NewPassword = "NewPassword123!"
    };

    _userManager
        .Setup(x => x.FindByIdAsync(userId.ToString()))
        .ReturnsAsync(user);

    _userManager
        .Setup(x => x.GeneratePasswordResetTokenAsync(user))
        .ReturnsAsync("reset-token");

    _userManager
        .Setup(x => x.ResetPasswordAsync(
            user,
            "reset-token",
            request.NewPassword))
        .ReturnsAsync(IdentityResult.Success);

    var result = await _service.ChangeUserPasswordAsync(
        userId,
        request);

    Assert.That(result.Success, Is.True);
    Assert.That(result.Data, Is.True);
    Assert.That(
        result.Message,
        Is.EqualTo("User password changed successfully."));

    _userManager.Verify(
        x => x.GeneratePasswordResetTokenAsync(user),
        Times.Once);

    _userManager.Verify(
        x => x.ResetPasswordAsync(
            user,
            "reset-token",
            request.NewPassword),
        Times.Once);
}
[Test]
public async Task UpdateMyProfileAsync_ShouldReturnError_WhenUserNotFound()
{
    var userId = Guid.NewGuid();

    var request = new UpdateFullNameRequestDto
    {
        FullName = "Updated Name"
    };

    _userManager
        .Setup(x => x.FindByIdAsync(userId.ToString()))
        .ReturnsAsync((ApplicationUser?)null);

    var result = await _service.UpdateMyProfileAsync(
        userId,
        request);

    Assert.That(result.Success, Is.False);
    Assert.That(result.Message, Is.EqualTo("User not found."));

    _userManager.Verify(
        x => x.UpdateAsync(It.IsAny<ApplicationUser>()),
        Times.Never);

    _userManager.Verify(
        x => x.GetRolesAsync(It.IsAny<ApplicationUser>()),
        Times.Never);

    _mapper.Verify(
        x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()),
        Times.Never);
}

[Test]
public async Task UpdateMyProfileAsync_ShouldReturnError_WhenUpdateFails()
{
    var userId = Guid.NewGuid();

    var user = new ApplicationUser
    {
        Id = userId,
        UserName = "scientist1",
        Email = "scientist1@formulatrix.com",
        FullName = "Old Name",
        IsActive = true
    };

    var request = new UpdateFullNameRequestDto
    {
        FullName = "Updated Name"
    };

    var identityError = new IdentityError
    {
        Description = "Failed to update user."
    };

    _userManager
        .Setup(x => x.FindByIdAsync(userId.ToString()))
        .ReturnsAsync(user);

    _userManager
        .Setup(x => x.UpdateAsync(user))
        .ReturnsAsync(IdentityResult.Failed(identityError));

    var result = await _service.UpdateMyProfileAsync(
        userId,
        request);

    Assert.That(result.Success, Is.False);
    Assert.That(
        result.Message,
        Is.EqualTo("Failed to update profile."));

    Assert.That(
        result.Errors,
        Does.Contain("Failed to update user."));

    Assert.That(user.FullName, Is.EqualTo("Updated Name"));

    _userManager.Verify(
        x => x.UpdateAsync(user),
        Times.Once);

    _userManager.Verify(
        x => x.GetRolesAsync(It.IsAny<ApplicationUser>()),
        Times.Never);

    _mapper.Verify(
        x => x.Map<UserResponseDto>(It.IsAny<ApplicationUser>()),
        Times.Never);
}

[Test]
public async Task UpdateMyProfileAsync_ShouldReturnSuccess_WhenProfileUpdated()
{
    var userId = Guid.NewGuid();

    var user = new ApplicationUser
    {
        Id = userId,
        UserName = "scientist1",
        Email = "scientist1@formulatrix.com",
        FullName = "Old Name",
        IsActive = true
    };

    var request = new UpdateFullNameRequestDto
    {
        FullName = "Updated Name"
    };

    var response = new UserResponseDto();

    _userManager
        .Setup(x => x.FindByIdAsync(userId.ToString()))
        .ReturnsAsync(user);

    _userManager
        .Setup(x => x.UpdateAsync(user))
        .ReturnsAsync(IdentityResult.Success);

    _userManager
        .Setup(x => x.GetRolesAsync(user))
        .ReturnsAsync(["Scientist"]);

    _mapper
        .Setup(x => x.Map<UserResponseDto>(user))
        .Returns(response);

    var result = await _service.UpdateMyProfileAsync(
        userId,
        request);

    Assert.That(result.Success, Is.True);
    Assert.That(result.Data, Is.EqualTo(response));
    Assert.That(
        result.Message,
        Is.EqualTo("Profile updated successfully."));

    Assert.That(user.FullName, Is.EqualTo("Updated Name"));

    _userManager.Verify(
        x => x.UpdateAsync(user),
        Times.Once);

    _userManager.Verify(
        x => x.GetRolesAsync(user),
        Times.Once);

    _mapper.Verify(
        x => x.Map<UserResponseDto>(user),
        Times.Once);
}

}