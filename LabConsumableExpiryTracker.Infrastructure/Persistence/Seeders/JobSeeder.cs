using Bogus;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence.Seeders;

public abstract class JobSeeder
{
    public static IReadOnlyCollection<Job> Generate(
        TimeProvider timeProvider,
        int jobCount = 100,
        int seed = 20260908)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (jobCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(jobCount),
                "Job count must be greater than zero.");
        }

        var faker = new Faker
        {
            Random = new Randomizer(seed)
        };

        var now = timeProvider.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        // Lifecycle timestamp tidak boleh berada di masa depan.
        var latestStart = now.AddMinutes(-15);
        var latestCompletion = now.AddMinutes(-1);

        var jobs = new List<Job>(jobCount);

        for (var jobIndex = 0; jobIndex < jobCount; jobIndex++)
        {
            var scenario = JobScenarios[jobIndex % JobScenarios.Length];

            var createdAt = new DateTimeOffset(
                    today.ToDateTime(new TimeOnly(8, 0)),
                    TimeSpan.Zero)
                .AddDays(-scenario.CreatedDayOffset)
                .AddHours(-faker.Random.Int(0, 9));

            var job = new Job(
                faker.Random.Guid(),
                $"JOB-{today.Year:D4}-{jobIndex + 1:D4}");

            if (scenario.Status is JobStatus.InProgress or JobStatus.Completed)
            {
                job.Start(Cap(
                    createdAt.AddMinutes(faker.Random.Int(15, 720)),
                    latestStart));
            }

            if (scenario.Status == JobStatus.Completed)
            {
                job.Complete(Cap(
                    (job.StartedAt ?? createdAt)
                    .AddMinutes(faker.Random.Int(30, 2880)),
                    latestCompletion));
            }

            jobs.Add(job);
        }

        return jobs;
    }
    private static DateTimeOffset Cap(
        DateTimeOffset value,
        DateTimeOffset maximum) =>
        value > maximum ? maximum : value;

    private static readonly (
        JobStatus Status,
        int CreatedDayOffset)[] JobScenarios =
        [
            (JobStatus.Completed, 120),
            (JobStatus.InProgress, 6),
            (JobStatus.Draft, 3),
            (JobStatus.Completed, 45),
            (JobStatus.InProgress, 2),
            (JobStatus.Draft, 1)
        ];
}