using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
namespace LabConsumableExpiryTracker.Tests.Domain;

[TestFixture]
public class LotTests
{
    private readonly Guid _itemId = Guid.NewGuid();
    [Test]
    public void Create_ShouldSetRemainingQuantityEqualInitialQuantity()
    {
        var lot = Lot.Create(
            _itemId,
            "LOT001",
            "SUP001",
            DateTimeOffset.UtcNow,
            "Supplier A",
            100,
            new DateOnly(2027, 1, 1),
            "Storage A"
        );
        Assert.Multiple(() =>
        {
            Assert.That(lot.InitialQuantity, Is.EqualTo(100));
            Assert.That(lot.RemainingQuantity, Is.EqualTo(100));
            Assert.That(lot.AdministrativeStatus, Is.EqualTo(LotAdministrativeStatus.Active));
        });
    }
    [Test]
    public void UpdateRemainingQuantity_ShouldReduceStock()
    {
        var lot = CreateLot();
        lot.UpdateRemainingQuantity(40);
        Assert.That(lot.RemainingQuantity, Is.EqualTo(60));
    }
    [Test]
    public void Consume_ShouldCreateConsumptionAndReduceQuantity()
    {
        var lot = CreateLot();
        var now = DateTimeOffset.UtcNow;
        var consumption = lot.Consume(Guid.NewGuid(), 20, now, Guid.NewGuid());
        Assert.Multiple(() =>
        {
            Assert.That(consumption.Quantity, Is.EqualTo(20));
            Assert.That(lot.RemainingQuantity, Is.EqualTo(80));
        });
    }
    [Test]
    public void IsEligible_WhenActiveAndNotExpired_ShouldReturnTrue()
    {
        var lot = CreateLot();
        var result = lot.IsEligible(DateTimeOffset.UtcNow);
        Assert.That(result, Is.True);
    }
    [Test]
    public void GetExpiryCondition_WhenExpired_ShouldReturnExpired()
    {
        var lot = Lot.Create(
            _itemId,
            "LOT002",
            null,
            DateTimeOffset.UtcNow,
            null,
            100,
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Storage"
        );
        var result = lot.GetExpiryCondition(DateTimeOffset.UtcNow, 30);
        Assert.That(result, Is.EqualTo(ExpiryCondition.Expired));
    }
    [Test]
    public void Consume_WhenLotExpired_ShouldThrowException()
    {
        var lot = CreateLot(expiryDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        Assert.Throws<InvalidOperationException>(() =>
            lot.Consume(Guid.NewGuid(), 20, DateTimeOffset.UtcNow, Guid.NewGuid())
        );
    }
    private static Lot CreateLot(DateOnly? expiryDate = null, decimal quantity = 100)
    {
        return Lot.Create(
            Guid.NewGuid(),
            "LOT001",
            "SUP001",
            DateTimeOffset.UtcNow,
            "Supplier",
            quantity,
            expiryDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            "Storage A"
        );
    }
}