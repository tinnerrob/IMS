using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IMS.Models;
using IMS.Models.Enums;
using IMS.Services;

namespace IMS.ViewModels;

public partial class ExceptionsViewModel : ObservableObject
{
    private readonly IDataStoreService _dataStore;
    private readonly IAuthService _authService;

    public ExceptionsViewModel(IDataStoreService dataStore, IAuthService authService)
    {
        _dataStore = dataStore;
        _authService = authService;
    }

    public void Initialize()
    {
        Refresh();
    }

    [ObservableProperty]
    private List<BulkQuantityPool> _lowStockPools = new();

    [ObservableProperty]
    private List<SerializedAsset> _damagedAssets = new();

    [ObservableProperty]
    private List<SmrServiceTicket> _openTickets = new();

    [ObservableProperty]
    private List<ScheduleAllocation> _conflictingAllocations = new();

    [ObservableProperty]
    private int _totalExceptions;

    [RelayCommand]
    private void Refresh()
    {
        var orgId = _authService.CurrentOrganization?.OrgId ?? Guid.Empty;

        LowStockPools = _dataStore.GetLowStockPools(orgId);
        DamagedAssets = _dataStore.GetAll<SerializedAsset>()
            .Where(a => a.CurrentStatus == AssetStatus.Damaged_Cosmetic || a.CurrentStatus == AssetStatus.In_Repair)
            .ToList();
        OpenTickets = _dataStore.GetOpenTickets();
        TotalExceptions = LowStockPools.Count + DamagedAssets.Count + OpenTickets.Count;
    }
}
