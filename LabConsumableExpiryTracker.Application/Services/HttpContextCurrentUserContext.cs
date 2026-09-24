using System.Security.Claims;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
namespace LabConsumableExpiryTracker.Application.Services;
public class HttpContextCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public HttpContextCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return Guid.Empty;
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            userIdClaim ??= user.FindFirstValue("sub");
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;
}
