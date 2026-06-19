using IMS.Models;
using IMS.Models.Enums;

namespace IMS.Services;

public class DataStoreService : IDataStoreService
{
    private readonly IFakeDataService _fakeData;
    private readonly IAuthService _authService;

    public DataStoreService(IFakeDataService fakeData, IAuthService authService)
    {
        _fakeData = fakeData;
        _authService = authService;
    }

    private Guid CurrentOrgId => _authService.CurrentOrganization?.OrgId ?? Guid.Empty;

    public List<T> GetAll<T>()
    {
        var data = GetDataSet<T>();
        return data.Where(FilterByOrg<T>()).ToList();
    }

    public T? GetById<T>(Guid id) where T : class
    {
        return GetAll<T>().FirstOrDefault(e => GetId(e) == id);
    }

    public List<T> GetByOrgId<T>(Guid orgId) where T : class
    {
        return GetDataSet<T>().Where(e => HasMatchingOrgId(e, orgId)).ToList();
    }

    // Inventory
    public List<AssetCategory> GetCategoryTree(Guid orgId)
    {
        var all = _fakeData.AssetCategories.Where(c => c.OrgId == orgId).ToList();
        var roots = all.Where(c => c.ParentCategoryId == null).ToList();
        foreach (var root in roots)
        {
            PopulateChildren(root, all);
        }
        return roots;
    }

    public List<AssetCategory> GetChildCategories(Guid parentId)
    {
        return _fakeData.AssetCategories.Where(c => c.ParentCategoryId == parentId).ToList();
    }

    public List<SerializedAsset> GetAssetsByCategory(Guid categoryId)
    {
        var cat = _fakeData.AssetCategories.FirstOrDefault(c => c.CategoryId == categoryId);
        if (cat == null) return new();

        // Get all descendant category IDs
        var allCats = _fakeData.AssetCategories.Where(c => c.OrgId == CurrentOrgId).ToList();
        var catIds = new HashSet<Guid> { categoryId };
        GetDescendantIds(categoryId, allCats, catIds);

        var skus = _fakeData.ProductCatalog
            .Where(p => catIds.Contains(p.CategoryId))
            .Select(p => p.Sku)
            .ToHashSet();

        return _fakeData.SerializedAssets
            .Where(a => a.OrgId == CurrentOrgId && skus.Contains(a.Sku))
            .ToList();
    }

    public List<SerializedAsset> GetAvailableAssets(Guid orgId)
    {
        return _fakeData.SerializedAssets
            .Where(a => a.OrgId == orgId && a.CurrentStatus == AssetStatus.Available)
            .ToList();
    }

    public List<BulkQuantityPool> GetLowStockPools(Guid orgId)
    {
        return _fakeData.BulkQuantityPools
            .Where(p => p.OrgId == orgId && p.TotalQuantityOwned <= p.MinSafetyStock)
            .ToList();
    }

    // Scheduling
    public List<ScheduleAllocation> GetAllocationsByProject(Guid projectId)
    {
        return _fakeData.ScheduleAllocations
            .Where(a => a.ProjectId == projectId)
            .ToList();
    }

    public List<ScheduleAllocation> GetAllocationsByDateRange(DateTime start, DateTime end)
    {
        return _fakeData.ScheduleAllocations
            .Where(a => a.StartDate < end && a.EndDate > start)
            .ToList();
    }

    public List<ScheduleAllocation> GetAllocationsForAsset(Guid assetId)
    {
        return _fakeData.ScheduleAllocations
            .Where(a => a.SerializedAssetId == assetId)
            .ToList();
    }

    public List<ScheduleAllocation> GetAllocationsForCategory(Guid categoryId)
    {
        return _fakeData.ScheduleAllocations
            .Where(a => a.CategoryId == categoryId)
            .ToList();
    }

    // SMR
    public List<SmrServiceTicket> GetTicketsByAsset(Guid assetId)
    {
        return _fakeData.SmrTickets
            .Where(t => t.AssetId == assetId)
            .ToList();
    }

    public List<SmrServiceTicket> GetOpenTickets()
    {
        return _fakeData.SmrTickets
            .Where(t => t.TicketStatus != TicketStatus.Resolved)
            .ToList();
    }

    public decimal GetTotalRepairCostForAsset(Guid assetId)
    {
        var ticketIds = _fakeData.SmrTickets
            .Where(t => t.AssetId == assetId)
            .Select(t => t.TicketId)
            .ToHashSet();

        var laborCost = _fakeData.SmrLaborLines
            .Where(l => ticketIds.Contains(l.TicketId))
            .Sum(l => l.CalculatedBurdenCost);

        var partsCost = _fakeData.SmrPartsUsage
            .Where(p => ticketIds.Contains(p.TicketId))
            .Sum(p => p.UnitCostAtConsumption * p.QuantityConsumed);

        return laborCost + partsCost;
    }

    // Leases
    public List<LeaseTransactionLedger> GetActiveLeases()
    {
        return _fakeData.LeaseTransactions
            .Where(l => l.ActualReturnTimestamp == null)
            .ToList();
    }

    public List<LeaseTransactionLedger> GetLeasesByProject(Guid projectId)
    {
        return _fakeData.LeaseTransactions
            .Where(l => l.ProjectId == projectId)
            .ToList();
    }

