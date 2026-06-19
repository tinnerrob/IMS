namespace IMS.Models;

public class ProductCatalog
{
    public string Sku { get; set; } = string.Empty;
    public Guid OrgId { get; set; }
    public Guid CategoryId { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string? Specifications { get; set; }
    public decimal BaseCost { get; set; }
}
