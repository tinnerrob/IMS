namespace IMS.Models;

public class LeaseTransactionLedger
{
    public Guid TransactionId { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Guid AssetId { get; set; }
    public DateTime OutboundTimestamp { get; set; }
    public Guid DispatchedByUserId { get; set; }
    public DateTime? ActualReturnTimestamp { get; set; }
    public Guid? ReceivedByUserId { get; set; }
    public string? ReturnCondition { get; set; }
}
