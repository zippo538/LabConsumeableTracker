namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
