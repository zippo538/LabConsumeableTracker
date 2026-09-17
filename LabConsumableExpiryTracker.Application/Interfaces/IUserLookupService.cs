namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IUserLookupService
{
    Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(
        IEnumerable<Guid> userIds, CancellationToken ct);
}