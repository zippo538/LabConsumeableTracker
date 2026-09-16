using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Repositories;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories;

public class ConsumptionRepository : Repository<Consumption, Guid>, IConsumptionRepository
{
    public ConsumptionRepository(AppDbContext context) : base(context)
    {
    }
}
