using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Tests.Domain;

[TestFixture]
public class RefreshTokenTests
{
    [Test]
    public void Token_NotExpiredAndNotRevoked_ShouldBeActive()
    {
        var token =
            new RefreshToken
            {
                ExpiresOnUtc =
                    DateTime.UtcNow.AddDays(1),
                IsRevoked = false
            };
        Assert.Multiple(() =>
        {
            Assert.That(
                token.IsExpired,
                Is.False);
            Assert.That(
                token.IsActive,
                Is.True);
        });
    }
    [Test]
    public void Token_Expired_ShouldNotBeActive()
    {
        var token =
            new RefreshToken
            {
                ExpiresOnUtc =
                    DateTime.UtcNow.AddDays(-1),
                IsRevoked = false
            };
        Assert.Multiple(() =>
        {
            Assert.That(
                token.IsExpired,
                Is.True);
            Assert.That(
                token.IsActive,
                Is.False);
        });
    }
    [Test]
    public void Token_Revoked_ShouldNotBeActive()
    {
        var token =
            new RefreshToken
            {
                ExpiresOnUtc =
                    DateTime.UtcNow.AddDays(1),
                IsRevoked = true
            };
        Assert.That(
            token.IsActive,
            Is.False);
    }
}