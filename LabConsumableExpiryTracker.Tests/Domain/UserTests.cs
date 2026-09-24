using NUnit.Framework;
using LabConsumableExpiryTracker.Domain.Models;
[TestFixture]
public class UserTests
{
    [Test]
    public void Constructor_ShouldSetUsernameAndEmail()
    {
        var user =
            new User(
                "john",
                "john@test.com"
            );
        Assert.Multiple(() =>
        {
            Assert.That(
                user.UserName,
                Is.EqualTo("john"));
            Assert.That(
                user.Email,
                Is.EqualTo("john@test.com"));
            Assert.That(
                user.IsActive,
                Is.True);
        });
    }
    [Test]
    public void Deactivate_ShouldSetUserInactive()
    {
        var user =
            new User(
                "john",
                "john@test.com"
            );
        user.Deactivate();
        Assert.That(
            user.IsActive,
            Is.False
        );
    }
}