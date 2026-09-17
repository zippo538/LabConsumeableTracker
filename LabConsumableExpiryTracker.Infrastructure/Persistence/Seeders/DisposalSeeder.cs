using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence.Seeders
{
    public class DisposalSeeder
    {
        public static IReadOnlyCollection<Disposal> Generate(
            IEnumerable<Lot> lots,
            IEnumerable<Consumption> consumptions,
            IEnumerable<Guid> actorIds,
            TimeProvider timeProvider,
            int disposalCount = 100,
            int seed = 20260908)
        {
            ArgumentNullException.ThrowIfNull(lots);
            ArgumentNullException.ThrowIfNull(consumptions);
            ArgumentNullException.ThrowIfNull(actorIds);
            ArgumentNullException.ThrowIfNull(timeProvider);

            if (disposalCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(disposalCount),
                    "Disposal count must be greater than zero.");
            }

            var actors = actorIds
                .Where(actorId => actorId != Guid.Empty)
                .Distinct()
                .ToArray();

            if (actors.Length == 0)
            {
                return [];
            }

            var faker = new Faker("en");
            faker.Random = new Randomizer(seed);

            var now = timeProvider.GetUtcNow();
            var latestAllowed = now.AddMinutes(-1);

            var availableLots = lots
                .Where(lot => lot.AdministrativeStatus !=
                              LotAdministrativeStatus.Disposed)
                .Where(lot => lot.RemainingQuantity > 0m)
                .Where(lot => lot.ReceivedAt < latestAllowed)
                .ToArray();

            if (availableLots.Length == 0)
            {
                return [];
            }

            var budgets = BuildBudgets(availableLots, consumptions);

            var disposals = new List<Disposal>(disposalCount);

            for (var disposalIndex = 0;
                 disposalIndex < disposalCount;
                 disposalIndex++)
            {
                var lot = SelectLotWithBudget(
                    availableLots,
                    budgets,
                    disposalIndex);

                if (lot is null)
                {
                    break;
                }

                var scenario = DisposalScenarios[
                    disposalIndex % DisposalScenarios.Length];

                var quantity = decimal.Min(
                    decimal.Round(
                        faker.Random.Decimal(
                            scenario.MinimumQuantity,
                            scenario.MaximumQuantity),
                        2,
                        MidpointRounding.AwayFromZero),
                    budgets[lot.Id]);

                budgets[lot.Id] -= quantity;

                disposals.Add(new Disposal(
                    faker.Random.Guid(),
                    lot.Id,
                    quantity,
                    DisposalReasons[
                        disposalIndex % DisposalReasons.Length],
                    ResolveDisposedAt(lot, latestAllowed, faker),
                    actors[disposalIndex % actors.Length]));
            }

            return disposals;
        }

        private static Dictionary<Guid, decimal> BuildBudgets(
            Lot[] availableLots,
            IEnumerable<Consumption> consumptions)
        {
            var consumedPerLot = consumptions
                .GroupBy(consumption => consumption.LotId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(consumption => consumption.Quantity));

            var budgets = new Dictionary<Guid, decimal>(
                availableLots.Length);

            foreach (var lot in availableLots)
            {
                consumedPerLot.TryGetValue(
                    lot.Id,
                    out var usedQuantity);

                budgets[lot.Id] = decimal.Max(
                    0m,
                    lot.RemainingQuantity - usedQuantity);
            }

            return budgets;
        }

        private static Lot? SelectLotWithBudget(
            Lot[] availableLots,
            IReadOnlyDictionary<Guid, decimal> budgets,
            int disposalIndex)
        {
            for (var offset = 0; offset < availableLots.Length; offset++)
            {
                var candidate = availableLots[
                    (disposalIndex + offset) % availableLots.Length];

                if (budgets[candidate.Id] > 0m)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static DateTimeOffset ResolveDisposedAt(
            Lot lot,
            DateTimeOffset latestAllowed,
            Faker faker)
        {
            var windowMinutes = Math.Max(
                1,
                (int)(latestAllowed - lot.ReceivedAt).TotalMinutes);

            return lot.ReceivedAt.AddMinutes(
                faker.Random.Int(1, windowMinutes));
        }

        private static readonly (
            decimal MinimumQuantity,
            decimal MaximumQuantity)[] DisposalScenarios =
        [
            (0.25m, 2m),
            (2m, 15m),
            (15m, 60m),
            (1m, 8m),
            (8m, 40m)
        ];

        private static readonly string[] DisposalReasons =
        [
            "Bahan rusak dan tidak dapat digunakan",
            "Kemasan rusak saat pengiriman",
            "Kontaminasi silang terdeteksi",
            "Kesalahan penyimpanan suhu",
            "Melebihi batas waktu pakai setelah dibuka",
            "Hasil uji QC tidak memenuhi spesifikasi"
        ];
    }
}
