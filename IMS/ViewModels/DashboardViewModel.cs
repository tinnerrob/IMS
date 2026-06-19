using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IMS.Models;
using IMS.Models.Enums;
using IMS.Services;

namespace IMS.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDataStoreService _dataStore;
    private readonly IAuthService _authService;

    public DashboardViewModel(IDataStoreService dataStore, IAuthService authService)
    {
        _dataStore = dataStore;
        _authService = authService;
    }

    public void Initialize()
    {
        Refresh();
    }

    [ObservableProperty]
    private int _totalAssets;

    [ObservableProperty]
    private int _availableAssets;

    [ObservableProperty]
    private int _inUseAssets;

    [ObservableProperty]
    private int _inRepairAssets;

    [ObservableProperty]
    private int _activeLeases;

    [ObservableProperty]
    private int _openSmrTickets;

    [ObservableProperty]
    private int _activeProjects;

    [ObservableProperty]
    private int _lowStockItems;

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    private string _userRole = string.Empty;

    [ObservableProperty]
    private List<LeaseTransactionLedger> _recentLeases = new();

    [ObservableProperty]
    private List<SmrServiceTicket> _recentTickets = new();

    [ObservableProperty]
    private List<AuditLogEntry> _recentActivity = new();

    [RelayCommand]
    private void Refresh()
    {
        var orgId = _authService.CurrentOrganization?.OrgId ?? Guid.Empty;
        var user = _authService.CurrentUser;

        WelcomeMessage = $"Welcome, {user?.DisplayName ?? "User"}";
        UserRole = $"Role: {user?.UserType.ToString().Replace("_", " ") ?? "N/A"}";

        var assets = _dataStore.GetAll<SerializedAsset>();
        TotalAssets = assets.Count;
        AvailableAssets = assets.Count(a => a.CurrentStatus == AssetStatus.Available);
        InUseAssets = assets.Count(a => a.CurrentStatus == AssetStatus.In_Use);
        InRepairAssets = assets.Count(a => a.CurrentStatus == AssetStatus.In_Repair || a.CurrentStatus == AssetStatus.Damaged_Cosmetic);

        ActiveLeases = _dataStore.GetActiveLeases().Count;
        OpenSmrTickets = _dataStore.GetOpenTickets().Count;
        ActiveProjects = _dataStore.GetAll<Project>().Count(p => p.EndDate >= DateTime.UtcNow);
        LowStockItems = _dataStore.GetLowStockPools(orgId).Count;

        RecentLeases = _dataStore.GetAll<LeaseTransactionLedger>()
            .OrderByDescending(l => l.OutboundTimestamp)
            .Take(5)
            .ToList();

        RecentTickets = _dataStore.GetAll<SmrServiceTicket>()
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .ToList();

        RecentActivity = _dataStore.GetAll<AuditLogEntry>()
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToList();
    }
}
