using IMS.Models.Enums;

namespace IMS.Models;

public class Organization
{
    public Guid OrgId { get; set; } = Guid.NewGuid();
    public string OrgName { get; set; } = string.Empty;
    public SubTier SubTier { get; set; } = SubTier.Standard;
}
