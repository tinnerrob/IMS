namespace IMS.Models;

public class BulkQuantityPool
{
    public Guid PoolId { get; set; } = Guid.NewGuid();
    public Guid OrgId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int TotalQuantityOwned { get; set; }
    public int MinSafetyStock { get; set; }
}
