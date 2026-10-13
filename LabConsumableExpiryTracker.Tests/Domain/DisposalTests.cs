using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Tests.Domain;

[TestFixture]
public class DisposalTests
{
    [Test]
    public void Create_ShouldCreateDisposal()
    {
        var lotId = Guid.NewGuid();
        var disposal =
            Disposal.Create(
                lotId,
                20,
                "Expired material",
                DateTimeOffset.UtcNow,
                Guid.NewGuid()
            );
        Assert.Multiple(() =>
        {
            Assert.That(disposal.LotId,
                Is.EqualTo(lotId));
            Assert.That(disposal.Quantity,
                Is.EqualTo(20));
            Assert.That(disposal.Reason,
                Is.EqualTo("Expired material"));
        });
    }
    [Test]
    public void Create_WithZeroQuantity_ShouldThrowException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            Disposal.Create(
                Guid.NewGuid(),
                0,
                "Expired",
                DateTimeOffset.UtcNow,
                Guid.NewGuid()
            );
        });
    }
    [Test]
    public void Create_WithEmptyReason_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Disposal.Create(
                Guid.NewGuid(),
                10,
                "",
                DateTimeOffset.UtcNow,
                Guid.NewGuid()
            );
        });
    }
}