using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
namespace LabConsumableExpiryTracker.Tests.Domain;

[TestFixture]
public class ItemTests
{
    private readonly Guid _itemId = Guid.NewGuid();
    [Test]
    public void Constructor_ShouldCreateItem_WithValidData()
    {
        var item = new Item(
            _itemId,
            "CHEM001",
            "Ethanol",
            UnitOfMeasure.Milliliter,
            100,
            30
        );
        Assert.Multiple(() =>
        {
            Assert.That(item.Id, Is.EqualTo(_itemId));
            Assert.That(item.Code, Is.EqualTo("CHEM001"));
            Assert.That(item.Name, Is.EqualTo("Ethanol"));
            Assert.That(item.BaseUnit,
                Is.EqualTo(UnitOfMeasure.Milliliter));
            Assert.That(item.MinimumStock,
                Is.EqualTo(100));
            Assert.That(item.ExpiringSoonDays,
                Is.EqualTo(30));
        });
    }
    [Test]
    public void UpdateDetails_WithEmptyName_ShouldThrowException()
    {
        var item = CreateItem();
        Assert.Throws<ArgumentException>(() =>
        {
            item.UpdateDetails(
                "",
                UnitOfMeasure.Milliliter,
                100,
                30
            );
        });
    }
    [Test]
    public void UpdateDetails_WithNegativeMinimumStock_ShouldThrowException()
    {
        var item = CreateItem();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            item.UpdateDetails(
                "Updated Item",
                UnitOfMeasure.Milliliter,
                -10,
                30
            );
        });
    }
    [Test]
    public void GetStockStatus_WhenQuantityZero_ShouldReturnOutOfStock()
    {
        var item = CreateItem();
        var result =
            item.GetStockStatus(0);
        Assert.That(
            result,
            Is.EqualTo(StockLevelStatus.OutOfStock)
        );
    }
    [Test]
    public void GetStockStatus_WhenBelowMinimum_ShouldReturnLowStock()
    {
        var item = CreateItem();
        var result =
            item.GetStockStatus(50);
        Assert.That(
            result,
            Is.EqualTo(StockLevelStatus.LowStock)
        );
    }
    [Test]
    public void GetStockStatus_WhenEqualMinimum_ShouldReturnAtMinimum()
    {
        var item = CreateItem();
        var result =
            item.GetStockStatus(100);
        Assert.That(
            result,
            Is.EqualTo(StockLevelStatus.AtMinimum)
        );
    }
    [Test]
    public void GetStockStatus_WhenAboveMinimum_ShouldReturnSufficient()
    {
        var item = CreateItem();
        var result =
            item.GetStockStatus(200);
        Assert.That(
            result,
            Is.EqualTo(StockLevelStatus.Sufficient)
        );
    }
    [Test]
    public void IsLowStock_WhenBelowMinimum_ShouldReturnTrue()
    {
        var item = CreateItem();
        Assert.That(
            item.IsLowStock(20),
            Is.True
        );
    }
    private static Item CreateItem()
    {
        return new Item(
            Guid.NewGuid(),
            "CHEM001",
            "Ethanol",
            UnitOfMeasure.Milliliter,
            100,
            30
        );
    }
}