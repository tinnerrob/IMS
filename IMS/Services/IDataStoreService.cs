using IMS.Models;

namespace IMS.Services;

public interface IDataStoreService
{
    // Generic queries
    List<T> GetAll<T>();
    T? GetById<T>(Guid id) where T : class;
    List<T> GetByOrgId<T>(Guid orgId) where T : class;

    // Inventory
    List<AssetCategory> GetCategoryTree(Guid orgId);
    List<AssetCategory> GetChildCategories(Guid parentId);
    List<SerializedAsset> GetAssetsByCategory(Guid categoryId);
    List<SerializedAsset> GetAvailableAssets(Guid orgId);
    List<BulkQuantityPool> GetLowStockPools(Guid orgId);

    // Scheduling
    List<ScheduleAllocation> GetAllocationsByProject(Guid projectId);
    List<ScheduleAllocation> GetAllocationsByDateRange(DateTime start, DateTime end);
    List<ScheduleAllocation> GetAllocationsForAsset(Guid assetId);
    List<ScheduleAllocation> GetAllocationsForCategory(Guid categoryId);

    // SMR
    List<SmrServiceTicket> GetTicketsByAsset(Guid assetId);
    List<SmrServiceTicket> GetOpenTickets();
    decimal GetTotalRepairCostForAsset(Guid assetId);

    // Leases
    List<LeaseTransactionLedger> GetActiveLeases();
    List<LeaseTransactionLedger> GetLeasesByProject(Guid projectId);
    List<LeaseTransactionLedger> GetLeasesByAsset(Guid assetId);

    // Reporting
    int GetAvailableCountForCategory(Guid categoryId, DateTime date);
    Dictionary<string, int> GetCategoryUtilization(DateTime start, DateTime end);
}
