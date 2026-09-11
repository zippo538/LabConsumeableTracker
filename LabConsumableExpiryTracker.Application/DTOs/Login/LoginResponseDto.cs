
namespace LabConsumableExpiryTracker.Application.DTOs.Login;

public record LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    
}