using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IMS.Models;
using IMS.Models.Enums;
using IMS.Services;

namespace IMS.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly IDataStoreService _dataStore;
    private readonly IAuthService _authService;

    public ReportsViewModel(IDataStoreService dataStore, IAuthService authService)
    {
        _dataStore = dataStore;
        _authService = authService;
    }

    public void Initialize()
    {
        Refresh();
    }

    [ObservableProperty]
    private DateTime _lookAheadDate = DateTime.Today;

    [ObservableProperty]
    private int _availableOnDate;

    [ObservableProperty]
    private int _inUseOnDate;

    [ObservableProperty]
    private int _totalAssets;

    [ObservableProperty]
    private decimal _totalMaintenanceCost;

    [ObservableProperty]
    private decimal _totalLaborCost;

    [ObservableProperty]
    private decimal _totalPartsCost;

    [ObservableProperty]
    private int _totalTickets;

    [ObservableProperty]
    private List<AssetCategory> _categoryUtilization = new();

    [RelayCommand]
    private void Refresh()
    {
        var assets = _dataStore.GetAll<SerializedAsset>();
        TotalAssets = assets.Count;

        // Maintenance costs
        var laborLines = _dataStore.GetAll<SmrLaborLineItem>();
        var partsLines = _dataStore.GetAll<SmrPartsUsageLedger>();
        TotalLaborCost = laborLines.Sum(l => l.CalculatedBurdenCost);
        TotalPartsCost = partsLines.Sum(p => p.UnitCostAtConsumption * p.QuantityConsumed);
        TotalMaintenanceCost = TotalLaborCost + TotalPartsCost;
        TotalTickets = _dataStore.GetAll<SmrServiceTicket>().Count;

        // Category utilization
        CategoryUtilization = _dataStore.GetCategoryTree(Guid.Empty);

        // Look-ahead
        UpdateLookAhead();
    }

    [RelayCommand]
    private void UpdateLookAhead()
    {
        var assets = _dataStore.GetAll<SerializedAsset>();
        var activeLeases = _dataStore.GetActiveLeases();

        TotalAssets = assets.Count;
        InUseOnDate = activeLeases.Count;
        AvailableOnDate = TotalAssets - InUseOnDate;
    }
}
