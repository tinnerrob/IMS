using IMS.Models.Enums;

namespace IMS.Models;

public class SmrServiceTicket
{
    public Guid TicketId { get; set; } = Guid.NewGuid();
    public Guid AssetId { get; set; }
    public Guid? AssignedTechnicianId { get; set; }
    public TicketType TicketType { get; set; }
    public TicketStatus TicketStatus { get; set; } = TicketStatus.Awaiting_Parts;
    public int? CurrentMeterReading { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public string? Description { get; set; }
}
