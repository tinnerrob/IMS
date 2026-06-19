namespace IMS.Models;

public class SmrPartsUsageLedger
{
    public Guid PartsLineId { get; set; } = Guid.NewGuid();
    public Guid TicketId { get; set; }
    public string PartSkuOrId { get; set; } = string.Empty;
    public int QuantityConsumed { get; set; }
    public decimal UnitCostAtConsumption { get; set; }
}
