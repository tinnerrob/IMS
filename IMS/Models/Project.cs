namespace IMS.Models;

public class Project
{
    public Guid ProjectId { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Notes { get; set; }
}
