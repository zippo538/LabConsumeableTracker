using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IConsumptionRepository : IRepository<Consumption, Guid>
{
    
}