namespace IMS.Models;

public class SmrLaborLineItem
{
    public Guid LaborLineId { get; set; } = Guid.NewGuid();
    public Guid TicketId { get; set; }
    public Guid TechnicianId { get; set; }
    public decimal HoursSpent { get; set; }
    public decimal CalculatedBurdenCost { get; set; }
}
