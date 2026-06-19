using IMS.Models.Enums;

namespace IMS.Models;

public class TenantUser
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public Guid OrgId { get; set; }
    public UserType UserType { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public decimal? InternalHourlyRate { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
