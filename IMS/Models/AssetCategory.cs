namespace IMS.Models;

public class AssetCategory
{
    public Guid CategoryId { get; set; } = Guid.NewGuid();
    public Guid OrgId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public List<AssetCategory> Children { get; set; } = new();
}
