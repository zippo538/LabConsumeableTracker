using Bogus;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence.Seeders
{
    public class LotSeeder
    {
        public static IReadOnlyCollection<Lot> Generate(
            IEnumerable<Guid> itemIds,
            TimeProvider timeProvider,
            int lotsPerItem = 4,
            int seed = 20260908)
        {
            ArgumentNullException.ThrowIfNull(itemIds);
            ArgumentNullException.ThrowIfNull(timeProvider);

            if (lotsPerItem <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lotsPerItem),
                    "Lots per item must be greater than zero.");
            }

            var validItemIds = itemIds
                .Where(itemId => itemId != Guid.Empty)
                .Distinct()
                .ToArray();

            if (validItemIds.Length == 0)
            {
                return [];
            }

            var faker = new Faker("en");
            faker.Random = new Randomizer(seed);

            var today = DateOnly.FromDateTime(
                timeProvider.GetUtcNow().UtcDateTime);

            var lots = new List<Lot>(
                validItemIds.Length * lotsPerItem);

            for (var itemIndex = 0;
                 itemIndex < validItemIds.Length;
                 itemIndex++)
            {
                var itemId = validItemIds[itemIndex];

                for (var lotIndex = 0;
                     lotIndex < lotsPerItem;
                     lotIndex++)
                {
                    var globalLotIndex =
                        (itemIndex * lotsPerItem) + lotIndex;

                    var scenario = LotScenarios[
                        globalLotIndex % LotScenarios.Length];

                    var initialQuantity = decimal.Round(
                        faker.Random.Decimal(25m, 500m),
                        2,
                        MidpointRounding.AwayFromZero);

                    var remainingQuantity = scenario.MustBeEmpty
                        ? 0m
                        : decimal.Round(
                            faker.Random.Decimal(1m, initialQuantity),
                            2,
                            MidpointRounding.AwayFromZero);

                    var receivedAt = new DateTimeOffset(
                            today.ToDateTime(new TimeOnly(8, 0)),
                            TimeSpan.Zero)
                        .AddDays(-faker.Random.Int(7, 365));

                    var itemKey = itemId
                        .ToString("N")[..8]
                        .ToUpperInvariant();

                    lots.Add(new Lot(
                        faker.Random.Guid(),
                        itemId,
                        $"LOT-{itemKey}-{lotIndex + 1:D3}",
                        faker.Random.Bool(0.8f)
                            ? $"SUP-{faker.Random.AlphaNumeric(8).ToUpperInvariant()}"
                            : null,
                        receivedAt,
                        faker.Company.CompanyName(),
                        initialQuantity,
                        remainingQuantity,
                        today.AddDays(
                            scenario.ExpiryDayOffset +
                            faker.Random.Int(0, 5)),
                        $"Rack-{faker.Random.Int(1, 8):D2}",
                        scenario.Status));
                }
            }

            return lots;
        }
        private static readonly (
    LotStatus Status,
    int ExpiryDayOffset,
    bool MustBeEmpty)[] LotScenarios =
    [
    (LotStatus.Active, 180, false),
    (LotStatus.Active, 7, false),
    (LotStatus.Quarantined, 90, false),
    (LotStatus.ManuallyBlocked, 120, false),
    (LotStatus.Disposed, 180, true),
    (LotStatus.Active, -30, false)
    ];
    }

}