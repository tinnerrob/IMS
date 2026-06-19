namespace IMS.Models;

public class AuditLogEntry
{
    public Guid LogId { get; set; } = Guid.NewGuid();
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
