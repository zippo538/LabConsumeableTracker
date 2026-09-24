using NUnit.Framework;
using LabConsumableExpiryTracker.Domain.Models;
[TestFixture]
public class ApplicationUserTests
{
    [Test]
    public void NewApplicationUser_ShouldHaveDefaultValues()
    {
        var user =
            new ApplicationUser();
        Assert.Multiple(() =>
        {
            Assert.That(
                user.IsActive,
                Is.True);
            Assert.That(
                user.RefreshTokens,
                Is.Not.Null);
        });
    }
}