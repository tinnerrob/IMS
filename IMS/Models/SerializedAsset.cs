using IMS.Models.Enums;

namespace IMS.Models;

public class SerializedAsset
{
    public Guid AssetId { get; set; } = Guid.NewGuid();
    public Guid OrgId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string BarcodeRfid { get; set; } = string.Empty;
    public AssetStatus CurrentStatus { get; set; } = AssetStatus.Available;
    public DateTime PurchaseDate { get; set; }
    public string? DamageHistory { get; set; }
    public int CurrentMeterReading { get; set; }
}
