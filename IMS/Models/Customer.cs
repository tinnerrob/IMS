using IMS.Models.Enums;

namespace IMS.Models;

public class Customer
{
    public Guid CustomerId { get; set; } = Guid.NewGuid();
    public Guid OrgId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public CreditStatus CreditStatus { get; set; } = CreditStatus.Approved;
    public string? ContactEmail { get; set; }
    public string? Phone { get; set; }
}
