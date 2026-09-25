using System.Security.Claims;
using LabConsumableExpiryTracker.Application.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class HttpContextCurrentUserContextTests
{
    private Mock<IHttpContextAccessor> _httpContextAccessor = null!;
    private HttpContextCurrentUserContext _context = null!;

    [SetUp]
    public void Setup()
    {
        _httpContextAccessor =
            new Mock<IHttpContextAccessor>(MockBehavior.Strict);

        _context = new HttpContextCurrentUserContext(
            _httpContextAccessor.Object);
    }

    [Test]
    public void UserId_ShouldReturnEmptyGuid_WhenHttpContextIsNull()
    {
        _httpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns((HttpContext?)null);

        var result = _context.UserId;

        Assert.That(result, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void UserId_ShouldReturnEmptyGuid_WhenUserIdClaimDoesNotExist()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity())
        };

        _httpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(context);

        var result = _context.UserId;

        Assert.That(result, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void UserId_ShouldReturnUserId_WhenNameIdentifierClaimIsValid()
    {
        var userId = Guid.NewGuid();

        var identity = new ClaimsIdentity(
        [
            new Claim(
                ClaimTypes.NameIdentifier,
                userId.ToString())
        ]);

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        _httpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(context);

        var result = _context.UserId;

        Assert.That(result, Is.EqualTo(userId));
    }

    [Test]
    public void UserId_ShouldReturnUserIdFromSub_WhenNameIdentifierIsMissing()
    {
        var userId = Guid.NewGuid();

        var identity = new ClaimsIdentity(
        [
            new Claim("sub", userId.ToString())
        ]);

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        _httpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(context);

        var result = _context.UserId;

        Assert.That(result, Is.EqualTo(userId));
    }

    [Test]
    public void UserId_ShouldReturnEmptyGuid_WhenUserIdClaimIsInvalid()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(
                ClaimTypes.NameIdentifier,
                "invalid-guid")
        ]);

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        _httpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(context);

        var result = _context.UserId;

        Assert.That(result, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void IsAuthenticated_ShouldReturnTrue_WhenUserIsAuthenticated()
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            ],
            authenticationType: "Bearer");

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        _httpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(context);

        var result = _context.IsAuthenticated;

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAuthenticated_ShouldReturnFalse_WhenUserIsNotAuthenticated()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity())
        };

        _httpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(context);

        var result = _context.IsAuthenticated;

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsInRole_ShouldReturnTrue_WhenUserHasRole()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Role, "Scientist")
        ]);

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        _httpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(context);

        var result = _context.IsInRole("Scientist");

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsInRole_ShouldReturnFalse_WhenUserDoesNotHaveRole()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Role, "Scientist")
        ]);

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        _httpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(context);

        var result = _context.IsInRole("WarehouseAdmin");

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsInRole_ShouldReturnFalse_WhenHttpContextIsNull()
    {
        _httpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns((HttpContext?)null);

        var result = _context.IsInRole("Scientist");

        Assert.That(result, Is.False);
    }
}