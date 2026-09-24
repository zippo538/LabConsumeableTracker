namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface ICurrentUserContext
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