    public List<LeaseTransactionLedger> GetLeasesByAsset(Guid assetId)
    {
        return _fakeData.LeaseTransactions
            .Where(l => l.AssetId == assetId)
            .ToList();
    }

    // Reporting
    public int GetAvailableCountForCategory(Guid categoryId, DateTime date)
    {
        var assets = GetAssetsByCategory(categoryId);
        var allocatedIds = _fakeData.ScheduleAllocations
            .Where(a => a.SerializedAssetId.HasValue
                && a.StartDate <= date
                && a.EndDate >= date)
            .Select(a => a.SerializedAssetId!.Value)
            .ToHashSet();

        return assets.Count(a => !allocatedIds.Contains(a.AssetId)
            && a.CurrentStatus == AssetStatus.Available);
    }

    public Dictionary<string, int> GetCategoryUtilization(DateTime start, DateTime end)
    {
        var result = new Dictionary<string, int>();
        var rootCats = _fakeData.AssetCategories
            .Where(c => c.OrgId == CurrentOrgId && c.ParentCategoryId == null)
            .ToList();

        foreach (var cat in rootCats)
        {
            var total = GetAssetsByCategory(cat.CategoryId).Count;
            var allocated = _fakeData.ScheduleAllocations
                .Count(a => a.CategoryId == cat.CategoryId
                    && a.StartDate < end && a.EndDate > start);
            var pct = total > 0 ? (int)((double)allocated / total * 100) : 0;
            result[cat.Name] = Math.Min(pct, 100);
        }
        return result;
    }

    // ─── Unified type registry ───
    // Single source of truth for all type-to-data-set mappings

    private List<T> GetDataSet<T>()
    {
        var data = GetDataSetInternal(typeof(T));
        return data as List<T> ?? new List<T>();
    }

    private object GetDataSetInternal(Type type)
    {
        if (type == typeof(Organization)) return _fakeData.Organizations;
        if (type == typeof(TenantUser)) return _fakeData.TenantUsers;
        if (type == typeof(AssetCategory)) return _fakeData.AssetCategories;
        if (type == typeof(ProductCatalog)) return _fakeData.ProductCatalog;
        if (type == typeof(SerializedAsset)) return _fakeData.SerializedAssets;
        if (type == typeof(BulkQuantityPool)) return _fakeData.BulkQuantityPools;
        if (type == typeof(Customer)) return _fakeData.Customers;
        if (type == typeof(Project)) return _fakeData.Projects;
        if (type == typeof(ScheduleAllocation)) return _fakeData.ScheduleAllocations;
        if (type == typeof(LeaseTransactionLedger)) return _fakeData.LeaseTransactions;
        if (type == typeof(SmrServiceTicket)) return _fakeData.SmrTickets;
        if (type == typeof(SmrLaborLineItem)) return _fakeData.SmrLaborLines;
        if (type == typeof(SmrPartsUsageLedger)) return _fakeData.SmrPartsUsage;
        if (type == typeof(AuditLogEntry)) return _fakeData.AuditLogs;
        return new List<object>();
    }

    private Func<T, bool> FilterByOrg<T>()
    {
        return e => !HasOrgId(e) || HasMatchingOrgId(e, CurrentOrgId);
    }

    private static bool HasOrgId<T>(T entity) => entity switch
    {
        Organization => true,
        TenantUser => true,
        AssetCategory => true,
        ProductCatalog => true,
        SerializedAsset => true,
        BulkQuantityPool => true,
        Customer => true,
        AuditLogEntry => true,
        _ => false
    };

    private static bool HasMatchingOrgId<T>(T entity, Guid orgId) => entity switch
    {
        Organization o => o.OrgId == orgId,
        TenantUser u => u.OrgId == orgId,
        AssetCategory c => c.OrgId == orgId,
        ProductCatalog p => p.OrgId == orgId,
        SerializedAsset a => a.OrgId == orgId,
        BulkQuantityPool b => b.OrgId == orgId,
        Customer c => c.OrgId == orgId,
        AuditLogEntry a => a.OrgId == orgId,
        _ => true
    };

    private static Guid GetId<T>(T entity) => entity switch
    {
        Organization o => o.OrgId,
        TenantUser u => u.UserId,
        AssetCategory c => c.CategoryId,
        SerializedAsset a => a.AssetId,
        BulkQuantityPool b => b.PoolId,
        Customer c => c.CustomerId,
        Project p => p.ProjectId,
        ScheduleAllocation a => a.AllocationId,
        LeaseTransactionLedger l => l.TransactionId,
        SmrServiceTicket t => t.TicketId,
        SmrLaborLineItem l => l.LaborLineId,
        SmrPartsUsageLedger p => p.PartsLineId,
        AuditLogEntry a => a.LogId,
        _ => Guid.Empty
    };

    private static void PopulateChildren(AssetCategory parent, List<AssetCategory> all)
    {
        parent.Children = all.Where(c => c.ParentCategoryId == parent.CategoryId).ToList();
        foreach (var child in parent.Children)
        {
            PopulateChildren(child, all);
        }
    }

    private static void GetDescendantIds(Guid parentId, List<AssetCategory> all, HashSet<Guid> ids)
    {
        var children = all.Where(c => c.ParentCategoryId == parentId).ToList();
        foreach (var child in children)
        {
            ids.Add(child.CategoryId);
            GetDescendantIds(child.CategoryId, all, ids);
        }
    }
}
