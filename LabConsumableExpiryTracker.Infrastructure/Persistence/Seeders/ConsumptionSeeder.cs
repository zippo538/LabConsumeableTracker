using Bogus;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence.Seeders;

public abstract class ConsumptionSeeder
{
    public static IReadOnlyCollection<Consumption> Generate(
        IEnumerable<Job> jobs,
        IEnumerable<Lot> lots,
        IEnumerable<Guid> actorIds,
        TimeProvider timeProvider,
        int consumptionCount = 100,
        int seed = 20260908)
    {
        ArgumentNullException.ThrowIfNull(jobs);
        ArgumentNullException.ThrowIfNull(lots);
        ArgumentNullException.ThrowIfNull(actorIds);
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (consumptionCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(consumptionCount),
                "Consumption count must be greater than zero.");
        }

        var eligibleJobs = jobs
            .Where(job => job.StartedAt.HasValue)
            .ToArray();

        var actors = actorIds
            .Where(actorId => actorId != Guid.Empty)
            .Distinct()
            .ToArray();

        var availableLots = lots
            .Where(lot => lot.RemainingQuantity > 0m)
            .ToArray();

        if (eligibleJobs.Length == 0 ||
            availableLots.Length == 0 ||
            actors.Length == 0)
        {
            return [];
        }

        // Sisa kuota per Lot agar total konsumsi tidak pernah
        // melampaui RemainingQuantity (BR-03, BR-04).
        var budgets = new Dictionary<Guid, decimal>(availableLots.Length);
        foreach (var lot in availableLots)
        {
            budgets[lot.Id] = lot.RemainingQuantity;
        }

        var faker = new Faker
        {
            Random = new Randomizer(seed)
        };

        var now = timeProvider.GetUtcNow();
        var consumptions = new List<Consumption>(consumptionCount);

        for (var consumptionIndex = 0;
             consumptionIndex < consumptionCount;
             consumptionIndex++)
        {
            var job = eligibleJobs[
                consumptionIndex % eligibleJobs.Length];

            var scenario = ConsumptionScenarios[
                consumptionIndex % ConsumptionScenarios.Length];

            var lotId = SelectLotWithBudget(
                availableLots,
                budgets,
                consumptionIndex);

            if (lotId == Guid.Empty)
            {
                break;
            }

            var quantity = decimal.Min(
                decimal.Round(
                    faker.Random.Decimal(
                        scenario.MinimumQuantity,
                        scenario.MaximumQuantity),
                    2,
                    MidpointRounding.AwayFromZero),
                budgets[lotId]);

            budgets[lotId] -= quantity;

            consumptions.Add(new Consumption(
                faker.Random.Guid(),
                job.Id,
                lotId,
                quantity,
                ResolveConsumedAt(job, now, faker),
                actors[consumptionIndex % actors.Length]));
        }

        return consumptions;
    }

    private static Guid SelectLotWithBudget(
        Lot[] availableLots,
        IReadOnlyDictionary<Guid, decimal> budgets,
        int consumptionIndex)
    {
        for (var offset = 0; offset < availableLots.Length; offset++)
        {
            var candidate = availableLots[
                (consumptionIndex + offset) % availableLots.Length];

            if (budgets[candidate.Id] > 0m)
            {
                return candidate.Id;
            }
        }

        return Guid.Empty;
    }

    private static DateTimeOffset ResolveConsumedAt(
        Job job,
        DateTimeOffset now,
        Faker faker)
    {
        var latestAllowed = now.AddMinutes(-1);
        var windowStart = job.StartedAt ?? latestAllowed;
        var windowEnd = job.CompletedAt ?? latestAllowed;

        if (windowEnd <= windowStart)
        {
            return windowStart;
        }

        var windowMinutes = (int)(windowEnd - windowStart).TotalMinutes;

        return windowStart.AddMinutes(
            faker.Random.Int(0, windowMinutes));
    }
    private static readonly (
        decimal MinimumQuantity,
        decimal MaximumQuantity)[] ConsumptionScenarios =
        [
            (0.50m, 5m),
            (5m, 25m),
            (25m, 100m),
            (1m, 10m),
            (10m, 50m)
        ];
}