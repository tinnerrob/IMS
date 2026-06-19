using IMS.Models.Enums;

namespace IMS.Models;

public class ScheduleAllocation
{
    public Guid AllocationId { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public AllocationTier AllocationTier { get; set; } = AllocationTier.Soft_Hold;
    public Guid? CategoryId { get; set; }
    public Guid? SerializedAssetId { get; set; }
    public int BulkQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Notes { get; set; }
    public string? Details { get; set; }
}
