using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Data.Entities;

namespace LabConsumableExpiryTracker.Tests.Domain;

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