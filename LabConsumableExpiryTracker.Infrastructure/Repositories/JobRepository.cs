using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace LabConsumableExpiryTracker.Infrastructure.Repositories;

public class JobRepository(AppDbContext context) : Repository<Job, Guid>(context), IJobRepository
{
    public Task<bool> ExistsByJobNumberAsync(string jobNumber, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jobNumber))
        {
            throw new ArgumentException(
                "Job number is required.",
                nameof(jobNumber));
        }
        var normalizedJobNumber = jobNumber.Trim();
        return DbSet
            .AsNoTracking()
            .AnyAsync(
                job => job.JobNumber == normalizedJobNumber,
                ct);
    }
    public Task<Job?> GetByJobNumberAsync(string jobNumber, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jobNumber))
        {
            throw new ArgumentException("Job number is required.", nameof(jobNumber));
        }
        var normalizedJobNumber = jobNumber.Trim();
        return DbSet
            .AsNoTracking()
            .SingleOrDefaultAsync(
                job => job.JobNumber == normalizedJobNumber,
                ct);
    }
}