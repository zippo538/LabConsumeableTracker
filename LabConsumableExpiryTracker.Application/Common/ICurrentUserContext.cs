namespace LabConsumableExpiryTracker.Application.Common.CurrentUser;

public interface ICurrentUserContext
{
    bool IsInRole(string role);
}
