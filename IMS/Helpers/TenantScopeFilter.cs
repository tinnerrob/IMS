namespace IMS.Helpers;

/// <summary>
/// Provides tenant-scoped filtering for data queries.
/// All data access should go through DataStoreService which applies tenant filtering automatically.
/// </summary>
public static class TenantScopeFilter
{
    public static Guid CurrentTenantId { get; set; } = Guid.Empty;
}
