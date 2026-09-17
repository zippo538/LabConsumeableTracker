namespace LabConsumableExpiryTracker.Application.Common.CurrentUser;

/// <summary>Server-side role check. It must use the authenticated principal, never request data.</summary>
public interface ICurrentUserContext
{
    bool IsInRole(string role);
}
