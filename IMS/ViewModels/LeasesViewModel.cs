using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IMS.Models;
using IMS.Services;

namespace IMS.ViewModels;

public partial class LeasesViewModel : ObservableObject
{
    private readonly IDataStoreService _dataStore;
    private readonly IAuthService _authService;

    public LeasesViewModel(IDataStoreService dataStore, IAuthService authService)
    {
        _dataStore = dataStore;
        _authService = authService;
        Refresh();
    }

    [ObservableProperty]
    private List<LeaseTransactionLedger> _activeLeases = new();

    [ObservableProperty]
    private List<LeaseTransactionLedger> _completedLeases = new();

    [ObservableProperty]
    private List<LeaseTransactionLedger> _allLeases = new();

    [ObservableProperty]
    private LeaseTransactionLedger? _selectedLease;

    [ObservableProperty]
    private bool _isLeaseSelected;

    [RelayCommand]
    private void Refresh()
    {
        AllLeases = _dataStore.GetAll<LeaseTransactionLedger>()
            .OrderByDescending(l => l.OutboundTimestamp)
            .ToList();
        ActiveLeases = _dataStore.GetActiveLeases();
        CompletedLeases = AllLeases.Where(l => l.ActualReturnTimestamp != null).ToList();
    }

    [RelayCommand]
    private void SelectLease(LeaseTransactionLedger lease)
    {
        SelectedLease = lease;
        IsLeaseSelected = true;
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedLease = null;
        IsLeaseSelected = false;
    }
}
