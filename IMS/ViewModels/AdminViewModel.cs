using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IMS.Models;
using IMS.Models.Enums;
using IMS.Services;

namespace IMS.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    private readonly IDataStoreService _dataStore;
    private readonly IAuthService _authService;

    public AdminViewModel(IDataStoreService dataStore, IAuthService authService)
    {
        _dataStore = dataStore;
        _authService = authService;
        Refresh();
    }

    [ObservableProperty]
    private List<TenantUser> _users = new();

    [ObservableProperty]
    private List<Organization> _organizations = new();

    [ObservableProperty]
    private List<AuditLogEntry> _auditLog = new();

    [ObservableProperty]
    private TenantUser? _selectedUser;

    [ObservableProperty]
    private bool _isAdmin;

    [RelayCommand]
    private void Refresh()
    {
        IsAdmin = _authService.CurrentUser?.UserType == UserType.System_Admin;
        Users = _dataStore.GetAll<TenantUser>().OrderBy(u => u.DisplayName).ToList();
        Organizations = _dataStore.GetAll<Organization>();
        AuditLog = _dataStore.GetAll<AuditLogEntry>()
            .OrderByDescending(a => a.Timestamp)
            .Take(50)
            .ToList();
    }

    [RelayCommand]
    private void SelectUser(TenantUser user)
    {
        SelectedUser = user;
    }
}
