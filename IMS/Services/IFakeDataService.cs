using IMS.Models;

namespace IMS.Services;

public interface IFakeDataService
{
    List<Organization> Organizations { get; }
    List<TenantUser> TenantUsers { get; }
    List<AssetCategory> AssetCategories { get; }
    List<ProductCatalog> ProductCatalog { get; }
    List<SerializedAsset> SerializedAssets { get; }
    List<BulkQuantityPool> BulkQuantityPools { get; }
    List<Customer> Customers { get; }
    List<Project> Projects { get; }
    List<ScheduleAllocation> ScheduleAllocations { get; }
    List<LeaseTransactionLedger> LeaseTransactions { get; }
    List<SmrServiceTicket> SmrTickets { get; }
    List<SmrLaborLineItem> SmrLaborLines { get; }
    List<SmrPartsUsageLedger> SmrPartsUsage { get; }
    List<AuditLogEntry> AuditLogs { get; }
    void Initialize();
}
