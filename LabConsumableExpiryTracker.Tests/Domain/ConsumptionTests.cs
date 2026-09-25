using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Tests.Domain;
[TestFixture]
public class ConsumptionTests
{
    [Test]
    public void Constructor_ShouldStoreConsumptionData()
    {
        var id = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var lotId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var consumption =
            new Consumption(
                id,
                jobId,
                lotId,
                50,
                DateTimeOffset.UtcNow,
                userId
            );
        Assert.Multiple(() =>
        {
            Assert.That(consumption.Id,
                Is.EqualTo(id));
            Assert.That(consumption.JobId,
                Is.EqualTo(jobId));
            Assert.That(consumption.LotId,
                Is.EqualTo(lotId));
            Assert.That(consumption.Quantity,
                Is.EqualTo(50));
            Assert.That(consumption.ConsumedBy,
                Is.EqualTo(userId));
        });
    }
}